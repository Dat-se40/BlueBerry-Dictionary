using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BlueBerryDictionary.Pages
{
    public partial class HistoryPage : Page
    {
        private List<HistoryWord> allWords;
        private string currentFilter = "all";

        public HistoryPage()
        {
            InitializeComponent();
            LoadHistoryData();
            // DisplayHistory();
            // SetActiveFilter("all");
        }

        private void LoadHistoryData()
        {
            // Sample data - Replace with actual database query
            allWords = new List<HistoryWord>
            {
                // Today
                new HistoryWord
                {
                    Word = "Beautiful",
                    Phonetic = "/ˈbjuː.tɪ.fəl/",
                    Type = "Adjective",
                    Meaning = "Đẹp, xinh đẹp - Có vẻ đẹp làm hài lòng các giác quan hoặc tâm trí",
                    Time = DateTime.Now.AddHours(-2),
                    ViewCount = 3,
                    IsFavorite = false,
                    Date = DateTime.Today
                },
                new HistoryWord
                {
                    Word = "Serendipity",
                    Phonetic = "/ˌser.ənˈdɪp.ə.ti/",
                    Type = "Noun",
                    Meaning = "Sự tình cờ may mắn - Khả năng tìm thấy điều tốt đẹp một cách ngẫu nhiên",
                    Time = DateTime.Now.AddHours(-3),
                    ViewCount = 1,
                    IsFavorite = false,
                    Date = DateTime.Today
                },
                new HistoryWord
                {
                    Word = "Eloquent",
                    Phonetic = "/ˈel.ə.kwənt/",
                    Type = "Adjective",
                    Meaning = "Hùng hồn, lưu loát - Có khả năng diễn đạt bằng lời nói hoặc viết một cách trôi chảy",
                    Time = DateTime.Now.AddHours(-4),
                    ViewCount = 2,
                    IsFavorite = false,
                    Date = DateTime.Today
                },
                // Yesterday
                new HistoryWord
                {
                    Word = "Magnificent",
                    Phonetic = "/mæɡˈnɪf.ɪ.sənt/",
                    Type = "Adjective",
                    Meaning = "Tráng lệ, nguy nga - Cực kỳ đẹp, chi tiết và ấn tượng",
                    Time = DateTime.Today.AddDays(-1).AddHours(19),
                    ViewCount = 5,
                    IsFavorite = false,
                    Date = DateTime.Today.AddDays(-1)
                },
                new HistoryWord
                {
                    Word = "Perseverance",
                    Phonetic = "/ˌpɜː.sɪˈvɪə.rəns/",
                    Type = "Noun",
                    Meaning = "Sự kiên trì, bền bỉ - Tiếp tục làm điều gì đó mặc dù khó khăn",
                    Time = DateTime.Today.AddDays(-1).AddHours(15),
                    ViewCount = 2,
                    IsFavorite = false,
                    Date = DateTime.Today.AddDays(-1)
                },
                new HistoryWord
                {
                    Word = "Ephemeral",
                    Phonetic = "/ɪˈfem.ər.əl/",
                    Type = "Adjective",
                    Meaning = "Phù du, thoáng qua - Tồn tại trong một khoảng thời gian rất ngắn",
                    Time = DateTime.Today.AddDays(-1).AddHours(11),
                    ViewCount = 1,
                    IsFavorite = false,
                    Date = DateTime.Today.AddDays(-1)
                },
                new HistoryWord
                {
                    Word = "Ambiguous",
                    Phonetic = "/æmˈbɪɡ.ju.əs/",
                    Type = "Adjective",
                    Meaning = "Mơ hồ, không rõ ràng - Có nhiều hơn một ý nghĩa có thể có",
                    Time = DateTime.Today.AddDays(-1).AddHours(9),
                    ViewCount = 4,
                    IsFavorite = false,
                    Date = DateTime.Today.AddDays(-1)
                },
                // This Week
                new HistoryWord
                {
                    Word = "Enthusiasm",
                    Phonetic = "/ɪnˈθjuː.zi.æz.əm/",
                    Type = "Noun",
                    Meaning = "Sự nhiệt tình, hăng hái - Cảm giác hứng thú và háo hức mạnh mẽ",
                    Time = DateTime.Today.AddDays(-2).AddHours(18),
                    ViewCount = 3,
                    IsFavorite = false,
                    Date = DateTime.Today.AddDays(-2)
                },
                new HistoryWord
                {
                    Word = "Diligent",
                    Phonetic = "/ˈdɪl.ɪ.dʒənt/",
                    Type = "Adjective",
                    Meaning = "Siêng năng, cần cù - Làm việc chăm chỉ với sự cẩn thận và nỗ lực",
                    Time = DateTime.Today.AddDays(-3).AddHours(14),
                    ViewCount = 2,
                    IsFavorite = false,
                    Date = DateTime.Today.AddDays(-3)
                },
                new HistoryWord
                {
                    Word = "Resilient",
                    Phonetic = "/rɪˈzɪl.i.ənt/",
                    Type = "Adjective",
                    Meaning = "Kiên cường, bền bỉ - Có khả năng hồi phục nhanh chóng sau khó khăn",
                    Time = DateTime.Today.AddDays(-4).AddHours(10),
                    ViewCount = 1,
                    IsFavorite = false,
                    Date = DateTime.Today.AddDays(-4)
                }
            };
        
            UpdateStats();
        }

        private void UpdateStats()
        {
            TotalWordsText.Text = allWords.Count.ToString();
            TodayWordsText.Text = allWords.Count(w => w.Date == DateTime.Today).ToString();
            WeekWordsText.Text = allWords.Count(w => w.Date >= DateTime.Today.AddDays(-7)).ToString();
            MonthWordsText.Text = allWords.Count(w => w.Date >= DateTime.Today.AddDays(-30)).ToString();
        }

        private void DisplayHistory()
        {
            HistoryContent.Children.Clear();

            var filteredWords = FilterWords(currentFilter);

            if (filteredWords.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                return;
            }

            EmptyState.Visibility = Visibility.Collapsed;

            // Group by date
            var groupedWords = filteredWords
                .OrderByDescending(w => w.Time)
                .GroupBy(w => w.Date);

            foreach (var group in groupedWords)
            {
                // Add day divider
                AddDayDivider(group.Key);

                // Add words grid
                var grid = CreateWordsGrid(group.ToList());
                HistoryContent.Children.Add(grid);
            }
        }

        private List<HistoryWord> FilterWords(string filter)
        {
            switch (filter)
            {
                case "today":
                    return allWords.Where(w => w.Date == DateTime.Today).ToList();
                case "week":
                    return allWords.Where(w => w.Date >= DateTime.Today.AddDays(-7)).ToList();
                case "month":
                    return allWords.Where(w => w.Date >= DateTime.Today.AddDays(-30)).ToList();
                case "favorites":
                    return allWords.Where(w => w.IsFavorite).ToList();
                default:
                    return allWords.ToList();
            }
        }

        private void AddDayDivider(DateTime date)
        {
            var divider = new Grid
            {
                Height = 60,
                Margin = new Thickness(0, 40, 0, 25)
            };

            var line = new Border
            {
                Height = 2,
                Background = new LinearGradientBrush(
                    (Color)ColorConverter.ConvertFromString("#00FFFFFF"),
                    (Color)ColorConverter.ConvertFromString("#5b7fff"),
                    new Point(0, 0.5),
                    new Point(1, 0.5)),
                VerticalAlignment = VerticalAlignment.Center
            };

            var label = new Border
            {
                Background = new LinearGradientBrush(
                    (Color)ColorConverter.ConvertFromString("#2d4acc"),
                    (Color)ColorConverter.ConvertFromString("#5b7fff"),
                    45),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(25, 8, 25, 8),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var labelText = new TextBlock
            {
                Text = GetDateLabel(date),
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = Brushes.White
            };

            label.Child = labelText;
            divider.Children.Add(line);
            divider.Children.Add(label);
            HistoryContent.Children.Add(divider);
        }

        private string GetDateLabel(DateTime date)
        {
            if (date == DateTime.Today)
                return "📅 Hôm nay - " + date.ToString("dd/MM/yyyy");
            else if (date == DateTime.Today.AddDays(-1))
                return "📅 Hôm qua - " + date.ToString("dd/MM/yyyy");
            else if (date >= DateTime.Today.AddDays(-7))
                return "📅 Tuần này";
            else
                return "📅 " + date.ToString("dd/MM/yyyy");
        }

        private Grid CreateWordsGrid(List<HistoryWord> words)
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 30)
            };

            // Create columns for responsive layout
            for (int i = 0; i < 3; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            int row = 0, col = 0;

            foreach (var word in words)
            {
                var card = CreateWordCard(word);
                Grid.SetRow(card, row);
                Grid.SetColumn(card, col);

                grid.Children.Add(card);

                col++;
                if (col >= 3)
                {
                    col = 0;
                    row++;
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
            }

            return grid;
        }

        private Border CreateWordCard(HistoryWord word)
        {
            var card = new Border
            {
                Style = (Style)FindResource("HistoryCardStyle")
            };

            var mainStack = new StackPanel();

            // Header
            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Word Info
            var wordInfo = new StackPanel();
            wordInfo.Children.Add(new TextBlock
            {
                Text = word.Word,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("TextBrush"),
                Margin = new Thickness(0, 0, 0, 5)
            });
            wordInfo.Children.Add(new TextBlock
            {
                Text = word.Phonetic,
                FontSize = 16,
                Foreground = (Brush)FindResource("TextBrush"),
                Opacity = 0.8,
                Margin = new Thickness(0, 0, 0, 8)
            });
            wordInfo.Children.Add(new Border
            {
                Background = (Brush)FindResource("AccentBrush"),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12, 4, 12, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = word.Type,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.White
                }
            });

            Grid.SetColumn(wordInfo, 0);
            header.Children.Add(wordInfo);

            // Actions
            var actions = new StackPanel { Orientation = Orientation.Horizontal };
            
            // Favorite Button
            var favBtn = new Button
            {
                Style = (Style)FindResource("ActionButtonStyle"),
                Content = new TextBlock { Text = word.IsFavorite ? "❤️" : "🤍", FontSize = 20 },
                Tag = word,
                Margin = new Thickness(0, 0, 8, 0)
            };
            favBtn.Click += FavoriteBtn_Click;

            // Delete Button
            var delBtn = new Button
            {
                Style = (Style)FindResource("ActionButtonStyle"),
                Content = new TextBlock { Text = "🗑️", FontSize = 20 },
                Tag = word
            };
            delBtn.Click += DeleteBtn_Click;

            actions.Children.Add(favBtn);
            actions.Children.Add(delBtn);
            Grid.SetColumn(actions, 1);
            header.Children.Add(actions);

            mainStack.Children.Add(header);

            // Body
            var body = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(51, 128, 128, 128)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 15, 0, 0),
                Margin = new Thickness(0, 15, 0, 0)
            };

            var bodyStack = new StackPanel();
            bodyStack.Children.Add(new TextBlock
            {
                Text = word.Meaning,
                FontSize = 15,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 24,
                Foreground = (Brush)FindResource("TextBrush"),
                Margin = new Thickness(0, 0, 0, 12)
            });

            // Footer
            var footer = new Grid { Margin = new Thickness(0, 15, 0, 0) };
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Time
            var timeStack = new StackPanel { Orientation = Orientation.Horizontal };
            timeStack.Children.Add(new TextBlock { Text = "🕐", FontSize = 14, Margin = new Thickness(0, 0, 6, 0) });
            timeStack.Children.Add(new TextBlock
            {
                Text = word.Time.ToString("HH:mm"),
                FontSize = 13,
                Foreground = (Brush)FindResource("TextBrush"),
                Opacity = 0.7
            });
            Grid.SetColumn(timeStack, 0);
            footer.Children.Add(timeStack);

            // View Count
            var viewStack = new StackPanel { Orientation = Orientation.Horizontal };
            viewStack.Children.Add(new TextBlock { Text = "👁️", FontSize = 14, Margin = new Thickness(0, 0, 6, 0) });
            viewStack.Children.Add(new TextBlock
            {
                Text = $"{word.ViewCount} lần",
                FontSize = 13,
                Foreground = (Brush)FindResource("TextBrush"),
                Opacity = 0.7
            });
            Grid.SetColumn(viewStack, 1);
            footer.Children.Add(viewStack);

            bodyStack.Children.Add(footer);
            body.Child = bodyStack;
            mainStack.Children.Add(body);

            card.Child = mainStack;
            card.MouseLeftButtonDown += (s, e) => ViewWord(word);

            return card;
        }

        private void ViewWord(HistoryWord word)
        {
            ShowNotification($"🔍 Đang xem chi tiết từ \"{word.Word}\"...");
            // Navigate to word detail page
            // NavigationService?.Navigate(new WordDetailPage(word.Word));
        }

        private void FavoriteBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var word = button.Tag as HistoryWord;
            word.IsFavorite = !word.IsFavorite;

            var textBlock = button.Content as TextBlock;
            textBlock.Text = word.IsFavorite ? "❤️" : "🤍";

            ShowNotification(word.IsFavorite ? "❤️ Đã thêm vào yêu thích!" : "💔 Đã xóa khỏi yêu thích!");
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var word = button.Tag as HistoryWord;

            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa từ này khỏi lịch sử?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                allWords.Remove(word);
                DisplayHistory();
                UpdateStats();
                ShowNotification("🗑️ Đã xóa từ khỏi lịch sử!");
            }
        }

        private void FilterBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var filter = button.Tag.ToString();

            SetActiveFilter(filter);
            currentFilter = filter;
            DisplayHistory();
            ShowNotification($"🔍 Đang lọc: {GetFilterLabel(filter)}");
        }

        private void SetActiveFilter(string filter)
        {
            foreach (var child in FiltersPanel.Children)
            {
                if (child is Button btn)
                {
                    if (btn.Tag.ToString() == filter)
                    {
                        btn.Background = (Brush)FindResource("AccentBrush");
                        btn.Foreground = Brushes.White;
                    }
                    else
                    {
                        btn.Background = (Brush)FindResource("CardBackground");
                        btn.Foreground = (Brush)FindResource("AccentBrush");
                    }
                }
            }
        }

        private string GetFilterLabel(string filter)
        {
            return filter switch
            {
                "all" => "Tất cả",
                "today" => "Hôm nay",
                "week" => "Tuần này",
                "month" => "Tháng này",
                "favorites" => "Yêu thích",
                _ => "Tất cả"
            };
        }

        private void ClearAllBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa toàn bộ lịch sử? Hành động này không thể hoàn tác!",
                "Xác nhận xóa tất cả",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                allWords.Clear();
                DisplayHistory();
                UpdateStats();
                ShowNotification("🗑️ Đã xóa toàn bộ lịch sử!");
            }
        }

        private void ShowNotification(string message)
        {
            var notification = new Border
            {
                Background = new LinearGradientBrush(
                    (Color)ColorConverter.ConvertFromString("#2d4acc"),
                    (Color)ColorConverter.ConvertFromString("#5b7fff"),
                    45),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(25, 15, 25, 15),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 100, 30, 0),
                Child = new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14
                }
            };

            var parent = Application.Current.MainWindow.Content as Grid;
            if (parent != null)
            {
                parent.Children.Add(notification);
                Panel.SetZIndex(notification, 1000);

                var slideIn = new DoubleAnimation(400, 0, TimeSpan.FromMilliseconds(300));
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300)) { BeginTime = TimeSpan.FromSeconds(3) };

                var transform = new TranslateTransform();
                notification.RenderTransform = transform;

                transform.BeginAnimation(TranslateTransform.XProperty, slideIn);
                notification.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3.3)
                };
                timer.Tick += (s, args) =>
                {
                    parent.Children.Remove(notification);
                    timer.Stop();
                };
                timer.Start();
            }
        }
    }

    // Model class
    public class HistoryWord
    {
        public string Word { get; set; }
        public string Phonetic { get; set; }
        public string Type { get; set; }
        public string Meaning { get; set; }
        public DateTime Time { get; set; }
        public DateTime Date { get; set; }
        public int ViewCount { get; set; }
        public bool IsFavorite { get; set; }
    }
}