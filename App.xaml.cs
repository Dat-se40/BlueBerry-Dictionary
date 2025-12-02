using System.Windows;

namespace BlueBerryDictionary
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ✅ Check if user is logged in
            bool isLoggedIn = CheckLoginState();

            if (!isLoggedIn)
            {
                // ========== SHOW LOGIN WINDOW ==========
                var loginWindow = new LoginWindow();
                bool? loginResult = loginWindow.ShowDialog(); // Blocking call

                if (loginResult == true)
                {
                    // User logged in successfully
                    System.Diagnostics.Debug.WriteLine("✅ User logged in, showing MainWindow");
                    ShowMainWindow();
                }
                else
                {
                    // User chose Guest mode (or closed window)
                    System.Diagnostics.Debug.WriteLine("✅ Guest mode, showing MainWindow");
                    ShowMainWindow();
                }
            }
            else
            {
                // User already logged in
                System.Diagnostics.Debug.WriteLine("✅ Already logged in, showing MainWindow");
                ShowMainWindow();
            }
        }

        /// <summary>
        /// Check if user is logged in (từ saved token)
        /// </summary>
        private bool CheckLoginState()
        {
            // TODO: return GoogleAuthService.Instance.TrySilentLoginAsync().Result;

            // Tạm thời return false để luôn hiện LoginWindow (demo)
            return false;
        }

        /// <summary>
        /// Show MainWindow
        /// </summary>
        private void ShowMainWindow()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
