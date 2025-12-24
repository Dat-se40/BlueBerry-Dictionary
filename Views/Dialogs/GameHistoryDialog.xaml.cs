using BlueBerryDictionary.Services;
using System;
using System.Windows;
using System.Windows.Media;

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
            ApplyGlobalFont();
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
                "Bạn có chắc muốn xóa toàn bộ lịch sử?\nHành động này không thể hoàn tác.",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                _gameLogService.ClearAllSessions();
                LoadHistoryData();
                MessageBox.Show(
                    "✅ Đã xóa toàn bộ lịch sử!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Thêm font chữ
        /// </summary>
        private void ApplyGlobalFont()
        {
            try
            {
                if (Application.Current.Resources.Contains("AppFontFamily"))
                {
                    this.FontFamily = (FontFamily)Application.Current.Resources["AppFontFamily"];
                }

                if (Application.Current.Resources.Contains("AppFontSize"))
                {
                    this.FontSize = (double)Application.Current.Resources["AppFontSize"];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font to dialog error: {ex.Message}");
            }
        }
    }
}