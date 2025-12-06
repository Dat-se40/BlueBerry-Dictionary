using BlueBerryDictionary.Services;
using BlueBerryDictionary.ViewModels;
using BlueBerryDictionary.Views.Pages;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Diagnostics;
using BlueBerryDictionary.Services.User;

namespace BlueBerryDictionary
{
    public partial class MainWindow : Window
    {
        private SearchViewModel _searchViewModel;
        private bool _isSidebarOpen = false;
        private bool _isDarkMode = false;
        private NavigationService _navigationService; // ✏️ Changed to concrete type

        public MainWindow()
        {
            InitializeComponent();

            // ✅ Initialize NavigationService with OnWordClicked callback
            _navigationService = new NavigationService(MainFrame, null); // Temp null

            // ✅ Initialize SearchViewModel with NavigationService
            _searchViewModel = new SearchViewModel(_navigationService);
            DataContext = _searchViewModel;

            // ✅ Update NavigationService callback
            _navigationService = new NavigationService(MainFrame, _searchViewModel.OnWordClicked);
            _searchViewModel = new SearchViewModel(_navigationService);
            DataContext = _searchViewModel;

            // ✅ Navigate to Home using NavigationService
            _navigationService.NavigateTo("Home");
            UpdateNavigationButtons();
            Dispatcher.ShutdownStarted += (s, e) => {
                TagService.Instance.SaveTags();
                TagService.Instance.SaveWords();

            };
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
                if (pageTag == "Account")
                {
                    // Check if user is guest
                    bool isGuest = UserDataManager.Instance.IsGuestMode;
                    string email = UserDataManager.Instance.CurrentUserEmail;

                    System.Diagnostics.Debug.WriteLine($"🔍 Account clicked: IsGuest={isGuest}, Email={email}");

                    if (isGuest)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Guest mode → Show LoginPromptPage");

                        // Create LoginPromptPage with NavigationService
                        var loginPromptPage = new Views.Pages.LoginPromptPage(_navigationService);

                        // Navigate using NavigateToPage (not NavigateTo)
                        _navigationService.NavigateToPage(loginPromptPage, "LoginPrompt");
                    }
                    else
                    {
                        // Logged in → Navigate to UserProfilePage normally
                        System.Diagnostics.Debug.WriteLine("✅ Logged in → Navigate to UserProfile");
                        _navigationService.NavigateTo("UserProfile");
                    }
                }
                else
                {
                    // Other pages → Navigate normally
                    _navigationService.NavigateTo(pageTag);
                }

