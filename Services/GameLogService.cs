using BlueBerryDictionary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlueBerryDictionary.Services
{
    public class GameLogService
    {
        private static GameLogService _instance;
        private static readonly object _lock = new object();
        
        private GameLog _gameLog;
        private readonly string _logPath;
        
        public static GameLogService Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new GameLogService();
                }
            }
        }
        
        private GameLogService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, @"..\..\..\Data\PersistentStorage\GameLogs");
            Directory.CreateDirectory(dataDir);
            _logPath = Path.Combine(dataDir, "GameHistory.json");
            
            LoadLog();
        }
        
        // ========== LOAD & SAVE ==========
        
        private void LoadLog()
        {
            try
            {
                if (File.Exists(_logPath))
                {
                    var json = File.ReadAllText(_logPath);
                    _gameLog = JsonConvert.DeserializeObject<GameLog>(json) ?? new GameLog();
                }
                else
                {
                    _gameLog = new GameLog();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Load game log error: {ex.Message}");
                _gameLog = new GameLog();
            }
        }
        
        private void SaveLog()
        {
            try
            {
                _gameLog.LastUpdated = DateTime.Now;
                var json = JsonConvert.SerializeObject(_gameLog, Formatting.Indented);
                File.WriteAllText(_logPath, json);
                Console.WriteLine($"✅ Game log saved: {_gameLog.Sessions.Count} sessions");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save game log error: {ex.Message}");
            }
        }
        
        // ========== ADD SESSION ==========
        
        public void AddSession(GameSession session)
        {
            _gameLog.Sessions.Add(session);
            _gameLog.TotalGamesPlayed++;
            _gameLog.TotalCardsStudied += session.TotalCards;
            SaveLog();
        }
        
        // ========== GET STATISTICS ==========
        
        public List<GameSession> GetAllSessions()
        {
            return _gameLog.Sessions.OrderByDescending(s => s.StartTime).ToList();
        }
        
        public List<GameSession> GetRecentSessions(int count = 10)
        {
            return _gameLog.Sessions
                .OrderByDescending(s => s.StartTime)
                .Take(count)
                .ToList();
        }
        
        public List<GameSession> GetSessionsByDate(DateTime date)
        {
            return _gameLog.Sessions
                .Where(s => s.StartTime.Date == date.Date)
                .OrderByDescending(s => s.StartTime)
                .ToList();
        }
        
        public List<GameSession> GetSessionsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _gameLog.Sessions
                .Where(s => s.StartTime.Date >= startDate.Date && s.StartTime.Date <= endDate.Date)
                .OrderByDescending(s => s.StartTime)
                .ToList();
        }
        
        public int GetTotalGamesPlayed() => _gameLog.TotalGamesPlayed;
        
        public int GetTotalCardsStudied() => _gameLog.TotalCardsStudied;
        
        public double GetAverageAccuracy()
        {
            if (_gameLog.Sessions.Count == 0) return 0;
            return _gameLog.Sessions.Average(s => s.AccuracyPercentage);
        }
        
        public TimeSpan GetTotalStudyTime()
        {
            return TimeSpan.FromSeconds(_gameLog.Sessions.Sum(s => s.Duration.TotalSeconds));
        }
        
        public Dictionary<string, int> GetDataSourceDistribution()
        {
            return _gameLog.Sessions
                .GroupBy(s => s.DataSourceName)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        
        // ========== DELETE ==========
        
        public void DeleteSession(string sessionId)
        {
            var session = _gameLog.Sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                _gameLog.Sessions.Remove(session);
                _gameLog.TotalGamesPlayed--;
                _gameLog.TotalCardsStudied -= session.TotalCards;
                SaveLog();
            }
        }
        
        public void ClearAllSessions()
        {
            _gameLog.Sessions.Clear();
            _gameLog.TotalGamesPlayed = 0;
            _gameLog.TotalCardsStudied = 0;
            SaveLog();
        }
    }
}