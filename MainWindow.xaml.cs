using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BlueBerryDictionary.ViewModels;
using BlueBerryDictionary.Views.Pages;

namespace BlueBerryDictionary
{
    public partial class MainWindow : Window
    {
        private SearchViewModel _searchViewModel;
        private bool _isSidebarOpen = false;
        private bool _isDarkMode = false;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize ViewModel
            _searchViewModel = new SearchViewModel();
            DataContext = _searchViewModel;

            // Navigate to Home page
            MainFrame.Navigate(new HomePage());
        }

        #region SideBar

        /// <summary>
        /// Toggle Sidebar (Hamburger button)
        /// </summary>
        private void HamburgerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isSidebarOpen)
                CloseSidebar();
            else
                OpenSidebar();
        }

        /// <summary>
        /// Open Sidebar with animation
        /// </summary>
        private void OpenSidebar()
        {
            _isSidebarOpen = true;
            Overlay.Visibility = Visibility.Visible;

            var animation = new DoubleAnimation
            {
                From = -280,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        /// <summary>
        /// Close Sidebar with animation
        /// </summary>
        private void CloseSidebar()
        {
            _isSidebarOpen = false;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = -280,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            animation.Completed += (s, e) => Overlay.Visibility = Visibility.Collapsed;
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        /// <summary>
        /// Close sidebar when clicking overlay
        /// </summary>
        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseSidebar();
        }

        /// <summary>
        /// Sidebar menu item click
        /// </summary>
        private void SidebarItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pageTag)
            {
                NavigateToPage(pageTag);
                CloseSidebar();
            }
        }
        /// <summary>
        /// Navigate to page based on tag
        /// </summary>
        private void NavigateToPage(string pageTag)
        {
            Page page = pageTag switch
            {
                //"Home" => new Home(),
                //"History" => new Home(), // TODO: Create History page
                //"Favourite" => new Home(), // TODO: Create Favourite page
                //"MyWords" => new Home(), // TODO: Create MyWords page
                //"Game" => new Home(), // TODO: Create Game page
                //"Offline" => new Home(), // TODO: Create Offline page
                //"Account" => new Home(), // TODO: Create Account page
                //"Setting" => new Home(), // TODO: Create Setting page
                //_ => new Home()
            };

            MainFrame.Navigate(page);
        }

        #endregion

        #region Navigation
        // ==================== NAVIGATION ====================

        /// <summary>
        /// Back button click
        /// </summary>
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        /// <summary>
        /// Forward button click
        /// </summary>
        private void ForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoForward)
                MainFrame.GoForward();
        }

        /// <summary>
        /// Reload button click
        /// </summary>
        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            // Rotate animation for reload icon
            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase()
            };

            var rotateTransform = new RotateTransform();
            ReloadIcon.RenderTransform = rotateTransform;
            ReloadIcon.RenderTransformOrigin = new Point(0.5, 0.5);
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

            // Refresh current page
            if (MainFrame.Content != null)
            {
                MainFrame.Refresh();
            }
        }

        /// <summary>
        /// Update navigation buttons when frame navigated
        /// </summary>
        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            BackBtn.IsEnabled = MainFrame.CanGoBack;
            ForwardBtn.IsEnabled = MainFrame.CanGoForward;
        }
        #endregion

        #region Search & Suggestion
        // ==================== SEARCH & SUGGESTIONS ====================

        /// <summary>
        /// Search input key down (Enter to search, Esc to close suggestions)
        /// </summary>
        private async void SearchInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Execute search command
                if (_searchViewModel.ExecuteSearchCommand.CanExecute(null))
                {
                    await _searchViewModel.ExecuteSearchCommand.ExecuteAsync(null);
                }
            }
            else if (e.Key == Key.Escape)
            {
                // Close suggestions popup
                _searchViewModel.IsSuggestionsOpen = false;
                SearchInput.Focus();
            }
            else if (e.Key == Key.Down && _searchViewModel.IsSuggestionsOpen)
            {
                // Navigate to suggestions list with arrow keys
                if (SuggestionsList.Items.Count > 0)
                {
                    SuggestionsList.Focus();
                    SuggestionsList.SelectedIndex = 0;

                    // Focus on first item
                    var listBoxItem = SuggestionsList.ItemContainerGenerator
                        .ContainerFromIndex(0) as ListBoxItem;
                    listBoxItem?.Focus();
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// When search input gets focus, show suggestions if available
        /// </summary>
        private void SearchInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_searchViewModel.SearchText) &&
                _searchViewModel.Suggestions.Count > 0)
            {
                _searchViewModel.IsSuggestionsOpen = true;
            }
        }

        /// <summary>
        /// When search input loses focus, close suggestions (with delay for click handling)
        /// </summary>
        private void SearchInput_LostFocus(object sender, RoutedEventArgs e)
        {
            // Delay to allow click on suggestion item
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!SuggestionsList.IsMouseOver && !SuggestionsPopup.IsMouseOver)
                {
                    _searchViewModel.IsSuggestionsOpen = false;
                }
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// Handle suggestion item click
        /// </summary>
        private async void SuggestionsList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Find clicked ListBoxItem
            var listBox = sender as ListBox;
            if (listBox == null) return;

            // Get the clicked item
            var clickedElement = e.OriginalSource as DependencyObject;
            var item = ItemsControl.ContainerFromElement(listBox, clickedElement) as ListBoxItem;

            if (item != null && item.Content is string selectedWord)
            {
                // Execute select suggestion command
                if (_searchViewModel.ExecuteSelectSuggestionCommand.CanExecute(selectedWord))
                {
                    await _searchViewModel.ExecuteSelectSuggestionCommand.ExecuteAsync(selectedWord);
                }
            }
        }
        #endregion

        #region THEME TOGGLE 

        /// <summary>
        /// Toggle between light and dark theme
        /// </summary>
        private void ThemeToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDarkMode = !_isDarkMode;

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

            if (_isDarkMode)
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
        #endregion

        private void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string term = SuggestionsList.SelectedItem.ToString() ?? "";
            Console.Write(term);    
        }
    }
}