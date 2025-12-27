using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BlueBerryDictionary.ViewModels;
using BlueBerryDictionary.Views.Dialogs;

namespace BlueBerryDictionary.Views.Pages
{
    public partial class GamePage : WordListPageBase
    {
        private GameViewModel _viewModel;

        public GamePage(Action<string> cardOnClicked) : base(cardOnClicked)
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
                var settings = settingsDialog.GameSettings;

                _viewModel.StartGame(
                    settings.Flashcards,
                    settings.DataSource,
                    settings.DataSourceName
                );

                GameSelectionPanel.Visibility = Visibility.Collapsed;
                GamePlayPanel.Visibility = Visibility.Visible;
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
            if (_viewModel.IsLastCard)
            {
                if (!_viewModel.KnownCards.Contains(_viewModel.CurrentCardIndex) && 
                    !_viewModel.SkippedCards.Contains(_viewModel.CurrentCardIndex))
                {
                    _viewModel.KnownCards.Add(_viewModel.CurrentCardIndex);
                }
        
                ShowCompletionDialog();
            }
            else
            {
                _viewModel.NextCard();
            }
        }

        private void SkipCard_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsLastCard)
            {
                _viewModel.SkipCurrentCard();
                ShowCompletionDialog();
            }
            else
            {
                _viewModel.SkipCurrentCard();
            }
        }

        private void ReviewSkipped_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.GoToFirstSkipped();
        }

        private void SkipNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int index)
            {
                _viewModel.GoToCard(index);
            }
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
                        break;

                    case GameCompletionDialog.CompletionAction.ReviewSkipped:
                        if (completionDialog.SelectedCardIndex.HasValue)
                        {
                            // Go to specific card
                            _viewModel.GoToCard(completionDialog.SelectedCardIndex.Value);
                        }
                        else
                        {
                            // Go to first skipped
                            _viewModel.GoToFirstSkipped();
                        }
                        break;
                }
            }
            else
            {
                // User closed or clicked Close button - return to game selection
                GamePlayPanel.Visibility = Visibility.Collapsed;
                GameSelectionPanel.Visibility = Visibility.Visible;
            }
        }

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit ? Progress will not be saved.",
                "Confirm Exit",
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