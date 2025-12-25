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

        private ImageSearchService()
        {
            _apiKey = Config.Instance.SerpApiKey; // dùng Config hiện có[file:57]

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

        /// <summary>
        /// Tạo query “thân thiện từ vựng” để giảm phim/brand.
        /// </summary>
        private string BuildQuery(string word, string? partOfSpeech = null)
        {
            word = word.Trim();
            // Heuristic: thêm meaning/definition để né brand.[web:64][web:76]
            if (!string.IsNullOrEmpty(partOfSpeech))
            {
                return partOfSpeech.ToLower() switch
                {
                    "verb" => $"{word} action meaning illustration",
                    "adjective" => $"{word} situation meaning",
                    "adverb" => $"{word} in context meaning",
                    _ => $"{word} word meaning illustration"
                };
            }

            return $"{word} word meaning illustration";
        }

        /// <summary>
        /// Chọn ảnh “đỡ phim/brand” nhất từ images_results.
        /// </summary>
        private JToken? PickBestImage(JArray images)
        {
            if (images == null || images.Count == 0) return null;

            // Những từ khóa/title/source muốn né.[web:76]
            string[] badTitleKeywords =
            {
                "official trailer", "trailer", "movie", "film",
                "poster", "season", "episode", "netflix", "marvel",
                "logo", "brand", "company", "corp"
            };

            string[] badSources =
            {
                "imdb", "rottentomatoes", "netflix",
                "hbo", "disney", "marvel", "dc comics"
            };

            // Ưu tiên những ảnh không chứa các pattern “phim/brand”.
            foreach (var img in images)
            {
                var title = img["title"]?.ToString() ?? "";
                var source = img["source"]?.ToString() ?? "";

                bool looksBadTitle = badTitleKeywords
                    .Any(k => title.Contains(k, StringComparison.OrdinalIgnoreCase));

                bool looksBadSource = badSources
                    .Any(k => source.Contains(k, StringComparison.OrdinalIgnoreCase));

                if (!looksBadTitle && !looksBadSource)
                    return img;
            }

            // Nếu tất cả đều “hơi phèn” thì lấy cái đầu.
            return images[0];
        }

        /// <summary>
        /// Lấy BitmapImage: RAM -> Disk -> SerpApi (có lọc).
        /// </summary>
        public async Task<BitmapImage?> FetchAndCacheAsync(
            string word,
            string? partOfSpeech = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(word)) return null;

            var key = NormalizeKey(word);

            // 1. RAM
            var cached = GetFromMemory(key);
            if (cached != null) return cached;

            // 2. Disk
            var diskImg = LoadFromDisk(key);
            if (diskImg != null)
            {
                _memoryCache[key] = diskImg;
                return diskImg;
            }

            // 3. Call SerpApi Google Images Light.[web:70]
            var query = BuildQuery(word, partOfSpeech);

            var url =
                "https://serpapi.com/search" +
                "?engine=google_images_light" +
                $"&q={Uri.EscapeDataString(query)}" +
                "&image_type=photo" +        // ưu tiên ảnh chụp[web:70]
                "&licenses=cl" +             // Creative Commons / ít film hơn[web:70][web:71]
                "&safe=active" +             // safe search[web:70]
                $"&api_key={_apiKey}";

            using var resp = await _httpClient.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            var obj = JObject.Parse(json);
            var arr = obj["images_results"] as JArray;
            if (arr == null || arr.Count == 0) return null;

            var best = PickBestImage(arr);
            if (best == null) return null;

            var imageUrl =
                best["thumbnail"]?.ToString()
                ?? best["original"]?.ToString();

            if (string.IsNullOrEmpty(imageUrl))
                return null;

            var bytes = await _httpClient.GetByteArrayAsync(imageUrl, ct);

            // Lưu file
            var path = GetImagePath(word);
            await File.WriteAllBytesAsync(path, bytes, ct);

            // Tạo Bitmap from bytes
            using var ms = new MemoryStream(bytes);
            var bmp2 = new BitmapImage();
            bmp2.BeginInit();
            bmp2.CacheOption = BitmapCacheOption.OnLoad;
            bmp2.StreamSource = ms;
            bmp2.EndInit();
            bmp2.Freeze();

            _memoryCache[key] = bmp2;
            return bmp2;
        }

        /// <summary>
        /// Dùng khi user bấm Download: đảm bảo ảnh đã tồn tại trong local.
        /// </summary>
        public async Task EnsureImageDownloadedAsync(
            string word,
            string? partOfSpeech = null,
            CancellationToken ct = default)
        {
            if (HasLocalImage(word)) return;
            await FetchAndCacheAsync(word, partOfSpeech, ct);
        }
    }
}
