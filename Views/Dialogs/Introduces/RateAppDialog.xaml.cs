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
            ApplyGlobalFont();
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
                1 => "😢 Very bad – We’ll try to improve",
                2 => "😕 Not good – Needs a lot of improvement",
                3 => "😐 Okay – Could be better",
                4 => "😊 Good – Thank you!",
                5 => "🤩 Awesome – You’re amazing!",
                _ => "Tap a star to rate"
            };
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRating == 0)
            {
                MessageBox.Show("Please select a star rating!", "Notification",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string feedback = FeedbackTextBox.Text.Trim();
            string subject = $"BlueBerry Dictionary - {_selectedRating}-star rating";
            string body = $"Rating: {_selectedRating}/5 stars%0D%0A%0D%0A" + $"Feedback:%0D%0A{feedback}";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{EMAIL}?subject={subject}&body={body}",
                    UseShellExecute = true
                });

                MessageBox.Show("Thank you for your rating! 💙\n\nYour email client has been opened. Please send the email to complete your review.",
                    "Thank you", MessageBoxButton.OK, MessageBoxImage.Information);


                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open the email client:\n{ex.Message}\n\nPlease send the email manually to: {EMAIL}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

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
                MessageBox.Show($"Cannot open GitHub:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Apply font từ App.Current.Resources
        /// </summary>
        private void ApplyGlobalFont()
        {
            try
            {
                if (Application.Current.Resources.Contains("AppFontFamily"))
                {
                    this.FontFamily = (System.Windows.Media.FontFamily)Application.Current.Resources["AppFontFamily"];
                }

                if (Application.Current.Resources.Contains("AppFontSize"))
                {
                    this.FontSize = (double)Application.Current.Resources["AppFontSize"];
                }

                System.Diagnostics.Debug.WriteLine($"✅ Applied font to {this.GetType().Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font to dialog error: {ex.Message}");
            }
        }

    }
}