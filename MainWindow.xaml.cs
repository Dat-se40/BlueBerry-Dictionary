

using BlueBerryDictionary.Views.Pages;
using BlueBerryDictionary.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BlueBerryDictionary
{
    public partial class MainWindow : Window
    {
        private bool isDarkMode = false;
        private bool isSidebarOpen = false;

        // Navigation history
        private Stack<Page> backStack = new Stack<Page>();
        private Stack<Page> forwardStack = new Stack<Page>();

        public MainWindow()
        {
            InitializeComponent();

            // Load HomePage mặc định
            MainFrame.Navigate(new HomePage());
        }

        // ============ HAMBURGER BUTTON ============
        private void HamburgerBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleSidebar();
        }

        private void ToggleSidebar()
        {
            isSidebarOpen = !isSidebarOpen;

            var transform = Sidebar.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                Sidebar.RenderTransform = transform;
            }

            DoubleAnimation animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            if (isSidebarOpen)
            {
                animation.To = 0;
                Overlay.Visibility = Visibility.Visible;
            }
            else
            {
                animation.To = -280;
                Overlay.Visibility = Visibility.Collapsed;
            }

            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isSidebarOpen)
            {
                ToggleSidebar();
            }
        }

        // ============ SIDEBAR NAVIGATION ============
        private void SidebarItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            string tag = button.Tag?.ToString();
            NavigateToPage(tag);
            ToggleSidebar();
        }

        private void NavigateToPage(string pageName)
        {
            Page newPage = null;

            switch (pageName)
            {
                case "Home":
                    newPage = new HomePage();
                    break;

                // TODO: Các page chưa làm - Tạm thời hiển thị thông báo
                case "History":
                case "Favourite":
                case "MyWords":
                case "Game":
                case "Offline":
                case "Setting":
                case "Account":
                    MessageBox.Show($"Trang '{pageName}' đang được phát triển...",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;

                default:
                    return;
            }

            if (newPage != null)
            {
                // Lưu trang hiện tại vào backStack
                if (MainFrame.Content != null)
                {
                    backStack.Push((Page)MainFrame.Content);
                }

                // Clear forward stack
                forwardStack.Clear();

                MainFrame.Navigate(newPage);
                UpdateNavigationButtons();
            }
        }

        // ============ NAVIGATION BUTTONS ============
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (backStack.Count > 0)
            {
                // Lưu trang hiện tại vào forwardStack
                forwardStack.Push((Page)MainFrame.Content);

                // Lấy trang trước đó
                var previousPage = backStack.Pop();
                MainFrame.Navigate(previousPage);

                UpdateNavigationButtons();
            }
        }

        private void ForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (forwardStack.Count > 0)
            {
                // Lưu trang hiện tại vào backStack
                backStack.Push((Page)MainFrame.Content);

                // Lấy trang tiếp theo
                var nextPage = forwardStack.Pop();
                MainFrame.Navigate(nextPage);

                UpdateNavigationButtons();
            }
        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            // Animation xoay cho reload button
            DoubleAnimation rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            RotateTransform rotateTransform = new RotateTransform();
            ReloadIcon.RenderTransform = rotateTransform;
            ReloadIcon.RenderTransformOrigin = new Point(0.5, 0.5);

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

            // Reload trang hiện tại
            if (MainFrame.Content != null)
            {
                MainFrame.Refresh();
            }
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateNavigationButtons();
        }

        private void UpdateNavigationButtons()
        {
            BackBtn.IsEnabled = backStack.Count > 0;
            ForwardBtn.IsEnabled = forwardStack.Count > 0;
        }

        // ============ SEARCH ============
        private void SearchInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchInput.Text == "Nhập từ cần tra...")
            {
                SearchInput.Text = "";
                SearchInput.Foreground = (SolidColorBrush)this.Resources["SearchText"];
            }
        }

        private void SearchInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchInput.Text))
            {
                SearchInput.Text = "Nhập từ cần tra...";
                SearchInput.Foreground = (SolidColorBrush)this.Resources["SearchPlaceholder"];
            }
        }

        private void SearchInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchBtn_Click(sender, e);
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchInput.Text;

            if (!string.IsNullOrWhiteSpace(searchText) && searchText != "Nhập từ cần tra...")
            {
                MessageBox.Show($"Đang tìm kiếm: {searchText}", "Tìm kiếm",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                // TODO: Implement search logic
            }
            else
            {
                MessageBox.Show("Vui lòng nhập từ cần tra!", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ============ THEME TOGGLE ============
        private void ThemeToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDarkMode = !isDarkMode;

            DoubleAnimation sliderAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            var transform = ThemeSlider.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                ThemeSlider.RenderTransform = transform;
            }

            if (isDarkMode)
            {
                // Chuyển sang Dark Mode
                sliderAnimation.To = 36;
                ThemeIcon.Text = "☀️";
                ApplyDarkMode();
            }
            else
            {
                // Chuyển sang Light Mode
                sliderAnimation.To = 0;
                ThemeIcon.Text = "🌙";
                ApplyLightMode();
            }

            transform.BeginAnimation(TranslateTransform.XProperty, sliderAnimation);
        }

        private void ApplyLightMode()
        {
            var app = Application.Current;

            // Main colors
            app.Resources["MainBackground"] = app.Resources["LightMainBackground"];
            app.Resources["NavbarBackground"] = app.Resources["LightNavbarBackground"];
            app.Resources["ToolbarBackground"] = app.Resources["LightToolbarBackground"];
            app.Resources["SidebarBackground"] = app.Resources["LightSidebarBackground"];
            app.Resources["CardBackground"] = app.Resources["LightCardBackground"];
            app.Resources["WordItemBackground"] = app.Resources["LightWordItemBackground"];
            app.Resources["WordItemHover"] = app.Resources["LightWordItemHover"];

            // Text and borders
            app.Resources["TextColor"] = app.Resources["LightTextColor"];
            app.Resources["BorderColor"] = app.Resources["LightBorderColor"];
            app.Resources["ButtonColor"] = app.Resources["LightButtonColor"];
            app.Resources["WordBorder"] = app.Resources["LightWordBorder"];
            app.Resources["ToolbarBorder"] = app.Resources["LightToolbarBorder"];

            // Search
            app.Resources["SearchBackground"] = app.Resources["LightSearchBackground"];
            app.Resources["SearchBorder"] = app.Resources["LightSearchBorder"];
            app.Resources["SearchIcon"] = app.Resources["LightSearchIcon"];
            app.Resources["SearchText"] = app.Resources["LightSearchText"];
            app.Resources["SearchPlaceholder"] = app.Resources["LightSearchPlaceholder"];
            app.Resources["SearchButton"] = app.Resources["LightSearchButton"];
            app.Resources["SearchButtonHover"] = app.Resources["LightSearchButtonHover"];

            // Buttons
            app.Resources["ToolButtonActive"] = app.Resources["LightToolButtonActive"];
            app.Resources["NavButtonColor"] = app.Resources["LightNavButtonColor"];
            app.Resources["NavButtonHover"] = app.Resources["LightNavButtonHover"];
            app.Resources["HamburgerBackground"] = app.Resources["LightHamburgerBackground"];
            app.Resources["HamburgerHover"] = app.Resources["LightHamburgerHover"];
            app.Resources["HamburgerIcon"] = app.Resources["LightHamburgerIcon"];

            // Sidebar
            app.Resources["SidebarHover"] = app.Resources["LightSidebarHover"];
            app.Resources["SidebarHoverText"] = app.Resources["LightSidebarHoverText"];

            // Theme toggle
            app.Resources["ThemeToggleBackground"] = app.Resources["LightThemeToggleBackground"];
            app.Resources["ThemeSliderBackground"] = app.Resources["LightThemeSliderBackground"];
            app.Resources["ThemeIconColor"] = app.Resources["LightThemeIconColor"];

            // Update search placeholder color
            if (SearchInput.Text == "Nhập từ cần tra...")
            {
                SearchInput.Foreground = (SolidColorBrush)app.Resources["SearchPlaceholder"];
            }
        }

        private void ApplyDarkMode()
        {
            var app = Application.Current;

            // Main colors
            app.Resources["MainBackground"] = app.Resources["DarkMainBackground"];
            app.Resources["NavbarBackground"] = app.Resources["DarkNavbarBackground"];
            app.Resources["ToolbarBackground"] = app.Resources["DarkToolbarBackground"];
            app.Resources["SidebarBackground"] = app.Resources["DarkSidebarBackground"];
            app.Resources["CardBackground"] = app.Resources["DarkCardBackground"];
            app.Resources["WordItemBackground"] = app.Resources["DarkWordItemBackground"];
            app.Resources["WordItemHover"] = app.Resources["DarkWordItemHover"];

            // Text and borders
            app.Resources["TextColor"] = app.Resources["DarkTextColor"];
            app.Resources["BorderColor"] = app.Resources["DarkBorderColor"];
            app.Resources["ButtonColor"] = app.Resources["DarkButtonColor"];
            app.Resources["WordBorder"] = app.Resources["DarkWordBorder"];
            app.Resources["ToolbarBorder"] = app.Resources["DarkToolbarBorder"];

            // Search
            app.Resources["SearchBackground"] = app.Resources["DarkSearchBackground"];
            app.Resources["SearchBorder"] = app.Resources["DarkSearchBorder"];
            app.Resources["SearchIcon"] = app.Resources["DarkSearchIcon"];
            app.Resources["SearchText"] = app.Resources["DarkSearchText"];
            app.Resources["SearchPlaceholder"] = app.Resources["DarkSearchPlaceholder"];
            app.Resources["SearchButton"] = app.Resources["DarkSearchButton"];
            app.Resources["SearchButtonHover"] = app.Resources["DarkSearchButtonHover"];

            // Buttons
            app.Resources["ToolButtonActive"] = app.Resources["DarkToolButtonActive"];
            app.Resources["NavButtonColor"] = app.Resources["DarkNavButtonColor"];
            app.Resources["NavButtonHover"] = app.Resources["DarkNavButtonHover"];
            app.Resources["HamburgerBackground"] = app.Resources["DarkHamburgerBackground"];
            app.Resources["HamburgerHover"] = app.Resources["DarkHamburgerHover"];
            app.Resources["HamburgerIcon"] = app.Resources["DarkHamburgerIcon"];

            // Sidebar
            app.Resources["SidebarHover"] = app.Resources["DarkSidebarHover"];
            app.Resources["SidebarHoverText"] = app.Resources["DarkSidebarHoverText"];

            // Theme toggle
            app.Resources["ThemeToggleBackground"] = app.Resources["DarkThemeToggleBackground"];
            app.Resources["ThemeSliderBackground"] = app.Resources["DarkThemeSliderBackground"];
            app.Resources["ThemeIconColor"] = app.Resources["DarkThemeIconColor"];

            // Update search placeholder color
            if (SearchInput.Text == "Nhập từ cần tra...")
            {
                SearchInput.Foreground = (SolidColorBrush)app.Resources["SearchPlaceholder"];
            }
        }

    }
}