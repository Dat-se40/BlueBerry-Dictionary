using BlueBerryDictionary.Pages;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.ViewModels;
using BlueBerryDictionary.Views.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NavigationService = BlueBerryDictionary.Services.NavigationService;

namespace BlueBerryDictionary
{
    public partial class MainWindow : Window
    {
        private SearchViewModel _searchViewModel;
        private bool _isSidebarOpen = false;
        private bool _isDarkMode = false;
        private INavigationService _navigationService;   
        public MainWindow()
        {
            InitializeComponent();

            // Initialize ViewModel
            _navigationService = new NavigationService(MainFrame); 
            _searchViewModel = new SearchViewModel(_navigationService);
            DataContext = _searchViewModel;

            // Navigate to Home page
            MainFrame.Navigate(new HomePage(_searchViewModel.OnWordClicked));
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
            Page? page = null;
            switch (pageTag)
            {
                case "Home":
                    page = new HomePage(_searchViewModel.OnWordClicked);
                    break;
                case "History":
                    var hisp = new HistoryPage(_searchViewModel.OnWordClicked); // TODO: Create History page
                    hisp.LoadData();
                    page = hisp;    
                    break;
                // Uncomment and implement these cases when the pages are available
                case "Favourite":
                    page = new FavouriteWordsPage(_searchViewModel.OnWordClicked);
                    break;
                case "MyWords":
                    page = new MyWordsPage(_searchViewModel.OnWordClicked);
                    break;
                //case "Game":
                //    page = new GamePage();
                //    break;
                //case "Offline":
                //    page = new OfflinePage();
                //    break;
                //case "Account":
                //    page = new AccountPage();
                //    break;
                //case "Setting":
                //    page = new SettingPage();
                //    break;
                default:
                    // Handle unknown or empty pageTag
                    page = new HomePage(_searchViewModel.OnWordClicked);
                    break;
            }
            Console.WriteLine("Navigate to " + page.ToString());
            _navigationService.Navigate(page,page.ToString());
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
            var currentPage = MainFrame.Content;
            if (currentPage != null)
            {
                try
                {
                    dynamic dynPage = currentPage;
                    dynPage.LoadData(); // sẽ gọi nếu có, nếu không thì runtime exception
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                {
                    // Page không có hàm LoadData, bỏ qua
                }
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
                if (_searchViewModel.ExecuteSearchCommand.CanExecute(null))
                {
                    await _searchViewModel.ExecuteSearchCommand.ExecuteAsync(null);

                    // Navigate to DetailsPage
                    if (_searchViewModel.HasResults &&
                        _searchViewModel.CurrentWords != null &&
                        _searchViewModel.CurrentWords.Count > 0)
                    {
                      //[se fix sau]  MainFrame.Navigate(new DetailsPage(_searchViewModel.CurrentWords[0]));
                    }
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
        private async void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsList.SelectedItem == null) return;
            string selectedWord = SuggestionsList.SelectedItem.ToString();
            _searchViewModel.SearchText = selectedWord;

            // Tự động search luôn (optional)
            //if (_searchViewModel.ExecuteSearchCommand.CanExecute(null))
            //{
            //    await _searchViewModel.ExecuteSearchCommand.ExecuteAsync(null);
            //    if (_searchViewModel.HasResults &&
            //        _searchViewModel.CurrentWords != null &&
            //        _searchViewModel.CurrentWords.Count > 0)
            //    {
            //        //[Se fix sau]MainFrame.Navigate(new DetailsPage(_searchViewModel.CurrentWords[0]));
            //    }
            //}

            _searchViewModel.IsSuggestionsOpen = false;
        }
        /// <summary>
        /// Search button click handler
        /// </summary>
        //private async void SearchBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (_searchViewModel.ExecuteSearchCommand.CanExecute(null))
        //    {
        //        await _searchViewModel.ExecuteSearchCommand.ExecuteAsync(null);

        //        // Navigate đến DetailsPage nếu tìm thấy từ
        //        if (_searchViewModel.HasResults &&
        //            _searchViewModel.CurrentWords != null &&
        //            _searchViewModel.CurrentWords.Count > 0)
        //        {
        //            MainFrame.Navigate(new DetailsPage(_searchViewModel.CurrentWords[0]));
        //        }
        //        else if (!_searchViewModel.HasResults)
        //        {
        //            MessageBox.Show($"Không tìm thấy từ '{_searchViewModel.SearchText}'",
        //                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //    }
        //}
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

            // Meaning Section
            app.Resources["MeaningBackground"] = app.Resources["LightMeaningBackground"];
            app.Resources["MeaningBorder"] = app.Resources["LightMeaningBorder"];
            app.Resources["MeaningBorderLeft"] = app.Resources["LightMeaningBorderLeft"];

            // Example
            app.Resources["ExampleBackground"] = app.Resources["LightExampleBackground"];
            app.Resources["ExampleBorder"] = app.Resources["LightExampleBorder"];

            // Related Section
            app.Resources["RelatedBackground"] = app.Resources["LightRelatedBackground"];
            app.Resources["RelatedBorder"] = app.Resources["LightRelatedBorder"];



            // Update search input color
            if (SearchInput.Text == "Nhập từ cần tra...")
            {
                // Nếu đang hiện placeholder → màu xám
                SearchInput.Foreground = (SolidColorBrush)app.Resources["SearchPlaceholder"];
            }
            else if (!string.IsNullOrEmpty(SearchInput.Text))
            {
                // Nếu đang có chữ → màu đen (Light Mode)
                SearchInput.Foreground = (SolidColorBrush)app.Resources["SearchText"];
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

            // Meaning Section
            app.Resources["MeaningBackground"] = app.Resources["DarkMeaningBackground"];
            app.Resources["MeaningBorder"] = app.Resources["DarkMeaningBorder"];
            app.Resources["MeaningBorderLeft"] = app.Resources["DarkMeaningBorderLeft"];

            // Example
            app.Resources["ExampleBackground"] = app.Resources["DarkExampleBackground"];
            app.Resources["ExampleBorder"] = app.Resources["DarkExampleBorder"];

            // Related Section
            app.Resources["RelatedBackground"] = app.Resources["DarkRelatedBackground"];
            app.Resources["RelatedBorder"] = app.Resources["DarkRelatedBorder"];


            // Update search input color
            if (SearchInput.Text == "Nhập từ cần tra...")
            {
                // Nếu đang hiện placeholder → màu xám
                SearchInput.Foreground = (SolidColorBrush)app.Resources["SearchPlaceholder"];
            }
            else if (!string.IsNullOrEmpty(SearchInput.Text))
            {
                // Nếu đang có chữ → màu trắng (Dark Mode)
                SearchInput.Foreground = (SolidColorBrush)app.Resources["SearchText"];
            }

        }
        #endregion

        

    }
}