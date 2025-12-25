using BlueBerryDictionary.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MyDictionary.Model.MerriamWebster
{
    /// <summary>
    /// Merriam-Webster API Response Model
    /// Docs: https://dictionaryapi.com/products/json
    /// </summary>
    #region Model cho việc Serialize từ json -> class
    public class MWEntry
    {
        /// <summary>
        /// Entry chính từ Dictionary API
        /// </summary>

        [JsonPropertyName("meta")]
        public MWMeta Meta { get; set; }

        [JsonPropertyName("hwi")]
        public MWHeadwordInfo Hwi { get; set; }

        [JsonPropertyName("fl")]
        public string FunctionalLabel { get; set; } // Part of speech

        [JsonPropertyName("def")]
        public List<MWDefinitionSection> Definitions { get; set; }

        [JsonPropertyName("shortdef")]
        public List<string> ShortDefinitions { get; set; }

        [JsonPropertyName("et")]
        public List<List<object>> Etymology { get; set; }
    }

    public class MWMeta
    {
        /// <summary>
        /// Metadata của từ (ID, stems, offensive flag...)
        /// </summary>

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("sort")]
        public string Sort { get; set; }

        [JsonPropertyName("stems")]
        public List<string> Stems { get; set; }

        [JsonPropertyName("offensive")]
        public bool Offensive { get; set; }
    }

    public class MWHeadwordInfo
    {
        /// <summary>
        /// Thông tin headword và phát âm
        /// </summary>

        [JsonPropertyName("hw")]
        public string Headword { get; set; }

        [JsonPropertyName("prs")]
        public List<MWPronunciation> Pronunciations { get; set; }
    }

    public class MWPronunciation
    {
        /// <summary>
        /// Dữ liệu phát âm IPA
        /// </summary>

        [JsonPropertyName("mw")]
        public string Mw { get; set; } // IPA pronunciation

        [JsonPropertyName("sound")]
        public MWSound Sound { get; set; }
    }

    public class MWSound
    {
        /// <summary>
        /// Metadata file âm thanh
        /// </summary>

        [JsonPropertyName("audio")]
        public string Audio { get; set; }

        [JsonPropertyName("ref")]
        public string Ref { get; set; }

        [JsonPropertyName("stat")]
        public string Stat { get; set; }
    }

    public class MWDefinitionSection
    {
        /// <summary>
        /// Section định nghĩa (cấu trúc nested phức tạp)
        /// </summary>

        [JsonPropertyName("sseq")]
        public List<List<List<object>>> Sseq { get; set; } 

        [JsonPropertyName("vd")]
        public string VerbDivider { get; set; }
    }

    /// <summary>
    /// Định nghĩa từ Thesaurus API
    /// </summary>

    public class MWThesaurusEntry
    {
        [JsonPropertyName("meta")]
        public MWMeta Meta { get; set; }

        [JsonPropertyName("hwi")]
        public MWHeadwordInfo Hwi { get; set; }

        [JsonPropertyName("fl")]
        public string FunctionalLabel { get; set; }

        [JsonPropertyName("def")]
        public List<MWThesaurusDefinition> Definitions { get; set; }

        [JsonPropertyName("shortdef")]
        public List<string> ShortDefinitions { get; set; }
    }

    public class MWThesaurusDefinition
    {
        [JsonPropertyName("sseq")]
        public List<List<List<object>>> Sseq { get; set; }
    }
    #endregion

    #region Tool parse, đưa 1 class Models.Words duy nhất, nhất quán dữ liệu
    public class MerriamWebsterParser
    {
        /// <summary>
        /// Parse Dictionary API response
        /// </summary>
        public static List<Word> ParseDictionary(string json)
        {
            try
            {
                var entries = JsonSerializer.Deserialize<List<MWEntry>>(json);
                if (entries == null || entries.Count == 0)
                    return new List<Word>();

                // Group entries theo từ (MW trả về nhiều entries cho cùng 1 từ)
                var groupedByWord = entries
                    .Where(e => e.Meta != null)
                    .GroupBy(e => CleanHeadword(e.Meta.Id))
                    .ToList();

                List<Word> results = new List<Word>();

                // Xử lý từng nhóm entries
                foreach (var group in groupedByWord)
                {
                    Word word = new Word
                    {
                        word = group.Key,
                        phonetic = ExtractPhonetic(group.First()),
                        phonetics = ExtractPhonetics(group.First()),
                        meanings = new List<Meaning>(),
                        sourceUrls = new List<string> { "https://www.merriam-webster.com/dictionary/" + group.Key }
                    };

                    // Merge tất cả entries thành meanings
                    foreach (var entry in group)
                    {
                        var meaning = ConvertToMeaning(entry);
                        if (meaning != null)
                        {
                            word.meanings.Add(meaning);
                        }
                    }

                    results.Add(word);
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MW Parse Error: {ex.Message}");
                return new List<Word>();
            }
        }

        /// <summary>
        /// Parse Thesaurus API response (synonyms/antonyms)
        /// </summary>
        public static (List<string> synonyms, List<string> antonyms) ParseThesaurus(string json)
        {
            try
            {
                var entries = JsonSerializer.Deserialize<List<MWThesaurusEntry>>(json);
                if (entries == null || entries.Count == 0)
                    return (new List<string>(), new List<string>());

                HashSet<string> synonyms = new HashSet<string>();
                HashSet<string> antonyms = new HashSet<string>();

                // Duyệt qua tất cả entries
                foreach (var entry in entries)
                {
                    if (entry.Definitions == null) continue;

                    foreach (var def in entry.Definitions)
                    {
                        if (def.Sseq == null) continue;

                        foreach (var sseq in def.Sseq)
                        {
                            foreach (var item in sseq)
                            {
                                if (item.Count < 2) continue;

                                var typeStr = item[0]?.ToString();
                                if (typeStr != "sense") continue;

                                var senseObj = item[1] as JsonElement?;
                                if (senseObj == null) continue;

                                // Extract synonyms
                                if (senseObj.Value.TryGetProperty("syn_list", out var synList))
                                {
                                    ExtractRelatedWords(synList, synonyms);
                                }

                                // Extract antonyms
                                if (senseObj.Value.TryGetProperty("ant_list", out var antList))
                                {
                                    ExtractRelatedWords(antList, antonyms);
                                }
                            }
                        }
                    }
                }

                return (synonyms.ToList(), antonyms.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MW Thesaurus Parse Error: {ex.Message}");
                return (new List<string>(), new List<string>());
            }
        }

        #region Helper methods

        /// <summary>
        /// Clean headword (remove subscripts như "hello:1")
        /// </summary>
        private static string CleanHeadword(string hw)
        {
            return Regex.Replace(hw, @":\d+$", "");
        }

        /// <summary>
        /// Extract phonetic text
        /// </summary>
        private static string ExtractPhonetic(MWEntry entry)
        {
            if (entry?.Hwi?.Pronunciations == null || entry.Hwi.Pronunciations.Count == 0)
                return "";

            return entry.Hwi.Pronunciations[0]?.Mw ?? "";
        }

        /// <summary>
        /// Extract phonetics list (với audio URLs)
        /// </summary>
        private static List<Phonetic> ExtractPhonetics(MWEntry entry)
        {
            List<Phonetic> phonetics = new List<Phonetic>();

            if (entry?.Hwi?.Pronunciations == null)
                return phonetics;

            foreach (var pron in entry.Hwi.Pronunciations)
            {
                string audioUrl = "";
                if (pron.Sound?.Audio != null)
                {
                    audioUrl = BuildAudioUrl(pron.Sound.Audio);
                }

                phonetics.Add(new Phonetic
                {
                    text = pron.Mw,
                    audio = audioUrl,
                    sourceUrl = "https://www.merriam-webster.com"
                });
            }

            return phonetics;
        }

        /// <summary>
        /// Build Merriam-Webster audio URL
        /// Format: https://media.merriam-webster.com/audio/prons/en/us/mp3/{subdir}/{filename}.mp3
        /// </summary>
        private static string BuildAudioUrl(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return "";

            string subdir;

            if (filename.StartsWith("bix"))
                subdir = "bix";
            else if (filename.StartsWith("gg"))
                subdir = "gg";
            else if (Regex.IsMatch(filename, @"^[0-9_]"))
                subdir = "number";
            else
                subdir = filename.Substring(0, 1);

            return $"https://media.merriam-webster.com/audio/prons/en/us/mp3/{subdir}/{filename}.mp3";
        }

        /// <summary>
        /// Convert MWEntry → Meaning
        /// </summary>
        private static Meaning ConvertToMeaning(MWEntry entry)
        {
            Meaning meaning = new Meaning
            {
                partOfSpeech = entry.FunctionalLabel ?? "unknown",
                definitions = new List<Definition>(),
                synonyms = new List<string>(),
                antonyms = new List<string>()
            };

            // Dùng shortdef (định nghĩa đơn giản)
            if (entry.ShortDefinitions != null)
            {
                foreach (var def in entry.ShortDefinitions)
                {
                    meaning.definitions.Add(new Definition
                    {
                        definition = def,
                        synonyms = new List<string>(),
                        antonyms = new List<string>(),
                        example = ""
                    });
                }
            }

            return meaning;
        }

        /// <summary>
        /// Extract related words from thesaurus JSON
        /// </summary>
        private static void ExtractRelatedWords(JsonElement list, HashSet<string> output)
        {
            try
            {
                foreach (var group in list.EnumerateArray())
                {
                    foreach (var item in group.EnumerateArray())
                    {
                        if (item.TryGetProperty("wd", out var word))
                        {
                            output.Add(word.GetString());
                        }
                    }
                }
            }
            catch { }
        }
        #endregion
    }
    #endregion
}