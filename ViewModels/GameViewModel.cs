using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;

namespace BlueBerryDictionary.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private readonly GameLogService _gameLogService;
        private readonly TagService _tagService;

        #region Properties

        private List<WordShortened> _flashcards = new List<WordShortened>();
        public List<WordShortened> Flashcards
        {
            get => _flashcards;
            set
            {
                _flashcards = value;
                OnPropertyChanged();
            }
        }

        private int _currentCardIndex;
        public int CurrentCardIndex
        {
            get => _currentCardIndex;
            set
            {
                _currentCardIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(IsLastCard));
                OnPropertyChanged(nameof(NextButtonText));
            }
        }

        private int _totalCards;
        public int TotalCards
        {
            get => _totalCards;
            set
            {
                _totalCards = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        private bool _isFlipped;
        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                _isFlipped = value;
                OnPropertyChanged();
            }
        }

        private bool _isAnimating;
        public bool IsAnimating
        {
            get => _isAnimating;
            set
            {
                _isAnimating = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<int> _skippedCards = new ObservableCollection<int>();
        public ObservableCollection<int> SkippedCards
        {
            get => _skippedCards;
            set
            {
                _skippedCards = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSkippedCards));
                OnPropertyChanged(nameof(SkipTrackerMessage));
            }
        }

        private List<int> _knownCards = new List<int>();
        public List<int> KnownCards
        {
            get => _knownCards;
            set
            {
                _knownCards = value;
                OnPropertyChanged();
            }
        }

        // Current Card Info
        public WordShortened CurrentCard =>
            Flashcards.Count > 0 && CurrentCardIndex < Flashcards.Count
                ? Flashcards[CurrentCardIndex]
                : null;

        // UI Computed Properties
        public string ProgressText => $"{CurrentCardIndex + 1}/{TotalCards}";
        public bool CanGoBack => CurrentCardIndex > 0;
        public bool IsLastCard => CurrentCardIndex >= TotalCards - 1;
        public string NextButtonText => IsLastCard ? "Finish ✓" : "Next (Known) ▶";
        public bool HasSkippedCards => SkippedCards.Count > 0;
        public string SkipTrackerMessage => HasSkippedCards ? "🚩 Skipped:" : "✅ No skipped cards yet!";

        // Session tracking
        private GameSession _currentSession;
        private DateTime _sessionStartTime;
        public string DataSourceName { get; set; }

        #endregion

        #region Constructor

        public GameViewModel()
        {
            _gameLogService = GameLogService.Instance;
            _tagService = TagService.Instance;
        }

        #endregion

        #region Game lifecycle

        /// <summary>
        /// Bắt đầu game với flashcards đã được chuẩn bị
        /// </summary>
        public void StartGame(List<WordShortened> cards, string dataSource, string dataSourceName)
        {
            Flashcards = cards;
            TotalCards = cards.Count;
            CurrentCardIndex = 0;
            IsFlipped = false;
            IsAnimating = false;
            SkippedCards.Clear();
            KnownCards.Clear();
            DataSourceName = dataSourceName;

            // Start session
            _sessionStartTime = DateTime.Now;
            _currentSession = new GameSession
            {
                StartTime = _sessionStartTime,
                DataSource = dataSource,
                DataSourceName = dataSourceName,
                TotalCards = TotalCards
            };

            OnPropertyChanged(nameof(CurrentCard));
        }

        /// <summary>
        /// Kết thúc game và lưu session
        /// </summary>
        public GameCompletionData CompleteGame()
        {
            int knownCount = KnownCards.Count;
            int unknownCount = SkippedCards.Count;
            int reviewedCount = TotalCards - (knownCount + unknownCount);
            knownCount += reviewedCount; // Auto-count reviewed as known

            int percentage = TotalCards > 0 ? (int)Math.Round((double)knownCount / TotalCards * 100) : 0;

            // Save session
            _currentSession.EndTime = DateTime.Now;
            _currentSession.Duration = _currentSession.EndTime - _currentSession.StartTime;
            _currentSession.KnownCards = knownCount;
            _currentSession.UnknownCards = unknownCount;
            _currentSession.AccuracyPercentage = percentage;
            _currentSession.SkippedCardIndices = SkippedCards.ToList();
            _currentSession.SkippedWords = SkippedCards
                .Select(idx => Flashcards[idx].Word)
                .ToList();

            _gameLogService.AddSession(_currentSession);

            return new GameCompletionData
            {
                Percentage = percentage,
                KnownCount = knownCount,
                UnknownCount = unknownCount,
                TotalCount = TotalCards,
                SkippedIndices = SkippedCards.ToList()
            };
        }

        #endregion

        #region Navigation

        public void GoToCard(int index)
        {
            if (index >= 0 && index < TotalCards)
            {
                CurrentCardIndex = index;
                IsFlipped = false;
                OnPropertyChanged(nameof(CurrentCard));
            }
        }

        public void NextCard()
        {
            // Đánh dấu known nếu chưa skip
            if (!KnownCards.Contains(CurrentCardIndex) && !SkippedCards.Contains(CurrentCardIndex))
            {
                KnownCards.Add(CurrentCardIndex);
            }

            // Xóa khỏi skipped nếu đã skip trước đó
            if (SkippedCards.Contains(CurrentCardIndex))
            {
                SkippedCards.Remove(CurrentCardIndex);
            }

            GoToCard(CurrentCardIndex + 1);
        }

        public void PreviousCard()
        {
            GoToCard(CurrentCardIndex - 1);
        }

        public void SkipCurrentCard()
        {
            // Đánh dấu skipped
            if (!SkippedCards.Contains(CurrentCardIndex))
            {
                SkippedCards.Add(CurrentCardIndex);
            }

            // Xóa khỏi known
            if (KnownCards.Contains(CurrentCardIndex))
            {
                KnownCards.Remove(CurrentCardIndex);
            }

            GoToCard(CurrentCardIndex + 1);
        }

        public void GoToFirstSkipped()
        {
            if (SkippedCards.Count > 0)
            {
                var firstSkipped = SkippedCards.OrderBy(x => x).First();
                GoToCard(firstSkipped);
            }
        }

        #endregion

        #region Flip animation
        public void ToggleFlip()
        {
            if (!IsAnimating)
            {
                IsFlipped = !IsFlipped;
            }
        }


        #endregion

        #region Restart
        public void RestartGame()
        {
            CurrentCardIndex = 0;
            SkippedCards.Clear();
            KnownCards.Clear();
            IsFlipped = false;
            IsAnimating = false;
            OnPropertyChanged(nameof(CurrentCard));
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    /// <summary>
    /// Data trả về khi hoàn thành game
    /// </summary>
    public class GameCompletionData
    {
        public int Percentage { get; set; }
        public int KnownCount { get; set; }
        public int UnknownCount { get; set; }
        public int TotalCount { get; set; }
        public List<int> SkippedIndices { get; set; }
    }
}