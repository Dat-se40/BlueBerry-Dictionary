using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueBerryDictionary.Models
{
    public class Tag
    {
        public string Id { get; set; } // Unique ID (guid)
        public string Name { get; set; } // Tên tag
        public string Icon { get; set; } // Emoji icon (🎯, 💼, 💬)
        public string Color { get; set; } // Màu hiển thị (#2D4ACC)
        public DateTime CreatedAt { get; set; }
        public List<string> RelatedWords { get; set; } // Danh sách word IDs

        public Tag()
        {
            Id = Guid.NewGuid().ToString();
            RelatedWords = new List<string>();
            CreatedAt = DateTime.Now;
        }

        // Helper methods
        public int WordCount => RelatedWords?.Count ?? 0;

        public void AddWord(string word)
        {
            if (!RelatedWords.Contains(word))
            {
                RelatedWords.Add(word);
            }
        }

        public void RemoveWord(string word)
        {
            RelatedWords.Remove(word);
        }
    }

}