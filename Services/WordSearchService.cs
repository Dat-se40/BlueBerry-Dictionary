using BlueBerryDictionary.Models;
using BlueBerryDictionary.Data;
using System.IO;

namespace MyDictionary.Services
{
    public class WordSearchService
    {
        private readonly WordCacheManager _cacheManager = new WordCacheManager();
        private List<string> _dictionary = FileStorage.BuildDictionary();

        #region
        /// <summary>
        /// Tìm kiếm chính xác một từ (async)
        /// </summary>
        //public async Task<SearchResponse> SearchExact(SearchRequest request)
        //{
        //    SearchResponse response = new SearchResponse() { _isSuccess = false };
        //    string term = request._searchTerm;
        //    string path = FileStorage.GetWordFilePath(term);

        //    var cachedWords = _cacheManager.GetWordsFormCache(term);
        //    if (cachedWords != null)
        //    {
        //        response._words = cachedWords;
        //    }
        //    else if (File.Exists(path))
        //    {
        //        response._words = await FileStorage.LoadWordAsync(term) ?? new List<Word>();
        //    }
        //    else
        //    {
        //        response._words = await ApiClient.FetchWordAsync(term);
        //    }

        //    if (response._words != null && response._words.Count > 0)
        //    {
        //        response._isSuccess = true;
        //        _cacheManager.AddToCache(term, response._words);
        //    }

        //    return response;
        //}
        #endregion


        #region Autocomplete
        public List<string>GetSuggestions(string request , int maxResults = 5)
        {
            string term = request.ToLower();
            // ========== BƯỚC 1: TÌM TỪ BẮT ĐẦU BẰNG SEARCH TERM ==========
            var exactMatches = _dictionary
                .Where(word => word.ToLower().StartsWith(term))
                .OrderBy(word => word.Length) // Ưu tiên từ ngắn hơn
                .Take(maxResults)
                .ToList();

            // ========== BƯỚC 2: NẾU KHÔNG ĐỦ, TÌM TỪ TƯƠNG TỰ (LEVENSHTEIN) ==========
            if (exactMatches.Count < maxResults)
            {
                var fuzzyMatches = _dictionary
                    .Where(word => !word.ToLower().StartsWith(term)) // Loại trừ từ đã có
                    .Select(word => new
                    {
                        Word = word,
                        Distance = CalcLevenshteinDistance(term, word.ToLower()),
                        LengthDiff = Math.Abs(word.Length - term.Length)
                    })
                    .Where(x => x.Distance <= Math.Max(2, term.Length / 2)) // Giới hạn khoảng cách
                    .OrderBy(x => x.Distance) // Ưu tiên khoảng cách nhỏ
                    .ThenBy(x => x.LengthDiff) // Ưu tiên độ dài tương tự
                    .Take(maxResults - exactMatches.Count)
                    .Select(x => x.Word)
                    .ToList();

                exactMatches.AddRange(fuzzyMatches);
            }
            return exactMatches;
        }


        /// <summary>
        /// Tính khoảng cách Levenshtein giữa 2 chuỗi
        /// (Dùng để tìm từ tương tự)
        /// </summary>
        public int CalcLevenshteinDistance(string s1, string s2)
        {
            int m = s1.Length, n = s2.Length;
            int[,] levenshtein = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++) levenshtein[i, 0] = i;
            for (int j = 0; j <= n; j++) levenshtein[0, j] = j;

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (s1[i - 1] == s2[j - 1])
                    {
                        levenshtein[i, j] = levenshtein[i - 1, j - 1];
                    }
                    else
                    {
                        levenshtein[i, j] = 1 + Math.Min(
                            levenshtein[i, j - 1],
                            Math.Min(levenshtein[i - 1, j], levenshtein[i - 1, j - 1])
                        );
                    }
                }
            }

            return levenshtein[m, n];
        }
        #endregion

        #region helper function
        /// Kiểm tra từ có tồn tại trong dictionary không
        public bool IsWordExists(string word)
        {
            return _dictionary.Any(w => w.Equals(word, StringComparison.OrdinalIgnoreCase));
        }
        /// Lấy tổng số từ trong dictionary
        public int GetTotalWords()
        {
            return _dictionary.Count;
        }
        #endregion
    }
}
