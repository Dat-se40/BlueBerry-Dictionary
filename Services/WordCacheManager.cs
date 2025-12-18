using BlueBerryDictionary.Models;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace MyDictionary.Services
{
    public class CacheEntry
    {
        public List<Word> _words { get; set; }
        public DateTime _lastAccessed { get; set; }
    }
    internal class WordCacheManager
    {
        private int _maxCacheSize = 100;
        private ConcurrentDictionary<string, CacheEntry> _memoryCache = new();

        // Singleton pattern
        private static WordCacheManager? _instance;
        private static readonly object _lock = new object();

        public static WordCacheManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new WordCacheManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private WordCacheManager() { }

        /// <summary>
        /// Thêm từ vào cache
        /// </summary>
        public void AddToCache(string word, List<Word> words)
        {
            if (_memoryCache.ContainsKey(word)) return;
            TrimCacheIfNeeded();
            if (_memoryCache.Count < _maxCacheSize)
            {
                _memoryCache.TryAdd(word, new CacheEntry()
                {
                    _lastAccessed = DateTime.Now,
                    _words = words
                });
            }
        }
        private void TrimCacheIfNeeded()
        {
            if(_memoryCache.Count > _maxCacheSize)
            {
                if (_memoryCache.Count >= _maxCacheSize)
                {
                    var oldest = _memoryCache.OrderBy(x => x.Value._lastAccessed).First();
                    _memoryCache.TryRemove(oldest.Key, out _);
                }
            }
        }
        public List<Word>? GetWordsFormCache(string key)
        {
            if (_memoryCache.ContainsKey(key)) return _memoryCache[key]._words;
            else return null; 
        }

        public List<CacheEntry> GetAllCacheEntries() 
        {
            return _memoryCache.Values.ToList();    
        }
    }
}
