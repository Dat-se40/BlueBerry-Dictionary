using BlueBerryDictionary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlueBerryDictionary.Services
{
    /// <summary>
    /// Service quản lý Tags và WordShortened
    /// Singleton pattern với thread-safe
    /// </summary>
    public class TagService
    {
        private static TagService _instance;
        private static readonly object _lock = new object();

        private Dictionary<string, Tag> _tags; // tagId -> Tag
        private Dictionary<string, WordShortened> _words; // word -> WordShortened

        private readonly string _tagsPath;
        private readonly string _wordsPath;

        public static TagService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new TagService();
                        }
                    }
                }
                return _instance;
            }
        }

        private TagService()
        {
            _tags = new Dictionary<string, Tag>();
            _words = new Dictionary<string, WordShortened>();

            // Setup paths
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, @"..\..\..\Data\PersistentStorage\StoredTag");
            _tagsPath = Path.Combine(dataDir, "Tags.json");
            _wordsPath = Path.Combine(dataDir, "MyWords.json");

            // Load data
            LoadData();
        }

        // ========================================
        // TAG OPERATIONS
        // ========================================

        /// <summary>
        /// Tạo tag mới
        /// </summary>
        public Tag CreateTag(string name, string icon = "🏷️", string color = "#2D4ACC")
        {
            var tag = new Tag
            {
                Name = name,
                Icon = icon,
                Color = color
            };

            _tags[tag.Id] = tag;
            SaveTags();
            return tag;
        }

        /// <summary>
        /// Lấy tất cả tags
        /// </summary>
        public List<Tag> GetAllTags()
        {
            return _tags.Values.OrderBy(t => t.Name).ToList();
        }

        /// <summary>
        /// Lấy tag theo ID
        /// </summary>
        public Tag GetTag(string tagId)
        {
            return _tags.TryGetValue(tagId, out var tag) ? tag : null;
        }

        /// <summary>
        /// Xóa tag
        /// </summary>
        public bool DeleteTag(string tagId)
        {
            if (_tags.Remove(tagId))
            {
                // Remove tag from all words
                foreach (var word in _words.Values)
                {
                    word.Tags.Remove(tagId);
                }

                SaveTags();
                SaveWords();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update tag info
        /// </summary>
        public bool UpdateTag(string tagId, string newName, string newIcon, string newColor)
        {
            if (_tags.TryGetValue(tagId, out var tag))
            {
                tag.Name = newName;
                tag.Icon = newIcon;
                tag.Color = newColor;
                SaveTags();
                return true;
            }
            return false;
        }

        // ========================================
        // WORD OPERATIONS
        // ========================================

        /// <summary>
        /// Thêm từ vào collection
        /// </summary>
        public WordShortened AddWord(Word fullWord, List<string> tagIds = null)
        {
            var shortened = WordShortened.FromWord(fullWord);
            if (shortened == null) return null;

            // Check if word exists
            if (_words.ContainsKey(shortened.Word))
            {
                return _words[shortened.Word];
            }

            // Add tags
            if (tagIds != null)
            {
                foreach (var tagId in tagIds)
                {
                    if (_tags.ContainsKey(tagId))
                    {
                        shortened.Tags.Add(tagId);
                        _tags[tagId].AddWord(shortened.Word);
                    }
                }
            }

            _words[shortened.Word] = shortened;
            SaveWords();
            SaveTags();

            return shortened;
        }

        /// <summary>
        /// Xóa từ khỏi collection
        /// </summary>
        public bool RemoveWord(string word)
        {
            if (_words.Remove(word))
            {
                // Remove from all tags
                foreach (var tag in _tags.Values)
                {
                    tag.RemoveWord(word);
                }

                SaveWords();
                SaveTags();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Lấy tất cả từ
        /// </summary>
        public List<WordShortened> GetAllWords()
        {
            return _words.Values.OrderByDescending(w => w.AddedAt).ToList();
        }

        /// <summary>
        /// Lấy từ theo tag
        /// </summary>
        public List<WordShortened> GetWordsByTag(string tagId)
        {
            var tag = GetTag(tagId);
            if (tag == null) return new List<WordShortened>();

            return tag.RelatedWords
                .Where(w => _words.ContainsKey(w))
                .Select(w => _words[w])
                .ToList();
        }

        /// <summary>
        /// Lấy từ theo chữ cái đầu
        /// </summary>
        public List<WordShortened> GetWordsByLetter(string letter)
        {
            if (letter.ToUpper() == "ALL")
                return GetAllWords();

            return _words.Values
                .Where(w => w.Word.StartsWith(letter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(w => w.Word)
                .ToList();
        }

        /// <summary>
        /// Lấy từ theo part of speech
        /// </summary>
        public List<WordShortened> GetWordsByPartOfSpeech(string pos)
        {
            return _words.Values
                .Where(w => w.PartOfSpeech.Equals(pos, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Thêm tag cho từ
        /// </summary>
        public bool AddTagToWord(string word, string tagId)
        {
            if (!_words.ContainsKey(word) || !_tags.ContainsKey(tagId))
                return false;

            var wordObj = _words[word];
            var tagObj = _tags[tagId];

            if (!wordObj.Tags.Contains(tagId))
            {
                wordObj.Tags.Add(tagId);
                tagObj.AddWord(word);

                SaveWords();
                SaveTags();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Xóa tag khỏi từ
        /// </summary>
        public bool RemoveTagFromWord(string word, string tagId)
        {
            if (!_words.ContainsKey(word) || !_tags.ContainsKey(tagId))
                return false;

            var wordObj = _words[word];
            var tagObj = _tags[tagId];

            wordObj.Tags.Remove(tagId);
            tagObj.RemoveWord(word);

            SaveWords();
            SaveTags();
            return true;
        }

        /// <summary>
        /// Tăng view count
        /// </summary>
        public void IncrementViewCount(string word)
        {
            if (_words.TryGetValue(word, out var wordObj))
            {
                wordObj.ViewCount++;
                SaveWords();
            }
        }

        // ========================================
        // STATISTICS
        // ========================================

        public int GetTotalWords() => _words.Count;
        public int GetTotalTags() => _tags.Count;

        public int GetWordsAddedThisWeek()
        {
            var weekAgo = DateTime.Now.AddDays(-7);
            return _words.Values.Count(w => w.AddedAt >= weekAgo);
        }

        public int GetWordsAddedThisMonth()
        {
            var monthAgo = DateTime.Now.AddMonths(-1);
            return _words.Values.Count(w => w.AddedAt >= monthAgo);
        }

        public Dictionary<string, int> GetLetterDistribution()
        {
            var distribution = new Dictionary<string, int>();
            foreach (var word in _words.Values)
            {
                var letter = word.Word[0].ToString().ToUpper();
                distribution[letter] = distribution.GetValueOrDefault(letter, 0) + 1;
            }
            return distribution;
        }

        // ========================================
        // PERSISTENCE
        // ========================================

        private void LoadData()
        {
            try
            {
                // Load tags
                if (File.Exists(_tagsPath))
                {
                    var json = File.ReadAllText(_tagsPath);
                    var tagList = JsonConvert.DeserializeObject<List<Tag>>(json);
                    _tags = tagList?.ToDictionary(t => t.Id, t => t) ?? new Dictionary<string, Tag>();
                }
                else
                {
                    // Create default tags
                    CreateDefaultTags();
                }

                // Load words
                if (File.Exists(_wordsPath))
                {
                    var json = File.ReadAllText(_wordsPath);
                    var wordList = JsonConvert.DeserializeObject<List<WordShortened>>(json);
                    _words = wordList?.ToDictionary(w => w.Word, w => w) ?? new Dictionary<string, WordShortened>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Load data error: {ex.Message}");
            }
        }

        private void SaveTags()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_tags.Values.ToList(), Formatting.Indented);
                Directory.CreateDirectory(Path.GetDirectoryName(_tagsPath));
                File.WriteAllText(_tagsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save tags error: {ex.Message}");
            }
        }

        private void SaveWords()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_words.Values.ToList(), Formatting.Indented);
                Directory.CreateDirectory(Path.GetDirectoryName(_wordsPath));
                File.WriteAllText(_wordsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save words error: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo tags mặc định
        /// </summary>
        private void CreateDefaultTags()
        {
            CreateTag("IELTS", "🎯", "#2D4ACC");
            CreateTag("Giao tiếp", "💬", "#10B981");
            CreateTag("Business", "💼", "#F59E0B");
            SaveTags();
        }
    }
}