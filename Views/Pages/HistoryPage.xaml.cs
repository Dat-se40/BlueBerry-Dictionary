using BlueBerryDictionary.Data;
using BlueBerryDictionary.Views.Pages;
using BlueBerryDictionary.Views.UserControls;
using MyDictionary.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BlueBerryDictionary.Pages
{
    public partial class HistoryPage : WordListPageBase, INotifyPropertyChanged
    {
        WordCacheManager _wordCacheManager;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<CacheEntry> _historyItems ; 
        public ObservableCollection<CacheEntry> HistoryItems 
        {
            get { return _historyItems; }   
            set 
            {
                _historyItems = value;
                OnPropertyChanged(nameof(HistoryItems));
                LoadDefCards(); 
            }
        }
        public HistoryPage(Action<string> action) : base(action) 
        {
            InitializeComponent();
            _wordCacheManager = WordCacheManager.Instance; 
        }
        public void LoadCache() 
        {
            var caches = _wordCacheManager.GetAllCacheEntries();
            Console.WriteLine("[history] Caches.size ==" + caches.Count); 
            HistoryItems = new ObservableCollection<CacheEntry>(caches) ;
        }
        void LoadDefCards() 
        {
            mainContent.Children.Clear();
            foreach (var item in HistoryItems)
            {
                var newCard = new WordDefinitionCard(item._words[0]);
                newCard.TimeStamp = item._lastAccessed.ToShortTimeString();
                newCard.MouseDown += (s, e) =>
                {
                    base.HandleWordClick(newCard.Word);
                };
                mainContent.Children.Add(newCard);  
            }
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}