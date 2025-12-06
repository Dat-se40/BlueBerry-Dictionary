using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.Views.Pages;
using BlueBerryDictionary.Views.UserControls;
using MyDictionary.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            }
        }
        public HistoryPage(Action<string> action) : base(action) 
        {
            InitializeComponent();
            _wordCacheManager = WordCacheManager.Instance; 
        }
        public override void LoadData() 
        {
            var caches = _wordCacheManager.GetAllCacheEntries();
            Console.WriteLine("[history] Caches.size ==" + caches.Count); 
            HistoryItems = new ObservableCollection<CacheEntry>(caches) ;
            LoadDefCards();
        }
        void LoadDefCards() 
        {
            // Se lam lai sau!
            mainContent.Children.Clear();
            foreach (var item in HistoryItems)
            {
                var newCard = new WordDefinitionCard(item._words[0]);
                newCard.TimeStamp = item._lastAccessed.ToShortTimeString();
                newCard.MouseDown += (s, e) =>
                {
                    base.HandleWordClick(newCard.Word);
                };
                if (TagService.Instance.FindWordInsensitive(newCard.Word) is WordShortened ws)
                {
                    newCard.IsFavorite = ws.isFavorited;
                }
                mainContent.Children.Add(newCard);
            }

        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}