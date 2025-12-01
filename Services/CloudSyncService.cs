using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlueBerryDictionary.ApiClient.Configuration;
using BlueBerryDictionary.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

using DriveFile = Google.Apis.Drive.v3.Data.File; // ✅ Alias

namespace BlueBerryDictionary.Services
{
    /// <summary>
    /// Service đồng bộ dữ liệu với Google Drive
    /// Singleton pattern
    /// </summary>
    public class CloudSyncService
    {
        private static CloudSyncService _instance;
        public static CloudSyncService Instance => _instance ??= new CloudSyncService();

        private DriveService _driveService;
        private const string APP_FOLDER_NAME = "BlueBerryDictionary";
        private string _appFolderId;

        private CloudSyncService()
        {
            // Paths sẽ lấy từ UserDataManager (dynamic theo user)
        }

        // ==================== INITIALIZE ====================

        public async Task InitializeAsync(UserCredential credential)
        {
            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = Config.Instance.AppName
            });

            _appFolderId = await GetOrCreateAppFolderAsync();
            Console.WriteLine($"✅ Drive initialized. Folder ID: {_appFolderId}");
        }

        // ==================== DOWNLOAD ALL ====================

        public async Task<SyncResult> DownloadAllDataAsync()
        {
            var result = new SyncResult();

            try
            {
                Console.WriteLine("📥 Downloading data from Drive...");

                var filesToDownload = new[] { "MyWords.json", "Tags.json", "GameLog.json" };

                foreach (var filename in filesToDownload)
                {
                    try
                    {
                        var fileId = await FindFileIdAsync(filename);
                        if (fileId != null)
                        {
                            var localPath = GetLocalFilePath(filename);
                            await DownloadFileAsync(fileId, localPath);

                            // Update metadata
                            UserDataManager.Instance.UpdateFileMetadata(filename, fileId);

                            result.Downloaded.Add(filename);
                            Console.WriteLine($"✅ Downloaded: {filename}");
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ File not found on Drive: {filename}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Download error for {filename}: {ex.Message}");
                        result.Errors.Add($"{filename}: {ex.Message}");
                    }
                }

                Console.WriteLine($"✅ Download completed: {result.Downloaded.Count} files");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Download all error: {ex.Message}");
                result.Errors.Add($"Download failed: {ex.Message}");
            }

            return result;
        }

        // ==================== UPLOAD ALL PENDING ====================

        public async Task UploadAllPendingAsync()
        {
            try
            {
                Console.WriteLine("📤 Uploading pending data...");

                var filesToUpload = new[] { "MyWords.json", "Tags.json", "GameLog.json" };

                foreach (var filename in filesToUpload)
                {
                    var localPath = GetLocalFilePath(filename);
                    if (File.Exists(localPath)) // ✅ System.IO.File
                    {
                        await UploadFileAsync(filename, localPath);

                        // Update metadata
                        UserDataManager.Instance.UpdateFileMetadata(filename);

                        Console.WriteLine($"✅ Uploaded: {filename}");
                    }
                }

                Console.WriteLine("✅ Upload completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Upload all error: {ex.Message}");
            }
        }

        // ==================== UPLOAD SINGLE FILE ====================

        public async Task UploadFileAsync(string filename, string localPath)
        {
            try
            {
                if (!File.Exists(localPath)) // ✅ System.IO.File
                {
                    Console.WriteLine($"⚠️ File not found: {localPath}");
                    return;
                }

                var fileMetadata = new DriveFile // ✅ Alias
                {
                    Name = filename,
                    Parents = new List<string> { _appFolderId }
                };

                using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);

                var existingFileId = await FindFileIdAsync(filename);

                if (existingFileId != null)
                {
                    var updateRequest = _driveService.Files.Update(
                        fileMetadata,
                        existingFileId,
                        stream,
                        "application/json"
                    );
                    updateRequest.Fields = "id, name, modifiedTime";
                    await updateRequest.UploadAsync();

                    Console.WriteLine($"📤 Updated: {filename}");
                }
                else
                {
                    var createRequest = _driveService.Files.Create(
                        fileMetadata,
                        stream,
                        "application/json"
                    );
                    createRequest.Fields = "id, name, modifiedTime";
                    await createRequest.UploadAsync();

                    Console.WriteLine($"📤 Uploaded: {filename}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Upload error for {filename}: {ex.Message}");
                throw;
            }
        }

        // ==================== PRIVATE HELPERS ====================

        private async Task DownloadFileAsync(string fileId, string localPath)
        {
            try
            {
                var request = _driveService.Files.Get(fileId);
                using var stream = new MemoryStream();

                await request.DownloadAsync(stream);

                var directory = Path.GetDirectoryName(localPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllBytesAsync(localPath, stream.ToArray()); // ✅ System.IO.File
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Download file error: {ex.Message}");
                throw;
            }
        }

        private async Task<string> FindFileIdAsync(string filename)
        {
            try
            {
                var request = _driveService.Files.List();
                request.Q = $"name='{filename}' and '{_appFolderId}' in parents and trashed=false";
                request.Fields = "files(id, name, modifiedTime)";
                request.PageSize = 1;

                var result = await request.ExecuteAsync();
                return result.Files?.FirstOrDefault()?.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Find file error: {ex.Message}");
                return null;
            }
        }

        private async Task<string> GetOrCreateAppFolderAsync()
        {
            try
            {
                var request = _driveService.Files.List();
                request.Q = $"name='{APP_FOLDER_NAME}' and mimeType='application/vnd.google-apps.folder' and trashed=false";
                request.Fields = "files(id, name)";
                request.PageSize = 1;

                var result = await request.ExecuteAsync();
                if (result.Files?.Count > 0)
                {
                    return result.Files[0].Id;
                }

                var folderMetadata = new DriveFile // ✅ Alias
                {
                    Name = APP_FOLDER_NAME,
                    MimeType = "application/vnd.google-apps.folder"
                };

                var createRequest = _driveService.Files.Create(folderMetadata);
                createRequest.Fields = "id";
                var folder = await createRequest.ExecuteAsync();

                return folder.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Folder creation error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get local file path (dynamic theo user)
        /// </summary>
        private string GetLocalFilePath(string filename)
        {
            return filename switch
            {
                "MyWords.json" => UserDataManager.Instance.GetMyWordsPath(),
                "Tags.json" => UserDataManager.Instance.GetTagsPath(),
                "GameLog.json" => UserDataManager.Instance.GetGameLogPath(),
                _ => throw new ArgumentException($"Unknown file: {filename}")
            };
        }
    }
}
