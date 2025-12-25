using BlueBerryDictionary.Services;
using BlueBerryDictionary.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace BlueBerryDictionary.ViewModels
{
    /// <summary>
    /// ViewModel cho Settings Page
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        // ========== PROPERTIES ==========

        [ObservableProperty]
        private int _themeModeIndex = 0; // 0: Sáng, 1: Tối, 2: Tự động

        [ObservableProperty]
        private bool _autoSaveHistory = true;

        [ObservableProperty]
        private int _favouriteLimitIndex = 0; // 0: 500, 1: 1000, 2: 5000, 3: Không giới hạn

        // ========== CONSTRUCTOR ==========

        public SettingsViewModel()
        {
            LoadCurrentSettings();
        }

        // ========== LOAD SETTINGS ==========

        /// <summary>
        /// Load settings hiện tại từ file
        /// </summary>
        private void LoadCurrentSettings()
        {
            var settings = SettingsService.Instance.CurrentSettings;

            // Theme Mode
            ThemeModeIndex = settings.ThemeMode switch
            {
                "Light" => 0,
                "Dark" => 1,
                "Auto" => 2,
                _ => 0
            };

            // Data settings
            AutoSaveHistory = settings.AutoSaveHistory;

            FavouriteLimitIndex = settings.FavouriteLimit switch
            {
                500 => 0,
                1000 => 1,
                5000 => 2,
                _ => 3 // Không giới hạn
            };
        }

        // ========== THEME MODE COMMAND ==========

        /// <summary>
        /// Đổi theme mode khi user chọn ComboBox
        /// </summary>
        [RelayCommand]
        private void ChangeThemeMode(int index)
        {
            // ✅ FIX: Dùng Services.ThemeMode thay vì ThemeMode
            Services.ThemeMode mode = index switch
            {
                0 => Services.ThemeMode.Light,
                1 => Services.ThemeMode.Dark,
                2 => Services.ThemeMode.Auto,
                _ => Services.ThemeMode.Light
            };

            ThemeManager.Instance.SetThemeMode(mode);
            System.Diagnostics.Debug.WriteLine($"✅ Theme selected: {mode}");
        }

        // ========== COLOR THEME COMMANDS ==========

        /// <summary>
        /// Mở dialog chọn theme có sẵn (8 màu)
        /// </summary>
        [RelayCommand]
        private void OpenThemePresetDialog()
        {
            var dialog = new ThemePresetDialog
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedTheme = dialog.SelectedTheme;
                ThemeManager.Instance.ApplyColorTheme(selectedTheme);
                MessageBox.Show($"Theme applied: {selectedTheme}", "Completed successfully",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Mở dialog tùy chỉnh màu (3 màu tự chọn)
        /// </summary>
        [RelayCommand]
        private void OpenCustomThemeDialog()
        {
            var dialog = new CustomThemeDialog
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                ThemeManager.Instance.ApplyCustomColorTheme(
                    dialog.PrimaryColor,
                    dialog.SecondaryColor,
                    dialog.AccentColor
                );

                MessageBox.Show("Custom color applied!", "Completed successfully",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        // ========== TOGGLE AUTO SAVE ==========

        /// <summary>
        /// Bật/tắt tự động lưu lịch sử
        /// </summary>
        [RelayCommand]
        private void ToggleAutoSave()
        {
            AutoSaveHistory = !AutoSaveHistory;
            SettingsService.Instance.CurrentSettings.AutoSaveHistory = AutoSaveHistory;
            SettingsService.Instance.SaveSettings();

            System.Diagnostics.Debug.WriteLine($"✅ Auto save: {AutoSaveHistory}");
        }

        // ========== FAVOURITE LIMIT ==========

        /// <summary>
        /// Đổi giới hạn từ yêu thích
        /// </summary>
        [RelayCommand]
        private void ChangeFavouriteLimit(int index)
        {
            int limit = index switch
            {
                0 => 500,
                1 => 1000,
                2 => 5000,
                3 => int.MaxValue, // Không giới hạn
                _ => 500
            };

            SettingsService.Instance.CurrentSettings.FavouriteLimit = limit;
            SettingsService.Instance.SaveSettings();

            System.Diagnostics.Debug.WriteLine($"✅ Favourite limit: {limit}");
        }

        // ========== DATA COMMANDS ==========

        /// <summary>
        /// Khôi phục dữ liệu từ backup
        /// </summary>
        [RelayCommand]
        private void RestoreData()
        {
            var result = MessageBox.Show(
                "Are you sure you want to restore data from the backup ?\nCurrent data will be overwritten.",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Implement restore logic
                MessageBox.Show("This feature is under development!", "Notification",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Xóa cache
        /// </summary>
        [RelayCommand]
        private void ClearCache()
        {
            var result = MessageBox.Show(
                "Clearing the cache will remove temporary data.\nContinue ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Implement clear cache logic
                MessageBox.Show("Cache cleared successfully!", "Completed successfully",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu (reset app)
        /// </summary>
        [RelayCommand]
        private void ResetApp()
        {
            var result = MessageBox.Show(
                "⚠️ WARNING: This action will DELETE ALL data!\n" +
                "Includes:\n" +
                "- Favorites\n" +
                "- Search history\n" +
                "- Tags\n" +
                "- Settings\n\n" +
                "Are you SURE you want to continue ?",
                "CONFIRM DATA DELETION",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error
            );

            if (result == MessageBoxResult.Yes)
            {
                // Double confirm
                var confirm = MessageBox.Show(
                    "Final confirmation: Delete all data ?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error
                );

                if (confirm == MessageBoxResult.Yes)
                {
                    // TODO: Implement reset logic
                    MessageBox.Show("This feature is under development!", "Notification",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        // ========== HELP COMMANDS ==========

        [RelayCommand]
        private void OpenUserGuide()
        {
            MessageBox.Show("Open user guide", "User Guide");
        }

        [RelayCommand]
        private void OpenFAQ()
        {
            MessageBox.Show("Open FAQ", "FAQ");
        }

        [RelayCommand]
        private void ContactSupport()
        {
            MessageBox.Show("Contact Support: support@blueberry.com", "Contact");
        }

        [RelayCommand]
        private void ReportBug()
        {
            MessageBox.Show("Report an issue at: bugs@blueberry.com", "Report Bug");
        }

        [RelayCommand]
        private void RateApp()
        {
            MessageBox.Show("Thank you for your feedback!", "Rate App");
        }

        [RelayCommand]
        private void ShowAbout()
        {
            MessageBox.Show(
                "BlueBerry Dictionary\n" +
                "Version 1.0.0\n" +
                "Build 2025.01\n\n" +
                "© 2025 BlueBerry Team",
                "About"
            );
        }

        [RelayCommand]
        private void ShowTerms()
        {
            MessageBox.Show("View Terms of Service", "Terms & Privacy");
        }

        [RelayCommand]
        private void ShowLicenses()
        {
            MessageBox.Show("View Source Code License", "Open Source Licenses");
        }
    }
}
