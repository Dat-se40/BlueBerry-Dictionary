using BlueBerryDictionary.Helpers;
using BlueBerryDictionary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlueBerryDictionary.Services
{
    /// <summary>
    /// Dịch vụ quản lý Tag và WordShortened (singleton)
    /// </summary>
    public class TagService
    {
        private static TagService _instance;
        private static readonly object _lock = new object();

        private Dictionary<string, Tag> _tags; // tagId -> Tag
        private Dictionary<string, WordShortened> _words; // word(lower) -> WordShortened

        public Action OnWordsChanged;

        private readonly string _tagsPath;
        private readonly string _wordsPath;

        public static TagService Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new TagService();
                }
            }
        }

        private TagService()
        {
            _tags = new Dictionary<string, Tag>();
            _words = new Dictionary<string, WordShortened>();

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = PathHelper.Combine(baseDir, @"..\..\..\Data\PersistentStorage\StoredTag");
            _tagsPath = PathHelper.Combine(dataDir, "Tags.json");
            _wordsPath = PathHelper.Combine(dataDir, "MyWords.json");

            LoadData();
        }

        // ====================== UTILITIES ======================

        private string Normalize(string word)
            => word?.Trim().ToLowerInvariant();

        public WordShortened FindWordInsensitive(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return null;

            var norm = Normalize(word);
            if (_words.TryGetValue(norm, out var val))
                return val;

            return _words.Values.FirstOrDefault(w =>
                w.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
        }

        // ====================== TAG ======================

        public Tag CreateTag(string name, string icon = "🏷️", string color = "#2D4ACC")
        {
            var tag = new Tag { Name = name, Icon = icon, Color = color };
            _tags[tag.Id] = tag;
            SaveTags();
            return tag;
        }

        public List<Tag> GetAllTags() =>
            _tags.Values.OrderBy(t => t.Name).ToList();

        public Tag GetTag(string tagId) =>
            _tags.TryGetValue(tagId, out var tag) ? tag : null;

        public bool DeleteTag(string tagId)
        {
            if (_tags.Remove(tagId))
            {
                foreach (var w in _words.Values)
                    w.Tags.Remove(tagId);

                SaveTags();
                SaveWords();
                return true;
            }
            return false;
        }

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

        // ====================== WORD ======================

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
            return FindWordInsensitive(word) != null;
        }

        public void AddNewWordShortened(WordShortened newWord)
        {
            if (newWord == null || string.IsNullOrWhiteSpace(newWord.Word))
                return;

            var key = Normalize(newWord.Word);
            if (!_words.ContainsKey(key))
            {
                _words[key] = newWord;
                SaveWords(); // ✅ luôn ghi file
            }
        }

        

        public void DeleteWordShortened(string word)
        {
            var found = FindWordInsensitive(word);
            if (found == null) return;

            var key = Normalize(found.Word);
            _words.Remove(key);

            foreach (var tag in _tags.Values)
                tag.RelatedWords.Remove(found.Word);

            OnWordsChanged?.Invoke();
            SaveWords();
            SaveTags();
        }

        public List<WordShortened> GetAllWords() =>
            _words.Values.OrderByDescending(w => w.AddedAt).ToList();

        public List<WordShortened> GetWordsByTag(string tagId)
        {
            var tag = GetTag(tagId);
            if (tag == null) return new();

            return tag.RelatedWords
                .Select(w => FindWordInsensitive(w))
                .Where(w => w != null)
                .ToList();
        }

        public List<WordShortened> GetWordsByLetter(string letter)
        {
            if (letter.ToUpper() == "ALL") return GetAllWords();

            return _words.Values
                .Where(w =>
                    !string.IsNullOrEmpty(w.Word) &&
                    w.Word.StartsWith(letter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(w => w.Word)
                .ToList();
        }

        public List<WordShortened> GetWordsByPartOfSpeech(string pos)
        {
            return _words.Values
                .Where(w =>
                    w.PartOfSpeech.Equals(pos, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // ====================== ADD/REMOVE TAG from WORD ======================

        public bool AddTagToWord(string word, string tagId)
        {
            var wordObj = FindWordInsensitive(word);
            if (wordObj == null || !_tags.ContainsKey(tagId))
                return false;

            if (!wordObj.Tags.Contains(tagId))
            {
                wordObj.Tags.Add(tagId);
                _tags[tagId].AddWord(wordObj.Word);
                SaveWords();
                SaveTags();
                return true;
            }

            return false;
        }

        public bool RemoveTagFromWord(string word, string tagId)
        {
            var wordObj = FindWordInsensitive(word);
            if (wordObj == null || !_tags.ContainsKey(tagId))
                return false;

            wordObj.Tags.Remove(tagId);
            _tags[tagId].RemoveWord(wordObj.Word);

            SaveWords();
            SaveTags();
            return true;
        }

        // ====================== FAVORITE ======================

        public bool ToggleFavorite(string word)
        {
            var wordObj = FindWordInsensitive(word);
            if (wordObj == null)
                return false;

            wordObj.isFavorited = !wordObj.isFavorited;
            SaveWords();
            return wordObj.isFavorited;
        }

        public bool IsFavorited(string word)
        {
            return FindWordInsensitive(word)?.isFavorited == true;
        }

        public List<WordShortened> GetFavoriteWords()
        {
            return _words.Values
                .Where(w => w.isFavorited)
                .OrderByDescending(w => w.AddedAt)
                .ToList();
        }

        // ====================== STATISTICS ======================

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
            var dist = new Dictionary<string, int>();
            foreach (var w in _words.Values)
            {
                if (string.IsNullOrEmpty(w.Word)) continue;
                var letter = w.Word[0].ToString().ToUpperInvariant();
                dist[letter] = dist.GetValueOrDefault(letter, 0) + 1;
            }
            return dist;
        }

        // ====================== IO ======================

        private void LoadData()
        {
            try
            {
                if (File.Exists(_tagsPath))
                {
                    var json = File.ReadAllText(_tagsPath);
                    var tagList = JsonConvert.DeserializeObject<List<Tag>>(json);
                    _tags = tagList?.ToDictionary(t => t.Id, t => t) ?? new();
                }
                else
                {
                    CreateDefaultTags();
                }

                if (File.Exists(_wordsPath))
                {
                    var json = File.ReadAllText(_wordsPath);
                    var wordList = JsonConvert.DeserializeObject<List<WordShortened>>(json);
                    _words = wordList?
                        .ToDictionary(w => Normalize(w.Word), w => w)
                        ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Load data error: {ex.Message}");
            }
        }

        public void SaveTags(string path = null)
        {
            if (path == null)
            {
                path = _tagsPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            try
            {
                var json = JsonConvert.SerializeObject(_tags.Values.ToList(), Formatting.Indented);

                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save tags error: {ex.Message}");
            }
        }

        public void SaveWords(string path = null)
        {
            if (path == null)
            {
                path = _wordsPath;
                Directory.CreateDirectory(Path.GetDirectoryName(_wordsPath));
            }
            try
            {
                var json = JsonConvert.SerializeObject(_words.Values.ToList(), Formatting.Indented);

                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save words error: {ex.Message}");
            }
        }
        private void CreateDefaultTags()
        {
            CreateTag("IELTS", "🎯", "#2D4ACC");
            CreateTag("Giao tiếp", "💬", "#10B981");
            CreateTag("Business", "💼", "#F59E0B");
            SaveTags();
        }
    }
}
