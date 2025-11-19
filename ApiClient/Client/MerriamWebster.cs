using System.Net.Http;
using BlueBerryDictionary.ApiClient.Configuration;

namespace BlueBerryDictionary.ApiClient.Client
{
    public class MerriamWebster
    {
        private readonly string _dictionaryKey;
        private readonly string _thesaurusKey;
        private readonly string _dictionaryUrl;
        private readonly string _thesaurusUrl;
        private static readonly HttpClient _httpClient = new HttpClient();

        public MerriamWebster()
        {
            _dictionaryKey = Config.Instance.MerriamWebsterDictionaryKey;
            _thesaurusKey = Config.Instance.MerriamWebsterThesaurusKey;
            _dictionaryUrl = Config.Instance.MerriamWebsterDictionaryEndpoint;
            _thesaurusUrl = Config.Instance.MerriamWebsterThesaurusEndpoint;
        }

        /// <summary>
        /// Tra từ điển Merriam-Webster
        /// </summary>
        public async Task<string> LookupWordAsync(string word)
        {
            try
            {
                string url = $"{_dictionaryUrl}{word}?key={_dictionaryKey}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                return json; // Trả về raw JSON để parse sau
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Merriam-Webster Dictionary Error: {ex.Message}");
                return null;
            }
        }

        public async Task<string> LookupThesaurusAsync(string word)
        {
            try
            {
                string url = $"{_thesaurusUrl}{word}?key={_thesaurusKey}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Merriam-Webster Thesaurus Error: {ex.Message}");
                return null;
            }
        }
    }
}