using System.Collections.Generic;

namespace BlueBerryDictionary.Models
{
    /// <summary>
    /// Kết quả login
    /// </summary>
    public class LoginResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public UserInfo UserInfo { get; set; }
        public SyncResult SyncResult { get; set; }
    }
}
