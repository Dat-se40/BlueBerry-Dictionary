using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.ViewModels;
using BlueBerryDictionary.Views.Pages;
using BlueBerryDictionary.Views.UserControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace BlueBerryDictionary.Pages
{
    public partial class MyWordsPage : WordListPageBase
    {
        private string currentFilter = "All"; // Lưu filter hiện tại
        private MyWordsViewModel myWordsViewModel;
       
        public MyWordsPage(Action<string> CardOnClicked) : base(CardOnClicked)
        {
            InitializeComponent();
            myWordsViewModel = new MyWordsViewModel();  
            this.DataContext = myWordsViewModel;
            myWordsViewModel.acOnFilterWordsChanged += this.LoadDefCards;
            myWordsViewModel.acOnTagChanged += this.LoadTags;
            LoadData(); 
        }
        public override void LoadData() 
        {
            LoadTags();
            LoadDefCards();
        }
        // Event handler cho các alphabet buttons
        private void AlphabetButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null) return;

            string selectedLetter = clickedButton.Tag.ToString();
            currentFilter = selectedLetter;

            // Cập nhật trạng thái Active/Inactive cho các button
            UpdateAlphabetButtons(selectedLetter);
            if (myWordsViewModel.FilterByLetterCommand.CanExecute(selectedLetter)) myWordsViewModel.FilterByLetterCommand.Execute(selectedLetter);
        }

        // Cập nhật trạng thái các button (Active/Inactive)
        private void UpdateAlphabetButtons(string activeLetter)
        {
            // Tìm tất cả các button alphabet trong StackPanel
            var stackPanel = FindVisualChild<StackPanel>(this);
            if (stackPanel == null) return;

            foreach (Button btn in stackPanel.Children.OfType<Button>())
            {
                if (btn.Tag.ToString() == activeLetter)
                {
                    btn.Tag = "Active";
                }
                else
                {
                    // Kiểm tra xem chữ cái này có từ không
                    bool hasWords = CheckIfLetterHasWords(btn.Tag.ToString());
                    btn.Tag = hasWords ? "Inactive" : "Inactive";
                }
            }
        }
        public  void LoadDefCards() 
        {
            var upload = myWordsViewModel.FilteredWords.Where(ws => ws.Tags.Count != 0 || ws.isFavorited == true);

            base.LoadDefCards(mainContent, upload);
            foreach (var child in mainContent.Children)
            {
                if (child is WordDefinitionCard wdc)
                {
                    wdc.DeleteWord += () => 
                    {
                        myWordsViewModel.FilteredWords.Remove(wdc._mainWord);
                        myWordsViewModel.UpdateStatistics(); 
                    };
                }
            }

        }
        private void LoadTags() 
        {
            stpTags.Children.Clear(); 
            var tags = myWordsViewModel.Tags;
            foreach (var tag in tags) stpTags.Children.Add(CreateTagItem(tag));
        }
        private Border CreateTagItem(Tag tag)
        {
            var border = new Border
            {
                Padding = new Thickness(15, 12,0,0),
                Margin = new Thickness(0, 0, 0, 5),
                CornerRadius = new CornerRadius(10),
                Cursor = Cursors.Hand
            };

            border.SetResourceReference(Border.BackgroundProperty, "Transparent");

            // Hover effect
            var style = new Style(typeof(Border));
            var trigger = new Trigger
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true
            };
            trigger.Setters.Add(new Setter(
                Border.BackgroundProperty,
                Application.Current.Resources["WordItemHover"]
            ));
            style.Triggers.Add(trigger);
            border.Style = style;

            // ===== GRID =====
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            // Icon
            var icon = new TextBlock
            {
                Text = tag.Icon ?? "📚",
                FontSize = 20,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Texts
            var textPanel = new StackPanel();

            var nameText = new TextBlock
            {
                Text = tag.Name,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };
            nameText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");

            var countText = new TextBlock
            {
                Name = "tbCount", 
                Text = $"{tag.WordCount} từ",
                Tag = tag.Id,
                FontSize = 13,
                Opacity = 0.7
            };
            countText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            countText.SetBinding(TextBlock.TextProperty,new Binding("WordCount") { StringFormat = "{0} từ" });
            countText.DataContext = tag;

            textPanel.Children.Add(nameText);
            textPanel.Children.Add(countText);

            // Layout
            Grid.SetColumn(icon, 0);
            Grid.SetColumn(textPanel, 1);

            grid.Children.Add(icon);
            grid.Children.Add(textPanel);
            border.Child = grid;
            border.MouseDown += (s, e) =>
            {
                if (myWordsViewModel.FilterByTagCommand.CanExecute(tag))  myWordsViewModel.FilterByTagCommand.Execute(tag);
            };
            return border;
        }

        // Load từ theo filter
        private void LoadWords(string letter)
        {
            // TODO: Lấy danh sách từ từ database/collection
            // Ví dụ:
            // var filteredWords = letter == "All" 
            //     ? allWords 
            //     : allWords.Where(w => w.Word.StartsWith(letter, StringComparison.OrdinalIgnoreCase));

            // TODO: Cập nhật UniformGrid với các word cards
            // WordsGrid.Children.Clear();
            // foreach (var word in filteredWords)
            // {
            //     WordsGrid.Children.Add(CreateWordCard(word));
            // }
        }

        // Cập nhật header "A (2 words)"
        private void UpdateLetterHeader(string letter)
        {
            // TODO: Tìm TextBlock header và cập nhật
            // int wordCount = GetWordCountForLetter(letter);
            // LetterHeaderText.Text = letter;
            // WordCountText.Text = $"({wordCount} words)";
        }

        // Kiểm tra chữ cái có từ không
        private bool CheckIfLetterHasWords(string letter)
        {
            if (letter == "All") return true;

            // TODO: Kiểm tra trong database/collection
            // return allWords.Any(w => w.Word.StartsWith(letter, StringComparison.OrdinalIgnoreCase));

            // Tạm thời hardcode
            return letter == "A" || letter == "B" || letter == "K";
        }

        // Helper method để tìm child control
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                    return (T)child;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void PartOfSpeechBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;  
            if (btn != null && myWordsViewModel.FilterByPartOfSpeechCommand.CanExecute(btn.Tag)) myWordsViewModel.FilterByPartOfSpeechCommand.Execute(btn.Tag);
        }
    }
}