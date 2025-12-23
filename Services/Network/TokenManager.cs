using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlueBerryDictionary.Services.Network
{
    /// <summary>
    /// Token Management (Singleton)
    /// ✅ Production-ready: Complete scope validation + Fast cleanup
    /// </summary>
    public class TokenManager
    {
        private static TokenManager _instance;
        public static TokenManager Instance => _instance ??= new TokenManager();

        private readonly string _credentialPath;
        private readonly string _scopesFilePath;
        private readonly string _tokenFilePath;

        private DateTime? _lastVerifyTime;
        private bool _lastVerifyResult;
        private const int VERIFY_CACHE_MINUTES = 5;

        private TokenManager()
        {
            _credentialPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BlueBerryDictionary", "credentials");
            Directory.CreateDirectory(_credentialPath);
            Console.WriteLine(_credentialPath);
            _scopesFilePath = Path.Combine(_credentialPath, "scopes.json");
            _tokenFilePath = Path.Combine(_credentialPath, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user");
        }

        // ==================== PUBLIC API ====================

        /// <summary>
        /// Check if can reuse existing token
        /// </summary>
        public async Task<TokenReuseResult> TryReuseTokenAsync(
            ClientSecrets clientSecrets,
            string[] requiredScopes)
        {
            try
            {
                if (!File.Exists(_tokenFilePath))
                {
                    return TokenReuseResult.Fail("Token file not found");
                }

                Console.WriteLine("🔍 [TokenManager] Checking existing token...");

                var tokenResponse = await LoadTokenResponseAsync();
                if (tokenResponse == null)
                {
                    return TokenReuseResult.Fail("Failed to load token");
                }

                // ✅ Check scope changes (fail fast)
                if (!await ValidateScopesAsync(requiredScopes))
                {
                    Console.WriteLine("⚠️ [TokenManager] Scopes changed → Clearing all tokens");
                    await ClearAllCredentialsAsync();
                    return TokenReuseResult.Fail("Required scopes changed - token cleared");
                }

                // ✅ Check expiry & refresh
                var validToken = await EnsureTokenValidAsync(clientSecrets, tokenResponse);
                if (validToken == null)
                {
                    Console.WriteLine("⚠️ [TokenManager] Token refresh failed");
                    await ClearAllCredentialsAsync();
                    return TokenReuseResult.Fail("Token expired and refresh failed");
                }

                // ✅ Verify token with Google
                if (!await VerifyTokenWithGoogleAsync(validToken.AccessToken, requiredScopes))
                {
                    Console.WriteLine("⚠️ [TokenManager] Token missing scopes");
                    await ClearAllCredentialsAsync();
                    return TokenReuseResult.Fail("Token missing required scopes");
                }

                // ✅ Create credential
                var credential = CreateCredential(clientSecrets, requiredScopes, validToken);
                if (credential == null)
                {
                    await ClearAllCredentialsAsync();
                    return TokenReuseResult.Fail("Failed to create credential");
                }

                Console.WriteLine("✅ [TokenManager] Token validation passed");
                return TokenReuseResult.Success(credential);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TokenManager] Token reuse failed: {ex.Message}");
                await ClearAllCredentialsAsync();
                return TokenReuseResult.Fail($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Save scopes ONLY after successful Drive auth
        /// </summary>
        public async Task SaveScopesAsync(string[] scopes)
        {
            try
            {
                if (scopes == null || scopes.Length == 0)
                {
                    return;
                }

                var metadata = new ScopeMetadata
                {
                    Scopes = scopes,
                    SavedAt = DateTime.UtcNow
                };

                var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
                await File.WriteAllTextAsync(_scopesFilePath, json);

                Console.WriteLine("✅ [TokenManager] Scopes saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TokenManager] Save scopes error: {ex.Message}");
            }
        }

        public string GetCredentialPath() => _credentialPath;

        // ==================== CLEAR METHODS ====================

        /// <summary>
        /// ✅ COMPLETE cleanup: token + scopes + folder
        /// Async version for login
        /// </summary>
        public async Task ClearAllCredentialsAsync()
        {
            try
            {
                // 1. Clear in-memory cache
                _lastVerifyTime = null;
                _lastVerifyResult = false;

                // 2. Delete token file
                if (File.Exists(_tokenFilePath))
                {
                    File.Delete(_tokenFilePath);
                    Console.WriteLine("✅ [TokenManager] Token file deleted");
                }

                // 3. Delete scopes file
                if (File.Exists(_scopesFilePath))
                {
                    File.Delete(_scopesFilePath);
                    Console.WriteLine("✅ [TokenManager] Scopes metadata deleted");
                }

                // 4. Delete DataStore
                try
                {
                    var dataStore = new FileDataStore(_credentialPath, true);
                    await dataStore.DeleteAsync<TokenResponse>("user");
                }
                catch { }

                // 5. Delete entire credential folder
                if (Directory.Exists(_credentialPath))
                {
                    try
                    {
                        Directory.Delete(_credentialPath, true);
                        Console.WriteLine("✅ [TokenManager] Credential folder deleted");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ [TokenManager] Folder delete warning: {ex.Message}");
                    }
                }

                // 6. Recreate empty folder for next login
                Directory.CreateDirectory(_credentialPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TokenManager] Clear error: {ex.Message}");
            }
        }

        // ==================== PRIVATE METHODS ====================

        private async Task<TokenResponse> LoadTokenResponseAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_tokenFilePath);
                return JsonConvert.DeserializeObject<TokenResponse>(json);
            }
            catch
            {
                return null;
            }
        }

        private async Task<TokenResponse> EnsureTokenValidAsync(
            ClientSecrets clientSecrets,
            TokenResponse tokenResponse)
        {
            var expiryTime = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);

            if (expiryTime > DateTime.UtcNow.AddMinutes(5))
            {
                Console.WriteLine($"✅ [TokenManager] Token valid until {expiryTime:HH:mm:ss}");
                return tokenResponse;
            }

            Console.WriteLine("⏰ [TokenManager] Token expired, refreshing...");
            return await RefreshTokenAsync(clientSecrets, tokenResponse);
        }

        private async Task<TokenResponse> RefreshTokenAsync(
            ClientSecrets clientSecrets,
            TokenResponse oldToken)
        {
            try
            {
                if (string.IsNullOrEmpty(oldToken.RefreshToken))
                {
                    return null;
                }

                using var httpClient = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientSecrets.ClientId),
                    new KeyValuePair<string, string>("client_secret", clientSecrets.ClientSecret),
                    new KeyValuePair<string, string>("refresh_token", oldToken.RefreshToken),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                });

                var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var newToken = JsonConvert.DeserializeObject<TokenResponse>(json);

                newToken.RefreshToken ??= oldToken.RefreshToken;
                newToken.IssuedUtc = DateTime.UtcNow;

                await File.WriteAllTextAsync(_tokenFilePath, JsonConvert.SerializeObject(newToken));

                Console.WriteLine("✅ [TokenManager] Token refreshed");
                return newToken;
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> ValidateScopesAsync(string[] requiredScopes)
        {
            try
            {
                if (!File.Exists(_scopesFilePath))
                {
                    return false;
                }

                var json = await File.ReadAllTextAsync(_scopesFilePath);
                var metadata = JsonConvert.DeserializeObject<ScopeMetadata>(json);

                if (metadata?.Scopes == null)
                {
                    return false;
                }

                var savedScopes = metadata.Scopes.OrderBy(s => s).ToArray();
                var currentScopes = requiredScopes.OrderBy(s => s).ToArray();

                return savedScopes.SequenceEqual(currentScopes);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ✅ Verify token scopes with cache to avoid rate limit
        /// </summary>
        private async Task<bool> VerifyTokenWithGoogleAsync(string accessToken, string[] requiredScopes)
        {
            try
            {
                // ✅ Cache for 5 minutes
                if (_lastVerifyTime.HasValue &&
                    (DateTime.UtcNow - _lastVerifyTime.Value).TotalMinutes < VERIFY_CACHE_MINUTES)
                {
                    Console.WriteLine("✅ [TokenManager] Using cached verification");
                    return _lastVerifyResult;
                }

                Console.WriteLine("🔍 [TokenManager] Verifying token scopes...");

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(
                    $"https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={accessToken}"
                );

                if (!response.IsSuccessStatusCode)
                {
                    _lastVerifyResult = false;
                    _lastVerifyTime = DateTime.UtcNow;
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonConvert.DeserializeObject<TokenInfo>(json);

                if (string.IsNullOrEmpty(tokenInfo?.Scope))
                {
                    _lastVerifyResult = false;
                    _lastVerifyTime = DateTime.UtcNow;
                    return false;
                }

                var grantedScopes = tokenInfo.Scope.Split(' ');

                foreach (var required in requiredScopes)
                {
                    if (!grantedScopes.Contains(required))
                    {
                        _lastVerifyResult = false;
                        _lastVerifyTime = DateTime.UtcNow;
                        return false;
                    }
                }

                Console.WriteLine("✅ [TokenManager] Scopes verified");
                _lastVerifyResult = true;
                _lastVerifyTime = DateTime.UtcNow;
                return true;
            }
            catch
            {
                _lastVerifyResult = false;
                _lastVerifyTime = DateTime.UtcNow;
                return false;
            }
        }

        private UserCredential CreateCredential(
            ClientSecrets clientSecrets,
            string[] scopes,
            TokenResponse tokenResponse)
        {
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return null;
            }

            try
            {
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = scopes,
                    DataStore = new FileDataStore(_credentialPath, true)
                });

                return new UserCredential(flow, "user", tokenResponse);
            }
            catch
            {
                return null;
            }
        }

        // ==================== MODELS ====================

        private class TokenInfo
        {
            public string Scope { get; set; }
        }

        private class ScopeMetadata
        {
            public string[] Scopes { get; set; }
            public DateTime SavedAt { get; set; }
        }
    }

    public class TokenReuseResult
    {
        public bool CanReuse { get; private set; }
        public string Reason { get; private set; }
        public UserCredential Credential { get; private set; }

        private TokenReuseResult() { }

        public static TokenReuseResult Success(UserCredential credential)
        {
            return new TokenReuseResult
            {
                CanReuse = true,
                Credential = credential
            };
        }

        public static TokenReuseResult Fail(string reason)
        {
            return new TokenReuseResult
            {
                CanReuse = false,
                Reason = reason
            };
        }
    }
}