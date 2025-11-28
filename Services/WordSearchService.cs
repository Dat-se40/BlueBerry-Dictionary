using BlueBerryDictionary.ApiClient;
using BlueBerryDictionary.Data;
using BlueBerryDictionary.Models;
using System.IO;
using System.Net.Http;

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
            
            // ========== TRY API 1: FREE DICTIONARY ==========
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                var words = await _apiClient.FetchFromFreeDictionary(word, cts.Token);
                if (words?.Count > 0)
                {
                    Console.WriteLine("✅ Found in Free Dictionary");
                    _cacheManager.AddToCache(word, words);
                    return words;
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ Free Dictionary timeout");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"⚠️ Free Dictionary failed: {ex.Message}");
            }
            // ========== TRY API 2: MERRIAM-WEBSTER ==========
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                Console.WriteLine("🔄 Trying Merriam-Webster...");
                var words = await _apiClient.FetchFromMerriamWebster(word, cts.Token);

                if (words?.Count > 0)
                {
                    Console.WriteLine("✅ Found in Merriam-Webster");
                    _cacheManager.AddToCache(word, words);
                    return words;
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ Merriam-Webster timeout");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Merriam-Webster failed: {ex.Message}");
            }
            // ========== NOT FOUND ==========
            Console.WriteLine($"❌ Word '{word}' not found");
            return new List<Word>();
        }

        public async Task<(string usAudio, string ukAudio)> GetAudioAsync(string word)
        {
            return await DictionaryApiClient.FetchAudioUrlsAsync(word);
        }
        #endregion

        #region Autocomplete
        public List<string>GetSuggestions(string request , int maxResults = 5)
        {
            string term = request.ToLower();
            // ========== BƯỚC 1: TÌM TỪ BẮT ĐẦU BẰNG SEARCH TERM ==========
            var exactMatches = _dictionary
                .Where(word => word.ToLower().StartsWith(term))
                .OrderBy(word => word.Length) // Ưu tiên từ ngắn hơn
                .Take(maxResults)
                .ToList();

            // ========== BƯỚC 2: NẾU KHÔNG ĐỦ, TÌM TỪ TƯƠNG TỰ (LEVENSHTEIN) ==========
            if (exactMatches.Count < maxResults)
            {
                var fuzzyMatches = _dictionary
                    .Where(word => !word.ToLower().StartsWith(term)) // Loại trừ từ đã có
                    .Select(word => new
                    {
                        Word = word,
                        Distance = CalcLevenshteinDistance(term, word.ToLower()),
                        LengthDiff = Math.Abs(word.Length - term.Length)
                    })
                    .Where(x => x.Distance <= Math.Max(2, term.Length / 2)) // Giới hạn khoảng cách
                    .OrderBy(x => x.Distance) // Ưu tiên khoảng cách nhỏ
                    .ThenBy(x => x.LengthDiff) // Ưu tiên độ dài tương tự
                    .Take(maxResults - exactMatches.Count)
                    .Select(x => x.Word)
                    .ToList();

                exactMatches.AddRange(fuzzyMatches);
            }
            return exactMatches;
        }


        /// <summary>
        /// Tính khoảng cách Levenshtein giữa 2 chuỗi
        /// (Dùng để tìm từ tương tự)
        /// </summary>
        public int CalcLevenshteinDistance(string s1, string s2)
        {
            int m = s1.Length, n = s2.Length;
            int[,] levenshtein = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++) levenshtein[i, 0] = i;
            for (int j = 0; j <= n; j++) levenshtein[0, j] = j;

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (s1[i - 1] == s2[j - 1])
                    {
                        levenshtein[i, j] = levenshtein[i - 1, j - 1];
                    }
                    else
                    {
                        levenshtein[i, j] = 1 + Math.Min(
                            levenshtein[i, j - 1],
                            Math.Min(levenshtein[i - 1, j], levenshtein[i - 1, j - 1])
                        );
                    }
                }
            }

            return levenshtein[m, n];
        }
        #endregion

        #region helper function
        /// Kiểm tra từ có tồn tại trong dictionary không
        public bool IsWordExists(string word)
        {
            return _dictionary.Any(w => w.Equals(word, StringComparison.OrdinalIgnoreCase));
        }
        /// Lấy tổng số từ trong dictionary
        public int GetTotalWords()
        {
            return _dictionary.Count;
        }
        #endregion
    }
}
