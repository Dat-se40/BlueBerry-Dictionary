using BlueBerryDictionary.ApiClient.Configuration;
using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services.User;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DriveFile = Google.Apis.Drive.v3.Data.File; // Alias

namespace BlueBerryDictionary.Services.Network
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
        public string _appFolderId { get; private set; }
        public static  readonly string[] essentialFile = new[] { "MyWords.json", "Tags.json", "GameLog.json" };
        private CloudSyncService()
        {
            // Paths sẽ lấy từ UserDataManager (dynamic theo user)
        }

        #region Initialize

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
        #endregion

        #region Download từ Drive

        public async Task<SyncResult> DownloadAllDataAsync()
        {
            var result = new SyncResult();

            try
            {
                Console.WriteLine("📥 Downloading data from Drive...");

                foreach (var filename in essentialFile)
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
        public async Task MergeMyWordsAsync(string driveJsonData)
        {
            
            try
            {
                // Parse data từ Drive
                var driveWords = JsonConvert.DeserializeObject<List<WordShortened>>(driveJsonData);
                if (driveWords == null) return;

                var tagService = TagService.Instance;
                var localWords = tagService.GetAllWords(); // Lấy data local hiện tại

                int added = 0, updated = 0, skipped = 0;

                foreach (var driveWord in driveWords)
                {
                    var localWord = localWords.FirstOrDefault(w =>
                        w.Word.Equals(driveWord.Word, StringComparison.OrdinalIgnoreCase));

                    if (localWord == null)
                    {
                        // Từ chỉ có trên Drive → Thêm vào local
                        tagService.AddNewWordShortened(driveWord);
                        added++;
                    }
                    else
                    {
                        // Từ có cả 2 chỗ → So sánh timestamp
                        if (driveWord.AddedAt > localWord.AddedAt)
                        {
                            // Drive mới hơn → Update local
                            tagService.DeleteWordShortened(localWord.Word);
                            tagService.AddNewWordShortened(driveWord);
                            updated++;
                        }
                        else
                        {
                            // Local mới hơn hoặc bằng → Giữ local
                            skipped++;
                        }
                    }
                }

                // Lưu sau khi merge
                tagService.SaveWords();

                Console.WriteLine($"✅ MyWords merged: +{added} ~{updated} ={skipped}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Merge MyWords error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Merge MyWords từ Drive với local
        /// Logic: Drive mới hơn → Update, Local mới hơn → Giữ
        /// </summary>
        public async Task MergeTagsAsync(string driveJsonData)
        {
            try
            {
                var driveTags = JsonConvert.DeserializeObject<List<Tag>>(driveJsonData);
                if (driveTags == null) return;

                var tagService = TagService.Instance;
                var localTags = tagService.GetAllTags();

                int added = 0, updated = 0;

                foreach (var driveTag in driveTags)
                {
                    var localTag = localTags.FirstOrDefault(t => t.Id == driveTag.Id);

                    if (localTag == null)
                    {
                        // Tag chỉ có trên Drive → Thêm
                        tagService.CreateTag(driveTag.Name, driveTag.Icon, driveTag.Color);
                        added++;
                    }
                    else
                    {
                        // Tag có cả 2 → Update nếu Drive mới hơn
                        if (driveTag.CreatedAt > localTag.CreatedAt)
                        {
                            tagService.UpdateTag(
                                localTag.Id,
                                driveTag.Name,
                                driveTag.Icon,
                                driveTag.Color
                            );
                            updated++;
                        }
                    }
                }

                tagService.SaveTags();

                Console.WriteLine($"✅ Tags merged: +{added} ~{updated}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Merge Tags error: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Download file từ Drive vào memory (KHÔNG ghi file local)
        /// </summary>
        private async Task<string> DownloadFileToMemoryAsync(string fileId)
        {
            var request = _driveService.Files.Get(fileId);
            using var stream = new MemoryStream();

            await request.DownloadAsync(stream);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(); // Trả về JSON string
        }


        #endregion

        #region Upload lên Drive
        /// <summary>
        /// Upload tất cả file pending lên Drive
        /// </summary>
        public async Task UploadAllPendingAsync()
        {
            try
            {
                Console.WriteLine("📤 Uploading pending data...");
                int checksum = 0; 
                foreach (var filename in essentialFile)
                {
                    var localPath = GetLocalFilePath(filename);
                    Console.WriteLine($"{filename} -> local path : {localPath} ");
                    if (File.Exists(localPath)) // ✅ System.IO.File
                    {
                        await UploadFileAsync(filename, localPath);

                        // Update metadata
                        UserDataManager.Instance.UpdateFileMetadata(filename);
                        checksum++; 
                        Console.WriteLine($"✅ Uploaded: {filename}");
                    }else 
                    {
                        Console.WriteLine($"Upload {filename} faild");
                    }
                }
                if (checksum == 3)
                    Console.WriteLine($"✅ Upload completed");
                else Console.WriteLine("Upload failed!"); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Upload all error: {ex.Message}");
            }
        }


        #endregion

        #region Upload lên Drive

        /// <summary>
        /// Upload/Update 1 file lên Drive
        /// LOGIC: Tìm file → Update nếu có, Create nếu chưa có
        /// </summary>
        public async Task UploadFileAsync(string filename, string localPath)
        {
            try
            {
                if (!File.Exists(localPath))
                {
                    Console.WriteLine($"⚠️ File not found: {localPath}");
                    return;
                }

                // Check xem file tồn tại chưa
                var existingFileId = await FindFileIdAsync(filename);

                if (existingFileId != null)
                {
                    // Update file cũ
                    Console.WriteLine($"🔄 Updating existing file: {filename} (ID: {existingFileId})");

                    using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);

                    // CÁCH 1: Update chỉ content (KHÔNG đổi metadata)
                    var updateRequest = _driveService.Files.Update(
                        new DriveFile(), //  Empty metadata (chỉ update content)
                        existingFileId,
                        stream,
                        "application/json"
                    );
                    updateRequest.Fields = "id, name, modifiedTime";

                    var updatedFile = await updateRequest.UploadAsync();

                    if (updatedFile.Status == Google.Apis.Upload.UploadStatus.Completed)
                    {
                        Console.WriteLine($"✅ Updated: {filename}");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Update incomplete: {updatedFile.Status}");
                    }
                }
                else
                {
                    // Tạo file mới
                    Console.WriteLine($"📤 Creating new file: {filename}");

                    var fileMetadata = new DriveFile
                    {
                        Name = filename,
                        Parents = new List<string> { _appFolderId }
                    };

                    using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);

                    var createRequest = _driveService.Files.Create(
                        fileMetadata,
                        stream,
                        "application/json"
                    );
                    createRequest.Fields = "id, name, modifiedTime";

                    var createdFile = await createRequest.UploadAsync();

                    if (createdFile.Status == Google.Apis.Upload.UploadStatus.Completed)
                    {
                        Console.WriteLine($"✅ Created: {filename}");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Create incomplete: {createdFile.Status}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Upload error for {filename}: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        #endregion

        #region Helper methods

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

                await File.WriteAllBytesAsync(localPath, stream.ToArray()); // System.IO.File
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Download file error: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Tìm file ID trên Drive theo tên
        /// </summary>
        private async Task<string> FindFileIdAsync(string filename)
        {
            try
            {
                var request = _driveService.Files.List();
                request.Q = $"name='{filename}' and '{_appFolderId}' in parents and trashed=false";
                request.Fields = "files(id, name, modifiedTime)";
                request.PageSize = 10; // Tăng lên 10 để debug

                var result = await request.ExecuteAsync();

                // LOG ĐỂ DEBUG
                Console.WriteLine($"🔍 Searching for: {filename}");
                Console.WriteLine($"📁 Files found: {result.Files?.Count ?? 0}");

                if (result.Files?.Count > 0)
                {
                    foreach (var file in result.Files)
                    {
                        Console.WriteLine($"  - ID: {file.Id}, Name: {file.Name}, Modified: {file.ModifiedTime}");
                    }
                    return result.Files[0].Id; // Lấy file đầu tiên
                }

                Console.WriteLine($"⚠️ File not found: {filename}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Find file error: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Lấy hoặc tạo folder app trên Drive
        /// </summary>
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

                var folderMetadata = new DriveFile // Alias
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
        /// Lấy đường dẫn file local theo tên (dynamic theo user)
        /// </summary>
        public string GetLocalFilePath(string filename)
        {
            return filename switch
            {
                "MyWords.json" => UserDataManager.Instance.GetMyWordsPath(),
                "Tags.json" => UserDataManager.Instance.GetTagsPath(),
                "GameLog.json" => UserDataManager.Instance.GetGameLogPath(),
                _ => throw new ArgumentException($"Unknown file: {filename}")
            };
        }

        #endregion
    }
}
