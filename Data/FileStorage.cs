using BlueBerryDictionary.Helpers;
using BlueBerryDictionary.Models;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace BlueBerryDictionary.Data
{
    #region Tương tác với PersientStorage
    internal class FileStorage
    {
        // folder lưu từ vựng
        static private string _storedWordPath = PathHelper.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            @"..\..\..\Data\PersistentStorage\StoredWord"
        );
        // folder lưu quotes
        static public string _storedQuotePath = PathHelper.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            @"..\..\..\Data\PersistentStorage\StoredQuote"
        );
        // file danh sách từ có sẵn
        static private string _listFile = PathHelper.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
           @"..\..\..\Data\PersistentStorage\AvailableWordList.txt"
        );


        /// <summary>
        /// Lấy đường dẫn file JSON của từ
        /// </summary>
        public static string GetWordFilePath(string word)
        {
            if (word.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                word = Path.GetFileNameWithoutExtension(word);
            }
            return Path.Combine(_storedWordPath, word + ".json");
        }

        /// <summary>
        /// Lấy danh sách từ đã lưu trong storage
        /// </summary
        public static List<string> GetStoredWordList()
        {
            if (!Directory.Exists(_storedWordPath)) return new List<string>();

            var result = Directory.GetFiles(_storedWordPath, "*.json");
            List<string> words = new List<string>();

            foreach (var r in result)
            {
                string word = Path.GetFileNameWithoutExtension(r);
                words.Add(word);
            }
            return words;
        }

        /// <summary>
        /// Lấy danh sách từ có sẵn từ file text
        /// </summary>
        public static List<string> GetAvailableWordList()
        {
            List<string> results = File.ReadAllLines(_listFile).Where(line => !string.IsNullOrWhiteSpace(line)).
                                    Select(line => line.Trim()).ToList().ToList();
            return results;
        }
        /// <summary>
        /// Load từ từ file JSON (async)
        /// </summary>
        public static async Task<List<Word>?> LoadWordAsync(string word)
        {
            string path = GetWordFilePath(word.ToLower());
            if (!File.Exists(path)) return null;

            var content = await File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<List<Word>>(content);
        }

        /// <summary>
        /// Lưu từ vào file JSON (nếu chưa tồn tại)
        /// </summary>
        public static bool LoadWordAsync(List<Word>? words)
        {
            if (words == null || words.Count == 0) return false;

            string path = GetWordFilePath(words[0].word);

            if (File.Exists(path)) return false;

            string content = JsonConvert.SerializeObject(words, Formatting.Indented);
            File.WriteAllTextAsync(path, content);
            return true;
        }


        /// <summary>
        /// Build dictionary hoàn chỉnh (merge available + stored)
        /// </summary>
        public static List<string> BuildDictionary()
        {

            var list1 = GetAvailableWordList();
            var list2 = GetStoredWordList();
            return list1.Concat(list2).Distinct().ToList();
        }
        public static void Download(List<Word> target)
        {
            string message = string.Empty;
            if (target != null && target.Count != 0)
            {
                var word = target[0].word;
                string path = GetWordFilePath(word);
                if (File.Exists(path))
                {
                    message = $"{word} existed in " + path;
                }
                else
                {
                    string content = JsonConvert.SerializeObject(target, Formatting.Indented);
                    File.WriteAllText(path, content);
                    message = $"{word} has been downloaded successfully in " + path;
                }

            }
            else
            {
                message = "have no word to download";
            }

            MessageBox.Show(message, "Download status", MessageBoxButton.OK);
        }

        /// <summary>
        /// Load quote theo ID
        /// </summary>
        public static async Task<Quote?> LoadQuoteAsync(int ID)
        {
            string path = Path.Combine(_storedQuotePath, $"quote_{ID}") + ".json";
            var result = await LoadQuoteAsync(path);
            return result;
        }
        public static async Task<Quote?> LoadQuoteAsync(string path)
        {
            if (!File.Exists(path)) return null;

            var content = await File.ReadAllTextAsync(path);
            var obj = JsonConvert.DeserializeObject<Quote>(content);
            return obj;
        }

    }

    #endregion
}
