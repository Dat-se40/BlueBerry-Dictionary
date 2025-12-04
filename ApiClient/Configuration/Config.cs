using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace BlueBerryDictionary.ApiClient.Configuration
{
    internal class Config
    {
        private static Config _instance;
        private readonly IConfigurationRoot _configuration;

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
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory() + @"..\..\..\..\ApiClient\Configuration")
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddEnvironmentVariables(); // ✅ Now works

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

        // ========== GOOGLE OAUTH ==========
        public string GoogleClientId =>
            Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
            ?? _configuration["GoogleOAuth:ClientId"];

        public string GoogleClientSecret =>
            Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
            ?? _configuration["GoogleOAuth:ClientSecret"];

        public string GoogleRedirectUri =>
            _configuration["GoogleOAuth:RedirectUri"]
            ?? "http://localhost:8080/";

        public string[] GoogleScopes =>
            _configuration.GetSection("GoogleOAuth:Scopes").Get<string[]>() // ✅ Now works
            ?? new[]
            {
                "https://www.googleapis.com/auth/drive.file",
                "https://www.googleapis.com/auth/userinfo.profile",
                "https://www.googleapis.com/auth/userinfo.email"
            };

        // ========== GOOGLE DRIVE ==========
        public string GoogleDriveAppFolderName =>
            _configuration["GoogleDrive:AppFolderName"] ?? "BlueBerryDictionary";

        public int GoogleDriveSyncIntervalSeconds =>
            int.Parse(_configuration["GoogleDrive:SyncIntervalSeconds"] ?? "300");

        public bool GoogleDriveEnableAutoSync =>
            bool.Parse(_configuration["GoogleDrive:EnableAutoSync"] ?? "true");

        // ========== APP SETTINGS ==========
        public string AppVersion => _configuration["App:Version"] ?? "1.0.0";
        public bool EnableOfflineMode => bool.Parse(_configuration["App:EnableOfflineMode"] ?? "true");

        public string AppName => "BlueBerry Dictionary";

    }
}
