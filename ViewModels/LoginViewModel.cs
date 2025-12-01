using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace BlueBerryDictionary.ViewModels
{
    /// <summary>
    /// ViewModel cho LoginWindow
    /// STUB VERSION - Commands chưa có logic thật
    /// </summary>
    public partial class LoginViewModel : ObservableObject
    {
        // ========== EVENTS ==========

        /// <summary>
        /// Event khi login Gmail thành công
        /// LoginWindow sẽ subscribe event này để close window
        /// </summary>
        public event EventHandler LoginSuccessEvent;

        /// <summary>
        /// Event khi chọn Guest mode
        /// LoginWindow sẽ subscribe event này để close window
        /// </summary>
        public event EventHandler GuestModeEvent;

        // ========== OBSERVABLE PROPERTIES ==========

        [ObservableProperty]
        private bool isGmailLoading;

        [ObservableProperty]
        private bool isGuestLoading;

        // ========== CONSTRUCTOR ==========

        public LoginViewModel()
        {
            // Constructor để sau này có thể inject services
            // TODO: Inject GoogleAuthService khi implement
        }

        // ========== RELAY COMMANDS ==========

        /// <summary>
        /// [STUB] Login bằng Gmail
        /// TODO: Implement GoogleAuthService.LoginAsync() sau khi duyệt giao diện
        /// </summary>
        [RelayCommand]
        private async Task LoginWithGmailAsync()
        {
            IsGmailLoading = true;

            try
            {
                // TODO: var result = await GoogleAuthService.Instance.LoginAsync();
                // TODO: if (result.Success)
                // TODO: {
                // TODO:     await GoogleDriveSyncService.Instance.InitializeAsync(result.Credential);
                // TODO:     UserDataManager.Instance.SetCurrentUser(result.UserEmail);
                // TODO:
                // TODO:     // ✅ Trigger event để LoginWindow close
                // TODO:     LoginSuccessEvent?.Invoke(this, EventArgs.Empty);
                // TODO: }
                // TODO: else
                // TODO: {
                // TODO:     MessageBox.Show(result.ErrorMessage, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                // TODO: }

                // Simulate loading (demo)
                await Task.Delay(2000);

                // ✅ Trigger event (demo)
                LoginSuccessEvent?.Invoke(this, EventArgs.Empty);

                System.Diagnostics.Debug.WriteLine("✅ Gmail Login success → Triggering LoginSuccessEvent");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Login error: {ex.Message}");
                // TODO: Show error dialog
                // MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsGmailLoading = false;
            }
        }

        /// <summary>
        /// [STUB] Tiếp tục với Guest mode
        /// TODO: Implement logic sau khi duyệt giao diện
        /// </summary>
        [RelayCommand]
        private async Task ContinueAsGuestAsync()
        {
            IsGuestLoading = true;

            try
            {
                // TODO: UserDataManager.Instance.SetCurrentUser("guest");
                // TODO:
                // TODO: // ✅ Trigger event để LoginWindow close
                // TODO: GuestModeEvent?.Invoke(this, EventArgs.Empty);

                // Simulate loading (demo)
                await Task.Delay(1500);

                // ✅ Trigger event (demo)
                GuestModeEvent?.Invoke(this, EventArgs.Empty);

                System.Diagnostics.Debug.WriteLine("✅ Guest Mode activated → Triggering GuestModeEvent");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Guest mode error: {ex.Message}");
            }
            finally
            {
                IsGuestLoading = false;
            }
        }
    }
}
