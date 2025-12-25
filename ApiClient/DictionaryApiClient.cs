using BlueBerryDictionary.ApiClient.Configuration;
using BlueBerryDictionary.Models;
using BlueBerryDictionary.ApiClient.Client;
using Newtonsoft.Json;
using System.Net.Http;
using MyDictionary.Model.MerriamWebster;

namespace BlueBerryDictionary.ApiClient
{
    public class DictionaryApiClient
    {
        /// <summary>
        /// Client chính để gọi các Dictionary API
        /// Hỗ trợ fallback chain: Free Dictionary → Merriam-Webster → Local
        /// </summary>

        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        private static readonly MerriamWebster _merriamService = new MerriamWebster();

        /// <summary>
        /// Fetch word với FALLBACK CHAIN + TIMEOUT
        /// </summary>
        public async Task<List<Word>> FetchFromFreeDictionary(string word, CancellationToken ct)
        {
            string url = Config.Instance.FreeDictionaryEndpoint + word;
            string json = await _client.GetStringAsync(url, ct);
            return JsonConvert.DeserializeObject<List<Word>>(json);
        }

        public async Task<List<Word>> FetchFromMerriamWebster(string word, CancellationToken ct)
        {
            string dictJson = await _merriamService.LookupWordAsync(word);
            if (string.IsNullOrEmpty(dictJson))
                return null;

            var words = MerriamWebsterParser.ParseDictionary(dictJson);

            // Fetch thesaurus song song (không chờ tuần tự)
            if (words?.Count > 0)
            {
                try
                {
                    // Không chờ thesaurus, fetch nó song song
                    var thesaurusTask = _merriamService.LookupThesaurusAsync(word);

                    string thesaurusJson = await Task.Run(() => thesaurusTask, ct);
                    if (!string.IsNullOrEmpty(thesaurusJson))
                    {
                        var (synonyms, antonyms) = MerriamWebsterParser.ParseThesaurus(thesaurusJson);
                        words[0].meanings[0].synonyms = synonyms;
                        words[0].meanings[0].antonyms = antonyms;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Thesaurus enrichment timeout/failed: {ex.Message}");
                }
            }

            return words;
        }

        /// <summary>
        /// Lấy audio US/UK từ nhiều nguồn (Cambridge → Free Dictionary → Merriam-Webster)
        /// </summary>
        public static async Task<(string usAudio, string ukAudio)> FetchAudioUrlsAsync(string word)
        {
            string usAudio = null;
            string ukAudio = null;

            // Nguồn 1: Cambridge (chất lượng tốt nhất)
            try
            {
                string cambridgeUS = BuildCambridgeAudioUrl(word, "us");
                string cambridgeUK = BuildCambridgeAudioUrl(word, "uk");

                var responseUS = await _client.GetAsync(cambridgeUS);
                // Kiểm tra US audio
                if (responseUS.IsSuccessStatusCode)
                    usAudio = cambridgeUS;

                var responseUK = await _client.GetAsync(cambridgeUK);
                // Kiểm tra UK audio
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

            // Nguồn 2: Free Dictionary
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

            // Nguồn 3: Merriam-Webster
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

        #region Helper methods
        /// <summary>
        /// Build URL audio từ Cambridge Dictionary
        /// </summary>
        private static string BuildCambridgeAudioUrl(string word, string accent)
        {
            string baseUrl = accent == "us"
                ? Config.Instance.CambridgeAudioUSEndpoint
                : Config.Instance.CambridgeAudioUKEndpoint;

            // Lấy chữ cái đầu để xác định thư mục
            string firstLetter = word.Length > 0 ? word.Substring(0, 1).ToLower() : "a";

            return $"{baseUrl}{firstLetter}/{word}.mp3";
        }

        /// <summary>
        /// Bổ sung synonyms/antonyms từ Thesaurus API
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