using BlueBerryDictionary.Views.Pages;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BlueBerryDictionary.Services
{
    /// <summary>
    /// Interface cho navigation service
    /// </summary>
    public interface INavigationService
    {
        void NavigateTo(string pageTag, Page customPage = null, string uniqueId = null);
        void NavigateToPage(Page page, string pageName);
        void GoBack();
        void GoForward();
        bool CanGoBack { get; }
        bool CanGoForward { get; }
    }

    /// <summary>
    /// Service điều hướng giữa các pages (hỗ trợ Back/Forward)
    /// </summary>
    public class NavigationService : INavigationService
    {
        private Frame _frame; 
        private Stack<string> _backStack = new Stack<string>();
        private Stack<string> _forwardStack = new Stack<string>();
        private string _currentPage;
        private Action<string> _onWordClick;
        private Action<object, System.Windows.RoutedEventArgs> _sidebarNavigate;

        public bool CanGoBack => _backStack.Count > 0;
        public bool CanGoForward => _forwardStack.Count > 0;

        /// <summary>
        /// Khởi tạo NavigationService
        /// </summary>
        public NavigationService(
            Frame frame,
            Action<string> onWordClick,
            Action<object, System.Windows.RoutedEventArgs> sidebarNavigate = null)
        {
            _frame = frame;
            _onWordClick = onWordClick;
            _sidebarNavigate = sidebarNavigate;

            // Clear Frame journal để tránh caching
            _frame.Navigated += (s, e) =>
            {
                while (_frame.CanGoBack)
                {
                    _frame.RemoveBackEntry();
                }
            };
        }

        /// <summary>
        /// Navigate tới page theo tag
        /// </summary>
        public void NavigateTo(string pageTag, Page customPage = null, string uniqueId = null)
        {
            // Lưu page hiện tại vào back stack
            if (!string.IsNullOrEmpty(_currentPage) && _currentPage != pageTag)
            {
                _backStack.Push(_currentPage);
                _forwardStack.Clear(); // Clear forward khi navigate mới
            }

            _currentPage = pageTag;

            // Create fresh page
            var page = (customPage != null ) ? customPage : CreatePage(pageTag);
            _frame.Navigate(page);

            ApplyFontToPage(page);

            _frame.Navigate(page);

            System.Console.WriteLine($"📄 {pageTag} | Back: {_backStack.Count} | Forward: {_forwardStack.Count}");
        }

        /// <summary>
        /// Quay lại page trước
        /// </summary>
        public void GoBack()
        {
            if (!CanGoBack) return;

            _forwardStack.Push(_currentPage);
            _currentPage = _backStack.Pop();

            var page = CreatePage(_currentPage);

            ApplyFontToPage(page);
            while (_frame.CanGoBack)
            {
                _frame.RemoveBackEntry();
            }

            _frame.Navigate(page);

            System.Console.WriteLine($"⬅️ {_currentPage} | Back: {_backStack.Count} | Forward: {_forwardStack.Count}");
        }

        /// <summary>
        /// Đi tới page tiếp theo
        /// </summary>
        public void GoForward()
        {
            if (!CanGoForward) return;

            _backStack.Push(_currentPage);
            _currentPage = _forwardStack.Pop();

            var page = CreatePage(_currentPage);

            ApplyFontToPage(page);

            while (_frame.CanGoBack)
            {
                _frame.RemoveBackEntry();
            }

            _frame.Navigate(page);

            System.Console.WriteLine($"➡️ {_currentPage} | Back: {_backStack.Count} | Forward: {_forwardStack.Count}");
        }

        /// <summary>
        /// Tạo instance page từ tag
        /// </summary>
        private Page CreatePage(string pageTag)
        {
            Page page = pageTag switch
            {
                "Home" => new Views.Pages.HomePage(_onWordClick, (s, e) =>
                {
                    if (s is Button btn && btn.Tag != null)
                    {
                        NavigateTo(btn.Tag.ToString());
                    }
                }),
                "History" => new Pages.HistoryPage(_onWordClick),
                "Favourite" => new Views.Pages.FavouriteWordsPage(_onWordClick),
                "MyWords" => new Pages.MyWordsPage(_onWordClick),
                "Game" => new Views.Pages.GamePage(_onWordClick),
                "Offline" => new OfflineModePage(_onWordClick),
                "Account" => new UserProfilePage(),
                "UserProfile" => new UserProfilePage(),
                "Setting" => new SettingsPage(),
                _ => new Views.Pages.HomePage(_onWordClick, _sidebarNavigate)
            }; 

            // Auto-load data
            if (page is Views.Pages.WordListPageBase basePage)
            {
                Console.WriteLine("[Navigation Service] " + pageTag + " is loading");
                basePage.LoadData();
            }

            return page;
        }
        /// <summary>
        /// Navigate to specific page instance (dùng cho LoginPromptPage)
        /// </summary>
        public void NavigateToPage(Page page, string pageName)
        {
            if (page == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ NavigateToPage: page is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"🔍 NavigateToPage called: {pageName}");

            // Save current to back stack
            if (!string.IsNullOrEmpty(_currentPage) && _currentPage != pageName)
            {
                _backStack.Push(_currentPage);
                _forwardStack.Clear();
            }

            _currentPage = pageName;
            ApplyFontToPage(page);
            // ép frame không giữ cache, luôn tạo fresh page
            while (_frame.CanGoBack)
            {
                _frame.RemoveBackEntry();
            }

            // Navigate to provided page instance
            _frame.Navigate(page);

            System.Diagnostics.Debug.WriteLine($"📄 {pageName} | Back: {_backStack.Count} | Forward: {_forwardStack.Count}");
        }

        /// <summary>
        /// Thuộc tính font từ resources và áp dụng cho page
        /// </summary>
        private void ApplyFontToPage(Page page)
        {
            try
            {
                if (System.Windows.Application.Current.Resources.Contains("AppFontFamily"))
                {
                    var fontFamily = System.Windows.Application.Current.Resources["AppFontFamily"] as System.Windows.Media.FontFamily;
                    if (fontFamily != null)
                    {
                        page.FontFamily = fontFamily;
                    }
                }

                if (System.Windows.Application.Current.Resources.Contains("AppFontSize"))
                {
                    var fontSize = System.Windows.Application.Current.Resources["AppFontSize"];
                    if (fontSize is double size)
                    {
                        page.FontSize = size;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Applied font to {page.GetType().Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font error: {ex.Message}");
            }
        }

    }
}