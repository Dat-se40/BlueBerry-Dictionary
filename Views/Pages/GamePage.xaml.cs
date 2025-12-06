using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BlueBerryDictionary.Views.Pages;

namespace BlueBerryDictionary.Pages
{
    public partial class GamePage : WordListPageBase
    {
        // Game State
        private int currentCardIndex = 0;
        private int totalCards = 0;
        private List<int> skippedCards = new List<int>();
        private List<int> knownCards = new List<int>();
        private bool isFlipped = false;
        private bool isAnimating = false;
        private int selectedCardCount = 10; // Default value

        // Sample flashcard data
        private List<Flashcard> flashcards = new List<Flashcard>
        {
            new Flashcard
            {
                Word = "Achievement",
                Phonetic = "/əˈtʃiːvmənt/",
                POS = "Noun",
                Meaning = "Thành tích, thành tựu",
                Example = "Her academic achievements were impressive.",
                ExampleVi = "Thành tích học tập của cô ấy rất ấn tượng."
            },
            new Flashcard
            {
                Word = "Abundant",
                Phonetic = "/əˈbʌndənt/",
                POS = "Adjective",
                Meaning = "Dồi dào, phong phú",
                Example = "The region has abundant natural resources.",
                ExampleVi = "Khu vực này có tài nguyên thiên nhiên dồi dào."
            },
            new Flashcard
            {
                Word = "Analyze",
                Phonetic = "/ˈæn.əl.aɪz/",
                POS = "Verb",
                Meaning = "Phân tích",
                Example = "We need to analyze the data carefully.",
                ExampleVi = "Chúng ta cần phân tích dữ liệu một cách cẩn thận."
            },
            new Flashcard
            {
                Word = "Benefit",
                Phonetic = "/ˈben.ɪ.fɪt/",
                POS = "Noun",
                Meaning = "Lợi ích, quyền lợi",
                Example = "The benefits of exercise are well known.",
                ExampleVi = "Lợi ích của việc tập thể dục đã được biết đến rộng rãi."
            },
            new Flashcard
            {
                Word = "Challenge",
                Phonetic = "/ˈtʃæl.ɪndʒ/",
                POS = "Noun",
                Meaning = "Thách thức, thử thách",
                Example = "This project presents many challenges.",
                ExampleVi = "Dự án này đặt ra nhiều thử thách."
            },
            new Flashcard
            {
                Word = "Develop",
                Phonetic = "/dɪˈvel.əp/",
                POS = "Verb",
                Meaning = "Phát triển, mở rộng",
                Example = "We need to develop new strategies.",
                ExampleVi = "Chúng ta cần phát triển các chiến lược mới."
            },
            new Flashcard
            {
                Word = "Environment",
                Phonetic = "/ɪnˈvaɪ.rən.mənt/",
                POS = "Noun",
                Meaning = "Môi trường",
                Example = "We must protect our environment.",
                ExampleVi = "Chúng ta phải bảo vệ môi trường của mình."
            },
            new Flashcard
            {
                Word = "Flexible",
                Phonetic = "/ˈflek.sə.bəl/",
                POS = "Adjective",
                Meaning = "Linh hoạt, mềm dẻo",
                Example = "The schedule is flexible and can be changed.",
                ExampleVi = "Lịch trình linh hoạt và có thể thay đổi."
            },
            new Flashcard
            {
                Word = "Generate",
                Phonetic = "/ˈdʒen.ə.reɪt/",
                POS = "Verb",
                Meaning = "Tạo ra, sinh ra",
                Example = "Solar panels generate electricity from sunlight.",
                ExampleVi = "Tấm pin mặt trời tạo ra điện từ ánh sáng mặt trời."
            },
            new Flashcard
            {
                Word = "Hypothesis",
                Phonetic = "/haɪˈpɒθ.ə.sɪs/",
                POS = "Noun",
                Meaning = "Giả thuyết",
                Example = "Scientists test their hypothesis through experiments.",
                ExampleVi = "Các nhà khoa học kiểm tra giả thuyết của họ thông qua thí nghiệm."
            }
        };

        public GamePage(Action<string> CardOnClicked) : base(CardOnClicked)
        {
            InitializeComponent();
            totalCards = flashcards.Count;
        }
        
