using BlueBerryDictionary.ApiClient;
using BlueBerryDictionary.Data;
using BlueBerryDictionary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyDictionary.Services
{
    public class WordSearchService
    {
        private readonly WordCacheManager _cacheManager;
        private readonly DictionaryApiClient _apiClient;
        private readonly List<string> _dictionary;

        public WordSearchService()
        {
            _cacheManager = WordCacheManager.Instance;
            _apiClient = new DictionaryApiClient();
            _dictionary = FileStorage.BuildDictionary();
        }

        #region Search
        public async Task<List<Word>> SearchWordAsync(string word, CancellationToken ct = default)
        {
            word = word.ToLower().Trim();

            // ========== CHECK CACHE ==========
            var cachedWords = _cacheManager.GetWordsFormCache(word);
            if (cachedWords != null)
            {
                Console.WriteLine("✅ Found in cache");
                return cachedWords;
            }

            // ========== CHECK LOCAL FILE ==========
            var localWords = await FileStorage.LoadWordAsync(word);
            if (localWords != null && localWords.Count > 0)
            {
                Console.WriteLine("✅ Found in local storage");
                _cacheManager.AddToCache(word, localWords);
                return localWords;
            }

            // ========== PARALLEL: Gọi 2 API cùng lúc ==========
            Console.WriteLine("🔄 Fetching from both APIs in parallel...");

            var merriamTask = FetchMerriamWebsterAsync(word, ct);
            var freeTask = FetchFreeDictionaryAsync(word, ct);

            // Chờ cả 2 API xong
            var results = await Task.WhenAll(merriamTask, freeTask);
            var merriamWords = results[0];
            var freeWords = results[1];

            // ========== MERGE khi cả 2 đã xong ==========
            if (merriamWords?.Count > 0 && freeWords?.Count > 0)
            {
                Console.WriteLine("✅ Both APIs returned data - Merging...");
                var merged = MergeApiResults(merriamWords[0], freeWords[0]);
                _cacheManager.AddToCache(word, new List<Word> { merged });
                return new List<Word> { merged };
            }
            else if (merriamWords?.Count > 0)
            {
                Console.WriteLine("✅ Using Merriam-Webster only");
                _cacheManager.AddToCache(word, merriamWords);
                return merriamWords;
            }
            else if (freeWords?.Count > 0)
            {
                Console.WriteLine("✅ Using Free Dictionary only");
                _cacheManager.AddToCache(word, freeWords);
                return freeWords;
            }

            Console.WriteLine($"❌ Word '{word}' not found in any API");
            return new List<Word>();
        }

        /// <summary>
        /// Fetch từ Merriam-Webster API
        /// </summary>
        private async Task<List<Word>> FetchMerriamWebsterAsync(string word, CancellationToken ct)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                Console.WriteLine("🔄 [MW] Fetching Merriam-Webster...");
                var words = await _apiClient.FetchFromMerriamWebster(word, cts.Token);

                if (words?.Count > 0)
                    Console.WriteLine($"✅ [MW] Got {words.Count} result(s)");
                else
                    Console.WriteLine("❌ [MW] No results");

                return words ?? new List<Word>();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ [MW] Timeout (3s)");
                return new List<Word>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [MW] Error: {ex.GetType().Name} - {ex.Message}");
                return new List<Word>();
            }
        }

        /// <summary>
        /// Fetch từ Free Dictionary API
        /// </summary>
        private async Task<List<Word>> FetchFreeDictionaryAsync(string word, CancellationToken ct)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                Console.WriteLine("🔄 [FREE] Fetching Free Dictionary...");
                var words = await _apiClient.FetchFromFreeDictionary(word, cts.Token);

                if (words?.Count > 0)
                    Console.WriteLine($"✅ [FREE] Got {words.Count} result(s)");
                else
                    Console.WriteLine("❌ [FREE] No results");

                return words ?? new List<Word>();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ [FREE] Timeout (3s)");
                return new List<Word>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [FREE] Error: {ex.GetType().Name} - {ex.Message}");
                return new List<Word>();
            }
        }

        /// <summary>
        /// Merge dữ liệu từ 2 API (synchronous, sau khi cả 2 đã xong)
        /// </summary>
        private Word MergeApiResults(Word merriamWord, Word freeWord)
        {
            try
            {
                for (int i = 0; i < merriamWord.meanings.Count; i++)
                {
                    var merriamMeaning = merriamWord.meanings[i];
                    var correspondingFreeMeaning = freeWord.meanings?
                        .FirstOrDefault(m => m.partOfSpeech == merriamMeaning.partOfSpeech);

                    if (correspondingFreeMeaning == null)
                        continue;

                    MergeDefinitions(merriamMeaning, correspondingFreeMeaning);
                    MergePhonetics(merriamWord, freeWord);
                }

                Console.WriteLine($"✅ Merged successfully: {merriamWord.word}");
                return merriamWord;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Merge error: {ex.Message}");
                return merriamWord;
            }
        }

        /// <summary>
        /// Merge definitions từ Free vào Merriam
        /// </summary>
        private void MergeDefinitions(Meaning merriamMeaning, Meaning freeMeaning)
        {
            if (freeMeaning.definitions?.Count == 0)
                return;

            if (merriamMeaning.definitions?.Count > 0)
            {
                for (int i = 0; i < merriamMeaning.definitions.Count && i < freeMeaning.definitions.Count; i++)
                {
                    var mDef = merriamMeaning.definitions[i];
                    var fDef = freeMeaning.definitions[i];

                    // Thêm example từ Free nếu Merriam thiếu
                    if (string.IsNullOrEmpty(mDef.example) && !string.IsNullOrEmpty(fDef.example))
                        mDef.example = fDef.example;

                    // Merge synonyms & antonyms
                    MergeSynonymsAntonyms(mDef, fDef);
                }
            }
            else if (freeMeaning.definitions?.Count > 0)
            {
                merriamMeaning.definitions = new List<Definition>(freeMeaning.definitions);
            }

            // Merge meaning-level synonyms/antonyms
            if (merriamMeaning.synonyms?.Count == 0 && freeMeaning.synonyms?.Count > 0)
                merriamMeaning.synonyms = new List<string>(freeMeaning.synonyms);

            if (merriamMeaning.antonyms?.Count == 0 && freeMeaning.antonyms?.Count > 0)
                merriamMeaning.antonyms = new List<string>(freeMeaning.antonyms);
        }

        /// <summary>
        /// Merge synonyms/antonyms của definition
        /// </summary>
        private void MergeSynonymsAntonyms(Definition mDef, Definition fDef)
        {
            // Synonyms
            if (mDef.synonyms?.Count == 0 && fDef.synonyms?.Count > 0)
                mDef.synonyms = new List<string>(fDef.synonyms);
            else if (mDef.synonyms?.Count > 0 && fDef.synonyms?.Count > 0)
            {
                var merged = new HashSet<string>(mDef.synonyms);
                foreach (var syn in fDef.synonyms)
                    merged.Add(syn);
                mDef.synonyms = merged.ToList();
            }

            // Antonyms
            if (mDef.antonyms?.Count == 0 && fDef.antonyms?.Count > 0)
                mDef.antonyms = new List<string>(fDef.antonyms);
            else if (mDef.antonyms?.Count > 0 && fDef.antonyms?.Count > 0)
            {
                var merged = new HashSet<string>(mDef.antonyms);
                foreach (var ant in fDef.antonyms)
                    merged.Add(ant);
                mDef.antonyms = merged.ToList();
            }
        }

        /// <summary>
        /// Merge audio từ Free vào Merriam
        /// </summary>
        private void MergePhonetics(Word merriamWord, Word freeWord)
        {
            if (freeWord.phonetics?.Count == 0 || merriamWord.phonetics?.Count == 0)
                return;

            foreach (var mwPhonetic in merriamWord.phonetics)
            {
                if (string.IsNullOrEmpty(mwPhonetic.audio))
                {
                    var freeAudio = freeWord.phonetics
                        .FirstOrDefault(p => !string.IsNullOrEmpty(p.audio))?.audio;
                    if (!string.IsNullOrEmpty(freeAudio))
                        mwPhonetic.audio = freeAudio;
                }
            }
        }

        public async Task<(string usAudio, string ukAudio)> GetAudioAsync(string word)
        {
            return await DictionaryApiClient.FetchAudioUrlsAsync(word);
        }
        #endregion

        #region Autocomplete
        public List<string> GetSuggestions(string request, int maxResults = 5)
        {
            string term = request.ToLower();

            var exactMatches = _dictionary
                .Where(word => word.ToLower().StartsWith(term))
                .OrderBy(word => word.Length)
                .Take(maxResults)
                .ToList();

            if (exactMatches.Count < maxResults)
            {
                var fuzzyMatches = _dictionary
                    .Where(word => !word.ToLower().StartsWith(term))
                    .Select(word => new { Word = word, Distance = CalcLevenshteinDistance(term, word.ToLower()) })
                    .Where(x => x.Distance <= Math.Max(2, term.Length / 2))
                    .OrderBy(x => x.Distance)
                    .Take(maxResults - exactMatches.Count)
                    .Select(x => x.Word)
                    .ToList();

                exactMatches.AddRange(fuzzyMatches);
            }

            return exactMatches;
        }

        public int CalcLevenshteinDistance(string s1, string s2)
        {
            int m = s1.Length, n = s2.Length;
            int[,] dp = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++) dp[i, 0] = i;
            for (int j = 0; j <= n; j++) dp[0, j] = j;

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (s1[i - 1] == s2[j - 1])
                        dp[i, j] = dp[i - 1, j - 1];
                    else
                        dp[i, j] = 1 + Math.Min(dp[i - 1, j], Math.Min(dp[i, j - 1], dp[i - 1, j - 1]));
                }
            }
            return dp[m, n];
        }
        #endregion

        #region Helper
        public bool IsWordExists(string word)
        {
            return _dictionary.Any(w => w.Equals(word, StringComparison.OrdinalIgnoreCase));
        }

        public int GetTotalWords()
        {
            return _dictionary.Count;
        }
        #endregion
    }
}
