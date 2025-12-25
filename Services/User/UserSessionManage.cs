using BlueBerryDictionary.Helpers;
using BlueBerryDictionary.Services.Network;
using Google.Apis.Oauth2.v2.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlueBerryDictionary.Services.User
{
    public class UserSessionManage
    {
        private static UserSessionManage _instance;
        public static UserSessionManage Instance => _instance ??= new UserSessionManage();

        private readonly string _sessionPath;
        private readonly string _loginLogPath;

        // ========== PROPERTIES ==========

        public bool IsGuest { get; private set; } = true;
        public string UserId { get; private set; }
        public string Email { get; private set; }
        public string DisplayName { get; private set; }
        public string AvatarUrl { get; private set; }

        // ========== EVENTS ==========

        public event EventHandler<bool> LoginStateChanged;

        // ========== CONSTRUCTOR ==========

        private UserSessionManage()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var systemDir = PathHelper.Combine(baseDir, @"..\..\..\Data\System");
            Directory.CreateDirectory(systemDir);

            _sessionPath = PathHelper.Combine(systemDir, "CurrentUser.json");
            _loginLogPath = PathHelper.Combine(systemDir, "LoginLog.json");
        }

        // ========== SET MODES ==========

        public void SetGuestMode()
        {
            IsGuest = true;
            UserId = "guest";
            Email = null;
            DisplayName = "Guest";
            AvatarUrl = null;

            UserDataManager.Instance.SetCurrentUser("guest");

            System.Diagnostics.Debug.WriteLine("✅ UserSession: Guest mode activated");
            LoginStateChanged?.Invoke(this, false);
        }

        public void SetLoggedInUser(string userId, string email, string displayName, string avatarUrl = null)
        {
            IsGuest = false;
            UserId = userId;
            Email = email;
            DisplayName = displayName;
            AvatarUrl = avatarUrl;

            UserDataManager.Instance.SetCurrentUser(email);

            System.Diagnostics.Debug.WriteLine($"✅ UserSession: Logged in as {email}");
            LoginStateChanged?.Invoke(this, true);
        }

        public void Clear()
        {
            SetGuestMode();
            ClearSession();
        }

        // ========== SAVE/LOAD SESSION ==========

        /// <summary>
        /// Save session (gọi sau khi login)
        /// </summary>
        public void SaveSession(UserInfo userInfo)
        {
            try
            {
                var json = JsonConvert.SerializeObject(userInfo, Formatting.Indented);
                File.WriteAllText(_sessionPath, json);
                Console.WriteLine($"✅ Session saved: {userInfo.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save session error: {ex.Message}");
            }
        }

        /// <summary>
        /// Load session (để silent login)
        /// </summary>
        public UserInfo LoadSession()
        {
            if (!File.Exists(_sessionPath))
                return null;

            try
            {
                var json = File.ReadAllText(_sessionPath);
                return JsonConvert.DeserializeObject<UserInfo>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Load session error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra session có valid không
        /// </summary>
        public bool IsSessionValid()
        {
            var session = LoadSession();
            if (session == null) return false;

            // Check token expiry
            if (session.TokenExpiry < DateTime.UtcNow)
            {
                Console.WriteLine("⚠️ Token expired");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Clear session (logout)
        /// </summary>
        public void ClearSession()
        {
            if (File.Exists(_sessionPath))
            {
                File.Delete(_sessionPath);
                Console.WriteLine("✅ Session cleared");
            }
        }

        // ========== LOGIN LOG ==========

        /// <summary>
        /// Add login record
        /// </summary>
        public void AddLoginLog(LoginRecord record)
        {
            try
            {
                var logs = LoadLoginLogs();
                logs.Add(record);

                // Keep only last 50 logs
                if (logs.Count > 50)
                    logs = logs.Skip(logs.Count - 50).ToList();

                var json = JsonConvert.SerializeObject(logs, Formatting.Indented);
                File.WriteAllText(_loginLogPath, json);

                Console.WriteLine($"✅ Login log added: {record.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Add login log error: {ex.Message}");
            }
        }

        /// <summary>
        /// Load login logs
        /// </summary>
        public List<LoginRecord> LoadLoginLogs()
        {
            if (!File.Exists(_loginLogPath))
                return new List<LoginRecord>();

            try
            {
                var json = File.ReadAllText(_loginLogPath);
                return JsonConvert.DeserializeObject<List<LoginRecord>>(json)
                       ?? new List<LoginRecord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Load login logs error: {ex.Message}");
                return new List<LoginRecord>();
            }
        }

        /// <summary>
        /// Update logout time cho login record hiện tại
        /// </summary>
        public void UpdateLogoutTime(string email)
        {
            try
            {
                var logs = LoadLoginLogs();
                var lastLogin = logs.LastOrDefault(l => l.Email == email && l.LogoutTime == null);

                if (lastLogin != null)
                {
                    lastLogin.LogoutTime = DateTime.UtcNow;

                    var json = JsonConvert.SerializeObject(logs, Formatting.Indented);
                    File.WriteAllText(_loginLogPath, json);

                    Console.WriteLine($"✅ Logout time updated: {email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Update logout time error: {ex.Message}");
            }
        }
    }
}
