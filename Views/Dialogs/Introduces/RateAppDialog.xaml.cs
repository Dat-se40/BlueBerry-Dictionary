using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class RateAppDialog : Window
    {
        private int _selectedRating = 0;
        private const string GITHUB_REPO = "https://github.com/Dat-se40/BlueBerry-Dictionary";
        private const string EMAIL = "24520280@gm.uit.edu.vn";

        public RateAppDialog()
        {
            InitializeComponent();
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int rating))
            {
                _selectedRating = rating;
                UpdateStars(rating);
                UpdateRatingText(rating);
                SubmitButton.IsEnabled = true;
                FeedbackSection.Visibility = Visibility.Visible;
            }
        }

        private void UpdateStars(int rating)
        {
            // Reset all stars
            Star1.Text = "☆";
            Star2.Text = "☆";
            Star3.Text = "☆";
            Star4.Text = "☆";
            Star5.Text = "☆";

            Star1.Foreground = Brushes.Gray;
            Star2.Foreground = Brushes.Gray;
            Star3.Foreground = Brushes.Gray;
            Star4.Foreground = Brushes.Gray;
            Star5.Foreground = Brushes.Gray;

            // Fill selected stars
            var goldBrush = new SolidColorBrush(Color.FromRgb(255, 215, 0));

            if (rating >= 1) { Star1.Text = "★"; Star1.Foreground = goldBrush; }
            if (rating >= 2) { Star2.Text = "★"; Star2.Foreground = goldBrush; }
            if (rating >= 3) { Star3.Text = "★"; Star3.Foreground = goldBrush; }
            if (rating >= 4) { Star4.Text = "★"; Star4.Foreground = goldBrush; }
            if (rating >= 5) { Star5.Text = "★"; Star5.Foreground = goldBrush; }
        }

        private void UpdateRatingText(int rating)
        {
            RatingText.Text = rating switch
            {
                1 => "😢 Rất tệ - Chúng tôi sẽ cố gắng cải thiện",
                2 => "😕 Không tốt - Cần nhiều cải tiến",
                3 => "😐 Ổn - Có thể tốt hơn",
                4 => "😊 Tốt - Cảm ơn bạn!",
                5 => "🤩 Tuyệt vời - Bạn là người tuyệt vời!",
                _ => "Click vào ngôi sao để đánh giá"
            };
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRating == 0)
            {
                MessageBox.Show("Vui lòng chọn số sao đánh giá!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string feedback = FeedbackTextBox.Text.Trim();
            string subject = $"BlueBerry Dictionary - Đánh giá {_selectedRating} sao";
            string body = $"Đánh giá: {_selectedRating}/5 sao%0D%0A%0D%0ANhận xét:%0D%0A{feedback}";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{EMAIL}?subject={subject}&body={body}",
                    UseShellExecute = true
                });

                MessageBox.Show("Cảm ơn bạn đã đánh giá! 💙\n\nEmail client đã được mở. Vui lòng gửi email để hoàn tất đánh giá.",
                    "Cảm ơn", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở email client:\n{ex.Message}\n\nVui lòng gửi email thủ công đến: {EMAIL}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GitHubStar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = GITHUB_REPO,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở GitHub:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}