using BlueBerryDictionary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace BlueBerryDictionary.Services
{
    /// <summary>
    /// Quản lý paths và data theo từng user (Singleton)
    /// </summary>
    public class UserDataManager
    {
        private static UserDataManager _instance;
        public static UserDataManager Instance => _instance ??= new UserDataManager();

        private readonly string _baseDataPath;
        private readonly string _systemPath;
        private string _currentUserFolder;

        // ========== PROPERTIES ==========

        public bool IsGuestMode => UserSessionManage.Instance.IsGuest;
        public string CurrentUserEmail => UserSessionManage.Instance.Email ?? "guest";

        // ========== CONSTRUCTOR ==========

        private UserDataManager()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _baseDataPath = System.IO.Path.GetFullPath(
                 Path.Combine(baseDir, @"..\..\..\Data\PersistentStorage\Users")
            );

            _systemPath = Path.GetFullPath(
                Path.Combine(baseDir, @"..\..\..\Data\System")
            );
            Directory.CreateDirectory(_baseDataPath);
            Directory.CreateDirectory(_systemPath);
            if (Directory.Exists(_baseDataPath)) Console.WriteLine("[UserDataManager] "  + _baseDataPath + " checked " );
            if (Directory.Exists(_systemPath)) Console.WriteLine("[UserDataManager] " + _systemPath + " checked ");
            // Default = guest
            _currentUserFolder = Path.Combine(_baseDataPath, "guest");
            Directory.CreateDirectory(_currentUserFolder);
            
        }

        // ========== SET USER ==========

        /// <summary>
        /// Set current user (sau khi login hoặc guest)
        /// </summary>
        public void SetCurrentUser(string email)
        {
            if (string.IsNullOrEmpty(email))
                email = "guest";

            // Sanitize email (remove invalid chars)
            var sanitized = email.Replace("@", "_at_").Replace(".", "_");

            _currentUserFolder = Path.Combine(_baseDataPath, sanitized);
            Directory.CreateDirectory(_currentUserFolder);

            Console.WriteLine($"✅ UserDataManager: Current user folder = {_currentUserFolder}");
            CreateEssentialFile();
        }
        
        // ========== GET PATHS ==========

        public string GetMyWordsPath() => Path.Combine(_currentUserFolder, "MyWords.json");
        public string GetTagsPath() => Path.Combine(_currentUserFolder, "Tags.json");
        public string GetGameLogPath() => Path.Combine(_currentUserFolder, "GameLog.json");
        public string GetSettingsPath() => Path.Combine(_currentUserFolder, "Settings.json");
        public string GetMetadataPath() => Path.Combine(_currentUserFolder, ".metadata.json");

        public string GetCurrentUserFolder() => _currentUserFolder;

        // ========== METADATA ==========

        /// <summary>
        /// Load metadata (để check sync state)
        /// </summary>
        public Dictionary<string, FileMetadata> LoadMetadata()
        {
            var path = GetMetadataPath();
            if (!File.Exists(path))
                return new Dictionary<string, FileMetadata>();

            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Dictionary<string, FileMetadata>>(json)
                       ?? new Dictionary<string, FileMetadata>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Load metadata error: {ex.Message}");
                return new Dictionary<string, FileMetadata>();
            }
        }

        /// <summary>
        /// Save metadata
        /// </summary>
        public void SaveMetadata(Dictionary<string, FileMetadata> metadata)
        {
            try
            {
                var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
                File.WriteAllText(GetMetadataPath(), json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save metadata error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update metadata cho 1 file
        /// </summary>
        public void UpdateFileMetadata(string filename, string driveFileId = null)
        {
            var metadata = LoadMetadata();

            var filePath = filename switch
            {
                "MyWords.json" => GetMyWordsPath(),
                "Tags.json" => GetTagsPath(),
                "GameLog.json" => GetGameLogPath(),
                _ => null
            };
            
            if (filePath == null || !File.Exists(filePath))
                return;

            var fileInfo = new FileInfo(filePath);

            metadata[filename] = new FileMetadata
            {
                FileName = filename,
                LastModified = fileInfo.LastWriteTimeUtc,
                FileSize = fileInfo.Length,
                Checksum = ComputeMD5(filePath),
                DriveFileId = driveFileId ?? metadata.GetValueOrDefault(filename)?.DriveFileId,
                LastSynced = DateTime.UtcNow
            };

            SaveMetadata(metadata);
        }

        // ========== HELPERS ==========

        private string ComputeMD5(string filePath)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        private void CreateEssentialFile() 
        {
            foreach (var file in CloudSyncService.essentialFile)
            {
                var filePath = file switch
                {
                    "MyWords.json" => GetMyWordsPath(),
                    "Tags.json" => GetTagsPath(),
                    "GameLog.json" => GetGameLogPath(),
                    _ => null
                };

                
                if (!File.Exists(filePath)) File.Create(filePath);
                else Console.WriteLine($"[UserDataManager:CreateEssentialFile] {filePath} has created!");
            }
        }

        /// <summary>
        /// Save thông tin để upload
        /// </summary>
        public void SaveEssentialFiles()
        {
            TagService.Instance.SaveTags(GetTagsPath());
            TagService.Instance.SaveWords(GetMyWordsPath()); 
        }
        // ========== LIST ALL USERS ==========

        /// <summary>
        /// Lấy danh sách email đã login (để show history)
        /// </summary>
        public List<string> GetAllUsers()
        {
            if (!Directory.Exists(_baseDataPath))
                return new List<string>();

            return Directory.GetDirectories(_baseDataPath)
                .Select(Path.GetFileName)
                .Where(name => name != "guest")
                .Select(name => name.Replace("_at_", "@").Replace("_", "."))
                .ToList();
        }
    }
}
