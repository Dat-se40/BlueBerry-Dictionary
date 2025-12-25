using BlueBerryDictionary.ApiClient.Configuration;
using BlueBerryDictionary.Services.User;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BlueBerryDictionary.Services.Network
{
    /// <summary>
    /// Google OAuth Authentication Service (Singleton)
    /// ✅ Production-ready: Blocking logout + Optimized login
    /// </summary>
    public class GoogleAuthService
    {
        private static GoogleAuthService _instance;
        public static GoogleAuthService Instance => _instance ??= new GoogleAuthService();

        private UserCredential _credential;
        private UserInfo _currentUser;

        private int _loginRetryCount = 0;
        private const int MAX_LOGIN_RETRIES = 1;

        public string CurrentUserEmail => _currentUser?.Email;
        public string CurrentUserName => _currentUser?.Name;
        public string CurrentUserAvatar => _currentUser?.Avatar;
        public bool IsLoggedIn => _credential != null;
        public UserInfo CurrentUser => _currentUser;

        public event EventHandler<bool> LoginStateChanged;

        private GoogleAuthService() { }

        // ==================== LOGIN ====================

        /// <summary>
        /// Login bằng Gmail (OAuth 2.0)
        /// ✅ Optimized: Credential cleanup + Retry logic
        /// </summary>
        public async Task<LoginResult> LoginAsync()
        {
            Console.WriteLine("🔐 [GoogleAuthService] Starting login...");

            try
            {
                var config = Config.Instance;

                if (string.IsNullOrEmpty(config.GoogleClientId) ||
                    config.GoogleClientId == "YOUR_CLIENT_ID.apps.googleusercontent.com")
                {
                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = "Google Auth not configured"
                    };
                }

                var clientSecrets = new ClientSecrets
                {
                    ClientId = config.GoogleClientId,
                    ClientSecret = config.GoogleClientSecret
                };

                // ✅ TRY REUSE TOKEN
                var reuseResult = await TokenManager.Instance.TryReuseTokenAsync(
                    clientSecrets,
                    config.GoogleScopes
                );

                if (reuseResult.CanReuse)
                {
                    Console.WriteLine("✅ [GoogleAuthService] Reusing existing token");
                    _credential = reuseResult.Credential;
                }
                else
                {
                    Console.WriteLine($"⚠️ [GoogleAuthService] Cannot reuse token: {reuseResult.Reason}");
                    Console.WriteLine("🔄 [GoogleAuthService] Starting fresh OAuth flow...");

                    // ✅ CRITICAL: Complete credential cleanup
                    await TokenManager.Instance.ClearAllCredentialsAsync();

                    // ✅ Authorize
                    var credPath = TokenManager.Instance.GetCredentialPath();
                    var dataStore = new FileDataStore(credPath, true);

                    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = clientSecrets,
                        Scopes = config.GoogleScopes,
                        DataStore = dataStore
                    });

                    Console.WriteLine("🌐 [GoogleAuthService] Opening browser for authorization...");

                    var codeReceiver = new LocalServerCodeReceiver();
                    var authApp = new AuthorizationCodeInstalledApp(flow, codeReceiver);

                    _credential = await authApp.AuthorizeAsync("user", CancellationToken.None);

                    if (_credential == null || _credential.Token == null ||
                        string.IsNullOrEmpty(_credential.Token.AccessToken))
                    {
                        return new LoginResult
                        {
                            Success = false,
                            ErrorMessage = "Authorization failed or was cancelled"
                        };
                    }

                    Console.WriteLine("🟢 [GoogleAuthService] Authorization completed");
                }

                // ✅ GET USER INFO
                var userInfo = await GetUserInfoAsync();
                if (userInfo == null)
                {
                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to get user info"
                    };
                }

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

                Console.WriteLine($"✅ [GoogleAuthService] Login successful: {_currentUser.Email}");

                UserSessionManage.Instance.SetLoggedInUser(
                    userInfo.Id,
                    userInfo.Email,
                    userInfo.Name,
                    userInfo.Picture
                );

                UserSessionManage.Instance.SaveSession(_currentUser);

                // ✅ INITIALIZE DRIVE SYNC
                try
                {
                    await CloudSyncService.Instance.InitializeAsync(_credential);
                    var syncResult = await CloudSyncService.Instance.DownloadAllDataAsync();

                    // ✅ SAVE SCOPES ONLY AFTER DRIVE SUCCESS
                    try
                    {
                        await TokenManager.Instance.SaveScopesAsync(config.GoogleScopes);
                    }
                    catch (Exception scopeEx)
                    {
                        Console.WriteLine($"⚠️ [GoogleAuthService] Warning: Failed to save scopes: {scopeEx.Message}");
                    }

                    _loginRetryCount = 0;

                    UserSessionManage.Instance.AddLoginLog(new LoginRecord
                    {
                        Email = _currentUser.Email,
                        LoginTime = DateTime.UtcNow,
                        Device = Environment.MachineName,
                        Status = "success",
                        SyncedFiles = syncResult.Downloaded,
                        DownloadedWords = syncResult.Downloaded.Count
                    });

                    LoginStateChanged?.Invoke(this, true);

                    return new LoginResult
                    {
                        Success = true,
                        UserInfo = _currentUser,
                        SyncResult = syncResult
                    };
                }
                catch (Exception driveEx)
                {
                    Console.WriteLine($"⚠️ [GoogleAuthService] Drive sync failed: {driveEx.Message}");

                    // ✅ RETRY LOGIC: Detect scope issue + retry
                    if ((driveEx.Message.Contains("insufficient authentication scopes") ||
                         driveEx.Message.Contains("Forbidden")) &&
                        _loginRetryCount < MAX_LOGIN_RETRIES)
                    {
                        _loginRetryCount++;
                        Console.WriteLine($"🔄 [GoogleAuthService] Retry {_loginRetryCount}/{MAX_LOGIN_RETRIES}...");

                        // Clear & retry
                        await TokenManager.Instance.ClearAllCredentialsAsync();
                        return await LoginAsync();
                    }

                    _loginRetryCount = 0;

                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = "Drive access denied. Please check permissions and try again."
                    };
                }
            }
            catch (OperationCanceledException)
            {
                _loginRetryCount = 0;
                Console.WriteLine("⚠️ [GoogleAuthService] Login cancelled by user");
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "Login cancelled"
                };
            }
            catch (Exception ex)
            {
                _loginRetryCount = 0;
                Console.WriteLine($"❌ [GoogleAuthService] Login failed: {ex.Message}");

                await TokenManager.Instance.ClearAllCredentialsAsync();

                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        // ==================== SILENT LOGIN ====================

        /// <summary>
        /// Auto login using saved token
        /// </summary>
        public async Task<bool> TrySilentLoginAsync()
        {
            try
            {
                Console.WriteLine("🔄 [GoogleAuthService] Trying silent login...");

                var config = Config.Instance;

                if (string.IsNullOrEmpty(config.GoogleClientId))
                {
                    return false;
                }

                var clientSecrets = new ClientSecrets
                {
                    ClientId = config.GoogleClientId,
                    ClientSecret = config.GoogleClientSecret
                };

                var reuseResult = await TokenManager.Instance.TryReuseTokenAsync(
                    clientSecrets,
                    config.GoogleScopes
                );

                if (!reuseResult.CanReuse)
                {
                    Console.WriteLine($"⚠️ [GoogleAuthService] Silent login failed: {reuseResult.Reason}");
                    return false;
                }

                _credential = reuseResult.Credential;

                _currentUser = UserSessionManage.Instance.LoadSession();

                if (_currentUser == null)
                {
                    Console.WriteLine("⚠️ [GoogleAuthService] No saved session");
                    return false;
                }

                Console.WriteLine($"✅ [GoogleAuthService] Silent login successful: {_currentUser.Email}");

                UserSessionManage.Instance.SetLoggedInUser(
                    _currentUser.Email,
                    _currentUser.Email,
                    _currentUser.Name,
                    _currentUser.Avatar
                );

                await CloudSyncService.Instance.InitializeAsync(_credential);

                LoginStateChanged?.Invoke(this, true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [GoogleAuthService] Silent login error: {ex.Message}");

                await TokenManager.Instance.ClearAllCredentialsAsync();

                return false;
            }
        }

        // ==================== LOGOUT ====================

        /// <summary>
        /// ✅ PRODUCTION-READY: Instant UI + Complete cleanup
        /// - UI response: < 100ms
        /// - Cleanup: Blocking (no race condition)
        /// - Next login: Clean & fast
        /// </summary>
        public async Task LogoutAsync()
        {
            if (!IsLoggedIn) return;

            try
            {
                Console.WriteLine("🔓 [GoogleAuthService] Logging out...");

                // ✅ 1. Save references BEFORE clearing
                var credentialToRevoke = _credential;
                var currentUserEmail = _currentUser?.Email;

                // ✅ 2. Clear in-memory INSTANTLY
                _credential = null;
                _currentUser = null;

                // ✅ 3. Update session
                if (currentUserEmail != null)
                {
                    UserSessionManage.Instance.UpdateLogoutTime(currentUserEmail);
                }
                UserSessionManage.Instance.Clear();

                // ✅ 4. Notify UI (instant)
                LoginStateChanged?.Invoke(this, false);

                Console.WriteLine("✅ [GoogleAuthService] Logout UI updated (instant)");

                // ✅ 5. BLOCKING cleanup (non-UI-blocking)
                try
                {
                    // Revoke token (sync with timeout)
                    if (credentialToRevoke?.Token?.RefreshToken != null)
                    {
                        try
                        {
                            var revokeTask = credentialToRevoke.RevokeTokenAsync(CancellationToken.None);
                            revokeTask.Wait(TimeSpan.FromSeconds(2));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ [GoogleAuthService] Token revoke warning: {ex.Message}");
                        }
                    }
                }
                catch { }

                try
                {
                    // Final sync (sync with timeout)
                    var syncTask = CloudSyncService.Instance.UploadAllPendingAsync();
                    syncTask.Wait(TimeSpan.FromSeconds(3));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [GoogleAuthService] Final sync warning: {ex.Message}");
                }

                // ✅ 6. DELETE CREDENTIALS (BLOCKING - CRITICAL!)
                Console.WriteLine("🗑️ [GoogleAuthService] Clearing credentials...");
                await TokenManager.Instance.ClearAllCredentialsAsync();

                Console.WriteLine("✅ [GoogleAuthService] Logout completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [GoogleAuthService] Logout error: {ex.Message}");
                throw;
            }
        }

        // ==================== HELPERS ====================

        private async Task<Google.Apis.Oauth2.v2.Data.Userinfo> GetUserInfoAsync()
        {
            try
            {
                var oauth2Service = new Oauth2Service(new BaseClientService.Initializer
                {
                    HttpClientInitializer = _credential,
                    ApplicationName = Config.Instance.AppName
                });

                return await oauth2Service.Userinfo.Get().ExecuteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [GoogleAuthService] Get user info error: {ex.Message}");
                return null;
            }
        }

        public UserCredential GetCredential() => _credential;
    }
}