using System.Windows;
using System.Windows.Controls;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class FAQDialog : Window
    {
        public FAQDialog()
        {
            InitializeComponent();
            LoadSearchFAQ(); // Load mặc định
        }

        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                // Reset all tabs
                foreach (var child in TabsPanel.Children)
                {
                    if (child is Button btn)
                    {
                        btn.Style = (Style)FindResource("TabButtonStyle");
                    }
                }

                // Set active tab
                button.Style = (Style)FindResource("ActiveTabStyle");

                // Load content
                LoadContent(tag);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadContent(string tag)
        {
            ContentPanel.Children.Clear();

            switch (tag)
            {
                case "search":
                    LoadSearchFAQ();
                    break;
                case "manage":
                    LoadManageFAQ();
                    break;
                case "theme":
                    LoadThemeFAQ();
                    break;
                case "sync":
                    LoadSyncFAQ();
                    break;
                case "bugs":
                    LoadBugsFAQ();
                    break;
            }
        }

        // ========== SEARCH FAQ ==========
        private void LoadSearchFAQ()
        {
            AddSection("🔍 ABOUT LOOKUP");

            AddQuestion("Q1: Why can’t I find a word?");
            AddAnswer("There are several reasons why a word may not be found:");
            AddBullet("✅ Check spelling – the app will suggest similar words");
            AddBullet("✅ Try a simpler base word (e.g., \"running\" → \"run\")");
            AddBullet("✅ Check your Internet connection (for online lookup)");
            AddBullet("✅ Some rare words may not be available in the database");

            AddQuestion("Q2: Why can’t audio be played?");
            AddAnswer("Please check the following:");
            AddBullet("✅ Check your speaker/headphones");
            AddBullet("✅ Check your Internet connection (audio is streamed from the server)");
            AddBullet("✅ Try playing again or restart the app");
            AddBullet("✅ Some rare words may not have audio");

            AddQuestion("Q3: How does offline mode work?");
            AddAnswer("Downloaded words are stored at:");
            AddAnswer("C:\\Users\\[YourName]\\AppData\\Local\\BlueBerryDictionary\\Data\\PersistentStorage\\StoredWord\\");
            AddAnswer("Only downloaded words can be searched offline. The entire dictionary is not downloaded because it is too large.");
        }

        // ========== MANAGE FAQ ==========
        private void LoadManageFAQ()
        {
            AddSection("📚 ABOUT VOCABULARY MANAGEMENT");

            AddQuestion("Q4: Is there a limit to the number of words in My Words?");
            AddAnswer("No limit! However, the app may become slower if you have more than 10,000 words.");
            AddAnswer("Recommendation: Use tags to organize words instead of saving too many.");

            AddQuestion("Q5: How can I back up my data?");
            AddAnswer("Method 1: Sign in with Google (Recommended)");
            AddBullet("• Data is automatically backed up to Google Drive");
            AddBullet("• The safest option!");
            AddAnswer("Method 2: Manual copy");
            AddBullet("• Go to the folder: C:\\Users\\[YourName]\\AppData\\Local\\BlueBerryDictionary\\");
            AddBullet("• Copy the entire Data/ folder");
            AddBullet("• Paste it to another device using the same path");

            AddQuestion("Q6: I accidentally deleted a word. Can it be restored?");
            AddAnswer("❌ There is no undo feature");
            AddAnswer("✅ If you have synced with Google Drive:");
            AddBullet("1. Sign out");
            AddBullet("2. Sign in again");
            AddBullet("3. Choose \"Keep cloud data\"");

            AddQuestion("Q7: Is there a limit to the number of tags?");
            AddAnswer("There is no limit to the number of tags. Each word can have multiple tags.");
            AddAnswer("Recommendation: Create 5–10 main tags (e.g., IELTS, TOEIC, Daily).");
        }

        // ========== THEME FAQ ==========
        private void LoadThemeFAQ()
        {
            AddSection("🎨 ABOUT INTERFACE");

            AddQuestion("Q8: Is the custom theme saved when the app is closed?");
            AddAnswer("✅ Yes, it is automatically saved in AppSettings.json");
            AddAnswer("✅ The theme is reloaded when the app restarts");

            AddQuestion("Q9: How can I reset to the default colors?");
            AddAnswer("1. Go to Settings");
            AddAnswer("2. Dropdown \"Change background\" → \"Default\"");
            AddAnswer("3. Confirm \"Yes\"");

            AddQuestion("Q10: Does the Light/Dark toggle affect custom themes?");
            AddAnswer("✅ Yes! Custom themes automatically adapt to Dark mode");
            AddAnswer("Colors will be darkened to match");

            AddQuestion("Q11: Is the font applied across the entire app?");
            AddAnswer("✅ Yes, it applies to all text in the app");
            AddAnswer("⚠️ Some icons (emojis) are not affected");
        }

        // ========== SYNC FAQ ==========
        private void LoadSyncFAQ()
        {
            AddSection("☁️ ABOUT SYNC");

            AddQuestion("Q12: How long does syncing take?");
            AddAnswer("First time (data merge): 10–30 seconds (depending on the number of words)");
            AddAnswer("Next times (incremental sync): 1–5 seconds");
            AddAnswer("Uploading 1 new word: < 1 second");

            AddQuestion("Q13: Where is the data stored on Google Drive?");
            AddAnswer("Folder: BlueBerryDictionary/Users/[email]/");
            AddAnswer("Files:");
            AddBullet("• MyWords.json (vocabulary)");
            AddBullet("• Tags.json (tags)");
            AddBullet("• Settings.json (settings)");

            AddQuestion("Q14: Can I use multiple devices?");
            AddAnswer("✅ Yes! Sign in with the same Google account");
            AddAnswer("Data is automatically synced across devices");
            AddAnswer("⚠️ It is recommended to use only one device at a time (to avoid conflicts)");

            AddQuestion("Q15: Can I use the app without Internet?");
            AddAnswer("✅ You can look up words (if they are downloaded for offline use)");
            AddAnswer("✅ View My Words, History, and Favorites");
            AddAnswer("❌ Sync is not available");
            AddAnswer("❌ Online word lookup is not available");

            AddQuestion("Q16: What should I do if I get a \"Sync failed\" error?");
            AddAnswer("Method 1: Check your connection");
            AddBullet("• Open a browser and try accessing google.com");
            AddBullet("• Check if a firewall is blocking the app");
            AddAnswer("Method 2: Sign out and sign in again");
            AddBullet("1. Sign out");
            AddBullet("2. Restart the app");
            AddBullet("3. Sign in again");
            AddBullet("4. Choose \"Merge data\"");
        }

        // ========== BUGS FAQ ==========
        private void LoadBugsFAQ()
        {
            AddSection("🐛 ABOUT TECHNICAL ISSUES");

            AddQuestion("Q17: The app crashes on startup");
            AddAnswer("1. Check if .NET 9.0 Runtime is installed correctly");
            AddAnswer("2. Delete the AppSettings.json file (the app will recreate it)");
            AddAnswer("3. Reinstall the app");

            AddQuestion("Q18: The app is slow or laggy");
            AddAnswer("Cause: Too many words in My Words (>10,000)");
            AddAnswer("Solutions:");
            AddBullet("1. Delete old or unused words");
            AddBullet("2. Export to a text file and keep only important words");
            AddBullet("3. Use filters instead of loading all words");

            AddQuestion("Q19: Cannot sign in with Google");
            AddAnswer("1. Check the default browser (must be Chrome / Edge / Firefox)");
            AddAnswer("2. Clear Google cookies");
            AddAnswer("3. Try signing in to Google in the browser first");
            AddAnswer("4. Temporarily disable antivirus software");

            AddQuestion("Q20: Icons / images are not displayed");
            AddAnswer("• Check if the Resources/ folder is complete");
            AddAnswer("• Reinstall the app");

            AddQuestion("❓ Still having issues?");
            AddAnswer("📧 Email: 24520280@gm.uit.edu.vn");
            AddAnswer("🐛 GitHub Issues: https://github.com/Dat-se40/BlueBerry-Dictionary/issues");
        }

        // ========== HELPER METHODS ==========
        private void AddSection(string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("SectionHeaderStyle")
            };
            ContentPanel.Children.Add(textBlock);
        }

        private void AddQuestion(string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("QuestionStyle")
            };
            ContentPanel.Children.Add(textBlock);
        }

        private void AddAnswer(string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("AnswerStyle")
            };
            ContentPanel.Children.Add(textBlock);
        }

        private void AddBullet(string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("BulletStyle")
            };
            ContentPanel.Children.Add(textBlock);
        }
    }
}
