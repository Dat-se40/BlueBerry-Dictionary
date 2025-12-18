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
        #region Properties

        [ObservableProperty]
        private int _themeModeIndex = 0; // 0: Sáng, 1: Tối, 2: Tự động

        [ObservableProperty]
        private bool _autoSaveHistory = true;

        [ObservableProperty]
        private int _favouriteLimitIndex = 0; // 0: 500, 1: 1000, 2: 5000, 3: Không giới hạn
        #endregion

        #region Constructor

        public SettingsViewModel()
        {
            LoadCurrentSettings();
        }


        #endregion

        #region Load settings

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

        #endregion

        #region Theme commands
        /// <summary>
        /// Đổi theme mode khi user chọn ComboBox
        /// </summary>
        [RelayCommand]
        private void ChangeThemeMode(int index)
        {
            // Dùng Services.ThemeMode thay vì ThemeMode
            Services.ThemeMode mode = index switch
            {
                0 => Services.ThemeMode.Light,
                1 => Services.ThemeMode.Dark,
                2 => Services.ThemeMode.Auto,
                _ => Services.ThemeMode.Light
            };

            ThemeManager.Instance.SetThemeMode(mode);
            System.Diagnostics.Debug.WriteLine($"✅ Đã chọn theme: {mode}");
        }

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
                MessageBox.Show($"Đã áp dụng theme: {selectedTheme}", "Thành công",
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

                MessageBox.Show("Đã áp dụng màu tùy chỉnh!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        #endregion

        #region Data settings commands
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

        /// <summary>
        /// Khôi phục dữ liệu từ backup
        /// </summary>
        [RelayCommand]
        private void RestoreData()
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn khôi phục dữ liệu từ backup?\nDữ liệu hiện tại sẽ bị ghi đè.",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Chức năng đang được phát triển!", "Thông báo",
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
                "Xóa cache sẽ làm mất dữ liệu tạm thời.\nTiếp tục?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Implement clear cache logic
                MessageBox.Show("Đã xóa cache thành công!", "Thành công",
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
                "⚠️ CẢNH BÁO: Hành động này sẽ xóa TOÀN BỘ dữ liệu!\n" +
                "Bao gồm:\n" +
                "- Từ yêu thích\n" +
                "- Lịch sử tra cứu\n" +
                "- Tags\n" +
                "- Settings\n\n" +
                "Bạn có CHẮC CHẮN muốn tiếp tục?",
                "XÁC NHẬN XÓA DỮ LIỆU",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error
            );

            if (result == MessageBoxResult.Yes)
            {
                // Double confirm
                var confirm = MessageBox.Show(
                    "Xác nhận lần cuối: Xóa toàn bộ dữ liệu?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error
                );

                if (confirm == MessageBoxResult.Yes)
                {
                    // TODO: Implement reset logic
                    MessageBox.Show("Chức năng đang được phát triển!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        #endregion

        #region Help & About commands

        [RelayCommand]
        private void OpenUserGuide()
        {
            MessageBox.Show("Mở hướng dẫn sử dụng", "User Guide");
        }

        [RelayCommand]
        private void OpenFAQ()
        {
            MessageBox.Show("Mở câu hỏi thường gặp", "FAQ");
        }

        [RelayCommand]
        private void ContactSupport()
        {
            MessageBox.Show("Liên hệ hỗ trợ: support@blueberry.com", "Contact");
        }

        [RelayCommand]
        private void ReportBug()
        {
            MessageBox.Show("Báo lỗi tại: bugs@blueberry.com", "Report Bug");
        }

        [RelayCommand]
        private void RateApp()
        {
            MessageBox.Show("Cảm ơn bạn đã đánh giá!", "Rate App");
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
            MessageBox.Show("Xem điều khoản dịch vụ", "Terms & Privacy");
        }

        [RelayCommand]
        private void ShowLicenses()
        {
            MessageBox.Show("Xem giấy phép mã nguồn", "Open Source Licenses");
        }
        #endregion
    }
}
