using System;
using System.Collections.Generic;

namespace BlueBerryDictionary.Models
{
    public class GameSession
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string DataSource { get; set; }
        public string DataSourceName { get; set; }
        public int TotalCards { get; set; }
        public int KnownCards { get; set; }
        public int UnknownCards { get; set; }
        public double AccuracyPercentage { get; set; }
        public TimeSpan Duration { get; set; }
        public List<int> SkippedCardIndices { get; set; }
        public List<string> SkippedWords { get; set; }
    
        // Property này để binding
        public string DurationText
        {
            get
            {
                if (Duration.TotalHours >= 1)
                    return $"{(int)Duration.TotalHours}h {Duration.Minutes}m";
                else if (Duration.TotalMinutes >= 1)
                    return $"{Duration.Minutes}m {Duration.Seconds}s";
                else
                    return $"{Duration.Seconds}s";
            }
        }
    
        public GameSession()
        {
            Id = Guid.NewGuid().ToString();
            SkippedCardIndices = new List<int>();
            SkippedWords = new List<string>();
        }
    }
    
    public class GameLog
    {
        public List<GameSession> Sessions { get; set; }
        public DateTime LastUpdated { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int TotalCardsStudied { get; set; }
        
        public GameLog()
        {
            Sessions = new List<GameSession>();
            LastUpdated = DateTime.Now;
            TotalGamesPlayed = 0;
            TotalCardsStudied = 0;
        }
    }
}