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
            System.Diagnostics.Debug.WriteLine($"✅ Theme selected: {mode}");
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


        #endregion

        #region Help & About commands

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
        #endregion
    }
}
