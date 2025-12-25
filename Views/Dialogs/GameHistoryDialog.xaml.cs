using System;
using System.Windows;
using BlueBerryDictionary.Services;

namespace BlueBerryDictionary.Views.Dialogs
{
    public partial class GameHistoryDialog : Window
    {
        private readonly GameLogService _gameLogService;

        public GameHistoryDialog()
        {
            InitializeComponent();
            _gameLogService = GameLogService.Instance;
            LoadHistoryData();
        }

        private void LoadHistoryData()
        {
            // Update statistics
            TxtTotalGames.Text = _gameLogService.GetTotalGamesPlayed().ToString();
            TxtTotalCards.Text = _gameLogService.GetTotalCardsStudied().ToString();
            TxtAvgAccuracy.Text = $"{_gameLogService.GetAverageAccuracy():F1}%";
            TxtTotalTime.Text = FormatTimeSpan(_gameLogService.GetTotalStudyTime());

            // Load sessions (recent 20)
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
                "Are you sure you want to delete all history ?\nThis action cannot be undone.",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                _gameLogService.ClearAllSessions();
                LoadHistoryData();
                MessageBox.Show(
                    "✅ All history has been deleted!",
                    "Completed successfully",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}