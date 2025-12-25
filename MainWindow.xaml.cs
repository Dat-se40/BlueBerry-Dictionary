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

            // Initialize NavigationService with OnWordClicked callback
            _navigationService = new NavigationService(MainFrame, null); // Temp null

            // Initialize SearchViewModel with NavigationService
            _searchViewModel = new SearchViewModel(_navigationService);
            DataContext = _searchViewModel;

            // Update NavigationService callback
            _navigationService = new NavigationService(MainFrame, _searchViewModel.OnWordClicked);
            _searchViewModel = new SearchViewModel(_navigationService);
            DataContext = _searchViewModel;

            // Navigate to Home using NavigationService
            _navigationService.NavigateTo("Home");
            UpdateNavigationButtons();

            // THÊM: Subscribe to theme change event
            ThemeManager.Instance.ThemeChanged += OnThemeChanged;

            // THÊM: Sync toggle với theme hiện tại
            SyncThemeToggle();

            Dispatcher.ShutdownStarted += (s, e) => {
                TagService.Instance.SaveTags();
                TagService.Instance.SaveWords();
                ThemeManager.Instance.ThemeChanged -= OnThemeChanged;

            };
        }

        // THÊM: Sync toggle button với theme hiện tại
        private void SyncThemeToggle()
        {
            bool isDark = ThemeManager.Instance.CurrentTheme == Services.ThemeMode.Dark;

            var transform = ThemeSlider.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                ThemeSlider.RenderTransform = transform;
            }

            if (isDark)
            {
                transform.X = 36;
                ThemeIcon.Text = "☀️";
            }
            else
            {
                transform.X = 0;
                ThemeIcon.Text = "🌙";
            }
        }

        // THÊM: Handler khi theme thay đổi (từ Settings)
        private void OnThemeChanged(Services.ThemeMode newTheme)
        {
            Dispatcher.Invoke(() => SyncThemeToggle());
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
            // Use NavigationService.GoBack
            _navigationService.GoBack();
            UpdateNavigationButtons();
        }

        /// <summary>
        /// Forward button click
        /// </summary>
        private void ForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            // Use NavigationService.GoForward
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

            // Reload using LoadData if available
            if (MainFrame.Content is WordListPageBase page)
            {
                page.LoadData();
            }
        }

        /// <summary>
        /// Update navigation button states
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
            // Use NavigationService state instead of Frame
            UpdateNavigationButtons();
        }

        #endregion

        #region Theme Toggle

        /// <summary>
        /// Toggle between light and dark theme
        /// </summary>
        private void ThemeToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Toggle theme qua ThemeManager
            var currentTheme = ThemeManager.Instance.CurrentTheme;
            var newTheme = currentTheme == Services.ThemeMode.Light
                ? Services.ThemeMode.Dark
                : Services.ThemeMode.Light;

            ThemeManager.Instance.SetThemeMode(newTheme);

            // Animate slider
            var transform = ThemeSlider.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                ThemeSlider.RenderTransform = transform;
            }

            DoubleAnimation sliderAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            if (newTheme == Services.ThemeMode.Dark)
            {
                sliderAnimation.To = 36;
                ThemeIcon.Text = "☀️";
            }
            else
            {
                sliderAnimation.To = 0;
                ThemeIcon.Text = "🌙";
            }

            transform.BeginAnimation(TranslateTransform.XProperty, sliderAnimation);
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
            // Đổi màu khi focus
            if (SearchInput.Text == "Enter word to look up...")
            {
                SearchInput.Text = "";
                SearchInput.Foreground = (Brush)Application.Current.Resources["SearchText"];
            }

            if (!string.IsNullOrWhiteSpace(_searchViewModel.SearchText) &&
                _searchViewModel.Suggestions.Count > 0)
            {
                _searchViewModel.IsSuggestionsOpen = true;
            }
        }

        private void SearchInput_LostFocus(object sender, RoutedEventArgs e)
        {
            // Hiện placeholder nếu rỗng
            if (string.IsNullOrWhiteSpace(SearchInput.Text))
            {
                SearchInput.Text = "Enter word to look up...";
                SearchInput.Foreground = (Brush)Application.Current.Resources["SearchPlaceholder"];
            }

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