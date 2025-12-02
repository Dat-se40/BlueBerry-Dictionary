using System;
using System.Collections.Generic;

namespace BlueBerryDictionary.Models
{
    /// <summary>
    /// Log đăng nhập (thêm vào LoginLog.json)
    /// </summary>
    public class LoginRecord
    {
        public string Email { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string Device { get; set; }
        public string Status { get; set; } // "success" | "failed"
        public List<string> SyncedFiles { get; set; } = new();
        public int DownloadedWords { get; set; }
        public int UploadedWords { get; set; }
    }
}
