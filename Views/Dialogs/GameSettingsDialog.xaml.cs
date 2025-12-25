using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;

namespace BlueBerryDictionary.Views.Dialogs
{
    public partial class GameSettingsDialog : Window
    {
        private readonly TagService _tagService;

        // Selected settings
        private string _selectedDataSource = "All";
        private Tag _selectedTag = null;
        private int _selectedCardCount = 10;

        // Return value
        public GameSettings GameSettings { get; private set; }

        public GameSettingsDialog()
        {
            InitializeComponent();
            _tagService = TagService.Instance;
            LoadDataSourceOptions();
        }

        // ========== DATA SOURCE ==========

        private void LoadDataSourceOptions()
        {
            DataSourceOptions.Children.Clear();

            var allButtons = new List<Button>();

            // All Words
            var btnAll = new Button
            {
                Content = "📚 All Words",
                Tag = "All",
                Style = (Style)FindResource("PopupItemStyle")
            };
            btnAll.Click += DataSourceOption_Click;
            allButtons.Add(btnAll);

            // Favorites
            var btnFav = new Button
            {
                Content = "⭐ Favorites",
                Tag = "Favorites",
                Style = (Style)FindResource("PopupItemStyle")
            };
            btnFav.Click += DataSourceOption_Click;
            allButtons.Add(btnFav);

            // Tags
            var tags = _tagService.GetAllTags();
            foreach (var tag in tags)
            {
                var btnTag = new Button
                {
                    Content = $"{tag.Icon} {tag.Name} ({tag.WordCount})",
                    Tag = tag.Id,
                    Style = (Style)FindResource("PopupItemStyle")
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

                // Separator after first 2 items
                if (i == 2 && allButtons.Count > 2)
                {
                    var separator = new Separator { Margin = new Thickness(0) };
                    DataSourceOptions.Children.Add(separator);
                }

                // Last item style
                if (i == allButtons.Count - 1)
                {
                    allButtons[i].Style = (Style)FindResource("PopupItemLastStyle");
                }

                DataSourceOptions.Children.Add(allButtons[i]);
            }

            UpdateAvailableCardsInfo();
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

                // Store tag if selected
                if (_selectedDataSource != "All" && _selectedDataSource != "Favorites")
                {
                    _selectedTag = _tagService.GetAllTags()
                        .FirstOrDefault(t => t.Id == _selectedDataSource);
                }
                else
                {
                    _selectedTag = null;
                }

                PopupDataSource.IsOpen = false;
                UpdateAvailableCardsInfo();
            }
        }

        private void UpdateAvailableCardsInfo()
        {
            var words = GetWordsFromSource();
            int availableCount = words.Count;

            TxtAvailableCards.Text = $"{availableCount} available words";

            // Update card count if exceeds available
            if (_selectedCardCount > availableCount)
            {
                _selectedCardCount = Math.Max(1, availableCount);
                TxtSelectedCount.Text = $"{_selectedCardCount} cards";
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

        // ========== CARD COUNT ==========

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

        // ========== ACTIONS ==========

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            var sourceWords = GetWordsFromSource();

            if (sourceWords.Count == 0)
            {
                MessageBox.Show(
                    "No words to study! Please select a different data source.",
                    "Notification",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            // Shuffle and take cards
            var random = new Random();
            var flashcards = sourceWords
                .OrderBy(x => random.Next())
                .Take(Math.Min(_selectedCardCount, sourceWords.Count))
                .ToList();

            // Create settings object
            GameSettings = new GameSettings
            {
                DataSource = _selectedDataSource,
                DataSourceName = TxtSelectedSource.Text,
                SelectedTag = _selectedTag,
                CardCount = _selectedCardCount,
                Flashcards = flashcards
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    // ========== HELPER CLASS ==========

    public class GameSettings
    {
        public string DataSource { get; set; }          // "All", "Favorites", "tagId"
        public string DataSourceName { get; set; }      // Display name
        public Tag SelectedTag { get; set; }            // null if not tag
        public int CardCount { get; set; }
        public List<WordShortened> Flashcards { get; set; }
    }
}