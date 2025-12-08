using BlueBerryDictionary.Models;
using BlueBerryDictionary.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.UserControls
{
    public partial class PackageCard : UserControl
    { 
        private readonly PackageCardViewModel packageCardViewModel;
        public PackageCard(TopicPackage topicPackage, OfflineModeViewModel parentVM)
        {
            InitializeComponent();
            packageCardViewModel = new PackageCardViewModel(topicPackage,parentVM);
            DataContext = packageCardViewModel;     
           
        }

        // Dependency Properties để bind từ bên ngoài
        public static readonly DependencyProperty PackageProperty =
            DependencyProperty.Register("Package", typeof(TopicPackage),
                typeof(PackageCard), new PropertyMetadata(null, OnPackageChanged));

        public TopicPackage Package
        {
            get => (TopicPackage)GetValue(PackageProperty);
            set => SetValue(PackageProperty, value);
        }

        private static void OnPackageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PackageCard card && e.NewValue is TopicPackage package)
            {
                   // Ko biết làm gì với cái này =)))
            }
        }
    }

    // ViewModel cho PackageCard
    public class PackageCardViewModel
    {
        private readonly TopicPackage _package;
        private readonly OfflineModeViewModel _parentVM;

        public PackageCardViewModel(TopicPackage package, OfflineModeViewModel parentVM)
        {
            _package = package;
            _parentVM = parentVM;

            OpenPackageCommand = new RelayCommand(OpenPackage);
            DownloadPackageCommand = new RelayCommand(DownloadPackage);
            DeletePackageCommand = new RelayCommand(DeletePackage);
        }

        // Properties for UI Binding
        public string Name => _package.Name;
        public string Description => _package.Description;
        public string Icon => _package.ThumbnailUrl;
        public int TotalItems => _package.TotalItems;
        public string Level => _package.Level;
        public bool IsDownloaded => _package.IsDownloaded;

        public string SizeText =>
            _package.SizeInBytes >= 1_000_000
                ? $"{_package.SizeInBytes / 1_000_000.0:F1} MB"
                : $"{_package.SizeInBytes / 1_000.0:F0} KB";

        public string DownloadDate => IsDownloaded
            ? System.DateTime.Now.ToString("dd/MM/yyyy")
            : "";

        public string BadgeText => _package.Category;

        public Brush BadgeColor =>
            _package.Level == "Advanced"
                ? new SolidColorBrush(Color.FromRgb(245, 158, 11))
                : new SolidColorBrush(Color.FromRgb(16, 185, 129));

        // Text các nút theo trạng thái
        public string DownloadButtonText =>
            IsDownloaded ? "🔁 Tải lại / Cập nhật" : "💾 Tải thông tin";

        public string OpenButtonText =>
            IsDownloaded ? "🔍 Mở gói" : "👁️ Xem trước";

        public string DeleteButtonText => "🗑️ Xóa";

        // Commands
        public ICommand OpenPackageCommand { get; }
        public ICommand DownloadPackageCommand { get; }
        public ICommand DeletePackageCommand { get; }

        private async void OpenPackage()
        {
            if (IsDownloaded)
            {
                _parentVM.OpenPackage(_package);
            }
            else
            {
                await _parentVM.PreviewPackageAsync(_package);
            }
        }

        private async void DownloadPackage()
        {
            await _parentVM.DownloadPackageAsync(_package);
        }

        private async void DeletePackage()
        {
            await _parentVM.DeletePackageAsync(_package);
        }
    }
}