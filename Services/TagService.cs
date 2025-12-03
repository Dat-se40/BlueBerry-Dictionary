using BlueBerryDictionary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlueBerryDictionary.Services
{
    /// <summary>4
    /// Service quản lý Tags và WordShortened
    /// Singleton pattern với thread-safe
    /// </summary>
    public class TagService 
    {
        private static TagService _instance;
        private static readonly object _lock = new object();

        private Dictionary<string, Tag> _tags; // tagId -> Tag
        private Dictionary<string, WordShortened> _words; // word -> WordShortened, có luôn các từ đã được thích
        public Action OnWordsChanged; 
        
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
        /// Thêm từ vào collection (hoặc cập nhật tags nếu đã tồn tại)
        /// </summary>
        public WordShortened AddWord(Word fullWord, List<string> tagIds = null)
        {
            if (fullWord == null) return null;

            var wordKey = fullWord.word;

            // ========================================
            // ✅ CASE 1: Từ ĐÃ TỒN TẠI → Cập nhật tags
            // ========================================
            if (_words.ContainsKey(wordKey))
            {
                Console.WriteLine($"📝 Word '{wordKey}' already exists, updating tags...");

                var existingWord = _words[wordKey];

                // ✅ Thêm tags mới (không trùng)
                if (tagIds != null && tagIds.Count > 0)
                {
                    foreach (var tagId in tagIds)
                    {
                        if (_tags.ContainsKey(tagId))
                        {
                            // Thêm tag vào word (nếu chưa có)
                            if (!existingWord.Tags.Contains(tagId))
                            {
                                existingWord.Tags.Add(tagId);
                                Console.WriteLine($"   ✅ Added tag '{_tags[tagId].Name}' to '{wordKey}'");
                            }

                            // Thêm word vào tag (nếu chưa có)
                            if (!_tags[tagId].RelatedWords.Contains(wordKey))
                            {
                                _tags[tagId].AddWord(wordKey);
                                Console.WriteLine($"   ✅ Added '{wordKey}' to tag '{_tags[tagId].Name}'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"   ❌ Tag not found: {tagId}");
                        }
                    }

                    SaveWords();
                    SaveTags();
                }

                return existingWord;
            }

            // ========================================
            // ✅ CASE 2: Từ CHƯA TỒN TẠI → Tạo mới
            // ========================================
            var shortened = WordShortened.FromWord(fullWord);
            if (shortened == null) return null;

            Console.WriteLine($"📝 Adding new word '{wordKey}' with {tagIds?.Count ?? 0} tags");

            // Add tags
            if (tagIds != null && tagIds.Count > 0)
            {
                foreach (var tagId in tagIds)
                {
                    if (_tags.ContainsKey(tagId))
                    {
                        shortened.Tags.Add(tagId);
                        _tags[tagId].AddWord(shortened.Word);
                        Console.WriteLine($"   ✅ Added to tag '{_tags[tagId].Name}'");
                    }
                    else
                    {
                        Console.WriteLine($"   ❌ Tag not found: {tagId}");
                    }
                }
            }

            _words[wordKey] = shortened;
            SaveWords();
            SaveTags();

            Console.WriteLine($"✅ Saved new word '{wordKey}' with {shortened.Tags.Count} tags");
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

        public void AddNewWordShortened(WordShortened newWord) 
        {
            if(!_words.ContainsKey(newWord.Word)) _words.Add(newWord.Word, newWord);
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
        /// Xóa 1 từ!
        /// </summary>
        public void DeleteWordShortened(string word) 
        {
            var ws = GetWordShortened(word);
            foreach (var item in _tags)
            {
                item.Value.RelatedWords.Remove(word); 
            }
            _words.Remove(word);
            OnWordsChanged?.Invoke();
            SaveTags();
            SaveWords(); 
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
// FAVORITE OPERATIONS
// ========================================

/// <summary>
/// Toggle favorite cho word (auto save)
/// </summary>
public bool ToggleFavorite(string word)
{
    if (string.IsNullOrWhiteSpace(word))
        return false;

    // v1 giữ word gốc, fallback insensitive
    var wordObj = _words.TryGetValue(word, out var val)
        ? val
        : _words.Values.FirstOrDefault(w =>
            w.Word.Equals(word, StringComparison.OrdinalIgnoreCase));

    if (wordObj == null)
        return false;

    wordObj.isFavorited = !wordObj.isFavorited;
    SaveWords();

    return wordObj.isFavorited;
}

/// <summary>
/// Check word có được favorite không
/// </summary>
public bool IsFavorited(string word)
{
    if (string.IsNullOrWhiteSpace(word))
        return false;

    var wordObj = _words.TryGetValue(word, out var val)
        ? val
        : _words.Values.FirstOrDefault(w =>
            w.Word.Equals(word, StringComparison.OrdinalIgnoreCase));

    return wordObj?.isFavorited == true;
}

/// <summary>
/// Lấy tất cả favorite words
/// </summary>
public List<WordShortened> GetFavoriteWords()
{
    return _words.Values
        .Where(w => w.isFavorited)
        .OrderByDescending(w => w.AddedAt)
        .ToList();
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
        public WordShortened GetWordShortened(string word) 
        {
            return _words.TryGetValue(word, out WordShortened valure) ? valure : null; 
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

        public void SaveTags()
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

        public void SaveWords()
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