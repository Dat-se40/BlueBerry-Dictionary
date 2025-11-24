using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlueBerryDictionary.Pages
{
    public partial class MyWordsPage : Page
    {
        private string currentFilter = "All"; // Lưu filter hiện tại
        
        public MyWordsPage()
        {
            InitializeComponent();
            LoadWords("All"); // Load tất cả từ lúc đầu
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
            
            // Lọc và hiển thị từ
            LoadWords(selectedLetter);
            
            // Cập nhật header "A (2 words)"
            UpdateLetterHeader(selectedLetter);
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
    }
}