using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BlueBerryDictionary.ViewModels
{
    public partial class OfflineModeViewModel : ObservableObject
    {
        private readonly PackageManager _packageManager;

        [ObservableProperty]
        private ObservableCollection<TopicPackage> _availablePackages;

        [ObservableProperty]
        private ObservableCollection<TopicPackage> _downloadedPackages;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _statusMessage;

        public OfflineModeViewModel()
        {
            _packageManager = PackageManager.Instance;
            AvailablePackages = new ObservableCollection<TopicPackage>();
            DownloadedPackages = new ObservableCollection<TopicPackage>();
        }

        /// <summary>
        /// Load data khi page được mở
        /// </summary>
        public async Task LoadDataAsync()
        {
            IsLoading = true;
            StatusMessage = "Đang tải danh sách packages...";

            try
            {
                // ✅ Lazy initialize
                await _packageManager.InitializeAsync();

                // ✅ Load vào UI
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
        /// ✅ Refresh danh sách packages từ server
        /// GỌI KHI: User ấn nút "Refresh" hoặc "Check for updates"
        /// </summary>
        [RelayCommand]
        private async Task RefreshFromServerAsync()
        {
            IsRefreshing = true;
            StatusMessage = "Đang kiểm tra cập nhật từ server...";

            try
            {
                // ✅ Fetch catalog mới từ server
                bool success = await _packageManager.FetchFromServerAsync();

                if (success)
                {
                    // ✅ Reload packages từ file mới
                    await _packageManager.InitializeAsync();

                    RefreshPackagesList();

                    StatusMessage = $"✅ Đã cập nhật! Tìm thấy {AvailablePackages.Count} packages";

                    MessageBox.Show(
                        "Danh sách packages đã được cập nhật!",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    StatusMessage = "⚠️ Không thể kết nối server, hiển thị catalog cũ";

                    MessageBox.Show(
                        "Không thể kết nối đến server.\nSử dụng danh sách packages đã lưu.",
                        "Cảnh báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Lỗi: {ex.Message}";
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        /// <summary>
        /// Refresh ObservableCollections từ PackageManager
        /// </summary>
        private void RefreshPackagesList()
        {
            // ✅ All packages
            AvailablePackages.Clear();
            foreach (var pkg in _packageManager.GetAllPackages())
            {
                AvailablePackages.Add(pkg);
            }

            // ✅ Downloaded packages
            DownloadedPackages.Clear();
            foreach (var pkg in _packageManager.GetDownloadedPackages())
            {
                DownloadedPackages.Add(pkg);
            }

            Console.WriteLine($"[OfflineModeViewModel] Available: {AvailablePackages.Count}, Downloaded: {DownloadedPackages.Count}");
        }

        // ==================== PACKAGE OPERATIONS ====================

        [RelayCommand]
        private async Task DownloadPackageAsync(TopicPackage package)
        {
            if (package.IsDownloaded)
            {
                MessageBox.Show("Package đã được tải rồi!", "Thông báo");
                return;
            }

            StatusMessage = $"Đang tải package: {package.Name}...";

            try
            {
                await _packageManager.DownloadPackageAsync(package.Id);

                RefreshPackagesList();

                StatusMessage = $"✅ Đã tải: {package.Name}";
                MessageBox.Show($"✅ Đã tải package: {package.Name}", "Thành công");
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Lỗi tải package";
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ImportPackageAsync(TopicPackage package)
        {
            if (!package.IsDownloaded)
            {
                MessageBox.Show("Vui lòng tải package trước!", "Cảnh báo");
                return;
            }

            var result = MessageBox.Show(
                $"Import {package.TotalItems} từ vào My Words?\n\nPackage: {package.Name}",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes) return;

            StatusMessage = $"Đang import: {package.Name}...";

            try
            {
                await _packageManager.ImportPackageAsync(package.Id);

                StatusMessage = $"✅ Đã import {package.TotalItems} từ";
                MessageBox.Show(
                    $"✅ Đã import {package.TotalItems} từ vào My Words!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Lỗi import";
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeletePackageAsync(TopicPackage package)
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

                StatusMessage = $"✅ Đã xóa: {package.Name}";
                MessageBox.Show($"Đã xóa package: {package.Name}", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
