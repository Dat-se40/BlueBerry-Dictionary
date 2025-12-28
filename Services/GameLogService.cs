using BlueBerryDictionary.Helpers;
using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlueBerryDictionary.Services
{
    /// <summary>
    /// Service quản lý lịch sử chơi game (Singleton)
    /// </summary>
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
            var dataDir = PathHelper.Combine(baseDir, @"..\..\..\Data\PersistentStorage\GameLogs");
            Directory.CreateDirectory(dataDir);
            _logPath = Path.Combine(dataDir, "GameLog.json");
            
            LoadLog();
        }

        #region Core Methods

        /// <summary>
        /// Thêm session mới
        /// </summary>
        public void AddSession(GameSession session)
        {
            _gameLog.Sessions.Add(session);
            _gameLog.TotalGamesPlayed++;
            _gameLog.TotalCardsStudied += session.TotalCards;
            SaveLog();
        }

        /// <summary>
        /// Lấy tất cả sessions (mới nhất trước)
        /// </summary>
        public List<GameSession> GetAllSessions()
        {
            return _gameLog.Sessions.OrderByDescending(s => s.StartTime).ToList();
        }

        /// <summary>
        /// Lấy N sessions gần nhất
        /// </summary>
        public List<GameSession> GetRecentSessions(int count = 10)
        {
            return _gameLog.Sessions
                .OrderByDescending(s => s.StartTime)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Xóa 1 session
        /// </summary>
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

        /// <summary>
        /// Xóa tất cả sessions
        /// </summary>
        public void ClearAllSessions()
        {
            _gameLog.Sessions.Clear();
            _gameLog.TotalGamesPlayed = 0;
            _gameLog.TotalCardsStudied = 0;
            SaveLog();
        }

        #endregion

        #region Statistics

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

        #endregion

        #region Load & Save

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
                
                // Auto sync to Drive
                _ = AutoSyncToDriveAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save game log error: {ex.Message}");
            }
        }

        private async Task AutoSyncToDriveAsync()
        {
            try
            {
                var cloudService = CloudSyncService.Instance;
                
                if (string.IsNullOrEmpty(cloudService._appFolderId))
                    return;

                await cloudService.UploadFileAsync("GameLog.json", _logPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ GameLog auto sync failed: {ex.Message}");
            }
        }

        #endregion
    }
}