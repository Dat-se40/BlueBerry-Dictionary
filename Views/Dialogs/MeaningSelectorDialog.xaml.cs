    using BlueBerryDictionary.Models;
    using BlueBerryDictionary.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    namespace BlueBerryDictionary.Views.Dialogs
    {
        /// <summary>
        /// Dialog để chọn meaning và tags muốn lưu vào My Words
        /// </summary>
        public partial class MeaningSelectorDialog : Window
        {
            public int SelectedMeaningIndex { get; private set; } = 0;
            public List<string> SelectedTagIds { get; private set; } = new List<string>();
            public bool IsCancelled { get; private set; } = true;

            private Word _word;
            private TagService _tagService;
            private List<Tag> _availableTags;
            private int _currentStep = 1; // 1: Select Meaning, 2: Select Tags

            public MeaningSelectorDialog(Word word)
            {
                InitializeComponent();
                _word = word;
                _tagService = TagService.Instance;
                _availableTags = _tagService.GetAllTags();

                LoadMeanings();
                ApplyGlobalFont();
        }

            private void LoadMeanings()
            {
                if (_word?.meanings == null || _word.meanings.Count == 0)
                {
                    Close();
                    return;
                }

                // Set word title
                WordTitleText.Text = _word.word;
                StepIndicator.Text = "Step 1/2: Select meaning";

                // If only 1 meaning, auto-select and go to tags
                if (_word.meanings.Count == 1)
                {
                    SelectedMeaningIndex = 0;
                    ShowTagSelection();
                    return;
                }

                // Show meanings
                MeaningsPanel.Visibility = Visibility.Visible;
                TagsPanel.Visibility = Visibility.Collapsed;

                // Load meanings into list
                for (int i = 0; i < _word.meanings.Count; i++)
                {
                    var meaning = _word.meanings[i];
                    var item = CreateMeaningItem(meaning, i);
                    MeaningsContainer.Children.Add(item);
                }
            }

            private Border CreateMeaningItem(Meaning meaning, int index)
            {
                var border = new Border
                {
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(20),
                    Margin = new Thickness(0, 0, 0, 15),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = index
                };
                border.SetResourceReference(Border.BackgroundProperty, "CardBackground");
                border.SetResourceReference(Border.BorderBrushProperty, "BorderColor");

                // Hover effect
                border.MouseEnter += (s, e) =>
                {
                    border.SetResourceReference(Border.BackgroundProperty, "WordItemHover");
                };
                border.MouseLeave += (s, e) =>
                {
                    border.SetResourceReference(Border.BackgroundProperty, "CardBackground");
                };

                // Click handler
                border.MouseDown += (s, e) =>
                {
                    SelectedMeaningIndex = (int)border.Tag;
                    ShowTagSelection();
                };

                var stackPanel = new StackPanel();

                // Part of Speech Badge
                var posBadge = new Border
                {
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(12, 6, 12, 6),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                posBadge.SetResourceReference(Border.BackgroundProperty, "ToolButtonActive");

                var posText = new TextBlock
                {
                    Text = meaning.partOfSpeech?.ToUpper() ?? "UNKNOWN",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                };
                posBadge.Child = posText;
                stackPanel.Children.Add(posBadge);

                // Definitions (max 3)
                var definitions = meaning.definitions?.Take(3).ToList() ?? new List<Definition>();
                for (int defIdx = 0; defIdx < definitions.Count; defIdx++)
                {
                    var def = definitions[defIdx];
                    var defText = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14,
                        Margin = new Thickness(0, 0, 0, 8)
                    };
                    defText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");

                    defText.Inlines.Add(new System.Windows.Documents.Run($"{defIdx + 1}. ")
                    {
                        FontWeight = FontWeights.Bold
                    });
                    defText.Inlines.Add(new System.Windows.Documents.Run(def.definition));

                    stackPanel.Children.Add(defText);

                    // Example
                    if (!string.IsNullOrEmpty(def.example))
                    {
                        var exampleBorder = new Border
                        {
                            BorderThickness = new Thickness(3, 0, 0, 0),
                            Padding = new Thickness(12, 8, 12, 8),
                            Margin = new Thickness(20, 5, 0, 10)
                        };
                        exampleBorder.SetResourceReference(Border.BackgroundProperty, "ExampleBackground");
                        exampleBorder.SetResourceReference(Border.BorderBrushProperty, "ExampleBorder");

                        var exampleText = new TextBlock
                        {
                            Text = $"\"{def.example}\"",
                            FontStyle = FontStyles.Italic,
                            FontSize = 13,
                            Opacity = 0.9,
                            TextWrapping = TextWrapping.Wrap
                        };
                        exampleText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                        exampleBorder.Child = exampleText;

                        stackPanel.Children.Add(exampleBorder);
                    }
                }

                // Show total definition count if > 3
                if (meaning.definitions?.Count > 3)
                {
                    var moreText = new TextBlock
                    {
                        Text = $"+ {meaning.definitions.Count - 3} more definitions...",
                        FontSize = 13,
                        FontStyle = FontStyles.Italic,
                        Opacity = 0.7,
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    moreText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                    stackPanel.Children.Add(moreText);
                }

                border.Child = stackPanel;
                return border;
            }

            private void ShowTagSelection()
            {
                _currentStep = 2;
                StepIndicator.Text = "Step 2/2: Select a tag for this word (optional)";

                MeaningsPanel.Visibility = Visibility.Collapsed;
                TagsPanel.Visibility = Visibility.Visible;

                // Show selected meaning info
                var selectedMeaning = _word.meanings[SelectedMeaningIndex];
                SelectedMeaningText.Text = $"Selected meaning: {selectedMeaning.partOfSpeech}";

                LoadTags();
            }

            private void LoadTags()
            {
                TagsContainer.Children.Clear();

                foreach (var tag in _availableTags)
                {
                    var tagItem = CreateTagItem(tag);
                    TagsContainer.Children.Add(tagItem);
                }

                // Add "Create New Tag" button
                var createNewBtn = CreateNewTagButton();
                TagsContainer.Children.Add(createNewBtn);
            }

            private Border CreateTagItem(Tag tag)
            {
                var border = new Border
                {
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(25),
                    Padding = new Thickness(15, 10, 15, 10),
                    Margin = new Thickness(0, 0, 10, 10),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = tag.Id
                };
                border.SetResourceReference(Border.BackgroundProperty, "SearchBackground");
                border.SetResourceReference(Border.BorderBrushProperty, "BorderColor");

                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                // Icon
                var icon = new TextBlock
                {
                    Text = tag.Icon,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Name
                var name = new TextBlock
                {
                    Text = tag.Name,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                name.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");

                stackPanel.Children.Add(icon);
                stackPanel.Children.Add(name);
                border.Child = stackPanel;

                // Toggle selection
                border.MouseDown += (s, e) =>
                {
                    string tagId = border.Tag.ToString();

                    if (SelectedTagIds.Contains(tagId))
                    {
                        // Deselect
                        SelectedTagIds.Remove(tagId);
                        border.SetResourceReference(Border.BackgroundProperty, "SearchBackground");
                        border.SetResourceReference(Border.BorderBrushProperty, "BorderColor");
                    }
                    else
                    {
                        // Select
                        SelectedTagIds.Add(tagId);
                        border.SetResourceReference(Border.BackgroundProperty, "ToolButtonActive");
                        border.BorderBrush = Brushes.Transparent;
                        name.Foreground = Brushes.White;
                    }
                };

                return border;
            }

            private Border CreateNewTagButton()
            {
                var border = new Border
                {
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(25),
                    Padding = new Thickness(15, 10, 15, 10),
                    Margin = new Thickness(0, 0, 10, 10),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(45, 74, 204)),
                    Background = Brushes.Transparent
                };

                border.MouseEnter += (s, e) =>
                {
                    border.Background = new SolidColorBrush(Color.FromArgb(20, 45, 74, 204));
                };
                border.MouseLeave += (s, e) =>
                {
                    border.Background = Brushes.Transparent;
                };

                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                var icon = new TextBlock
                {
                    Text = "➕",
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var text = new TextBlock
                {
                    Text = "Create new tag",
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(45, 74, 204)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                stackPanel.Children.Add(icon);
                stackPanel.Children.Add(text);
                border.Child = stackPanel;

                border.MouseDown += (s, e) =>
                {
                    ShowCreateTagDialog();
                };

                return border;
            }

            private void ShowCreateTagDialog()
            {
                var dialog = new TagPickerDialog()
                {
                    Owner = this
                };

                if (dialog.ShowDialog() == true)
                {
                    // Reload tags
                    _availableTags = _tagService.GetAllTags();
                    LoadTags();

                    // Auto-select newly created tag
                    if (!string.IsNullOrEmpty(dialog.CreatedTagId))
                    {
                        SelectedTagIds.Add(dialog.CreatedTagId);
                    }
                }
            }

            private void BackButton_Click(object sender, RoutedEventArgs e)
            {
                if (_currentStep == 2)
                {
                    // Back to meaning selection
                    _currentStep = 1;
                    StepIndicator.Text = "Step 1/2: Select meaning";
                    MeaningsPanel.Visibility = Visibility.Visible;
                    TagsPanel.Visibility = Visibility.Collapsed;
                }
            }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            IsCancelled = false;

            try
            {
                var selectedMeaning = _word.meanings[SelectedMeaningIndex];

                // Sử dụng FromWord method với meaningIndex
                var wordShortened = WordShortened.FromWord(_word, SelectedMeaningIndex);

                if (wordShortened == null)
                {
                    MessageBox.Show("Error creating word. Please try again.");
                    return;
                }
                // Nếu không có example, tạo note tự động
                if (string.IsNullOrEmpty(wordShortened.Example))
                {
                    wordShortened.note = $"Part of Speech: {selectedMeaning.partOfSpeech.ToUpper()}\n" +
                                          $"Saved: {DateTime.Now:g}";
                }
                else
                {
                    // Có example, nhưng thêm metadata vào note (optional)
                    wordShortened.note = $"Meaning #{SelectedMeaningIndex + 1} - {selectedMeaning.partOfSpeech}";
                }

                // ========== SAVE TO SERVICE ==========
                TagService.Instance.AddNewWordShortened(wordShortened);

                Console.WriteLine($"✅ Saved: {wordShortened.Word} ({selectedMeaning.partOfSpeech})");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save error: {ex.Message}");
                MessageBox.Show($"Error saving word: {ex.Message}");
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
                IsCancelled = true;
                DialogResult = false;
                Close();
            }

        private void SkipTagsButton_Click(object sender, RoutedEventArgs e)
        {
                // Skip tags, just save the word
                IsCancelled = false;
                DialogResult = true;
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