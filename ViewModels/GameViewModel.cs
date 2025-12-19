using System;
using System.Collections.Generic;
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
        
        // Game State
        private List<WordShortened> _flashcards;
        private int _currentCardIndex;
        private List<int> _skippedCards;
        private List<int> _knownCards;
        private bool _isFlipped;
        private bool _isAnimating;
        
        // Session Data
        private GameSession _currentSession;
        private DateTime _sessionStartTime;

        public GameViewModel()
        {
            _gameLogService = GameLogService.Instance;
            _flashcards = new List<WordShortened>();
            _skippedCards = new List<int>();
            _knownCards = new List<int>();
        }

        // ============ PROPERTIES ============

        public WordShortened CurrentCard => 
            _flashcards != null && _currentCardIndex < _flashcards.Count 
            ? _flashcards[_currentCardIndex] 
            : null;

        public int CurrentIndex
        {
            get => _currentCardIndex;
            set
            {
                _currentCardIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentCard));
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(IsLastCard));
                OnPropertyChanged(nameof(NextButtonText));
            }
        }

        public int TotalCards => _flashcards?.Count ?? 0;

        public string ProgressText => $"{CurrentIndex + 1}/{TotalCards}";

        public bool CanGoBack => CurrentIndex > 0;

        public bool IsLastCard => CurrentIndex >= TotalCards - 1;

        public string NextButtonText => IsLastCard ? "Finish ✓" : "Next (Known) ▶";

        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                _isFlipped = value;
                OnPropertyChanged();
            }
        }

        public bool IsAnimating
        {
            get => _isAnimating;
            set
            {
                _isAnimating = value;
                OnPropertyChanged();
            }
        }

        public List<int> SkippedCards => _skippedCards;

        public bool HasSkippedCards => _skippedCards.Count > 0;

        public string SkipTrackerMessage => 
            HasSkippedCards ? "🚩 Skipped:" : "✅ No skipped cards yet!";

        // ============ GAME CONTROL ============

        public void StartGame(List<WordShortened> flashcards, string dataSource, string dataSourceName)
        {
            _flashcards = flashcards;
            _currentCardIndex = 0;
            _skippedCards.Clear();
            _knownCards.Clear();
            _isFlipped = false;

            // Start new session
            _sessionStartTime = DateTime.Now;
            _currentSession = new GameSession
            {
                StartTime = _sessionStartTime,
                DataSource = dataSource,
                DataSourceName = dataSourceName,
                TotalCards = TotalCards
            };

            OnPropertyChanged(nameof(CurrentCard));
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(IsLastCard));
            OnPropertyChanged(nameof(NextButtonText));
            OnPropertyChanged(nameof(SkipTrackerMessage));
        }

        public void NextCard()
        {
            // Mark as known
            if (!_knownCards.Contains(CurrentIndex) && !_skippedCards.Contains(CurrentIndex))
            {
                _knownCards.Add(CurrentIndex);
            }

            // Remove from skipped if was skipped before
            if (_skippedCards.Contains(CurrentIndex))
            {
                _skippedCards.Remove(CurrentIndex);
                OnPropertyChanged(nameof(HasSkippedCards));
                OnPropertyChanged(nameof(SkipTrackerMessage));
            }

            if (CurrentIndex < TotalCards - 1)
            {
                CurrentIndex++;
            }
        }

        public void PreviousCard()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
        }

        public void SkipCurrentCard()
        {
            // Mark as skipped (unknown)
            if (!_skippedCards.Contains(CurrentIndex))
            {
                _skippedCards.Add(CurrentIndex);
                OnPropertyChanged(nameof(HasSkippedCards));
                OnPropertyChanged(nameof(SkipTrackerMessage));
            }

            // Remove from known if was known before
            if (_knownCards.Contains(CurrentIndex))
            {
                _knownCards.Remove(CurrentIndex);
            }
        }

        public void GoToCard(int index)
        {
            if (index >= 0 && index < TotalCards)
            {
                CurrentIndex = index;
            }
        }

        public void GoToFirstSkipped()
        {
            if (_skippedCards.Count > 0)
            {
                var firstSkipped = _skippedCards.OrderBy(x => x).First();
                GoToCard(firstSkipped);
            }
        }

        public void RestartGame()
        {
            CurrentIndex = 0;
            _skippedCards.Clear();
            _knownCards.Clear();
            _isFlipped = false;

            OnPropertyChanged(nameof(HasSkippedCards));
            OnPropertyChanged(nameof(SkipTrackerMessage));
        }

        // ============ COMPLETION ============

        public class CompletionData
        {
            public int Percentage { get; set; }
            public int KnownCount { get; set; }
            public int UnknownCount { get; set; }
            public int TotalCount { get; set; }
            public List<int> SkippedIndices { get; set; }
        }

        public CompletionData CompleteGame()
        {
            int knownCount = _knownCards.Count;
            int unknownCount = _skippedCards.Count;
            int reviewedCount = TotalCards - (knownCount + unknownCount);

            int percentage = TotalCards > 0 
                ? (int)Math.Round((double)knownCount / TotalCards * 100) 
                : 0;

            // Save session
            _currentSession.EndTime = DateTime.Now;
            _currentSession.Duration = _currentSession.EndTime - _currentSession.StartTime;
            _currentSession.KnownCards = knownCount;
            _currentSession.UnknownCards = unknownCount;
            _currentSession.AccuracyPercentage = percentage;
            _currentSession.SkippedCardIndices = new List<int>(_skippedCards);

            // Save skipped words
            _currentSession.SkippedWords = _skippedCards
                .Select(idx => _flashcards[idx].Word)
                .ToList();

            _gameLogService.AddSession(_currentSession);

            return new CompletionData
            {
                Percentage = percentage,
                KnownCount = knownCount,
                UnknownCount = unknownCount,
                TotalCount = TotalCards,
                SkippedIndices = new List<int>(_skippedCards)
            };
        }

        // ============ INotifyPropertyChanged ============

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}