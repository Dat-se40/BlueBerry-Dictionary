using BlueBerryDictionary.ApiClient.Configuration;
using BlueBerryDictionary.Models;
using BlueBerryDictionary.ApiClient.Client;
using Newtonsoft.Json;
using System.Net.Http;
using MyDictionary.Model.MerriamWebster;

namespace BlueBerryDictionary.ApiClient
{
    public class ApiClient
    {
        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        private static readonly MerriamWebster _merriamService = new MerriamWebster();

        /// <summary>
        /// Fetch word với FALLBACK CHAIN + TIMEOUT
        /// 1. Free Dictionary (3s timeout)
        /// 2. Merriam-Webster (3s timeout)
        /// 3. Local Dictionary
        /// </summary>
        public static async Task<List<Word>> FetchWordAsync(string word, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"🔍 Searching: {word}");

            // ========== TRY 1: FREE DICTIONARY ==========
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                string url = Config.Instance.FreeDictionaryEndpoint + word;
                string json = await _client.GetStringAsync(url, cts.Token);

                var result = JsonConvert.DeserializeObject<List<Word>>(json);

                if (result != null && result.Count > 0)
                {
                    Console.WriteLine("✅ Found in Free Dictionary");
                    return result;
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

            // ========== TRY 2: MERRIAM-WEBSTER ==========
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                Console.WriteLine("🔄 Trying Merriam-Webster...");

                // Fetch dictionary
                string dictJson = await _merriamService.LookupWordAsync(word);

                if (!string.IsNullOrEmpty(dictJson))
                {
                    var words = MerriamWebsterParser.ParseDictionary(dictJson);

                    if (words != null && words.Count > 0)
                    {
                        // Enrich với thesaurus (synonyms/antonyms)
                        await EnrichWithThesaurus(words[0], word);

                        Console.WriteLine("✅ Found in Merriam-Webster");
                        return words;
                    }
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
            Console.WriteLine($"❌ Word '{word}' not found in any API");
            return new List<Word>();
        }

        /// <summary>
        /// Lấy audio US/UK từ nhiều nguồn (Cambridge → Free Dictionary → Merriam-Webster)
        /// </summary>
        public static async Task<(string usAudio, string ukAudio)> FetchAudioUrlsAsync(string word)
        {
            string usAudio = null;
            string ukAudio = null;

            // ========== TRY 1: CAMBRIDGE (BEST QUALITY) ==========
            try
            {
                string cambridgeUS = BuildCambridgeAudioUrl(word, "us");
                string cambridgeUK = BuildCambridgeAudioUrl(word, "uk");

                var responseUS = await _client.GetAsync(cambridgeUS);
                if (responseUS.IsSuccessStatusCode)
                    usAudio = cambridgeUS;

                var responseUK = await _client.GetAsync(cambridgeUK);
                if (responseUK.IsSuccessStatusCode)
                    ukAudio = cambridgeUK;

                if (usAudio != null && ukAudio != null)
                {
                    Console.WriteLine("✅ Audio from Cambridge");
                    return (usAudio, ukAudio);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Cambridge audio failed: {ex.Message}");
            }

            // ========== TRY 2: FREE DICTIONARY ==========
            try
            {
                string url = Config.Instance.FreeDictionaryEndpoint + word;
                string json = await _client.GetStringAsync(url);
                var words = JsonConvert.DeserializeObject<List<Word>>(json);

                if (words?[0]?.phonetics != null)
                {
                    foreach (var phonetic in words[0].phonetics)
                    {
                        if (!string.IsNullOrEmpty(phonetic.audio))
                        {
                            if (phonetic.audio.Contains("-us") || phonetic.audio.Contains("_us"))
                                usAudio ??= phonetic.audio;
                            else if (phonetic.audio.Contains("-uk") || phonetic.audio.Contains("_uk"))
                                ukAudio ??= phonetic.audio;
                        }
                    }

                    if (usAudio != null || ukAudio != null)
                    {
                        Console.WriteLine("✅ Audio from Free Dictionary");
                        return (usAudio, ukAudio);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Free Dictionary audio failed: {ex.Message}");
            }

            // ========== TRY 3: MERRIAM-WEBSTER ==========
            try
            {
                string dictJson = await _merriamService.LookupWordAsync(word);
                if (!string.IsNullOrEmpty(dictJson))
                {
                    var words = MerriamWebsterParser.ParseDictionary(dictJson);
                    if (words?[0]?.phonetics != null && words[0].phonetics.Count > 0)
                    {
                        usAudio = words[0].phonetics[0].audio;
                        Console.WriteLine("✅ Audio from Merriam-Webster");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Merriam-Webster audio failed: {ex.Message}");
            }

            return (usAudio, ukAudio);
        }

        #region helper function 
        private static string BuildCambridgeAudioUrl(string word, string accent)
        {
            string baseUrl = accent == "us"
                ? Config.Instance.CambridgeAudioUSEndpoint
                : Config.Instance.CambridgeAudioUKEndpoint;

            // Get first letter for subdirectory
            string firstLetter = word.Length > 0 ? word.Substring(0, 1).ToLower() : "a";

            return $"{baseUrl}{firstLetter}/{word}.mp3";
        }

        /// <summary>
        /// Enrich word with thesaurus data
        /// </summary>
        private static async Task EnrichWithThesaurus(Word word, string searchTerm)
        {
            try
            {
                string thesaurusJson = await _merriamService.LookupThesaurusAsync(searchTerm);
                if (!string.IsNullOrEmpty(thesaurusJson))
                {
                    var (synonyms, antonyms) = MerriamWebsterParser.ParseThesaurus(thesaurusJson);

                    // Merge vào meaning đầu tiên
                    if (word.meanings != null && word.meanings.Count > 0)
                    {
                        word.meanings[0].synonyms = synonyms;
                        word.meanings[0].antonyms = antonyms;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Thesaurus enrichment failed: {ex.Message}");
            }
        }
        #endregion
    }
}