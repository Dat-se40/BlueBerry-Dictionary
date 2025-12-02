using BlueBerryDictionary.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Apis.Drive.v3.Data;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueBerryDictionary.ViewModels
{
    /// <summary>
    /// ViewModel cho UserProfilePage
    /// </summary>
    public partial class UserProfileViewModel : ObservableObject
    {
        // ========== OBSERVABLE PROPERTIES ==========

        [ObservableProperty]
        private string _userId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameCharCount))]
        private string _displayName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NicknameCharCount))]
        private string _nickname;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasAvatar))]
        private string _avatarUrl;

        [ObservableProperty]
        private BitmapImage _avatarImage; // ✅ THÊM: để bind vào Image control

        [ObservableProperty]
        private string _syncStatusText = "Last synced: Never";

        [ObservableProperty]
        private bool _isDownloading;

        [ObservableProperty]
        private bool _isUploading;

        [ObservableProperty]
        private bool _isSaving;

        // ========== COMPUTED PROPERTIES ==========

        public bool HasAvatar => !string.IsNullOrEmpty(AvatarUrl);
        public string DisplayNameCharCount => $"{DisplayName?.Length ?? 0} / 50";
        public string NicknameCharCount => $"{Nickname?.Length ?? 0} / 30";

        // ========== CONSTRUCTOR ==========

        public UserProfileViewModel()
        {
            LoadUserProfile();
        }

        // ========== DATA LOADING ==========

        /// <summary>
        /// Load user profile từ GoogleAuthService & UserSessionManage
        /// </summary>
        private async void LoadUserProfile()
        {
            try
            {
                var session = UserSessionManage.Instance;
                var authService = GoogleAuthService.Instance;

                if (session.IsGuest)
                {
                    // Guest mode
                    UserId = "guest";
                    DisplayName = "Guest User";
                    Nickname = "";
                    Email = "Not logged in";
                    AvatarUrl = null;
                    SyncStatusText = "Login to sync data";
                }
                else
                {
                    // Logged in
                    UserId = session.UserId ?? session.Email;
                    DisplayName = session.DisplayName ?? "User";
                    Nickname = ""; // TODO: Load từ Settings.json nếu có
                    Email = session.Email;
                    AvatarUrl = session.AvatarUrl;

                    // Load avatar từ URL
                    if (!string.IsNullOrEmpty(AvatarUrl))
                    {
                        await LoadAvatarAsync(AvatarUrl);
                    }

                    // Update sync status
                    var lastSync = authService.CurrentUser?.LastLogin;
                    if (lastSync.HasValue)
                    {
                        SyncStatusText = $"Last synced: {lastSync.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
                    }
                }

                Console.WriteLine($"✅ Profile loaded: {DisplayName} ({Email})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Load profile error: {ex.Message}");
            }
        }

        /// <summary>
        /// Load avatar từ URL (Google profile picture)
        /// </summary>
        private async Task LoadAvatarAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(url);

                var bitmap = new BitmapImage();
                using var stream = new MemoryStream(imageBytes);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();

                AvatarImage = bitmap;
                Console.WriteLine("✅ Avatar loaded from URL");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Load avatar error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update sync status text
        /// </summary>
        private void UpdateSyncStatus()
        {
            SyncStatusText = $"Last synced: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        // ========== RELAY COMMANDS ==========

        /// <summary>
        /// Upload avatar (local file)
        /// </summary>
        [RelayCommand]
        private async Task UploadAvatarAsync()
        {
            try
            {
                var fileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                    Title = "Select Avatar"
                };

                if (fileDialog.ShowDialog() == true)
                {
                    // Validate file size (max 5MB)
                    var fileInfo = new FileInfo(fileDialog.FileName);
                    if (fileInfo.Length > 5 * 1024 * 1024)
                    {
                        MessageBox.Show("Image size must be less than 5MB", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Load image locally
                    var bitmap = new BitmapImage(new Uri(fileDialog.FileName));
                    AvatarImage = bitmap;
                    AvatarUrl = fileDialog.FileName; // Save local path (hoặc upload lên Drive)

                    // TODO: Upload to Drive
                    // await CloudSyncService.Instance.UploadAvatarAsync(fileDialog.FileName);

                    await SaveProfileAsync();
                    Console.WriteLine("✅ Avatar uploaded");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Upload avatar error: {ex.Message}");
                MessageBox.Show($"Failed to upload avatar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Save profile changes
        /// </summary>
        [RelayCommand]
        private async Task SaveProfileAsync()
        {
            // Validate
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                MessageBox.Show("Display name cannot be empty", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DisplayName.Length > 50)
            {
                MessageBox.Show("Display name cannot exceed 50 characters", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Nickname?.Length > 30)
            {
                MessageBox.Show("Nickname cannot exceed 30 characters", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsSaving = true;

            try
            {
                // TODO: Save to Settings.json
                // var settingsPath = UserDataManager.Instance.GetSettingsPath();
                // var settings = new { DisplayName, Nickname, AvatarUrl };
                // File.WriteAllText(settingsPath, JsonConvert.SerializeObject(settings));

                // TODO: Upload to Drive if logged in
                if (GoogleAuthService.Instance.IsLoggedIn)
                {
                    // await CloudSyncService.Instance.UploadFileAsync("Settings.json", settingsPath);
                }

                await Task.Delay(500); // Simulate save
                UpdateSyncStatus();

                MessageBox.Show("Profile saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Console.WriteLine("✅ Profile saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save profile error: {ex.Message}");
                MessageBox.Show($"Failed to save profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Download data from Google Drive
        /// </summary>
        [RelayCommand]
        private async Task DownloadFromDriveAsync()
        {
            if (!GoogleAuthService.Instance.IsLoggedIn)
            {
                MessageBox.Show("Please login to download data", "Not Logged In", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsDownloading = true;

            try
            {
                var syncResult = await CloudSyncService.Instance.DownloadAllDataAsync();

                // Reload UI
                LoadUserProfile();

                // Show summary
                var message = $"Download completed!\n\n" +
                             $"📥 Downloaded: {syncResult.Downloaded.Count} files\n" +
                             $"✅ In sync: {syncResult.InSync.Count} files";

                if (syncResult.HasConflicts)
                {
                    message += $"\n⚠️ Conflicts: {syncResult.Conflicts.Count} files";
                }

                if (syncResult.HasErrors)
                {
                    message += $"\n❌ Errors: {syncResult.Errors.Count}";
                }

                MessageBox.Show(message, "Sync Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateSyncStatus();
                Console.WriteLine("✅ Download completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Download error: {ex.Message}");
                MessageBox.Show($"Failed to download data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsDownloading = false;
            }
        }

        /// <summary>
        /// Upload data to Google Drive
        /// </summary>
        [RelayCommand]
        private async Task UploadToDriveAsync()
        {
            if (!GoogleAuthService.Instance.IsLoggedIn)
            {
                MessageBox.Show("Please login to upload data", "Not Logged In", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsUploading = true;
            
            try
            {
                UserDataManager.Instance.SaveEssentialFiles();
                await CloudSyncService.Instance.UploadAllPendingAsync();

                MessageBox.Show("Upload completed!", "Sync Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateSyncStatus();
                Console.WriteLine("✅ Upload completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Upload error: {ex.Message}");
                MessageBox.Show($"Failed to upload data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsUploading = false;
            }
        }

        /// <summary>
        /// Logout
        /// </summary>
        [RelayCommand]
        private async Task LogoutAsync()
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?\n\nYour data will be synced before logging out.",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes) return;

            try
            {
                // Final sync
                if (GoogleAuthService.Instance.IsLoggedIn)
                {
                    await CloudSyncService.Instance.UploadAllPendingAsync();
                }

                // Logout
                await GoogleAuthService.Instance.LogoutAsync();

                MessageBox.Show("Logged out successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Console.WriteLine("✅ Logout successful");

                // TODO: Navigate to LoginWindow
                // Close MainWindow and show LoginWindow
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Logout error: {ex.Message}");
                MessageBox.Show($"Failed to logout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
