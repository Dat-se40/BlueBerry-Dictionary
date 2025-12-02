using System.Windows;

namespace BlueBerryDictionary
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ✅ CRITICAL: Set ShutdownMode để app không tắt khi LoginWindow close
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Check if user is logged in
            bool isLoggedIn = CheckLoginState();

            if (!isLoggedIn)
            {
                // Show LoginWindow (blocking call)
                var loginWindow = new LoginWindow();
                bool? loginResult = loginWindow.ShowDialog();

                // ✅ Show MainWindow AFTER LoginWindow closed
                if (loginResult == true)
                {
                    // User logged in successfully
                    System.Diagnostics.Debug.WriteLine("✅ User logged in → Showing MainWindow");
                }
                else if (loginResult == false)
                {
                    // User chose Guest mode
                    System.Diagnostics.Debug.WriteLine("✅ Guest mode → Showing MainWindow");
                }
                else
                {
                    // User closed window without choosing (X button)
                    System.Diagnostics.Debug.WriteLine("⚠️ User cancelled login → Exit app");
                    this.Shutdown();
                    return;
                }

                // ✅ Show MainWindow (MUST be after LoginWindow closes)
                ShowMainWindow();

                // ✅ Restore normal shutdown mode
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            else
            {
                // User already logged in
                System.Diagnostics.Debug.WriteLine("✅ Already logged in → Showing MainWindow");
                ShowMainWindow();
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
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
            this.MainWindow = mainWindow; // ✅ Set as MainWindow
            mainWindow.Show();
        }
    }
}