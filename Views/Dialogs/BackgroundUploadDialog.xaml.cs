using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BlueBerryDictionary.Views.Dialogs
{
    public partial class BackgroundUploadDialog : Window
    {
        public string SelectedImagePath { get; private set; }

        public BackgroundUploadDialog()
        {
            InitializeComponent();
        }

        private void UploadArea_Click(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                Title = "Chọn ảnh nền"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedImagePath = openFileDialog.FileName;

                // Hiển thị preview
                var bitmap = new BitmapImage(new System.Uri(SelectedImagePath));
                PreviewImage.Source = bitmap;
                PreviewImage.Visibility = Visibility.Visible;
                PlaceholderPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            SelectedImagePath = null;
            PreviewImage.Source = null;
            PreviewImage.Visibility = Visibility.Collapsed;
            PlaceholderPanel.Visibility = Visibility.Visible;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedImagePath))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn ảnh trước khi áp dụng!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}