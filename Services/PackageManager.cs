using BlueBerryDictionary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlueBerryDictionary.Services
{
    internal class PackageManager
    {
        private static PackageManager _instance;
        public static PackageManager Instance => _instance ??= new PackageManager();

        private List<TopicPackage> _availablePackages = new();
        private bool _isInitialized = false;
        public static readonly string DownloadedPackagesFolder = Path.Combine(
                                        AppDomain.CurrentDomain.BaseDirectory,
                                            @"..\..\..\Data\PackageStorage");
        public static readonly string AvailablePackagePath = Path.Combine(
                                      DownloadedPackagesFolder, @"..\PersistentStorage\AvailblePackages.json");
        private readonly string _availablePackageUrl; // sẽ cập nhật link gg drive sau
        private PackageManager() { }

        // ==================== INITIALIZE ====================

        /// <summary>
        /// ✅ Lazy Load: GỌI KHI USER VÀO OFFLINE MODE PAGE
        /// LOGIC:
        /// 1. Fetch từ server (nếu có mạng)
        /// 2. Nếu fail → Load từ file local (fallback)
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            Console.WriteLine("📦 Loading offline packages...");

            // ✅ BƯỚC 1: Thử fetch từ server
            bool fetchSuccess = await FetchFromServerAsync();

            // ✅ BƯỚC 2: Load từ file (local hoặc vừa download)
            bool loadSuccess = LoadPackageListFromFile();


            // ✅ BƯỚC 4: Check packages nào đã download
            CheckDownloadedPackages();

            _isInitialized = true;
            Console.WriteLine($"✅ Loaded {_availablePackages.Count} packages");
        }

        // ==================== FETCH FROM SERVER ====================

        /// <summary>
        /// Fetch catalog mới nhất từ server
        /// RETURN: true nếu thành công, false nếu fail (không có mạng, server lỗi...)
        /// </summary>
        public async Task<bool> FetchFromServerAsync()
        {
            if (string.IsNullOrEmpty(_availablePackageUrl))
            {
                Console.WriteLine("⚠️ Server URL not configured");
                return false;
            }

            try
            {
                Console.WriteLine("🌐 Fetching package catalog from server...");

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                string json = await client.GetStringAsync(_availablePackageUrl);

                // ✅ Validate JSON trước khi save
                var packages = JsonConvert.DeserializeObject<List<TopicPackage>>(json);
                if (packages == null || packages.Count == 0)
                {
                    Console.WriteLine("⚠️ Server returned empty catalog");
                    return false;
                }

                // ✅ Tạo folder nếu chưa có
                var directory = Path.GetDirectoryName(AvailablePackagePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // ✅ Lưu file
                await File.WriteAllTextAsync(AvailablePackagePath, json);

                Console.WriteLine($"✅ Fetched {packages.Count} packages from server");
                return true;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"⚠️ Network error: {ex.Message}");
                return false; // Không có mạng → Dùng file local
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⚠️ Request timeout");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fetch error: {ex.Message}");
                return false;
            }
        }

        // ==================== LOAD FROM FILE ====================

        /// <summary>
        /// Load package catalog từ file local
        /// RETURN: true nếu thành công, false nếu file không tồn tại hoặc lỗi
        /// </summary>
        private bool LoadPackageListFromFile()
        {
            try
            {
                if (!File.Exists(AvailablePackagePath))
                {
                    Console.WriteLine("⚠️ Package catalog file not found");
                    return false;
                }

                // ✅ ĐỌC FILE CONTENT (KHÔNG PHẢI PATH!)
                string json = File.ReadAllText(AvailablePackagePath);

                // ✅ Deserialize
                _availablePackages = JsonConvert.DeserializeObject<List<TopicPackage>>(json);

                if (_availablePackages == null || _availablePackages.Count == 0)
                {
                    Console.WriteLine("⚠️ Catalog file is empty or invalid");
                    return false;
                }

                Console.WriteLine($"✅ Loaded {_availablePackages.Count} packages from local file");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Load file error: {ex.Message}");
                _availablePackages = new List<TopicPackage>();
                return false;
            }
        }

        // ==================== CHECK DOWNLOADED ====================

        /// <summary>
        /// Kiểm tra packages nào đã download
        /// </summary>
        private void CheckDownloadedPackages()
        {
            if (!Directory.Exists(DownloadedPackagesFolder))
            {
                Directory.CreateDirectory(DownloadedPackagesFolder);
                return;
            }

            foreach (var package in _availablePackages)
            {
                var localPath = Path.Combine(DownloadedPackagesFolder, $"{package.Id}.json");
                if (File.Exists(localPath))
                {
                    package.IsDownloaded = true;
                    package.LocalPath = localPath;
                    Console.WriteLine($"✅ Package '{package.Name}' is already downloaded");
                }
            }
        }

        // ==================== PUBLIC API ====================

        public List<TopicPackage> GetAllPackages() => _availablePackages;

        public List<TopicPackage> GetPackagesByCategory(string category)
        {
            return _availablePackages.Where(p =>
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        public List<TopicPackage> GetDownloadedPackages()
        {
            return _availablePackages.Where(p => p.IsDownloaded).ToList();
        }

        public TopicPackage GetPackageById(string id)
        {
            return _availablePackages.FirstOrDefault(p => p.Id == id);
        }

        public async Task DownloadPackageAsync(string packageId, IProgress<double> progress = null)
        {
            var package = GetPackageById(packageId);
            if (package == null)
            {
                throw new Exception($"Package '{packageId}' not found");
            }

            // TODO: Implement download logic với progress reporting
            await package.DownloadAsync();

            CheckDownloadedPackages(); // Refresh downloaded status
        }

        public async Task ImportPackageAsync(string packageId)
        {
            var package = GetPackageById(packageId);
            if (package == null || !package.IsDownloaded)
            {
                throw new Exception("Package not downloaded");
            }

            await package.ImportToLocalAsync();
        }

        public async Task DeletePackageAsync(string packageId)
        {
            var package = GetPackageById(packageId);
            if (package?.LocalPath != null && File.Exists(package.LocalPath))
            {
                File.Delete(package.LocalPath);
                package.IsDownloaded = false;
                package.LocalPath = null;
                Console.WriteLine($"🗑️ Deleted package: {package.Name}");
            }
        }

    }
}
