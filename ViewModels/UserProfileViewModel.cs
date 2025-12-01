using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace BlueBerryDictionary.ViewModels
{
    /// <summary>
    /// ViewModel cho UserProfilePage
    /// STUB VERSION - Commands chưa có logic thật
    /// </summary>
    public partial class UserProfileViewModel : ObservableObject
    {
        // ========== OBSERVABLE PROPERTIES ==========

        [ObservableProperty]
        private string userId = "google_123456789";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameCharCount))]
        private string displayName = "John Doe";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NicknameCharCount))]
        private string nickname = "Johnny";

        [ObservableProperty]
        private string email = "johndoe@gmail.com";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasAvatar))]
        private string avatarUrl;

        [ObservableProperty]
        private string syncStatusText = "Last synced: Never";

        [ObservableProperty]
        private bool isDownloading;

        [ObservableProperty]
        private bool isUploading;

        [ObservableProperty]
        private bool isSaving;

        // ========== COMPUTED PROPERTIES ==========

        public bool HasAvatar => !string.IsNullOrEmpty(AvatarUrl);
        public string DisplayNameCharCount => $"{DisplayName?.Length ?? 0} / 50";
        public string NicknameCharCount => $"{Nickname?.Length ?? 0} / 30";

        // ========== CONSTRUCTOR ==========

        public UserProfileViewModel()
        {
            // TODO: Inject services khi implement
            // LoadUserProfile();
        }

        // ========== DATA LOADING ==========

        /// <summary>
        /// [STUB] Load user profile từ UserDataManager
        /// TODO: Implement sau khi có UserDataManager
        /// </summary>
        private void LoadUserProfile()
        {
            // TODO: var userData = UserDataManager.Instance.GetUserProfile();
            // TODO: if (userData != null)
            // TODO: {
            // TODO:     UserId = userData.UserId;
            // TODO:     DisplayName = userData.DisplayName;
            // TODO:     Nickname = userData.Nickname;
            // TODO:     Email = userData.Email;
            // TODO:     AvatarUrl = userData.AvatarUrl;
            // TODO:     SyncStatusText = $"Last synced: {userData.LastSyncTime}";
            // TODO: }
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
        /// [STUB] Upload avatar
        /// TODO: Open file picker, validate, upload to Drive
        /// </summary>
        [RelayCommand]
        private async Task UploadAvatarAsync()
        {
            try
            {
                // TODO: var fileDialog = new OpenFileDialog
                // TODO: {
                // TODO:     Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                // TODO:     Title = "Select Avatar"
                // TODO: };
                // TODO:
                // TODO: if (fileDialog.ShowDialog() == true)
                // TODO: {
                // TODO:     // Validate file size (max 5MB)
                // TODO:     var fileInfo = new FileInfo(fileDialog.FileName);
                // TODO:     if (fileInfo.Length > 5 * 1024 * 1024)
                // TODO:     {
                // TODO:         MessageBox.Show("Image size must be less than 5MB");
                // TODO:         return;
                // TODO:     }
                // TODO:
                // TODO:     // Convert to base64 or upload to Drive
                // TODO:     AvatarUrl = await GoogleDriveSyncService.Instance.UploadAvatarAsync(fileDialog.FileName);
                // TODO:
                // TODO:     // Save to profile
                // TODO:     await SaveProfileAsync();
                // TODO: }

                await Task.Delay(500);
                System.Diagnostics.Debug.WriteLine("✅ Upload Avatar clicked (STUB)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Upload avatar error: {ex.Message}");
                MessageBox.Show($"Failed to upload avatar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// [STUB] Save profile changes
        /// TODO: Validate inputs, save to UserDataManager, upload to Drive
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
                // TODO: var profileData = new UserProfile
                // TODO: {
                // TODO:     UserId = UserId,
                // TODO:     DisplayName = DisplayName.Trim(),
                // TODO:     Nickname = Nickname?.Trim(),
                // TODO:     Email = Email,
                // TODO:     AvatarUrl = AvatarUrl
                // TODO: };
                // TODO:
                // TODO: // Save to local
                // TODO: UserDataManager.Instance.UpdateProfile(profileData);
                // TODO:
                // TODO: // Upload to Drive if logged in
                // TODO: if (GoogleAuthService.Instance.IsLoggedIn)
                // TODO: {
                // TODO:     await GoogleDriveSyncService.Instance.UploadProfileAsync(profileData);
                // TODO: }

                await Task.Delay(1000);
                UpdateSyncStatus();

                MessageBox.Show("Profile saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Debug.WriteLine("✅ Save Profile clicked (STUB)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Save profile error: {ex.Message}");
                MessageBox.Show($"Failed to save profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// [STUB] Download data from Google Drive
        /// TODO: Call GoogleDriveSyncService.DownloadAllDataAsync()
        /// </summary>
        [RelayCommand]
        private async Task DownloadFromDriveAsync()
        {
            IsDownloading = true;

            try
            {
                // TODO: var syncResult = await GoogleDriveSyncService.Instance.SyncAllDataAsync();
                // TODO:
                // TODO: // Reload UI
                // TODO: LoadUserProfile();
                // TODO:
                // TODO: // Show summary
                // TODO: var message = $"Download completed!\n\n" +
                // TODO:              $"📥 Downloaded: {syncResult.Downloaded.Count} files\n" +
                // TODO:              $"✅ In sync: {syncResult.InSync.Count} files";
                // TODO:
                // TODO: if (syncResult.HasConflicts)
                // TODO: {
                // TODO:     message += $"\n⚠️ Conflicts: {syncResult.Conflicts.Count} files";
                // TODO: }
                // TODO:
                // TODO: MessageBox.Show(message, "Sync Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                await Task.Delay(2000);
                UpdateSyncStatus();

                System.Diagnostics.Debug.WriteLine("✅ Download from Drive clicked (STUB)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Download error: {ex.Message}");
                MessageBox.Show($"Failed to download data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsDownloading = false;
            }
        }

        /// <summary>
        /// [STUB] Upload data to Google Drive
        /// TODO: Call GoogleDriveSyncService.UploadAllDataAsync()
        /// </summary>
        [RelayCommand]
        private async Task UploadToDriveAsync()
        {
            IsUploading = true;

            try
            {
                // TODO: var syncResult = await GoogleDriveSyncService.Instance.SyncAllDataAsync();
                // TODO:
                // TODO: var message = $"Upload completed!\n\n" +
                // TODO:              $"📤 Uploaded: {syncResult.Uploaded.Count} files\n" +
                // TODO:              $"✅ In sync: {syncResult.InSync.Count} files";
                // TODO:
                // TODO: MessageBox.Show(message, "Sync Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                await Task.Delay(2000);
                UpdateSyncStatus();

                System.Diagnostics.Debug.WriteLine("✅ Upload to Drive clicked (STUB)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Upload error: {ex.Message}");
                MessageBox.Show($"Failed to upload data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsUploading = false;
            }
        }

        /// <summary>
        /// [STUB] Logout
        /// TODO: Final sync, revoke token, redirect to login
        /// </summary>
        [RelayCommand]
        private async Task LogoutAsync()
        {
            // Show confirmation
            var result = MessageBox.Show(
                "Are you sure you want to logout?\n\nYour data will be synced before logging out.",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // TODO: // Final sync
                // TODO: await GoogleDriveSyncService.Instance.SyncAllDataAsync();
                // TODO:
                // TODO: // Revoke token
                // TODO: await GoogleAuthService.Instance.LogoutAsync();
                // TODO:
                // TODO: // Clear session
                // TODO: UserDataManager.Instance.ClearSession();
                // TODO:
                // TODO: // Navigate to login
                // TODO: NavigationService.NavigateTo("Login");

                await Task.Delay(500);

                MessageBox.Show("Logged out successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Debug.WriteLine("✅ Logout clicked (STUB)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Logout error: {ex.Message}");
                MessageBox.Show($"Failed to logout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
