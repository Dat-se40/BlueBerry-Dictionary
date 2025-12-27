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
    public partial class RemoveTagDialog : Window
    {
        private readonly TagService _tagService;
        public List<string> RemovedTagIds { get; private set; } = new();

        // ========== STATIC EVENT (để Page listen) ==========
        public static event Action OnTagsDeleted;

        public RemoveTagDialog()
        {
            InitializeComponent();
            _tagService = TagService.Instance;
            LoadTags();
            ApplyGlobalFont();
        }

        private void LoadTags()
        {
            var allTags = _tagService.GetAllTags();
            if (allTags == null || allTags.Count == 0)
            {
                MessageBox.Show("There are no tags to delete yet!", "Notification",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TagsList.ItemsSource = allTags;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIds = new List<string>();

                foreach (var item in TagsList.Items)
                {
                    var container = TagsList.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                    if (container != null)
                    {
                        var check = FindDescendant<CheckBox>(container);
                        if (check != null && check.IsChecked == true)
                            selectedIds.Add(check.Tag.ToString());
                    }
                }

                if (selectedIds.Count == 0)
                {
                    MessageBox.Show("Please select at least one tag!", "Notification",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Are you sure you want to delete {selectedIds.Count} tag(s) ?",
                    "Confirmation", 
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    // ========== DELETE TAGS ==========
                    foreach (string id in selectedIds)
                    {
                        _tagService.DeleteTag(id);
                        RemovedTagIds.Add(id);
                        Console.WriteLine($"❌ Deleted tag: {id}");
                    }

                    MessageBox.Show($"Successfully deleted {selectedIds.Count} tag(s).",
                        "Completed successfully", MessageBoxButton.OK, MessageBoxImage.Information);

                    // ========== TRIGGER EVENT FOR UI UPDATE ==========
                    OnTagsDeleted?.Invoke();

                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error deleting tag: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Generic helper to find control
        private T FindDescendant<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;

                var desc = FindDescendant<T>(child);
                if (desc != null)
                    return desc;
            }

            return null;
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
