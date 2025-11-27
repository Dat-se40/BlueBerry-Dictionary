using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Model lưu trữ từ vựng chính, mỗi api đều parse sang class Word
namespace BlueBerryDictionary.Models
{
    #region Thông tin đầy đủ của một từ, phục vụ việc lưu trữ, hiển thị thông tin chi tiết
    public class Definition
    {
        public string definition { get; set; }
        public List<string> synonyms { get; set; }
        public List<string> antonyms { get; set; }
        public string example { get; set; }

    }

    public class Meaning
    {
        public string partOfSpeech { get; set; }
        public List<Definition> definitions { get; set; }
        public List<string> synonyms { get; set; }
        public List<string> antonyms { get; set; }


    }

    public class Phonetic
    {
        public string text { get; set; }
        public string audio { get; set; }
        public string sourceUrl { get; set; }
        public License license { get; set; }
    }
    public class License
    {
        public string name { get; set; }
        public string url { get; set; }
    }
    public class Word
    {
        public string word { get; set; }
        public string phonetic { get; set; }
        public List<Phonetic> phonetics { get; set; }
        public List<Meaning> meanings { get; set; }
        public License license { get; set; }
        public List<string> sourceUrls { get; set; }


    }
    #endregion
    #region  Phiên bản rút gọn của Word để hiển thị trong cards & Phiên bản rút gọn của Word để hiển thị trong cards
    public class WordShortened
    {
        public string Word { get; set; }
        public string Phonetic { get; set; }
        public string PartOfSpeech { get; set; } // noun, verb, adj...
        public string Definition { get; set; } // Định nghĩa ngắn nhất
        public string Example { get; set; } // Ví dụ đầu tiên
        public List<string> Synonyms { get; set; } // Từ đồng nghĩa
        public List<string> Antonyms { get; set; } // Từ trái nghĩa
        public List<string> Tags { get; set; } // Tag IDs
        public DateTime AddedAt { get; set; }
        public int ViewCount { get; set; }
        public int MeaningIndex { get; set; } // Lưu index của meaning được chọn (để reference sau)

        public WordShortened()
        {
            Tags = new List<string>();
            Synonyms = new List<string>();
            Antonyms = new List<string>();
            AddedAt = DateTime.Now;
            MeaningIndex = 0;
        }

        /// <summary>
        /// Tạo WordShortened từ Word đầy đủ - Tự động chọn meaning đầu tiên
        /// </summary>
        public static WordShortened FromWord(Word word)
        {
            return FromWord(word, 0);
        }

        /// <summary>
        /// Tạo WordShortened từ Word đầy đủ với meaning cụ thể
        /// </summary>
        /// <param name="word">Word đầy đủ</param>
        /// <param name="meaningIndex">Index của meaning muốn lưu (0-based)</param>
        public static WordShortened FromWord(Word word, int meaningIndex)
        {
            // Validate input
            if (word == null || word.meanings == null || word.meanings.Count == 0)
                return null;

            // Validate meaningIndex
            if (meaningIndex < 0 || meaningIndex >= word.meanings.Count)
            {
                Console.WriteLine($"⚠️ Invalid meaningIndex {meaningIndex}, using 0 instead");
                meaningIndex = 0;
            }

            var selectedMeaning = word.meanings[meaningIndex];
            var firstDef = selectedMeaning.definitions?.FirstOrDefault();

            return new WordShortened
            {
                Word = word.word,
                Phonetic = word.phonetic ?? "",
                PartOfSpeech = selectedMeaning.partOfSpeech ?? "unknown",
                Definition = firstDef?.definition ?? "",
                Example = firstDef?.example ?? "",
                MeaningIndex = meaningIndex,

                // Lấy synonyms & antonyms (ưu tiên từ definition, fallback về meaning)
                Synonyms = firstDef?.synonyms?.Count > 0
                    ? firstDef.synonyms
                    : (selectedMeaning.synonyms ?? new List<string>()),

                Antonyms = firstDef?.antonyms?.Count > 0
                    ? firstDef.antonyms
                    : (selectedMeaning.antonyms ?? new List<string>()),

                AddedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Tạo WordShortened với nhiều options (Builder pattern style)
        /// </summary>
        public static WordShortened FromWordAdvanced(
            Word word,
            int meaningIndex = 0,
            int definitionIndex = 0,
            bool includeSynonyms = true,
            bool includeAntonyms = true)
        {
            if (word == null || word.meanings == null || word.meanings.Count == 0)
                return null;

            // Validate meaningIndex
            meaningIndex = Math.Max(0, Math.Min(meaningIndex, word.meanings.Count - 1));

            var selectedMeaning = word.meanings[meaningIndex];

            // Validate definitionIndex
            if (selectedMeaning.definitions == null || selectedMeaning.definitions.Count == 0)
                return null;

            definitionIndex = Math.Max(0, Math.Min(definitionIndex, selectedMeaning.definitions.Count - 1));
            var selectedDef = selectedMeaning.definitions[definitionIndex];

            var shortened = new WordShortened
            {
                Word = word.word,
                Phonetic = word.phonetic ?? "",
                PartOfSpeech = selectedMeaning.partOfSpeech ?? "unknown",
                Definition = selectedDef?.definition ?? "",
                Example = selectedDef?.example ?? "",
                MeaningIndex = meaningIndex,
                AddedAt = DateTime.Now
            };

            // Conditionally add synonyms/antonyms
            if (includeSynonyms)
            {
                shortened.Synonyms = selectedDef?.synonyms?.Count > 0
                    ? selectedDef.synonyms
                    : (selectedMeaning.synonyms ?? new List<string>());
            }

            if (includeAntonyms)
            {
                shortened.Antonyms = selectedDef?.antonyms?.Count > 0
                    ? selectedDef.antonyms
                    : (selectedMeaning.antonyms ?? new List<string>());
            }

            return shortened;
        }

        /// <summary>
        /// Format cho display trong UI
        /// </summary>
        public string GetDisplayText()
        {
            return $"{Word} ({PartOfSpeech}): {Definition}";
        }

        /// <summary>
        /// Format đầy đủ với example
        /// </summary>
        public string GetFullDisplayText()
        {
            var result = GetDisplayText();
            if (!string.IsNullOrEmpty(Example))
            {
                result += $"\nExample: \"{Example}\"";
            }
            return result;
        }

        /// <summary>
        /// Kiểm tra có synonyms/antonyms không
        /// </summary>
        public bool HasRelatedWords => (Synonyms?.Count > 0) || (Antonyms?.Count > 0);
    }
    #endregion

    /// <summary>
    /// Word collection với metadata
    /// </summary>
    public class WordCollection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<WordShortened> Words { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }

        public WordCollection()
        {
            Id = Guid.NewGuid().ToString();
            Words = new List<WordShortened>();
            CreatedAt = DateTime.Now;
            LastModified = DateTime.Now;
        }
    }
}
#endregion