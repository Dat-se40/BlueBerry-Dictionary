using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BlueBerryDictionary.Views.Pages;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.Models;

namespace BlueBerryDictionary.Pages
{
    public partial class GamePage : WordListPageBase
    {
        private readonly GameLogService _gameLogService;
        private GameSession _currentSession;
        private DateTime _sessionStartTime;

        private readonly TagService _tagService;

        // Game State
        private int currentCardIndex = 0;
        private int totalCards = 0;
        private List<int> skippedCards = new List<int>();
        private List<int> knownCards = new List<int>();
        private bool isFlipped = false;
        private bool isAnimating = false;
        private int selectedCardCount = 10;

        // Data sources
        private List<WordShortened> flashcards = new List<WordShortened>();
        private string selectedDataSource = "All"; // "All", "Favorites", "TagId"
        private Tag selectedTag = null;

        public GamePage(Action<string> CardOnClicked) : base(CardOnClicked)
        {
            InitializeComponent();
            _tagService = TagService.Instance;
            _gameLogService = GameLogService.Instance;

            // Load tags into combo box
            LoadDataSourceOptions();
        }

        public override void LoadData()
        {
            // Can be used to refresh data if needed
        }

        // ============ DATA SOURCE SELECTION ============

        private void LoadDataSourceOptions()
        {
            DataSourceOptions.Children.Clear();
    
            // ✅ Khai báo List để lưu tất cả buttons
            var allButtons = new List<Button>();
    
            // Add "All Words" option
            var btnAll = new Button
            {
                Content = "📚 All Words",
                Tag = "All",
                Style = (Style)FindResource("PopupItemStyle"),
                Margin = new Thickness(0)
            };
            btnAll.Click += DataSourceOption_Click;
            allButtons.Add(btnAll);  // ✅ Thêm vào list
    
            // Add "Favorites" option
            var btnFav = new Button
            {
                Content = "⭐ Favorites",
                Tag = "Favorites",
                Style = (Style)FindResource("PopupItemStyle"),
                Margin = new Thickness(0)
            };
            btnFav.Click += DataSourceOption_Click;
            allButtons.Add(btnFav);  // ✅ Thêm vào list
    
            // Add tags
            var tags = _tagService.GetAllTags();
            foreach (var tag in tags)
            {
                var btnTag = new Button
                {
                    Content = $"{tag.Icon} {tag.Name} ({tag.WordCount})",
                    Tag = tag.Id,
                    Style = (Style)FindResource("PopupItemStyle"),
                    Margin = new Thickness(0)
                };
                btnTag.Click += DataSourceOption_Click;
                allButtons.Add(btnTag);  // ✅ Thêm vào list
            }
    
            // ✅ Add buttons vào UI
            for (int i = 0; i < allButtons.Count; i++)
            {
                if (i == 0)
                {
                    allButtons[i].Style = (Style)FindResource("PopupItemFirstStyle");
                }
                
                // Add separator sau 2 items đầu
                if (i == 2)
                {
                    var separator = new Separator { Margin = new Thickness(0) };
                    DataSourceOptions.Children.Add(separator);
                }
        
                // Item cuối cùng dùng LastStyle
                if (i == allButtons.Count - 1)
                {
                    allButtons[i].Style = (Style)FindResource("PopupItemLastStyle");
                }
        
                DataSourceOptions.Children.Add(allButtons[i]);
            }
    
            // Set default
            selectedDataSource = "All";
            TxtSelectedSource.Text = "📚 All Words";
            UpdateAvailableCardsInfo();
        }

        private void BtnDataSource_Click(object sender, RoutedEventArgs e)
        {
            PopupDataSource.IsOpen = !PopupDataSource.IsOpen;

            if (PopupDataSource.IsOpen)
            {
                PopupDataSource.Width = BtnDataSource.ActualWidth;
            }
        }

