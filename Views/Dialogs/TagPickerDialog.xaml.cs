using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Dialogs
{
    /// <summary>
    /// Dialog để tạo tag mới
    /// </summary>
    public partial class TagPickerDialog : Window
    {
        private TagService _tagService;
        public string CreatedTagId { get; private set; }

        private string _selectedIcon = "🏷️";
        private string _selectedColor = "#2D4ACC";
        
        private readonly string[] _availableIcons = new[]
        {
            "🏷️", "📚", "🎯", "💼", "💬", "🎓", "🌟", "💡",
            "📝", "⭐", "🔥", "💎", "🚀", "🎨", "🎮", "📖",
            "🌈", "🎵", "🏆", "🌸", "🎪", "🎭", "🎬", "📱"
        };

        private readonly string[] _availableColors = new[]
        {
            "#2D4ACC", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6",
            "#EC4899", "#06B6D4", "#84CC16", "#F97316", "#6366F1"
        };

        public TagPickerDialog()
        {
            InitializeComponent();
            _tagService = TagService.Instance;
            LoadIcons();
            LoadColors();
            ApplyGlobalFont();
            
        }

        private void LoadIcons()
        {
            IconsPanel.Children.Clear();

            foreach (var icon in _availableIcons)
            {
                var btn = CreateIconButton(icon);
                IconsPanel.Children.Add(btn);
            }
        }

        private Button CreateIconButton(string icon)
        {
            var btn = new Button
            {
                Content = icon,
                Width = 50,
                Height = 50,
                FontSize = 24,
                Margin = new Thickness(5),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = icon 
            };
            btn.SetResourceReference(Button.ForegroundProperty, "TextColor"); 
            btn.Style = FindResource("IconButtonStyle") as Style;

            btn.Click += (s, e) =>
            {
                _selectedIcon = icon;
                UpdateIconSelection();
                UpdatePreview();
            };

            return btn;
        }

        private void UpdateIconSelection()
        {
            foreach (Button btn in IconsPanel.Children)
            {
                if (btn.Tag.ToString() == _selectedIcon)
                {
                    btn.Tag = "Selected";
                    btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_selectedColor));
                    btn.BorderThickness = new Thickness(3);
                }
                else
                {
                    btn.Tag = btn.Content.ToString();
                    btn.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
                    btn.BorderThickness = new Thickness(2);
                }
            }
        }

        private void LoadColors()
        {
            ColorsPanel.Children.Clear();

            foreach (var color in _availableColors)
            {
                var btn = CreateColorButton(color);
                ColorsPanel.Children.Add(btn);
            }
        }

        private Button CreateColorButton(string colorHex)
        {
            var btn = new Button
            {
                Width = 50,
                Height = 50,
                Margin = new Thickness(5),
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(2),
                Tag = colorHex
            };

            btn.Template = CreateColorButtonTemplate();

            btn.Click += (s, e) =>
            {
                _selectedColor = colorHex;
                UpdateColorSelection();
                UpdatePreview();
            };

            return btn;
        }

        private ControlTemplate CreateColorButtonTemplate()
        {
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            factory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            factory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));

            var template = new ControlTemplate(typeof(Button));
            template.VisualTree = factory;

            return template;
        }

        private void UpdateColorSelection()
        {
            foreach (Button btn in ColorsPanel.Children)
            {
                if (btn.Tag.ToString() == _selectedColor)
                {
                    btn.BorderBrush = Brushes.Black;
                    btn.BorderThickness = new Thickness(3);
                }
                else
                {
                    btn.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
                    btn.BorderThickness = new Thickness(2);
                }
            }

            UpdateIconSelection(); // Update icon border color
        }

        private void UpdatePreview()
        {
            PreviewIcon.Text = _selectedIcon;
            PreviewName.Text = string.IsNullOrWhiteSpace(TagNameInput.Text)
                ? "Tag Name"
                : TagNameInput.Text;
            PreviewBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_selectedColor));
        }

        private void TagNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
            ValidateInput();
        }

        private void ValidateInput()
        {
            bool isValid = !string.IsNullOrWhiteSpace(TagNameInput.Text);
            CreateButton.IsEnabled = isValid;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var tagName = TagNameInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(tagName))
            {
                MessageBox.Show("Please enter a tag name!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check duplicate
            if (_tagService.GetAllTags().Exists(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("This tag already exists!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create tag
            var newTag = _tagService.CreateTag(tagName, _selectedIcon, _selectedColor);
            CreatedTagId = newTag.Id;

            MessageBox.Show($"✅ Tag '{{tagName}}' created successfully!", "Completed successfully",
                MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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