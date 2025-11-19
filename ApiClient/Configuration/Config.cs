using Microsoft.Extensions.Configuration;
using System.IO;

namespace BlueBerryDictionary.ApiClient.Configuration
{
    internal class Config
    {
        private static Config _instance;
        private readonly IConfigurationRoot _configuration; // Fix type to IConfigurationRoot

        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Config();
                }
                return _instance;
            }
        }

        private Config()
        {
            // Làm ơn đừng thay đổi đường dẫn ở đây mà
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory() + @"..\..\..\..\ApiClient\Configuration")
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        // ========== API KEYS ==========
        public string MerriamWebsterDictionaryKey => _configuration["ApiKeys:MerriamWebsterDictionary"];
        public string MerriamWebsterThesaurusKey => _configuration["ApiKeys:MerriamWebsterThesaurus"];
        public string PixabayKey => _configuration["ApiKeys:Pixabay"];

        // ========== API ENDPOINTS ==========
        public string FreeDictionaryEndpoint => _configuration["ApiEndpoints:FreeDictionary"];
        public string MerriamWebsterDictionaryEndpoint => _configuration["ApiEndpoints:MerriamWebsterDictionary"];
        public string MerriamWebsterThesaurusEndpoint => _configuration["ApiEndpoints:MerriamWebsterThesaurus"];
        public string PixabayEndpoint => _configuration["ApiEndpoints:Pixabay"];
        public string CambridgeAudioUSEndpoint => _configuration["ApiEndpoints:CambridgeAudioUS"];
        public string CambridgeAudioUKEndpoint => _configuration["ApiEndpoints:CambridgeAudioUK"];
    }
}