        public override void LoadData()
        {
            // Implement if base class requires
            // Can be left empty if not needed
        }

        // ============ GAME SELECTION ===========
        private void GameCard_Click(object sender, MouseButtonEventArgs e)
        {
            GameSelectionPanel.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Visible;
        }

        // ============ SETTINGS ============

        private void BackToSelection_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Collapsed;
            GameSelectionPanel.Visibility = Visibility.Visible;
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            // Get number of cards from popup selection
            int requestedCards = selectedCardCount;

            // Limit to available cards
            totalCards = Math.Min(requestedCards, flashcards.Count);

            // Reset game state
            currentCardIndex = 0;
            skippedCards.Clear();
            knownCards.Clear();
            isFlipped = false;

            // Show game play area
            SettingsPanel.Visibility = Visibility.Collapsed;
            GamePlayPanel.Visibility = Visibility.Visible;
            FlashcardContainer.Visibility = Visibility.Visible;
            CompletionContainer.Visibility = Visibility.Collapsed;

            // Load first card
            LoadFlashcard(0);
            UpdateProgress();
            UpdateSkipTracker();
            UpdateNavigationButtons();
        }
        
        // ============ CARD COUNT POPUP ============

        private void BtnCardCount_Click(object sender, RoutedEventArgs e)
        {
            PopupCardCount.IsOpen = !PopupCardCount.IsOpen;
    
            // Set popup width bằng với button
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
            TxtPOS.Text = card.POS.ToUpper();
            TxtMeaning.Text = card.Meaning;
            TxtExample.Text = $"\"{card.Example}\"";

            UpdateProgress();
            UpdateNavigationButtons();
        }

        private void FlipCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (isAnimating) return;
    
            isAnimating = true;
    
            if (!isFlipped)
            {
                // Flip to back
                var storyboard = (Storyboard)FindResource("FlipToBackPhase1");
                storyboard.Begin(this);
            }
            else
            {
                // Flip to front
                var storyboard = (Storyboard)FindResource("FlipToFrontPhase1");
                storyboard.Begin(this);
            }
        }

        private void FlipToBackPhase1_Completed(object sender, EventArgs e)
        {
            // Khi scale về 0, đổi nội dung
            CardFront.Visibility = Visibility.Collapsed;
            CardBack.Visibility = Visibility.Visible;
    
            // Bắt đầu phase 2
            var storyboard = (Storyboard)FindResource("FlipToBackPhase2");
            storyboard.Begin(this);
        }
         
        private void FlipToFrontPhase1_Completed(object sender, EventArgs e)
        {
            // Khi scale về 0, đổi nội dung
            CardBack.Visibility = Visibility.Collapsed;
            CardFront.Visibility = Visibility.Visible;
    
            // Bắt đầu phase 2
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
                MessageBox.Show("✅ No skipped cards to review!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
            
            // Cards that were reviewed but not explicitly marked
            knownCount += reviewedCount;

            int percentage = totalCards > 0 ? (int)Math.Round((double)knownCount / totalCards * 100) : 0;

            // Update percentage
            TxtPercentage.Text = percentage + "%";

            // Update stats
            TxtKnownCount.Text = $"{knownCount} cards ({percentage}%)";
            TxtUnknownCount.Text = $"{unknownCount} cards ({100 - percentage}%)";
            TxtTotalCount.Text = $"{totalCards} cards";

            // Update skipped list
            SkippedNumbersCompletion.Children.Clear();

            if (skippedCards.Count > 0)
            {
                SkippedListCompletion.Visibility = Visibility.Visible;
                BtnReviewSkippedCompletion.Visibility = Visibility.Visible;

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
                SkippedListCompletion.Visibility = Visibility.Collapsed;
                BtnReviewSkippedCompletion.Visibility = Visibility.Collapsed;
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
                MessageBox.Show("✅ No skipped cards to review!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            CompletionContainer.Visibility = Visibility.Collapsed;
            FlashcardContainer.Visibility = Visibility.Visible;

            // Start from first skipped card
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
    }
    
    

    // ============ FLASHCARD MODEL ============
    public class Flashcard
    {
        public string Word { get; set; }
        public string Phonetic { get; set; }
        public string POS { get; set; }
        public string Meaning { get; set; }
        public string Example { get; set; }
        public string ExampleVi { get; set; }
    }
}