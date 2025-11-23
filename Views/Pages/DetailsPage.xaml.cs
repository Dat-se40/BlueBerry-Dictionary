
using BlueBerryDictionary.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Pages
{
    public partial class DetailsPage : Page, IDisposable
    {
        private Word _word;
        private MediaPlayer _mediaPlayer;
        private bool _isFavorite = false;
        private bool _disposed = false;

        // Audio URLs
        private string _usAudioUrl;
        private string _ukAudioUrl;

        public DetailsPage(Word word)
        {
            InitializeComponent();
            _word = word;
            _mediaPlayer = new MediaPlayer();

            // Subscribe to Unloaded event
            this.Unloaded += DetailsPage_Unloaded;

            LoadWordData();
        }

        private void LoadWordData()
        {
            if (_word == null) return;

            // Hiển thị word title
            WordTitle.Text = CapitalizeFirstLetter(_word.word);

            // Load phonetics
            LoadPhonetics();

            // Load meanings
            LoadMeanings();
        }

        private void LoadPhonetics()
        {
            var phonetics = _word.phonetics ?? new System.Collections.Generic.List<Phonetic>();

            // Tìm US phonetic
            var usPhonetic = phonetics.FirstOrDefault(p => p.text != null && p.text.Contains("ˈ")) ?? phonetics.FirstOrDefault();
            if (usPhonetic != null && !string.IsNullOrEmpty(usPhonetic.text))
            {
                PhoneticUSTextBlock.Text = usPhonetic.text;
                _usAudioUrl = usPhonetic.audio;
                PhoneticUSBorder.Tag = _usAudioUrl;
            }
            else
            {
                PhoneticUSTextBlock.Text = "/n/a/";
            }

            // Tìm UK phonetic
            var ukPhonetic = phonetics.FirstOrDefault(p => p.text != null && p != usPhonetic) ?? usPhonetic;
            if (ukPhonetic != null && !string.IsNullOrEmpty(ukPhonetic.text))
            {
                PhoneticUKTextBlock.Text = ukPhonetic.text;
                _ukAudioUrl = ukPhonetic.audio;
                PhoneticUKBorder.Tag = _ukAudioUrl;
            }
            else
            {
                PhoneticUKTextBlock.Text = "/n/a/";
            }
        }

        private void LoadMeanings()
        {
            MeaningsContainer.Children.Clear();

            if (_word.meanings == null || _word.meanings.Count == 0)
            {
                var errorText = new TextBlock
                {
                    Text = "Không có định nghĩa",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 0)
                };
                MeaningsContainer.Children.Add(errorText);
                return;
            }

            foreach (var meaning in _word.meanings)
            {
                var meaningCard = CreateMeaningCard(meaning);
                MeaningsContainer.Children.Add(meaningCard);
            }
        }

        private Border CreateMeaningCard(Meaning meaning)
        {
            var card = new Border
            {
                BorderThickness = new Thickness(2, 2, 2, 2),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(4, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 20)
            };
            card.SetResourceReference(Border.BackgroundProperty, "MeaningBackground");
            card.SetResourceReference(Border.BorderBrushProperty, "MeaningBorder");

            // left border decoration
            var leftBorder = new Border
            {
                Width = 4,
                CornerRadius = new CornerRadius(12, 0, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            leftBorder.SetResourceReference(Border.BackgroundProperty, "MeaningBorderLeft");

            var grid = new Grid();
            grid.Children.Add(leftBorder);

            var stackPanel = new StackPanel();

            // Part of Speech Badge
            var badge = new Border
            {
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(6, 16, 6, 16),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 20)
            };
            badge.SetResourceReference(Border.BackgroundProperty, "ToolButtonActive");

            badge.Child = new TextBlock
            {
                Text = meaning.partOfSpeech.ToUpper(),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            stackPanel.Children.Add(badge);

            // Definitions
            if (meaning.definitions != null)
            {
                int index = 1;
                foreach (var def in meaning.definitions.Take(5))
                {
                    var defPanel = CreateDefinitionPanel(def, index);
                    stackPanel.Children.Add(defPanel);
                    index++;
                }
            }

            // Synonyms/Antonyms
            var relatedSection = CreateRelatedSection(meaning);
            if (relatedSection != null)
            {
                stackPanel.Children.Add(relatedSection);
            }

            card.Child = stackPanel;
            return card;
        }

        private StackPanel CreateDefinitionPanel(Definition def, int index)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

            // Definition text
            var defText = new TextBlock
            {
                FontSize = 15,
                TextWrapping = TextWrapping.Wrap
            };
            defText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");

            var numberRun = new Run($"{index}. ")
            {
                FontWeight = FontWeights.Bold
            };
            defText.Inlines.Add(numberRun);
            defText.Inlines.Add(new Run(def.definition));
            panel.Children.Add(defText);

            // Example
            if (!string.IsNullOrEmpty(def.example))
            {
                var exampleBorder = new Border
                {
                    BorderThickness = new Thickness(3, 0, 0, 0),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12, 15, 12, 15),
                    Margin = new Thickness(25, 10, 0, 0)
                };
                exampleBorder.SetResourceReference(Border.BackgroundProperty, "ExampleBackground");
                exampleBorder.SetResourceReference(Border.BorderBrushProperty, "ExampleBorder");

                var exampleText = new TextBlock
                {
                    Text = $"\"{def.example}\"",
                    FontStyle = FontStyles.Italic,
                    FontSize = 14,
                    Opacity = 0.9,
                    TextWrapping = TextWrapping.Wrap
                };
                exampleText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                exampleBorder.Child = exampleText;

                panel.Children.Add(exampleBorder);
            }

            return panel;
        }

        private Border CreateRelatedSection(Meaning meaning)
        {
            var hasSynonyms = meaning.synonyms != null && meaning.synonyms.Count > 0;
            var hasAntonyms = meaning.antonyms != null && meaning.antonyms.Count > 0;

            if (!hasSynonyms && !hasAntonyms) return null;

            var border = new Border
            {
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 25, 0, 0)
            };
            border.SetResourceReference(Border.BackgroundProperty, "RelatedBackground");
            border.SetResourceReference(Border.BorderBrushProperty, "RelatedBorder");

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            if (hasSynonyms)
            {
                var synPanel = CreateRelatedPanel("Synonyms (Từ đồng nghĩa)", meaning.synonyms.Take(6).ToList());
                Grid.SetColumn(synPanel, 0);
                synPanel.Margin = new Thickness(0, 0, 10, 0);
                grid.Children.Add(synPanel);
            }

            if (hasAntonyms)
            {
                var antPanel = CreateRelatedPanel("Antonyms (Từ trái nghĩa)", meaning.antonyms.Take(6).ToList());
                Grid.SetColumn(antPanel, 1);
                antPanel.Margin = new Thickness(10, 0, 0, 0);
                grid.Children.Add(antPanel);
            }

            border.Child = grid;
            return border;
        }

        private StackPanel CreateRelatedPanel(string title, System.Collections.Generic.List<string> words)
        {
            var panel = new StackPanel();
            var titleText = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Opacity = 0.9,
                Margin = new Thickness(0, 0, 0, 10)
            };
            titleText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");

            panel.Children.Add(titleText);

            var wrapPanel = new WrapPanel();
            foreach (var word in words)
            {
                var tag = new Border
                {
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(6, 14, 6, 14),
                    Margin = new Thickness(0, 0, 8, 8),
                    Cursor = Cursors.Hand
                };
                tag.SetResourceReference(Border.BackgroundProperty, "SearchBackground");
                tag.SetResourceReference(Border.BorderBrushProperty, "BorderColor");

                var tagText = new TextBlock
                {
                    Text = word,
                    FontSize = 13
                };
                tagText.SetResourceReference(TextBlock.ForegroundProperty, "ButtonColor");
                tag.Child = tagText;


                string capturedWord = word;
                tag.MouseDown += (s, e) => RelatedWord_Click(capturedWord);

                wrapPanel.Children.Add(tag);
            }
            panel.Children.Add(wrapPanel);

            return panel;
        }

        // ========== EVENT HANDLERS ==========

        private void PlayAudioUS_Click(object sender, MouseButtonEventArgs e)
        {
            PlayAudio(_usAudioUrl, "US");
        }

        private void PlayAudioUK_Click(object sender, MouseButtonEventArgs e)
        {
            PlayAudio(_ukAudioUrl, "UK");
        }

        private void PlayAudio(string audioUrl, string accent)
        {
            if (string.IsNullOrEmpty(audioUrl))
            {
                MessageBox.Show($"Không có audio {accent}", "Thông báo");
                return;
            }

            try
            {
                if (_mediaPlayer != null && !_disposed)
                {
                    _mediaPlayer.Stop();
                    _mediaPlayer.Open(new Uri(audioUrl, UriKind.Absolute));
                    _mediaPlayer.Play();
                    Console.WriteLine($"🔊 Playing {accent}: {audioUrl}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi phát âm: {ex.Message}", "Lỗi");
            }
        }

        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            _isFavorite = !_isFavorite;
            MessageBox.Show(_isFavorite ? "Đã thêm vào yêu thích" : "Đã xóa khỏi yêu thích");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng lưu từ đang phát triển");
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_word.word);
            MessageBox.Show("Đã sao chép từ");
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng tải offline đang phát triển");
        }

        private void RelatedWord_Click(string word)
        {
            MessageBox.Show($"Bạn click từ: {word}");
        }

        private string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        // ========== CLEANUP (IDisposable) ==========

        private void DetailsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _mediaPlayer != null)
                {
                    try
                    {
                        if (Application.Current.Dispatcher.CheckAccess())
                        {
                            _mediaPlayer.Stop();
                            _mediaPlayer.Close();
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _mediaPlayer.Stop();
                                _mediaPlayer.Close();
                            });
                        }
                        _mediaPlayer = null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Cleanup warning: {ex.Message}");
                    }
                }
                _disposed = true;
            }
        }

        ~DetailsPage()
        {
            Dispose(false);
        }
    }
}
