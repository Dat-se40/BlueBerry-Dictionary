using BlueBerryDictionary.Models;
using System.Collections.Concurrent;

namespace MyDictionary.Services
{
    internal class WordCacheManager
    {
        private int _maxCacheSize = 100;
        private ConcurrentDictionary<string, CacheEntry> _memoryCache = new(); 

        private class CacheEntry
        {
            public List<Word> _words { get; set; }  
            public DateTime _lastAccessed { get; set; } 
        }
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
    }
}
