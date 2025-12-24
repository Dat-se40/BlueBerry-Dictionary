using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BlueBerryDictionary.ViewModels
{
    public partial class OfflineModeViewModel : ObservableObject
    {
        private readonly PackageManager _packageManager;

        #region Events

        /// <summary>
        /// Event khi danh sách packages thay đổi
        /// </summary>
        public event Action OnPackagesChanged;

        #endregion

        #region Observable properties
        [ObservableProperty]
        private ObservableCollection<TopicPackage> _availablePackages;

        [ObservableProperty]
        private ObservableCollection<TopicPackage> _downloadedPackages;

        // Stats Properties
        [ObservableProperty]
        private int _downloadedCount;

        [ObservableProperty]
        private string _totalWords;

        [ObservableProperty]
        private string _totalSize;

        [ObservableProperty]
        private int _availableCount;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage;

        #endregion

        #region Constructor
        public OfflineModeViewModel()
        {
            _packageManager = PackageManager.Instance;
            AvailablePackages = new ObservableCollection<TopicPackage>();
            DownloadedPackages = new ObservableCollection<TopicPackage>();
        }
        #endregion

        #region Data loading
        /// <summary>
        /// Load data khi page được mở
        /// </summary>
        public async Task LoadDataAsync()
        {
            IsLoading = true;
            StatusMessage = "Đang tải danh sách packages...";

            try
            {
                // Lazy initialize PackageManager
                await _packageManager.InitializeAsync();

                // Refresh UI
                RefreshPackagesList();

                StatusMessage = $"Đã tải {AvailablePackages.Count} packages";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
                Console.WriteLine($"❌ LoadData error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Refresh danh sách packages và stats
        /// </summary>
        private void RefreshPackagesList()
        {
            // Load all packages
            AvailablePackages.Clear();
            foreach (var pkg in _packageManager.GetAllPackages())
            {
                AvailablePackages.Add(pkg);
            }

            // Load downloaded packages
            DownloadedPackages.Clear();
            foreach (var pkg in _packageManager.GetDownloadedPackages())
            {
                DownloadedPackages.Add(pkg);
            }

            // Update stats
            UpdateStats();

            // Notify UI
            OnPackagesChanged?.Invoke();

            Console.WriteLine($"[OfflineModeViewModel] Available: {AvailablePackages.Count}, Downloaded: {DownloadedPackages.Count}");
        }

        /// <summary>
        /// Cập nhật thống kê
        /// </summary>
        private void UpdateStats()
        {
            DownloadedCount = DownloadedPackages.Count;
            AvailableCount = AvailablePackages.Count;

            // Tính tổng số từ
            var totalWordsCount = DownloadedPackages.Sum(p => p.TotalItems);
            TotalWords = totalWordsCount >= 1000
                ? $"{totalWordsCount / 1000.0:F1}K"
                : totalWordsCount.ToString();

            // Tính tổng dung lượng
            var totalBytes = DownloadedPackages.Sum(p => p.SizeInBytes);
            TotalSize = totalBytes >= 1_000_000
                ? $"{totalBytes / 1_000_000.0:F0} MB"
                : $"{totalBytes / 1_000.0:F0} KB";
        }

        #endregion

        #region Commands
        /// <summary>
        /// Refresh packages từ server
        /// </summary>
        [RelayCommand]
        private async Task RefreshFromServerAsync()
        {
            StatusMessage = "Đang kiểm tra cập nhật...";

            try
            {
                bool success = await _packageManager.FetchFromServerAsync();

                if (success)
                {
                    // Reload packages
                    await _packageManager.InitializeAsync();
                    RefreshPackagesList();

                    MessageBox.Show(
                        "Danh sách packages đã được cập nhật!",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "Không thể kết nối server.\nSử dụng danh sách đã lưu.",
                        "Cảnh báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Download package
        /// </summary>
        public async Task DownloadPackageAsync(TopicPackage package)
        {
            try
            {
                await _packageManager.DownloadPackageAsync(package.Id);
                RefreshPackagesList();
                MessageBox.Show($"✅ Đã tải package: {package.Name}", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Tải bản xem trước
        /// </summary>
        public async Task PreviewPackageAsync(TopicPackage meta)
        {
            try
            {
                var full = await _packageManager.LoadPackagePreviewAsync(meta);

                var dialog = new PackageDetailsDialog(full)
                {
                    Owner = Application.Current.MainWindow
                };
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Open package details
        /// </summary>
        public void OpenPackage(TopicPackage package)
        {
            if (!package.IsDownloaded)
            {
                MessageBox.Show("Vui lòng tải package trước!", "Cảnh báo");
                return;
            }

            var dialog = new Views.Dialogs.PackageDetailsDialog(package)
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                // Refresh after importing words
                RefreshPackagesList();
            }
        }

        /// <summary>
        /// Delete package
        /// </summary>
        public async Task DeletePackageAsync(TopicPackage package)
        {
            var result = MessageBox.Show(
                $"Xóa package '{package.Name}'?\n\n(Các từ đã import vào My Words sẽ KHÔNG bị xóa)",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _packageManager.DeletePackageAsync(package.Id);
                RefreshPackagesList();
                MessageBox.Show($"Đã xóa package: {package.Name}", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}