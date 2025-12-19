using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BlueBerryDictionary.Views.Pages;
using BlueBerryDictionary.Views.Dialogs;
using BlueBerryDictionary.ViewModels;

namespace BlueBerryDictionary.Pages
{
    public partial class GamePage : WordListPageBase
    {
        private GameViewModel _viewModel;

        public GamePage(Action<string> CardOnClicked) : base(CardOnClicked)
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            DataContext = _viewModel;
        }

        public override void LoadData()
        {
            // Refresh if needed
        }

        // ========== GAME SELECTION ==========

        private void GameCard_Click(object sender, MouseButtonEventArgs e)
        {
            var settingsDialog = new GameSettingsDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (settingsDialog.ShowDialog() == true)
            {
                var settings = settingsDialog.Settings;

                _viewModel.StartGame(
                    settings.Flashcards,
                    settings.DataSource,
                    settings.DataSourceName
                );

                GameSelectionPanel.Visibility = Visibility.Collapsed;
                GamePlayPanel.Visibility = Visibility.Visible;

                UpdateSkipTracker();
            }
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyDialog = new GameHistoryDialog
            {
                Owner = Window.GetWindow(this)
            };
            historyDialog.ShowDialog();
        }

        // ========== FLASHCARD ACTIONS ==========

        private void FlipCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsAnimating) return;

            _viewModel.IsAnimating = true;

            if (!_viewModel.IsFlipped)
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

        private void PreviousCard_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.PreviousCard();
        }

        private void NextCard_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.NextCard();
            UpdateSkipTracker();
            if (_viewModel.IsLastCard)
            {
                ShowCompletionDialog();
            }
            else
            {
                _viewModel.CurrentIndex++; // Hoặc _viewModel.NextCard() nhưng không đánh dấu Known
            }
        }

        private void SkipCard_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SkipCurrentCard();
            UpdateSkipTracker();

            if (_viewModel.IsLastCard)
            {
                ShowCompletionDialog();
            }
        }

        private void ReviewSkipped_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.GoToFirstSkipped();
        }

        // ========== FLIP ANIMATIONS ==========

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
            _viewModel.IsAnimating = false;
            _viewModel.IsFlipped = !_viewModel.IsFlipped;
        }

        // ========== SKIP TRACKER UI ==========

        private void UpdateSkipTracker()
        {
            SkipNumbersPanel.Children.Clear();

            if (!_viewModel.HasSkippedCards)
            {
                SkipTracker.Background = new SolidColorBrush(Color.FromRgb(209, 231, 221));
                SkipTracker.BorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                TxtSkipMessage.Foreground = new SolidColorBrush(Color.FromRgb(15, 81, 50));
                BtnReviewSkipped.Visibility = Visibility.Collapsed;
            }
            else
            {
                SkipTracker.Background = new SolidColorBrush(Color.FromRgb(255, 243, 205));
                SkipTracker.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                TxtSkipMessage.Foreground = new SolidColorBrush(Color.FromRgb(133, 100, 4));
                BtnReviewSkipped.Visibility = Visibility.Visible;

                var sortedSkipped = _viewModel.SkippedCards.OrderBy(x => x).ToList();
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
                        if (s is Button btn && btn.Tag is int cardIndex)
                        {
                            _viewModel.GoToCard(cardIndex);
                        }
                    };

                    SkipNumbersPanel.Children.Add(button);
                }
            }
        }

        // ========== COMPLETION DIALOG ==========

        private void ShowCompletionDialog()
        {
            var completionData = _viewModel.CompleteGame();

            var completionDialog = new GameCompletionDialog
            {
                Owner = Window.GetWindow(this)
            };

            completionDialog.SetCompletionData(
                completionData.Percentage,
                completionData.KnownCount,
                completionData.UnknownCount,
                completionData.TotalCount,
                completionData.SkippedIndices
            );

            var result = completionDialog.ShowDialog();

            if (result == true)
            {
                switch (completionDialog.UserAction)
                {
                    case GameCompletionDialog.CompletionAction.Restart:
                        _viewModel.RestartGame();
                        UpdateSkipTracker();
                        break;

                    case GameCompletionDialog.CompletionAction.ReviewSkipped:
                        if (completionDialog.SelectedCardIndex.HasValue)
                        {
                            _viewModel.GoToCard(completionDialog.SelectedCardIndex.Value);
                        }
                        else
                        {
                            _viewModel.GoToFirstSkipped();
                        }
                        break;
                }
            }
            else
            {
                // User closed - return to game selection
                GamePlayPanel.Visibility = Visibility.Collapsed;
                GameSelectionPanel.Visibility = Visibility.Visible;
            }
        }

        // ========== EXIT GAME ==========

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
            }
        }
    }
}