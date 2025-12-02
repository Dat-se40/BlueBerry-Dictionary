using System;

namespace BlueBerryDictionary.Models
{
    /// <summary>
    /// Thông tin user hiện tại (lưu trong CurrentUserInfo.json)
    /// </summary>
    public class UserInfo
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public DateTime LastLogin { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiry { get; set; }
        public string DeviceId { get; set; }
        public string AppVersion { get; set; }

        public UserInfo()
        {
            DeviceId = Environment.MachineName;
            AppVersion = "1.0.0";
        }
    }
}
