using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BlueBerryDictionary.ViewModels;
using BlueBerryDictionary.Views.UserControls;

namespace BlueBerryDictionary.Views.Pages
{
    public partial class OfflineModePage : WordListPageBase
    {
        private OfflineModeViewModel _viewModel;

        public OfflineModePage(Action<string> onWordClick) : base(onWordClick)
        {
            InitializeComponent();
            _viewModel = new OfflineModeViewModel();
            DataContext = _viewModel;

            // Subscribe to changes
            _viewModel.OnPackagesChanged += LoadPackageCards;
        }

        public override async void LoadData()
        {
            await _viewModel.LoadDataAsync();
        }

        private void LoadPackageCards()
        {
            // Clear existing cards
            DownloadedPackagesGrid.Children.Clear();
            AvailablePackagesGrid.Children.Clear();

            // Load downloaded packages
            if (_viewModel.DownloadedPackages.Count > 0)
            {
                DownloadedEmptyState.Visibility = Visibility.Collapsed;
                foreach (var package in _viewModel.DownloadedPackages)
                {
                    var card = new PackageCard(package, _viewModel);
                    DownloadedPackagesGrid.Children.Add(card);
                }
            }
            else
            {
                DownloadedEmptyState.Visibility = Visibility.Visible;
            }

            // Load available packages (chỉ hiện packages chưa tải)
            var availablePackages = _viewModel.AvailablePackages
                .Where(p => !p.IsDownloaded)
                .ToList();

            foreach (var package in availablePackages)
            {
                var card = new PackageCard(package, _viewModel);
                card.DataContext = new PackageCardViewModel(package, _viewModel);
                AvailablePackagesGrid.Children.Add(card);
            }
        }
    }
}