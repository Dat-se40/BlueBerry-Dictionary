using BlueBerryDictionary.Data;
using BlueBerryDictionary.Services;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace BlueBerryDictionary.Models
{
    /// <summary>
    /// Lớp cơ sở cho các package
    /// </summary>
    public abstract class PackageBase<T>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; } // Người tạo package
        public int TotalItems { get; set; } // Số lượng items
        public long SizeInBytes { get; set; } // Kích thước file
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Version { get; set; } = "1.0";

        //  CRITICAL: URL Google Drive để download
        public string DownloadUrl { get; set; }

        //  Container chứa data
        public List<T> Container { get; set; } = new List<T>();

        //  Trạng thái download
        [JsonIgnore]
        public bool IsDownloaded { get; set; }

        [JsonIgnore]
        public string LocalPath { get; set; } // Path file local sau khi download

        // Serialize Container thành JSON
        public string SerializeContainer()
        {
            return JsonConvert.SerializeObject(Container, Formatting.Indented);
        }

        // Deserialize JSON → Container
        public void DeserializeContainer(string jsonContent)
        {
            Container = JsonConvert.DeserializeObject<List<T>>(jsonContent) ?? new List<T>();
        }

        // Abstract method để implement ở subclass
        public abstract Task DownloadAsync();
        public abstract Task ImportToLocalAsync(); // Import vào TagService/FileStorage
    }

    /// <summary>
    /// Package chứa các topics (mỗi topic = 1 collection từ vựng)
    /// VD: IELTS Vocabulary, Business English, Daily Conversation
    /// </summary>
    public class TopicPackage : PackageBase<TopicCollection>
    {
        public string Category { get; set; } // "IELTS", "Business", "Daily"
        public string Level { get; set; } // "Beginner", "Intermediate", "Advanced"
        public string ThumbnailUrl { get; set; } // Icon cho UI

        /// <summary>
        /// Download package từ Google Drive về local
        /// </summary>
        public override async Task DownloadAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadUrl))
                {
                    throw new Exception("Download URL is missing");
                }

                // Tạo folder lưu packages
                var packagesFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\..\Data\PackageStorages"
                );
                Directory.CreateDirectory(packagesFolder);

                // Tên file local
                LocalPath = Path.Combine(packagesFolder, $"{Id}.json");

                // ✅ Download từ URL (có thể dùng HttpClient hoặc Google Drive API)
                using var client = new HttpClient();
                var jsonData = await client.GetStringAsync(DownloadUrl);

                // Lưu file
                await File.WriteAllTextAsync(LocalPath, jsonData);

                // Deserialize vào Container
                DeserializeContainer(jsonData);

                IsDownloaded = true;
                Console.WriteLine($"✅ Downloaded package: {Name} ({Container.Count} topics)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Download error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Import data vào TagService (tạo tags + words)
        /// </summary>
        public override async Task ImportToLocalAsync()
        {
            if (!IsDownloaded || Container == null)
            {
                throw new Exception("Package not downloaded yet");
            }

            var tagService = TagService.Instance;
            int totalWordsAdded = 0;

            foreach (var topic in Container)
            {
                // ✅ Tạo Tag cho mỗi topic
                var tag = tagService.CreateTag(
                    name: topic.Name,
                    icon: topic.Icon ?? "📚",
                    color: topic.Color ?? "#2D4ACC"
                );

                // ✅ Import từng Word FULL vào TagService
                foreach (var fullWord in topic.Words)
                {
                    // Convert Word → WordShortened (lấy meaning đầu tiên)
                    var shortened = WordShortened.FromWord(fullWord, meaningIndex: 0);
                    if (shortened != null)
                    {
                        // Add tag vào word
                        shortened.Tags.Add(tag.Id);

                        // Lưu vào TagService
                        tagService.AddNewWordShortened(shortened);

                        // ✅ LƯU FILE FULL WORD VÀO OFFLINE STORAGE
                        FileStorage.LoadWordAsync(new List<Word> { fullWord });

                        totalWordsAdded++;
                    }
                }
            }

            // Save tất cả
            tagService.SaveWords();
            tagService.SaveTags();

            Console.WriteLine($"✅ Imported {totalWordsAdded} words from package: {Name}");
        }
    }
}
