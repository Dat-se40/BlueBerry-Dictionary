using BlueBerryDictionary.ApiClient.Configuration;
using BlueBerryDictionary.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;

namespace MyDictionary.Services
{
    public class ImageSearchService
    {
        private static readonly Lazy<ImageSearchService> _instance =
            new(() => new ImageSearchService());
        public static ImageSearchService Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, BitmapImage> _memoryCache = new();
        private readonly string _imagesFolder;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient = new HttpClient();

        // Cấu hình retry
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        private ImageSearchService()
        {
            _apiKey = Config.Instance.SerpApiKey;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _imagesFolder = PathHelper.Combine(baseDir, @"..\..\..\Data\PersistentStorage\Images");
            Directory.CreateDirectory(_imagesFolder);
        }

        private string NormalizeKey(string word) => word.Trim().ToLowerInvariant();

        private string GetImagePath(string word) =>
            PathHelper.Combine(_imagesFolder, $"{NormalizeKey(word)}.jpg");

        public bool HasLocalImage(string word) => File.Exists(GetImagePath(word));

        public BitmapImage? GetFromMemory(string word)
        {
            if (_memoryCache.TryGetValue(NormalizeKey(word), out var bmp))
                return bmp;
            return null;
        }

        public BitmapImage? LoadFromDisk(string word)
        {
            try
            {
                var path = GetImagePath(word);
                if (!File.Exists(path)) return null;

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch (Exception ex)
            {
                // Log error và xóa file corrupt
                Console.WriteLine($"Error loading image from disk: {ex.Message}");
                try { File.Delete(GetImagePath(word)); } catch { }
                return null;
            }
        }

        /// <summary>
        /// Tạo query tối ưu cho từng loại từ
        /// </summary>
        private string BuildQuery(string word, string? partOfSpeech = null)
        {
            word = word.Trim();

            if (!string.IsNullOrEmpty(partOfSpeech))
            {
                return partOfSpeech.ToLower() switch
                {
                    "noun" => $"{word} object thing illustration",
                    "verb" => $"{word} action activity demonstration",
                    "adjective" => $"{word} characteristic quality example",
                    "adverb" => $"{word} manner way example",
                    _ => $"{word} meaning definition illustration"
                };
            }

            return $"{word} meaning definition illustration";
        }

        /// <summary>
        /// Tính điểm chất lượng cho mỗi ảnh
        /// </summary>
        private int ScoreImage(JToken img)
        {
            int score = 0;
            var title = img["title"]?.ToString()?.ToLower() ?? "";
            var source = img["source"]?.ToString()?.ToLower() ?? "";
            var original = img["original"]?.ToString() ?? "";

            // Từ khóa xấu - trừ điểm
            string[] badKeywords =
            {
                "trailer", "movie", "film", "poster", "season", "episode",
                "netflix", "hbo", "disney", "marvel", "imdb", "prime video",
                "logo", "brand", "merchandise", "t-shirt", "mug", "shop",
                "buy", "price", "sale", "amazon", "ebay"
            };

            foreach (var bad in badKeywords)
            {
                if (title.Contains(bad)) score -= 10;
                if (source.Contains(bad)) score -= 5;
            }

            // Từ khóa tốt - cộng điểm
            string[] goodKeywords =
            {
                "definition", "meaning", "example", "illustration",
                "diagram", "concept", "educational", "dictionary",
                "wikipedia", "britannica"
            };

            foreach (var good in goodKeywords)
            {
                if (title.Contains(good)) score += 10;
                if (source.Contains(good)) score += 5;
            }

            // Domain tốt
            string[] trustedDomains =
            {
                "wikipedia", "britannica", "merriam-webster",
                "dictionary.com", "educational", ".edu", ".gov"
            };

            foreach (var domain in trustedDomains)
            {
                if (source.Contains(domain)) score += 15;
            }

            // Ưu tiên ảnh có độ phân giải cao
            if (original.Contains("_b.jpg") || original.Contains("large"))
                score += 5;

            return score;
        }

        /// <summary>
        /// Chọn ảnh tốt nhất dựa trên scoring
        /// </summary>
        private JToken? PickBestImage(JArray images)
        {
            if (images == null || images.Count == 0) return null;

            var scored = images
                .Select(img => new { Image = img, Score = ScoreImage(img) })
                .OrderByDescending(x => x.Score)
                .ToList();

            // Trả về ảnh có điểm cao nhất (hoặc ảnh đầu nếu tất cả điểm âm)
            return scored.FirstOrDefault()?.Image;
        }

        /// <summary>
        /// Build URL với các tham số tối ưu
        /// </summary>
        private string BuildSerpApiUrl(string query, int page = 1)
        {
            var parameters = new Dictionary<string, string>
            {
                ["engine"] = "google_images_light",
                ["q"] = query,
                ["image_type"] = "photo",
                ["safe"] = "active",
                ["licenses"] = "cl",  // Creative Commons
                ["gl"] = "us",        // Geolocation
                ["hl"] = "en",        // Language
                ["api_key"] = _apiKey
            };

            if (page > 1)
                parameters["start"] = ((page - 1) * 20).ToString();

            var queryString = string.Join("&",
                parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

            return $"https://serpapi.com/search?{queryString}";
        }

        /// <summary>
        /// Fetch ảnh từ SerpApi với retry logic
        /// </summary>
        private async Task<JArray?> FetchImagesFromApi(
            string query,
            int page = 1,
            CancellationToken ct = default)
        {
            var url = BuildSerpApiUrl(query, page);

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    using var resp = await _httpClient.GetAsync(url, ct);

                    if (!resp.IsSuccessStatusCode)
                    {
                        if (attempt < MaxRetries - 1)
                        {
                            await Task.Delay(RetryDelayMs * (attempt + 1), ct);
                            continue;
                        }
                        return null;
                    }

                    var json = await resp.Content.ReadAsStringAsync(ct);
                    var obj = JObject.Parse(json);

                    // Kiểm tra lỗi từ API
                    if (obj["error"] != null)
                    {
                        Console.WriteLine($"SerpApi error: {obj["error"]}");
                        return null;
                    }

                    return obj["images_results"] as JArray;
                }
                catch (Exception ex) when (attempt < MaxRetries - 1)
                {
                    Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}");
                    await Task.Delay(RetryDelayMs * (attempt + 1), ct);
                }
            }

