using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BlueBerryDictionary.Models
{
    public class Quote
    {
        public string content = string.Empty;
        public string author = string.Empty;
        public string imageUrl = string.Empty;
        public List<ShortenWord> relativeWords;
        public Quote()
        {
            relativeWords = new List<ShortenWord>();
        }
    }
    public struct ShortenWord
    {
        public string word;
        public string phonetic;
        public string meaning;
        public string partOfSpeech;
    }
}
