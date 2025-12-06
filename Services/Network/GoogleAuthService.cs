using BlueBerryDictionary.ApiClient.Configuration;
using BlueBerryDictionary.Services.User;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BlueBerryDictionary.Services.Network
{
    /// <summary>
    /// Google OAuth Authentication Service (Singleton)
    /// </summary>
    public class GoogleAuthService
    {
        private static GoogleAuthService _instance;
        public static GoogleAuthService Instance => _instance ??= new GoogleAuthService();

        private UserCredential _credential;
        private UserInfo _currentUser; // ✅ THÊM: Lưu UserInfo đầy đủ

        // ✅ UPDATE: Properties từ _currentUser
        public string CurrentUserEmail => _currentUser?.Email;
        public string CurrentUserName => _currentUser?.Name;
        public string CurrentUserAvatar => _currentUser?.Avatar;
        public bool IsLoggedIn => _credential != null;
        public UserInfo CurrentUser => _currentUser; // ✅ THÊM

        // Event để notify UI
        public event EventHandler<bool> LoginStateChanged;

        private GoogleAuthService() { }

        // ==================== LOGIN ====================

        /// <summary>
        /// Login bằng Gmail (OAuth 2.0)
        /// </summary>
        public async Task<LoginResult> LoginAsync()
        {
            Console.WriteLine("🔐 Starting Google OAuth login...");

            try
            {
                var config = Config.Instance;
                Console.WriteLine($"🟢 Config loaded: ClientId={config.GoogleClientId?.Substring(0, 20)}...");

                // ✅ Validate config
                if (string.IsNullOrEmpty(config.GoogleClientId) ||
                    config.GoogleClientId == "YOUR_CLIENT_ID.apps.googleusercontent.com")
                {
                    Console.WriteLine("❌ Google OAuth not configured");
                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = "Google OAuth not configured. Please update appsettings.json"
                    };
                }

                var clientSecrets = new ClientSecrets
                {
                    ClientId = config.GoogleClientId,
                    ClientSecret = config.GoogleClientSecret
                };

                Console.WriteLine("🟢 Calling GoogleWebAuthorizationBroker.AuthorizeAsync()...");
                Console.WriteLine("🟢 Browser should open now...");

                // ✅ Authorize - Mở browser để login
                // 📁 Thư mục lưu token
                var credPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BlueBerryDictionary", "credentials");
                Directory.CreateDirectory(credPath);

                // 🔐 Ủy quyền OAuth
                _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    config.GoogleScopes,
                    "user",
                    CancellationToken.None,
                    new Google.Apis.Util.Store.FileDataStore(credPath, true)  // ✅ thêm dòng này
                );


                Console.WriteLine("🟢 Authorization completed, getting user info...");

                // ✅ Get user info
                var oauth2Service = new Oauth2Service(new BaseClientService.Initializer
                {
                    HttpClientInitializer = _credential,
                    ApplicationName = config.AppName
                });

                var userInfo = await oauth2Service.Userinfo.Get().ExecuteAsync();

                // ✅ Tạo UserInfo object
                _currentUser = new UserInfo
                {
                    Email = userInfo.Email,
                    Name = userInfo.Name,
                    Avatar = userInfo.Picture,
                    LastLogin = DateTime.UtcNow,
                    AccessToken = _credential.Token.AccessToken,
                    RefreshToken = _credential.Token.RefreshToken,
                    TokenExpiry = _credential.Token.IssuedUtc.AddSeconds(_credential.Token.ExpiresInSeconds ?? 3600)
                };

                Console.WriteLine($"✅ Login successful: {_currentUser.Email}");

                // ✅ Set logged-in user trong UserSessionManage
                UserSessionManage.Instance.SetLoggedInUser(
                    userInfo.Id,
                    userInfo.Email,
                    userInfo.Name,
                    userInfo.Picture
                );

                // ✅ Save session
                UserSessionManage.Instance.SaveSession(_currentUser);

                // ✅ Initialize Drive sync
                await CloudSyncService.Instance.InitializeAsync(_credential);

                // ✅ Download data từ Drive
                var syncResult = await CloudSyncService.Instance.DownloadAllDataAsync();

                Console.WriteLine($"📥 Downloaded: {syncResult.Downloaded.Count} files");

                // ✅ Add login log
                UserSessionManage.Instance.AddLoginLog(new LoginRecord
                {
                    Email = _currentUser.Email,
                    LoginTime = DateTime.UtcNow,
                    Device = Environment.MachineName,
                    Status = "success",
                    SyncedFiles = syncResult.Downloaded,
                    DownloadedWords = syncResult.Downloaded.Count
                });

                // Notify UI
                LoginStateChanged?.Invoke(this, true);

                return new LoginResult
                {
                    Success = true,
                    UserInfo = _currentUser,
                    SyncResult = syncResult
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login failed: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        // ==================== SILENT LOGIN ====================

        /// <summary>
        /// Thử đăng nhập tự động (dùng saved token)
        /// </summary>
        public async Task<bool> TrySilentLoginAsync()
        {
            try
            {
                Console.WriteLine("🔄 Trying silent login...");

                // Kiểm tra session
                if (!UserSessionManage.Instance.IsSessionValid())
                {
                    Console.WriteLine("⚠️ No valid session");
                    return false;
                }

                // Load session
                _currentUser = UserSessionManage.Instance.LoadSession();

                // Tạo lại credential từ saved token
                var tokenResponse = new TokenResponse
                {
                    AccessToken = _currentUser.AccessToken,
                    RefreshToken = _currentUser.RefreshToken,
                    ExpiresInSeconds = (long)(_currentUser.TokenExpiry - DateTime.UtcNow).TotalSeconds,
                    IssuedUtc = _currentUser.LastLogin
                };

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = Config.Instance.GoogleClientId,
                        ClientSecret = Config.Instance.GoogleClientSecret
                    }
                });

                _credential = new UserCredential(flow, "user", tokenResponse);

                Console.WriteLine($"✅ Silent login successful: {_currentUser.Email}");

                // Set logged-in user
                UserSessionManage.Instance.SetLoggedInUser(
                    _currentUser.Email,
                    _currentUser.Email,
                    _currentUser.Name,
                    _currentUser.Avatar
                );

                // Initialize Drive sync
                await CloudSyncService.Instance.InitializeAsync(_credential);

                // Notify UI
                LoginStateChanged?.Invoke(this, true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Silent login failed: {ex.Message}");
                return false;
            }
        }

        // ==================== LOGOUT ====================

        /// <summary>
        /// Logout và revoke token
        /// </summary>
        public async Task LogoutAsync()
        {
            if (!IsLoggedIn) return;

            try
            {
                Console.WriteLine("🔓 Logging out...");

                // ✅ Final sync
                await CloudSyncService.Instance.UploadAllPendingAsync();

                // Revoke token
                if (_credential.Token.RefreshToken != null)
                {
                    await _credential.RevokeTokenAsync(CancellationToken.None);
                }

                // ✅ Update logout time in log
                UserSessionManage.Instance.UpdateLogoutTime(_currentUser.Email);

                // ✅ Clear session (tự động chuyển về Guest)
                UserSessionManage.Instance.Clear();

                // Clear in-memory data
                _credential = null;
                _currentUser = null;

                // Notify UI
                LoginStateChanged?.Invoke(this, false);

                Console.WriteLine("✅ Logout successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Logout error: {ex.Message}");
                throw;
            }
        }

        // ==================== GET CREDENTIAL ====================

        public UserCredential GetCredential() => _credential;
    }
}
