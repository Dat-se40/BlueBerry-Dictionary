using BlueBerryDictionary.Services.Network;
using System.IO;
using System.Threading.Tasks;
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
                    AsyncData();
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
        private async void AsyncData()
        {
            var instance = CloudSyncService.Instance;

            try
            {
                // ✅ ĐỌC FILE RA JSON STRING
                string mywordsPath = instance.GetLocalFilePath(CloudSyncService.essentialFile[0]);
                string tagsPath = instance.GetLocalFilePath(CloudSyncService.essentialFile[1]);

                string mywordsJson = await File.ReadAllTextAsync(mywordsPath);
                string tagsJson = await File.ReadAllTextAsync(tagsPath);

                // ✅ TRUYỀN JSON STRING VÀO MERGE
                await Task.WhenAll(
                    instance.MergeMyWordsAsync(mywordsJson),
                    instance.MergeTagsAsync(tagsJson)
                );

                Console.WriteLine("✅ Async data merge completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Async data failed: {ex.Message}");
                MessageBox.Show($"Lỗi đồng bộ dữ liệu:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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