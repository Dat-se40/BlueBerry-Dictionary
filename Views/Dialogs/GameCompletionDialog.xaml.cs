using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BlueBerryDictionary.Views.Dialogs
{
    public partial class GameCompletionDialog : Window
    {
        public enum CompletionAction
        {
            Close,
            Restart,
            ReviewSkipped
        }

        public CompletionAction UserAction { get; private set; } = CompletionAction.Close;
        public int? SelectedCardIndex { get; private set; }

        public GameCompletionDialog()
        {
            InitializeComponent();
        }

        public void SetCompletionData(
            int percentage,
            int knownCount,
            int unknownCount,
            int totalCount,
            List<int> skippedIndices)
        {
            TxtPercentage.Text = $"{percentage}%";
            TxtKnownCount.Text = $"{knownCount} cards ({percentage}%)";
            TxtUnknownCount.Text = $"{unknownCount} cards ({100 - percentage}%)";
            TxtTotalCount.Text = $"{totalCount} cards";

            if (skippedIndices != null && skippedIndices.Count > 0)
            {
                // Show skipped list
                SkippedListContainer.Visibility = Visibility.Visible;
                
                // Convert indices to display format: "#1", "#2", etc.
                var displayItems = skippedIndices
                    .OrderBy(i => i)
                    .Select(i => new { Index = i, Display = $"#{i + 1}" })
                    .ToList();
                
                SkippedNumbersList.ItemsSource = displayItems;

                // Show 3-button layout
                Actions2Buttons.Visibility = Visibility.Collapsed;
                Actions3Buttons.Visibility = Visibility.Visible;
            }
            else
            {
                // Hide skipped list
                SkippedListContainer.Visibility = Visibility.Collapsed;

                // Show 2-button layout
                Actions3Buttons.Visibility = Visibility.Collapsed;
                Actions2Buttons.Visibility = Visibility.Visible;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            UserAction = CompletionAction.Close;
            DialogResult = false;
            Close();
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            UserAction = CompletionAction.Restart;
            DialogResult = true;
            Close();
        }

        private void ReviewSkipped_Click(object sender, RoutedEventArgs e)
        {
            UserAction = CompletionAction.ReviewSkipped;
            DialogResult = true;
            Close();
        }

        private void SkipNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                // Get the data item (contains both Index and Display)
                var dataItem = btn.DataContext;
                if (dataItem != null)
                {
                    var indexProperty = dataItem.GetType().GetProperty("Index");
                    if (indexProperty != null)
                    {
                        SelectedCardIndex = (int)indexProperty.GetValue(dataItem);
                        UserAction = CompletionAction.ReviewSkipped;
                        DialogResult = true;
                        Close();
                    }
                }
            }
        }
    }
}