using BlueBerryDictionary.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BlueBerryDictionary
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();

            // Initialize ViewModel
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            _viewModel.LoginSuccessEvent += OnLoginSuccess;
            _viewModel.GuestModeEvent += OnGuestMode;

            // Load logo
        }

        /// <summary>
        /// Xử lý khi login Gmail thành công
        /// </summary>
        private void OnLoginSuccess(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("✅ LoginWindow: Login success event received");
            this.DialogResult = true; // true = logged in
            this.Close();
        }

        /// <summary>
        /// Xử lý khi chọn Guest mode
        /// </summary>
        private void OnGuestMode(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("✅ LoginWindow: Guest mode event received");
            this.DialogResult = false; // false = guest mode
            this.Close();
        }

        /// <summary>
        /// Cleanup khi đóng window
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe events để tránh memory leak
            _viewModel.LoginSuccessEvent -= OnLoginSuccess;
            _viewModel.GuestModeEvent -= OnGuestMode;
            base.OnClosed(e);
        }
    }
}
