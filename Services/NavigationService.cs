// NavigationService.cs - Complete implementation
using BlueBerryDictionary.Views.Pages;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BlueBerryDictionary.Services
{
    public interface INavigationService
    {
        void NavigateTo(string pageTag, Page customPage = null, string uniqueId = null);
        void NavigateToPage(Page page, string pageName);
        void GoBack();
        void GoForward();
        bool CanGoBack { get; }
        bool CanGoForward { get; }
    }

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

        public NavigationService(
            Frame frame,
            Action<string> onWordClick,
            Action<object, System.Windows.RoutedEventArgs> sidebarNavigate = null)
        {
            _frame = frame;
            _onWordClick = onWordClick;
            _sidebarNavigate = sidebarNavigate;

            // Clear Frame's journal to prevent caching
            _frame.Navigated += (s, e) =>
            {
                while (_frame.CanGoBack)
                {
                    _frame.RemoveBackEntry();
                }
            };
        }

        public void NavigateTo(string pageTag, Page customPage = null, string uniqueId = null)
        {
            // Save current to back stack
            if (!string.IsNullOrEmpty(_currentPage) && _currentPage != pageTag)
            {
                _backStack.Push(_currentPage);
                _forwardStack.Clear(); // User navigated away
            }

            _currentPage = pageTag;

            // Create fresh page
            var page = (customPage != null ) ? customPage : CreatePage(pageTag);
            _frame.Navigate(page);

            System.Console.WriteLine($"📄 {pageTag} | Back: {_backStack.Count} | Forward: {_forwardStack.Count}");
        }

        public void GoBack()
        {
            if (!CanGoBack) return;

            _forwardStack.Push(_currentPage);
            _currentPage = _backStack.Pop();

            var page = CreatePage(_currentPage);
            _frame.Navigate(page);

            System.Console.WriteLine($"⬅️ {_currentPage} | Back: {_backStack.Count} | Forward: {_forwardStack.Count}");
        }

        public void GoForward()
        {
            if (!CanGoForward) return;

            _backStack.Push(_currentPage);
            _currentPage = _forwardStack.Pop();

            var page = CreatePage(_currentPage);
            _frame.Navigate(page);

            System.Console.WriteLine($"➡️ {_currentPage} | Back: {_backStack.Count} | Forward: {_forwardStack.Count}");
        }

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
                "Account" => new UserProfilePage() ,
                "UserProfile" => new UserProfilePage(),
                "Setting" => new SettingsPage() ,
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

            // Navigate to provided page instance
            _frame.Navigate(page);

            System.Diagnostics.Debug.WriteLine($"📄 {pageName} | Back: {_backStack.Count} | Forward: {_forwardStack.Count}");
        }
    }
}