using System;
using System.Collections.Generic;
using System.Linq;
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
                FlashcardContainer.Visibility = Visibility.Visible;
                CompletionContainer.Visibility = Visibility.Collapsed;
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
                ShowCompletion();
            }
            else
            {
                _viewModel.NextCard();
            }
        }

        private void SkipCard_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SkipCurrentCard();

            if (_viewModel.IsLastCard)
            {
                ShowCompletion();
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

        // ========== COMPLETION SCREEN ==========

        private void ShowCompletion()
        {
            FlashcardContainer.Visibility = Visibility.Collapsed;
            CompletionContainer.Visibility = Visibility.Visible;

            var completionData = _viewModel.CompleteGame();

            TxtPercentage.Text = $"{completionData.Percentage}%";
            TxtKnownCount.Text = $"{completionData.KnownCount} cards ({completionData.Percentage}%)";
            TxtUnknownCount.Text = $"{completionData.UnknownCount} cards ({100 - completionData.Percentage}%)";
            TxtTotalCount.Text = $"{completionData.TotalCount} cards";

            if (completionData.SkippedIndices.Count > 0)
            {
                Actions2Buttons.Visibility = Visibility.Collapsed;
                Actions3Buttons.Visibility = Visibility.Visible;
                SkippedListCompletion.Visibility = Visibility.Visible;
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
            _viewModel.RestartGame();
            CompletionContainer.Visibility = Visibility.Collapsed;
            FlashcardContainer.Visibility = Visibility.Visible;
        }

        private void ReviewSkippedOnly_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.GoToFirstSkipped();
            CompletionContainer.Visibility = Visibility.Collapsed;
            FlashcardContainer.Visibility = Visibility.Visible;
        }

        private void SkipNumberCompletion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int index)
            {
                _viewModel.GoToCard(index);
                CompletionContainer.Visibility = Visibility.Collapsed;
                FlashcardContainer.Visibility = Visibility.Visible;
            }
        }

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