        private void DataSourceOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                TxtSelectedSource.Text = btn.Content.ToString();
                selectedDataSource = btn.Tag.ToString();
                PopupDataSource.IsOpen = false;
                UpdateAvailableCardsInfo();
            }
        }

        private void UpdateAvailableCardsInfo()
        {
            var words = GetWordsFromSource();
            int availableCount = words.Count;

            TxtAvailableCards.Text = $"{availableCount} từ khả dụng";

            // Update card count options based on available cards
            UpdateCardCountOptions(availableCount);
        }

        private void UpdateCardCountOptions(int maxCards)
        {
            // Limit card count selector to available cards
            var options = new[] { 5, 10, 15, 20, 30 };
            var validOptions = options.Where(x => x <= maxCards).ToList();

            if (maxCards < 5)
            {
                validOptions.Add(maxCards);
            }

            // Update selected count if it exceeds max
            if (selectedCardCount > maxCards)
            {
                selectedCardCount = Math.Max(1, maxCards);
                TxtSelectedCount.Text = $"{selectedCardCount} thẻ";
            }
        }

        private List<WordShortened> GetWordsFromSource()
        {
            switch (selectedDataSource)
            {
                case "All":
                    return _tagService.GetAllWords();

                case "Favorites":
                    return _tagService.GetFavoriteWords();

                default:
                    // It's a tag ID
                    return _tagService.GetWordsByTag(selectedDataSource);
            }
        }

        // ============ GAME SELECTION ===========

        private void GameCard_Click(object sender, MouseButtonEventArgs e)
        {
            GameSelectionPanel.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Visible;
            UpdateAvailableCardsInfo();
        }

        // ============ SETTINGS ============

        private void BackToSelection_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Collapsed;
            GameSelectionPanel.Visibility = Visibility.Visible;
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            var sourceWords = GetWordsFromSource();

            if (sourceWords.Count == 0)
            {
                MessageBox.Show(
                    "Không có từ nào để học! Vui lòng chọn nguồn dữ liệu khác.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            var random = new Random();
            flashcards = sourceWords
                .OrderBy(x => random.Next())
                .Take(Math.Min(selectedCardCount, sourceWords.Count))
                .ToList();

            totalCards = flashcards.Count;
            currentCardIndex = 0;
            skippedCards.Clear();
            knownCards.Clear();
            isFlipped = false;

            // ✅ Bắt đầu session mới
            _sessionStartTime = DateTime.Now;
            _currentSession = new GameSession
            {
                StartTime = _sessionStartTime,
                DataSource = selectedDataSource,
                DataSourceName = TxtSelectedSource.Text, // Lấy từ TextBlock
                TotalCards = totalCards
            };

            SettingsPanel.Visibility = Visibility.Collapsed;
            GamePlayPanel.Visibility = Visibility.Visible;
            FlashcardContainer.Visibility = Visibility.Visible;
            CompletionContainer.Visibility = Visibility.Collapsed;

            LoadFlashcard(0);
            UpdateProgress();
            UpdateSkipTracker();
            UpdateNavigationButtons();
        }

        // ============ CARD COUNT POPUP ============

        private void BtnCardCount_Click(object sender, RoutedEventArgs e)
        {
            PopupCardCount.IsOpen = !PopupCardCount.IsOpen;

            if (PopupCardCount.IsOpen)
            {
                PopupCardCount.Width = BtnCardCount.ActualWidth;
            }
        }

        private void CardCountOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                TxtSelectedCount.Text = btn.Content.ToString();
                selectedCardCount = int.Parse(btn.Tag.ToString());
                PopupCardCount.IsOpen = false;
            }
        }

        // ============ FLASHCARD FUNCTIONS ============

        private void LoadFlashcard(int index)
        {
            if (index < 0 || index >= totalCards) return;

            var card = flashcards[index];
            currentCardIndex = index;

            // Reset flip state
            isFlipped = false;
            isAnimating = false;
            CardFront.Visibility = Visibility.Visible;
            CardBack.Visibility = Visibility.Collapsed;
            FlipTransform.ScaleX = 1;

            // Update card content
            TxtWord.Text = card.Word;
            TxtPhonetic.Text = card.Phonetic;
            TxtPOS.Text = card.PartOfSpeech.ToUpper();
            TxtMeaning.Text = card.Definition;

            // Handle example
            if (!string.IsNullOrEmpty(card.Example))
            {
                TxtExample.Text = $"\"{card.Example}\"";
                TxtExample.Visibility = Visibility.Visible;
            }
            else
            {
                TxtExample.Visibility = Visibility.Collapsed;
            }

            UpdateProgress();
            UpdateNavigationButtons();
        }

        private void FlipCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (isAnimating) return;

            isAnimating = true;

            if (!isFlipped)
            {
                var storyboard = (Storyboard)FindResource("FlipToBackPhase1");
                storyboard.Begin(this);
            }
            else
            {
                var storyboard = (Storyboard)FindResource("FlipToFrontPhase1");
                storyboard.Begin(this);
            }
        }

        private void FlipToBackPhase1_Completed(object sender, EventArgs e)
        {
            CardFront.Visibility = Visibility.Collapsed;
            CardBack.Visibility = Visibility.Visible;

            var storyboard = (Storyboard)FindResource("FlipToBackPhase2");
            storyboard.Begin(this);
        }

        private void FlipToFrontPhase1_Completed(object sender, EventArgs e)
        {
            CardBack.Visibility = Visibility.Collapsed;
            CardFront.Visibility = Visibility.Visible;

            var storyboard = (Storyboard)FindResource("FlipToFrontPhase2");
            storyboard.Begin(this);
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            isAnimating = false;
            isFlipped = !isFlipped;
        }

        private void PreviousCard_Click(object sender, RoutedEventArgs e)
        {
            if (currentCardIndex > 0)
            {
                LoadFlashcard(currentCardIndex - 1);
            }
        }

        private void NextCard_Click(object sender, RoutedEventArgs e)
        {
            // Mark as known
            if (!knownCards.Contains(currentCardIndex) && !skippedCards.Contains(currentCardIndex))
            {
                knownCards.Add(currentCardIndex);
            }

            // Remove from skipped if was skipped before
            if (skippedCards.Contains(currentCardIndex))
            {
                skippedCards.Remove(currentCardIndex);
                UpdateSkipTracker();
            }

            // Check if last card
            if (currentCardIndex >= totalCards - 1)
            {
                ShowCompletion();
            }
            else
            {
                LoadFlashcard(currentCardIndex + 1);
            }
        }

        private void SkipCard_Click(object sender, RoutedEventArgs e)
        {
            // Mark as skipped (unknown)
            if (!skippedCards.Contains(currentCardIndex))
            {
                skippedCards.Add(currentCardIndex);
                UpdateSkipTracker();
            }

            // Remove from known if was known before
            if (knownCards.Contains(currentCardIndex))
            {
                knownCards.Remove(currentCardIndex);
            }

            // Move to next card
            if (currentCardIndex >= totalCards - 1)
            {
                ShowCompletion();
            }
            else
            {
                LoadFlashcard(currentCardIndex + 1);
            }
        }

        // ============ UI UPDATE FUNCTIONS ============

        private void UpdateProgress()
        {
            TxtProgress.Text = $"{currentCardIndex + 1}/{totalCards}";
        }

        private void UpdateNavigationButtons()
        {
            BtnPrevious.IsEnabled = currentCardIndex > 0;

            if (currentCardIndex >= totalCards - 1)
            {
                BtnNext.Content = "Finish ✓";
            }
            else
            {
                BtnNext.Content = "Next (Known) ▶";
            }
        }

        private void UpdateSkipTracker()
        {
            SkipNumbersPanel.Children.Clear();

            if (skippedCards.Count == 0)
            {
                SkipTracker.Background = new SolidColorBrush(Color.FromRgb(209, 231, 221));
                SkipTracker.BorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129));

                var dockPanel = SkipTracker.Child as DockPanel;
                if (dockPanel != null && dockPanel.Children.Count > 0)
                {
                    var textBlock = dockPanel.Children[0] as TextBlock;
                    if (textBlock != null)
                    {
                        textBlock.Text = "✅ No skipped cards yet!";
                        textBlock.Foreground = new SolidColorBrush(Color.FromRgb(15, 81, 50));
                    }
                }

                BtnReviewSkipped.Visibility = Visibility.Collapsed;
            }
            else
            {
                SkipTracker.Background = new SolidColorBrush(Color.FromRgb(255, 243, 205));
                SkipTracker.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));

                var dockPanel = SkipTracker.Child as DockPanel;
                if (dockPanel != null && dockPanel.Children.Count > 0)
                {
                    var textBlock = dockPanel.Children[0] as TextBlock;
                    if (textBlock != null)
                    {
                        textBlock.Text = "🚩 Skipped:";
                        textBlock.Foreground = new SolidColorBrush(Color.FromRgb(133, 100, 4));
                    }
                }

                BtnReviewSkipped.Visibility = Visibility.Visible;

                var sortedSkipped = skippedCards.OrderBy(x => x).ToList();
                foreach (var index in sortedSkipped)
                {
                    var button = new Button
                    {
                        Content = $"#{index + 1}",
                        Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                        Foreground = new SolidColorBrush(Color.FromRgb(133, 100, 4)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(224, 168, 0)),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(10, 4, 10, 4),
                        Margin = new Thickness(4),
                        FontWeight = FontWeights.SemiBold,
                        Cursor = Cursors.Hand,
                        Tag = index
                    };
                    
                    Style borderStyle = new Style(typeof(Border));
                    borderStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(5)));
                    button.Resources.Add(typeof(Border), borderStyle);

                    button.Click += (s, e) =>
                    {
                        var btn = s as Button;
                        if (btn != null)
                        {
                            int cardIndex = (int)btn.Tag;
                            LoadFlashcard(cardIndex);
                        }
                    };

                    SkipNumbersPanel.Children.Add(button);
                }
            }
        }

        private void ReviewSkipped_Click(object sender, RoutedEventArgs e)
        {
            if (skippedCards.Count == 0)
            {
                MessageBox.Show("✅ No skipped cards to review!", "Info", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var firstSkipped = skippedCards.OrderBy(x => x).First();
            LoadFlashcard(firstSkipped);
        }

        // ============ COMPLETION SCREEN ============

        private void ShowCompletion()
        {
            FlashcardContainer.Visibility = Visibility.Collapsed;
            CompletionContainer.Visibility = Visibility.Visible;

            int knownCount = knownCards.Count;
            int unknownCount = skippedCards.Count;
            int reviewedCount = totalCards - (knownCount + unknownCount);

            knownCount += reviewedCount;

            int percentage = totalCards > 0 ? (int)Math.Round((double)knownCount / totalCards * 100) : 0;

            TxtPercentage.Text = percentage + "%";
            TxtKnownCount.Text = $"{knownCount} cards ({percentage}%)";
            TxtUnknownCount.Text = $"{unknownCount} cards ({100 - percentage}%)";
            TxtTotalCount.Text = $"{totalCards} cards";

            // ✅ Lưu session vào log
            _currentSession.EndTime = DateTime.Now;
            _currentSession.Duration = _currentSession.EndTime - _currentSession.StartTime;
            _currentSession.KnownCards = knownCount;
            _currentSession.UnknownCards = unknownCount;
            _currentSession.AccuracyPercentage = percentage;
            _currentSession.SkippedCardIndices = new List<int>(skippedCards);

            // Lưu tên các từ bị skip
            _currentSession.SkippedWords = skippedCards
                .Select(idx => flashcards[idx].Word)
                .ToList();

            _gameLogService.AddSession(_currentSession);

            Console.WriteLine($"✅ Session saved: {percentage}% accuracy, {_currentSession.Duration.TotalSeconds}s");

            SkippedNumbersCompletion.Children.Clear();

            if (skippedCards.Count > 0)
            {
                Actions2Buttons.Visibility = Visibility.Collapsed;
                Actions3Buttons.Visibility = Visibility.Visible;
                SkippedListCompletion.Visibility = Visibility.Visible;

                var sortedSkipped = skippedCards.OrderBy(x => x).ToList();
                foreach (var index in sortedSkipped)
                {
                    var button = new Button
                    {
                        Content = $"#{index + 1}",
                        Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                        Foreground = new SolidColorBrush(Color.FromRgb(133, 100, 4)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(224, 168, 0)),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(10, 4, 10, 4),
                        Margin = new Thickness(4),
                        FontWeight = FontWeights.SemiBold,
                        Cursor = Cursors.Hand,
                        Tag = index
                    };
                    
                    Style borderStyle = new Style(typeof(Border));
                    borderStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(5)));
                    button.Resources.Add(typeof(Border), borderStyle);

                    button.Click += (s, e) =>
                    {
                        var btn = s as Button;
                        if (btn != null)
                        {
                            int cardIndex = (int)btn.Tag;
                            CompletionContainer.Visibility = Visibility.Collapsed;
                            FlashcardContainer.Visibility = Visibility.Visible;
                            LoadFlashcard(cardIndex);
                        }
                    };

                    SkippedNumbersCompletion.Children.Add(button);
                }
            }
            else
            {
                Actions3Buttons.Visibility = Visibility.Collapsed;
                Actions2Buttons.Visibility = Visibility.Visible;
                SkippedListCompletion.Visibility = Visibility.Collapsed;
            }
        }

        private void RestartGame_Click(object sender, RoutedEventArgs e)
        {
            CompletionContainer.Visibility = Visibility.Collapsed;
            FlashcardContainer.Visibility = Visibility.Visible;

            currentCardIndex = 0;
            skippedCards.Clear();
            knownCards.Clear();
            isFlipped = false;

            LoadFlashcard(0);
            UpdateSkipTracker();
            UpdateNavigationButtons();
        }

        private void ReviewSkippedOnly_Click(object sender, RoutedEventArgs e)
        {
            if (skippedCards.Count == 0)
            {
                MessageBox.Show("✅ No skipped cards to review!", "Info", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            CompletionContainer.Visibility = Visibility.Collapsed;
            FlashcardContainer.Visibility = Visibility.Visible;

            var firstSkipped = skippedCards.OrderBy(x => x).First();
            LoadFlashcard(firstSkipped);
        }

        // ============ EXIT GAME ============

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn thoát? Tiến trình sẽ không được lưu.",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                GamePlayPanel.Visibility = Visibility.Collapsed;
                GameSelectionPanel.Visibility = Visibility.Visible;
                CompletionContainer.Visibility = Visibility.Collapsed;
                FlashcardContainer.Visibility = Visibility.Collapsed;
            }
        }

        // ============ HISTORY ============

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            LoadHistoryData();
            HistoryOverlay.Visibility = Visibility.Visible;
        }

        private void CloseHistory_Click(object sender, RoutedEventArgs e)
        {
            HistoryOverlay.Visibility = Visibility.Collapsed;
        }

        private void LoadHistoryData()
        {
            // Update statistics
            TxtHistoryTotalGames.Text = _gameLogService.GetTotalGamesPlayed().ToString();
            TxtHistoryTotalCards.Text = _gameLogService.GetTotalCardsStudied().ToString();
            TxtHistoryAvgAccuracy.Text = $"{_gameLogService.GetAverageAccuracy():F1}%";
            TxtHistoryTotalTime.Text = FormatTimeSpan(_gameLogService.GetTotalStudyTime());
    
            var sessions = _gameLogService.GetRecentSessions(20);
            
            HistoryList.ItemsSource = sessions;
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            else if (ts.TotalMinutes >= 1)
                return $"{ts.Minutes}m {ts.Seconds}s";
            else
                return $"{ts.Seconds}s";
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa toàn bộ lịch sử?\nHành động này không thể hoàn tác.",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                _gameLogService.ClearAllSessions();
                LoadHistoryData();
                MessageBox.Show("✅ Đã xóa toàn bộ lịch sử!", "Thành công", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}