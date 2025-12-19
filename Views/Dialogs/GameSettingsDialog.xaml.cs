using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.Models;

namespace BlueBerryDictionary.Views.Dialogs
{
    public partial class GameSettingsDialog : Window
    {
        private readonly TagService _tagService;
        
        public class GameSettings
        {
            public List<WordShortened> Flashcards { get; set; }
            public string DataSource { get; set; }
            public string DataSourceName { get; set; }
        }

        public GameSettings Settings { get; private set; }

        private string _selectedDataSource = "All";
        private int _selectedCardCount = 10;

        public GameSettingsDialog()
        {
            InitializeComponent();
            _tagService = TagService.Instance;
            LoadDataSourceOptions();
            UpdateAvailableCardsInfo();
        }

        // ============ DATA SOURCE SELECTION ============

        private void LoadDataSourceOptions()
        {
            DataSourceOptions.Children.Clear();

            var allButtons = new List<Button>();

            // Add "All Words" option
            var btnAll = new Button
            {
                Content = "📚 All Words",
                Tag = "All",
                Style = (Style)FindResource("PopupItemStyle"),
                Margin = new Thickness(0)
            };
            btnAll.Click += DataSourceOption_Click;
            allButtons.Add(btnAll);

            // Add "Favorites" option
            var btnFav = new Button
            {
                Content = "⭐ Favorites",
                Tag = "Favorites",
                Style = (Style)FindResource("PopupItemStyle"),
                Margin = new Thickness(0)
            };
            btnFav.Click += DataSourceOption_Click;
            allButtons.Add(btnFav);

            // Add tags
            var tags = _tagService.GetAllTags();
            foreach (var tag in tags)
            {
                var btnTag = new Button
                {
                    Content = $"{tag.Icon} {tag.Name} ({tag.WordCount})",
                    Tag = tag.Id,
                    Style = (Style)FindResource("PopupItemStyle"),
                    Margin = new Thickness(0)
                };
                btnTag.Click += DataSourceOption_Click;
                allButtons.Add(btnTag);
            }

            // Apply styles
            for (int i = 0; i < allButtons.Count; i++)
            {
                if (i == 0)
                {
                    allButtons[i].Style = (Style)FindResource("PopupItemFirstStyle");
                }

                // Add separator after first 2 items
                if (i == 2)
                {
                    var separator = new Separator { Margin = new Thickness(0) };
                    DataSourceOptions.Children.Add(separator);
                }

                // Last item
                if (i == allButtons.Count - 1)
                {
                    allButtons[i].Style = (Style)FindResource("PopupItemLastStyle");
                }

                DataSourceOptions.Children.Add(allButtons[i]);
            }

            // Set default
            _selectedDataSource = "All";
            TxtSelectedSource.Text = "📚 All Words";
        }

        private void BtnDataSource_Click(object sender, RoutedEventArgs e)
        {
            PopupDataSource.IsOpen = !PopupDataSource.IsOpen;
            if (PopupDataSource.IsOpen)
            {
                PopupDataSource.Width = BtnDataSource.ActualWidth;
            }
        }

        private void DataSourceOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                TxtSelectedSource.Text = btn.Content.ToString();
                _selectedDataSource = btn.Tag.ToString();
                PopupDataSource.IsOpen = false;
                UpdateAvailableCardsInfo();
            }
        }

        private void UpdateAvailableCardsInfo()
        {
            var words = GetWordsFromSource();
            int availableCount = words.Count;

            TxtAvailableCards.Text = $"{availableCount} từ khả dụng";
            UpdateCardCountOptions(availableCount);
        }

        private void UpdateCardCountOptions(int maxCards)
        {
            if (_selectedCardCount > maxCards && maxCards > 0)
            {
                _selectedCardCount = Math.Max(1, maxCards);
                TxtSelectedCount.Text = $"{_selectedCardCount} thẻ";
            }
        }

        private List<WordShortened> GetWordsFromSource()
        {
            switch (_selectedDataSource)
            {
                case "All":
                    return _tagService.GetAllWords();
                case "Favorites":
                    return _tagService.GetFavoriteWords();
                default:
                    return _tagService.GetWordsByTag(_selectedDataSource);
            }
        }

        // ============ CARD COUNT SELECTION ============

        private void BtnCardCount_Click(object sender, RoutedEventArgs e)
        {
            PopupCardCount.IsOpen = !PopupCardCount.IsOpen;
            if (PopupCardCount.IsOpen)
            {
                PopupCardCount.Width = BtnCardCount.ActualWidth;
            }
        }

        private void CardCountOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                TxtSelectedCount.Text = btn.Content.ToString();
                _selectedCardCount = int.Parse(btn.Tag.ToString());
                PopupCardCount.IsOpen = false;
            }
        }

        // ============ ACTIONS ============

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            var sourceWords = GetWordsFromSource();

            if (sourceWords.Count == 0)
            {
                MessageBox.Show(
                    "Không có từ nào để học! Vui lòng chọn nguồn dữ liệu khác.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            var random = new Random();
            var flashcards = sourceWords
                .OrderBy(x => random.Next())
                .Take(Math.Min(_selectedCardCount, sourceWords.Count))
                .ToList();

            Settings = new GameSettings
            {
                Flashcards = flashcards,
                DataSource = _selectedDataSource,
                DataSourceName = TxtSelectedSource.Text
            };

            DialogResult = true;
            Close();
        }
    }
}