            return null;
        }

        /// <summary>
        /// Download ảnh với validation
        /// </summary>
        private async Task<byte[]?> DownloadImageBytes(
            string imageUrl,
            CancellationToken ct = default)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(imageUrl, ct);

                // Validate: ít nhất 1KB và không quá 10MB
                if (bytes.Length < 1024 || bytes.Length > 10 * 1024 * 1024)
                    return null;

                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tạo BitmapImage từ bytes
        /// </summary>
        private BitmapImage? CreateBitmapFromBytes(byte[] bytes)
        {
            try
            {
                using var ms = new MemoryStream(bytes);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating bitmap: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy BitmapImage: RAM -> Disk -> SerpApi (upgraded)
        /// </summary>
        public async Task<BitmapImage?> FetchAndCacheAsync(
            string word,
            string? partOfSpeech = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(word)) return null;

            var key = NormalizeKey(word);

            // 1. Kiểm tra RAM cache
            var cached = GetFromMemory(key);
            if (cached != null) return cached;

            // 2. Kiểm tra Disk cache
            var diskImg = LoadFromDisk(key);
            if (diskImg != null)
            {
                _memoryCache[key] = diskImg;
                return diskImg;
            }

            // 3. Fetch từ SerpApi
            var query = BuildQuery(word, partOfSpeech);
            var images = await FetchImagesFromApi(query, 1, ct);

            if (images == null || images.Count == 0)
            {
                // Thử query đơn giản hơn
                query = word;
                images = await FetchImagesFromApi(query, 1, ct);
                if (images == null || images.Count == 0)
                    return null;
            }

            // 4. Chọn ảnh tốt nhất
            var best = PickBestImage(images);
            if (best == null) return null;

            // 5. Lấy URL ảnh (ưu tiên original > thumbnail)
            var imageUrl = best["original"]?.ToString()
                          ?? best["thumbnail"]?.ToString();

            if (string.IsNullOrEmpty(imageUrl))
                return null;

            // 6. Download ảnh
            var bytes = await DownloadImageBytes(imageUrl, ct);
            if (bytes == null) return null;

            // 7. Lưu vào disk
            try
            {
                var path = GetImagePath(word);
                await File.WriteAllBytesAsync(path, bytes, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image to disk: {ex.Message}");
            }

            // 8. Tạo BitmapImage và cache vào RAM
            var bmp = CreateBitmapFromBytes(bytes);
            if (bmp != null)
            {
                _memoryCache[key] = bmp;
            }

            return bmp;
        }

        /// <summary>
        /// Đảm bảo ảnh đã được download (dùng khi user bấm Download)
        /// </summary>
        public async Task EnsureImageDownloadedAsync(
            string word,
            string? partOfSpeech = null,
            CancellationToken ct = default)
        {
            if (HasLocalImage(word)) return;
            await FetchAndCacheAsync(word, partOfSpeech, ct);
        }

        /// <summary>
        /// Xóa cache (RAM + Disk) cho một từ cụ thể
        /// </summary>
        public void ClearCache(string word)
        {
            var key = NormalizeKey(word);
            _memoryCache.TryRemove(key, out _);

            try
            {
                var path = GetImagePath(word);
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa toàn bộ cache
        /// </summary>
        public void ClearAllCache()
        {
            _memoryCache.Clear();

            try
            {
                if (Directory.Exists(_imagesFolder))
                {
                    foreach (var file in Directory.GetFiles(_imagesFolder, "*.jpg"))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing all cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy kích thước cache hiện tại (MB)
        /// </summary>
        public double GetCacheSizeMB()
        {
            try
            {
                if (!Directory.Exists(_imagesFolder))
                    return 0;

                var totalBytes = Directory.GetFiles(_imagesFolder, "*.jpg")
                    .Sum(file => new FileInfo(file).Length);

                return totalBytes / (1024.0 * 1024.0);
            }
            catch
            {
                return 0;
            }
        }
    }
}