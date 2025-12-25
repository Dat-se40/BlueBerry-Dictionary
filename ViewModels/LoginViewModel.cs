using BlueBerryDictionary.Services.Network;
using BlueBerryDictionary.Services.User;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BlueBerryDictionary.ViewModels
{
    /// <summary>
    /// ViewModel cho LoginWindow
    /// STUB VERSION - Commands chưa có logic thật
    /// </summary>
    public partial class LoginViewModel : ObservableObject
    {
        #region Events

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

        #endregion

        #region Observable properties

        [ObservableProperty]
        private bool isGmailLoading;

        [ObservableProperty]
        private bool isGuestLoading;

        #endregion

        #region Constructor

        public LoginViewModel()
        {
            // Constructor để sau này có thể inject services
            // TODO: Inject GoogleAuthService khi implement
        }
        #endregion

        #region Commands

        /// <summary>
        /// Login bằng Gmail (OAuth 2.0)
        /// </summary>
        [RelayCommand]
        private async Task LoginWithGmailAsync()
        {
            IsGmailLoading = true;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                // gọi service thật
                var result = await GoogleAuthService.Instance.LoginAsync();

                if (result.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Gmail login success: {result.UserInfo?.Email}");

                    // Set current user
                    UserSessionManage.Instance.SetLoggedInUser(
                        result.UserInfo?.Email,
                        result.UserInfo?.Email,
                        result.UserInfo?.Name,
                        result.UserInfo?.Avatar
                    );

                    // Close LoginWindow
                    LoginSuccessEvent?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Gmail login failed: {result.ErrorMessage}");
                    System.Windows.MessageBox.Show(
                        result.ErrorMessage ?? "Login failed.",
                        "Google Login",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Login exception: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"Login failed: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
            finally
            {
                IsGmailLoading = false;
            }
            Mouse.OverrideCursor = null;
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
        #endregion
    }
}