                UpdateNavigationButtons();
                CloseSidebar();
            }
        }
        // Xử lý sự kiện khi click vào Hyperlink
        
        #endregion

        #region Navigation

        /// <summary>
        /// Back button click
        /// </summary>
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            // ✅ Use NavigationService.GoBack
            _navigationService.GoBack();
            UpdateNavigationButtons();
        }

        /// <summary>
        /// Forward button click
        /// </summary>
        private void ForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            // ✅ Use NavigationService.GoForward
            _navigationService.GoForward();
            UpdateNavigationButtons();
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

            // ✅ Reload using LoadData if available
            if (MainFrame.Content is WordListPageBase page)
            {
                page.LoadData();
            }
        }

        /// <summary>
        /// ✅ NEW: Update navigation button states
        /// </summary>
        private void UpdateNavigationButtons()
        {
            BackBtn.IsEnabled = _navigationService.CanGoBack;
            ForwardBtn.IsEnabled = _navigationService.CanGoForward;
        }

        /// <summary>
        /// Update navigation buttons when frame navigated
        /// </summary>
        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // ✅ Use NavigationService state instead of Frame
            UpdateNavigationButtons();
        }

        #endregion

        #region Theme Toggle

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
                sliderAnimation.To = 36;
                ThemeIcon.Text = "☀️";
                ApplyDarkMode();
            }
            else
            {
                sliderAnimation.To = 0;
                ThemeIcon.Text = "🌙";
                ApplyLightMode();
            }

            transform.BeginAnimation(TranslateTransform.XProperty, sliderAnimation);
        }

        private void ApplyLightMode()
        {
            var app = Application.Current;

            app.Resources["MainBackground"] = app.Resources["LightMainBackground"];
            app.Resources["NavbarBackground"] = app.Resources["LightNavbarBackground"];
            app.Resources["ToolbarBackground"] = app.Resources["LightToolbarBackground"];
            app.Resources["SidebarBackground"] = app.Resources["LightSidebarBackground"];
            app.Resources["CardBackground"] = app.Resources["LightCardBackground"];
            app.Resources["WordItemBackground"] = app.Resources["LightWordItemBackground"];
            app.Resources["WordItemHover"] = app.Resources["LightWordItemHover"];
            app.Resources["TextColor"] = app.Resources["LightTextColor"];
            app.Resources["BorderColor"] = app.Resources["LightBorderColor"];
            app.Resources["ButtonColor"] = app.Resources["LightButtonColor"];
            app.Resources["WordBorder"] = app.Resources["LightWordBorder"];
            app.Resources["ToolbarBorder"] = app.Resources["LightToolbarBorder"];
            app.Resources["SearchBackground"] = app.Resources["LightSearchBackground"];
            app.Resources["SearchBorder"] = app.Resources["LightSearchBorder"];
            app.Resources["SearchIcon"] = app.Resources["LightSearchIcon"];
            app.Resources["SearchText"] = app.Resources["LightSearchText"];
            app.Resources["SearchPlaceholder"] = app.Resources["LightSearchPlaceholder"];
            app.Resources["SearchButton"] = app.Resources["LightSearchButton"];
            app.Resources["SearchButtonHover"] = app.Resources["LightSearchButtonHover"];
            app.Resources["ToolButtonActive"] = app.Resources["LightToolButtonActive"];
            app.Resources["NavButtonColor"] = app.Resources["LightNavButtonColor"];
            app.Resources["NavButtonHover"] = app.Resources["LightNavButtonHover"];
            app.Resources["HamburgerBackground"] = app.Resources["LightHamburgerBackground"];
            app.Resources["HamburgerHover"] = app.Resources["LightHamburgerHover"];
            app.Resources["HamburgerIcon"] = app.Resources["LightHamburgerIcon"];
            app.Resources["SidebarHover"] = app.Resources["LightSidebarHover"];
            app.Resources["SidebarHoverText"] = app.Resources["LightSidebarHoverText"];
            app.Resources["ThemeToggleBackground"] = app.Resources["LightThemeToggleBackground"];
            app.Resources["ThemeSliderBackground"] = app.Resources["LightThemeSliderBackground"];
            app.Resources["ThemeIconColor"] = app.Resources["LightThemeIconColor"];
            app.Resources["MeaningBackground"] = app.Resources["LightMeaningBackground"];
            app.Resources["MeaningBorder"] = app.Resources["LightMeaningBorder"];
            app.Resources["MeaningBorderLeft"] = app.Resources["LightMeaningBorderLeft"];
            app.Resources["ExampleBackground"] = app.Resources["LightExampleBackground"];
            app.Resources["ExampleBorder"] = app.Resources["LightExampleBorder"];
            app.Resources["RelatedBackground"] = app.Resources["LightRelatedBackground"];
            app.Resources["RelatedBorder"] = app.Resources["LightRelatedBorder"];

            if (SearchInput.Text == "Nhập từ cần tra...")
            {
                SearchInput.Foreground = (SolidColorBrush)app.Resources["SearchPlaceholder"];
            }
            else if (!string.IsNullOrEmpty(SearchInput.Text))
            {
                SearchInput.Foreground = (SolidColorBrush)app.Resources["SearchText"];
            }
        }

        private void ApplyDarkMode()
        {
            var app = Application.Current;

            app.Resources["MainBackground"] = app.Resources["DarkMainBackground"];
            app.Resources["NavbarBackground"] = app.Resources["DarkNavbarBackground"];
            app.Resources["ToolbarBackground"] = app.Resources["DarkToolbarBackground"];
            app.Resources["SidebarBackground"] = app.Resources["DarkSidebarBackground"];
            app.Resources["CardBackground"] = app.Resources["DarkCardBackground"];
            app.Resources["WordItemBackground"] = app.Resources["DarkWordItemBackground"];
            app.Resources["WordItemHover"] = app.Resources["DarkWordItemHover"];
            app.Resources["TextColor"] = app.Resources["DarkTextColor"];
            app.Resources["BorderColor"] = app.Resources["DarkBorderColor"];
            app.Resources["ButtonColor"] = app.Resources["DarkButtonColor"];
            app.Resources["WordBorder"] = app.Resources["DarkWordBorder"];
            app.Resources["ToolbarBorder"] = app.Resources["DarkToolbarBorder"];
            app.Resources["SearchBackground"] = app.Resources["DarkSearchBackground"];
            app.Resources["SearchBorder"] = app.Resources["DarkSearchBorder"];
            app.Resources["SearchIcon"] = app.Resources["DarkSearchIcon"];
            app.Resources["SearchText"] = app.Resources["DarkSearchText"];
            app.Resources["SearchPlaceholder"] = app.Resources["DarkSearchPlaceholder"];
            app.Resources["SearchButton"] = app.Resources["DarkSearchButton"];
            app.Resources["SearchButtonHover"] = app.Resources["DarkSearchButtonHover"];
            app.Resources["ToolButtonActive"] = app.Resources["DarkToolButtonActive"];
            app.Resources["NavButtonColor"] = app.Resources["DarkNavButtonColor"];
            app.Resources["NavButtonHover"] = app.Resources["DarkNavButtonHover"];
            app.Resources["HamburgerBackground"] = app.Resources["DarkHamburgerBackground"];
            app.Resources["HamburgerHover"] = app.Resources["DarkHamburgerHover"];
            app.Resources["HamburgerIcon"] = app.Resources["DarkHamburgerIcon"];
            app.Resources["SidebarHover"] = app.Resources["DarkSidebarHover"];
            app.Resources["SidebarHoverText"] = app.Resources["DarkSidebarHoverText"];
            app.Resources["ThemeToggleBackground"] = app.Resources["DarkThemeToggleBackground"];
            app.Resources["ThemeSliderBackground"] = app.Resources["DarkThemeSliderBackground"];
            app.Resources["ThemeIconColor"] = app.Resources["DarkThemeIconColor"];
            app.Resources["MeaningBackground"] = app.Resources["DarkMeaningBackground"];
            app.Resources["MeaningBorder"] = app.Resources["DarkMeaningBorder"];
            app.Resources["MeaningBorderLeft"] = app.Resources["DarkMeaningBorderLeft"];
            app.Resources["ExampleBackground"] = app.Resources["DarkExampleBackground"];
            app.Resources["ExampleBorder"] = app.Resources["DarkExampleBorder"];
            app.Resources["RelatedBackground"] = app.Resources["DarkRelatedBackground"];
            app.Resources["RelatedBorder"] = app.Resources["DarkRelatedBorder"];

            if (SearchInput.Text == "Nhập từ cần tra...")
            {
                SearchInput.Foreground = (SolidColorBrush)app.Resources["SearchPlaceholder"];
            }
            else if (!string.IsNullOrEmpty(SearchInput.Text))
            {
                SearchInput.Foreground = (SolidColorBrush)app.Resources["SearchText"];
            }
        }

        #endregion

        #region Search

        private async void SearchInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_searchViewModel.ExcuteSearchAndNavigateCommand.CanExecute(null))
                {
                    await _searchViewModel.ExcuteSearchAndNavigateCommand.ExecuteAsync(null);
                }
            }
            else if (e.Key == Key.Escape)
            {
                _searchViewModel.IsSuggestionsOpen = false;
                SearchInput.Focus();
            }
            else if (e.Key == Key.Down && _searchViewModel.IsSuggestionsOpen)
            {
                if (SuggestionsList.Items.Count > 0)
                {
                    SuggestionsList.Focus();
                    SuggestionsList.SelectedIndex = 0;

                    var listBoxItem = SuggestionsList.ItemContainerGenerator
                        .ContainerFromIndex(0) as ListBoxItem;
                    listBoxItem?.Focus();
                }
                e.Handled = true;
            }
        }

        private void SearchInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_searchViewModel.SearchText) &&
                _searchViewModel.Suggestions.Count > 0)
            {
                _searchViewModel.IsSuggestionsOpen = true;
            }
        }

        private void SearchInput_LostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!SuggestionsList.IsMouseOver && !SuggestionsPopup.IsMouseOver)
                {
                    _searchViewModel.IsSuggestionsOpen = false;
                }
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private async void SuggestionsList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null) return;

            var clickedElement = e.OriginalSource as DependencyObject;
            var item = ItemsControl.ContainerFromElement(listBox, clickedElement) as ListBoxItem;

            if (item != null && item.Content is string selectedWord)
            {
                if (_searchViewModel.ExecuteSelectSuggestionCommand.CanExecute(selectedWord))
                {
                    await _searchViewModel.ExecuteSelectSuggestionCommand.ExecuteAsync(selectedWord);
                }
            }
        }

        private async void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsList.SelectedItem == null) return;
            string selectedWord = SuggestionsList.SelectedItem.ToString();
            _searchViewModel.SearchText = selectedWord;
            _searchViewModel.IsSuggestionsOpen = false;
        }

        #endregion
    }
}