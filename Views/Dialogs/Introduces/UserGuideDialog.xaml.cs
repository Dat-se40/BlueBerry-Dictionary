using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class UserGuideDialog : Window
    {
        public UserGuideDialog()
        {
            InitializeComponent();
            LoadIntroContent(); // Load giới thiệu mặc định
            ApplyGlobalFont();
        }

        /// <summary>
        /// Handle tab click
        /// </summary>
        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                // Reset all tabs to normal style
                foreach (var child in TabsPanel.Children)
                {
                    if (child is Button btn)
                    {
                        btn.Style = (Style)FindResource("TabButtonStyle");
                    }
                }

                // Set clicked tab to active style
                button.Style = (Style)FindResource("ActiveTabStyle");

                // Load content based on tag
                LoadContent(tag);
            }
        }

        /// <summary>
        /// Load content based on selected tab
        /// </summary>
        private void LoadContent(string tag)
        {
            ContentPanel.Children.Clear();

            switch (tag)
            {
                case "intro":
                    LoadIntroContent();
                    break;
                case "search":
                    LoadSearchContent();
                    break;
                case "manage":
                    LoadManageContent();
                    break;
                case "history":
                    LoadHistoryContent();
                    break;
                case "favourite":
                    LoadFavouriteContent();
                    break;
                case "theme":
                    LoadThemeContent();
                    break;
                case "sync":
                    LoadSyncContent();
                    break;
            }
        }

        /// <summary>
        /// Close button click
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ==================== CONTENT LOADERS ====================

        /// <summary>
        /// Load Giới thiệu content
        /// </summary>
        private void LoadIntroContent()
        {
            AddSectionHeader("🎯 INTRODUCTION");

            AddBodyText(
                "BlueBerry Dictionary is a smart English dictionary app developed to provide users with the most effective vocabulary lookup and learning experience. The app is not just a simple lookup tool, but also a reliable companion to help you build and manage your personal vocabulary library efficiently and systematically.");

            AddBodyText(
                "With a friendly, modern interface and many advanced features, BlueBerry Dictionary is suitable for all learners – from students preparing for IELTS or TOEIC exams to professionals who want to improve their specialized vocabulary. Notably, data synchronization via Google Drive allows you to learn anytime, anywhere without worrying about losing your data.");

            AddSubHeader("✨ Key Features");
            AddBullet("✅ Look up words from multiple trusted sources");
            AddBullet("✅ Accurate US 🇺🇸 and UK 🇬🇧 pronunciation");
            AddBullet("✅ Manage vocabulary with a tag system");
            AddBullet("✅ Save search history and favorite words");
            AddBullet("✅ Customize interface (23 themes + custom colors)");
            AddBullet("✅ Sync data via Google Drive");
            AddBullet("✅ Support offline mode");

            AddSubHeader("👥 Development Team");
            AddBodyText("Subject: Lập trình trực quan");
            AddBodyText("Lecturer: ThS. Mai Trọng Khang");
            AddBodyText("Semester: 1 - Academic Year 2024-2025");
            AddBodyText("Team Members:");
            AddBullet("• Nguyễn Tấn Đạt");
            AddBullet("• Võ Nguyễn Thanh Hương");
            AddBullet("• Phan Thế Phong");
        }

        /// <summary>
        /// Load Tra cứu content
        /// </summary>
        private void LoadSearchContent()
        {
            AddSectionHeader("🔍 WORD SEARCH");

            AddSubHeader("📖 Basic Lookup");
            AddBodyText(
                "The vocabulary lookup feature is the heart of BlueBerry Dictionary. When you open the app, the search bar prominently located at the center of the screen is ready to assist you.");

            AddStep("Step 1: Enter the word you want to look up in the search bar");
            AddStep("Step 2: Select a suggested word or press Enter");
            AddBullet("• The app displays suggestions as you type");
            AddBullet("• Suggestions are based on similarity to the word you entered");

            AddStep("Step 3: View word information");
            AddBullet("✅ Pronunciation: US 🇺🇸 and UK 🇬🇧");
            AddBullet("✅ Meaning: All meanings of the word (noun, verb, adjective, etc.)");
            AddBullet("✅ Examples: Sample sentences illustrating usage");
            AddBullet("✅ Synonyms/Antonyms (if available)");

            AddSubHeader("🔊 Pronunciation Guide");
            AddBodyText(
                "BlueBerry Dictionary provides both American and British accents, allowing you to choose the accent that suits your learning goals.");
            AddStep("• US Pronunciation: Click 🔊 next to US phonetic");
            AddStep("• UK Pronunciation: Click 🔊 next to UK phonetic");


            AddSubHeader("💾 Save Words");
            AddBodyText("After looking up a useful word, you can save it for later review.");

            AddStep("Method 1: Save the entire word");
            AddBullet("1. Click the 'Save Word' button (💾) at the top-right corner");
            AddBullet("2. The word will be saved in My Words with all meanings");

            AddStep("Method 2: Save specific meanings (Recommended)");
            AddBullet("1. Click the 'Save Word' button (💾)");
            AddBullet("2. Select the meanings you want to save");
            AddBullet("3. (Optional) Assign tags to the word");
            AddBullet("4. Click 'Save'");

            AddSubHeader("❤️ Favorites");
            AddBullet("• Click the ❤️ icon to add/remove a word from your favorites");
            AddBullet("• Red heart = Favorited");
            AddBullet("• Gray heart = Not favorited");

            AddSubHeader("🌐 Offline Mode");
            AddBodyText("One of the major advantages of BlueBerry Dictionary is its offline capability.");

            AddStep("Download words to your device:");
            AddBullet("1. Look up the word online for the first time");
            AddBullet("2. Click the 'Download' button (📥)");
            AddBullet("3. The word will be saved locally");

            AddStep("Using offline:");
            AddBullet(
                "• Next time you look up the word, the app automatically uses the offline version (if available)");
            AddBullet("• Icon 📡: Green = Online, Gray = Offline");
        }

        /// <summary>
        /// Load Quản lý từ content
        /// </summary>
        private void LoadManageContent()
        {
            AddSectionHeader("📚 PERSONAL VOCABULARY MANAGEMENT");

            AddSubHeader("📖 My Words - Your Vocabulary Library");
            AddBodyText(
                "My Words is the central feature of BlueBerry Dictionary, where you build and manage your personal vocabulary library.");
            AddStep("Access: Sidebar → My Words");

            AddSubHeader("🏷️ Create and Manage Tags");
            AddBodyText("Tags are a powerful tool to organize your vocabulary your way.");

            AddStep("Step 1: Create a new tag");
            AddBullet("1. Click the '🏷️ Create New Tag' button");
            AddBullet("2. Fill in the details:");
            AddBullet("   • Tag name: e.g., 'IELTS', 'Business English'");
            AddBullet("   • Choose icon: Click a sample icon or enter an emoji");
            AddBullet("   • Choose color: Click the color palette");
            AddBullet("3. Click 'Create'");

            AddStep("Step 2: Assign tags to words");
            AddBullet("Method 1: When saving a word");
            AddBullet("• Select a tag from the dropdown in the 'Select meanings to save' dialog");
            AddBullet("Method 2: Assign after saving");
            AddBullet("1. Go to My Words");
            AddBullet("2. Click the word you want to tag");
            AddBullet("3. Click 'Assign Tag'");
            AddBullet("4. Choose a tag from the list");

            AddStep("Delete tags:");
            AddBullet("1. Click the ⚙️ icon on the tag card");
            AddBullet("2. Select the tags you want to delete");
            AddBullet("3. Confirm");

            AddSubHeader("🔍 Filter Words");
            AddBodyText(
                "With hundreds or thousands of words in My Words, searching and filtering becomes extremely important.");

            AddStep("Filter by letter:");
            AddBullet("• Click a letter (A-Z) on the top sidebar");
            AddBullet("• Select 'All' to remove filtering");

            AddStep("Filter by word type:");
            AddBullet("• 'Word Type' dropdown → Choose:");
            AddBullet("   - All");
            AddBullet("   - Noun");
            AddBullet("   - Verb");
            AddBullet("   - Adjective");
            AddBullet("   - Adverb");

            AddStep("Filter by tag:");
            AddBullet("• Click a tag on the top sidebar");
            AddBullet("• Only words with that tag will be displayed");

            AddStep("Quick search:");
            AddBullet("• Type a word in the 'Search in saved words...' box");
            AddBullet("• Results appear in real-time");

            AddSubHeader("📊 Vocabulary Statistics");
            AddBodyText("At the top-right corner, you will see useful statistics:");
            AddBullet("• 📚 Total words: Total words saved");
            AddBullet("• 🏷️ Tags: Number of tags created");
            AddBullet("• 🆕 New words this week: Words added in the last 7 days");
            AddBullet("• 📅 New words this month: Words added in the last 30 days");

            AddSectionHeader("✏️ Delete Word");
            AddStep("1. Click the '❌' icon next to the word");
            AddStep("2. The word will be deleted from your library");

            AddSectionHeader("🏷️ Tags (Organize Vocabulary)");
            AddStep("Show tag created time, color, and icon");
            AddStep("To create a tag:");
            AddBullet("1. Click '🏷️' in DetailsPage");
            AddBullet("2. Enter tag name (e.g., 'IELTS Speaking', 'Business')");
            AddBullet("3. Choose an icon and color");
            AddBullet("4. Click 'Create'");

            AddStep("Attach tag:");
            AddBullet("• In History/Favorite → select a word → click 🏷️");

            AddStep("Delete tag:");
            AddBullet("1. Click '❌' button");
            AddBullet("2. Select tags to remove");
            AddBullet("3. Confirm deletion (tag removed from system but words remain)");

        }

        /// <summary>
        /// Load Lịch sử content
        /// </summary>
        private void LoadHistoryContent()
        {
            AddSectionHeader("📜 SEARCH HISTORY");

            AddBodyText(
                "The History page records all your word lookups, creating a timeline of your vocabulary learning journey. Each time you look up a word, it is automatically added to History with an accurate timestamp.");

            AddStep("Access: Sidebar → History");

            AddSubHeader("✨ Features");
            AddBullet("✅ View all looked-up words (up to 100 most recent)");
            AddBullet("✅ Display lookup time");
            AddBullet("✅ Click on a word to view details");
            AddBullet("✅ Delete individual words or clear the entire history");

            AddSubHeader("🗑️ Delete History");
            AddStep("Delete a single word:");
            AddBullet("• Hover over the word → Click 🗑️");

            AddStep("Delete all history:");
            AddBullet("• Click 'Clear All History' → Confirm");

            AddSubHeader("💡 Tips");
            AddBodyText(
                "This history is very useful when you want to find a word you looked up but forgot to save. Instead of looking it up from scratch, just go to History, scroll down, or use the search function to quickly find the word.");
        }

        /// <summary>
        /// Load Yêu thích content
        /// </summary>
        private void LoadFavouriteContent()
        {
            AddSectionHeader("❤️ FAVORITE WORDS");

            AddBodyText(
                "Favourite Words is where you store the words that are especially important to you—these could be the hardest-to-remember words, your personal favorites, or words you want to review more frequently. Think of it as a 'shortlist' within your larger vocabulary library.");

            AddStep("Access: Sidebar → Favourite Words");

            AddSubHeader("✨ Features");
            AddBullet("✅ View all words marked ❤️");
            AddBullet("✅ Filter by letters A-Z");
            AddBullet("✅ Filter by word type (noun, verb, adjective, etc.)");
            AddBullet("✅ Quick search");
            AddBullet("✅ Click to view details");

            AddSubHeader("💡 Tips for Effective Use");
            AddBodyText("The My Words and Favourite features should be used together but for different purposes:");
            AddBullet("• My Words: Main vocabulary library, stores all learned words");
            AddBullet("• Favourite: Priority list, contains only hard-to-remember words for frequent review");

            AddBodyText(
                "Every day before you start studying, open Favourite and quickly review the words there. When you feel confident about a word, you can remove it from Favourite—it will still remain in My Words if you need to look it up later.");
        }

        /// <summary>
        /// Load Giao diện content
        /// </summary>
        private void LoadThemeContent()
        {
            AddSectionHeader("🎨 INTERFACE CUSTOMIZATION");

            AddBodyText(
                "BlueBerry Dictionary offers various options so you can personalize the interface according to your preferences and learning environment.");

            AddStep("Access: Sidebar → ⚙️ Settings");

            AddSubHeader("🌓 Light/Dark Mode");
            AddBodyText(
                "The app provides both Light and Dark modes so you can choose the one that suits you best.");

            AddStep("Method 1: From Settings");
            AddBullet("1. Go to Settings");
            AddBullet("2. Dropdown 'Display Mode'");
            AddBullet("3. Select:");
            AddBullet("   • Light - Suitable for daytime");
            AddBullet("   • Dark - Suitable for nighttime");
            AddBullet("   • Auto - Follows system setting");

            AddStep("Method 2: Quick Toggle");
            AddBullet("• Click the 🌙/☀️ button at the top-right corner");
            AddBullet("• Quickly switch between Light and Dark");

            AddSubHeader("🎨 Change Interface Colors");
            AddBodyText("Besides Light/Dark mode, you can fully customize the color scheme of the interface.");

            AddStep("Option 1: Default Color");
            AddBullet("• Choose 'Default' in the 'Change Background' dropdown");
            AddBullet("• Pastel blue (Blue Gradient)");

            AddStep("Option 2: Predefined Themes");
            AddBullet("1. Dropdown 'Change Background' → 'Select a theme...'");
            AddBullet("2. Choose one of 23 themes:");
            AddBullet("   • Pastel Dream (Light Pink)");
            AddBullet("   • Lavender Mist (Lavender)");
            AddBullet("   • Aqua Fresh (Aqua Blue)");
            AddBullet("   • Ocean Gradient (Blue Gradient)");
            AddBullet("   • ...and 19 other themes");
            AddBullet("3. Click 'Apply'");

            AddStep("Option 3: Custom Colors");
            AddBullet("1. Dropdown 'Change Background' → 'Custom Colors...'");
            AddBullet("2. Choose 3 colors:");
            AddBullet("   • Primary: Main color (navbar, buttons)");
            AddBullet("   • Secondary: Secondary color (backgrounds)");
            AddBullet("   • Accent: Accent color (text, icons)");
            AddBullet("3. Preview in real-time");
            AddBullet("4. Click 'Apply'");

            AddStep("Reset to Default:");
            AddBullet("• Select 'Default' → Confirm 'Yes'");

            AddSubHeader("🔤 Change Font");
            AddBodyText(
                "Fonts greatly affect user experience. BlueBerry Dictionary allows you to change the font throughout the app.");

            AddStep("Select a font:");
            AddBullet("1. Dropdown 'Font' → 'Select font...'");
            AddBullet("2. Choose from the list (Arial, Calibri, Times New Roman...)");
            AddBullet("3. Adjust size using the slider (10-24pt)");
            AddBullet("4. Preview changes");
            AddBullet("5. Click 'Apply'");

            AddStep("Reset to Default:");
            AddBullet("• Select 'Default' (Segoe UI 14pt)");

        }

        /// <summary>
        /// Load Đồng bộ content
        /// </summary>
        private void LoadSyncContent()
        {
            AddSectionHeader("🔐 LOGIN & SYNC");

            AddSubHeader("🌟 Why Sign In?");
            AddBodyText(
                "One of the most powerful features of BlueBerry Dictionary is data synchronization via Google Drive. This means all your words, tags, history, and settings are safely backed up on the cloud and synchronized across multiple devices.");

            AddBodyText(
                "Imagine you are studying at home on your computer and have saved 500 words. The next day, you bring your laptop to school to review. Simply sign in with the same Google account, and all that data will automatically be downloaded to your laptop, just like you were using your home computer.");

            AddSubHeader("🔑 How to Sign In");
            AddStep("First-time use:");
            AddBullet("1. Open the app");
            AddBullet("2. The login screen appears");
            AddBullet("3. Click 'Sign in with Google'");
            AddBullet("4. Choose your Google account");
            AddBullet("5. Grant access to Google Drive");

            AddStep("Guest Mode:");
            AddBullet("• Click 'Continue as Guest'");
            AddBullet("• No sync, data is stored locally only");

            AddSubHeader("☁️ Data Synchronization");
            AddBodyText(
                "Once signed in, any action you take to add, edit, or delete words will be automatically synchronized to Google Drive within a few seconds.");

            AddStep("Data that is synchronized:");
            AddBullet("✅ My Words (saved words)");
            AddBullet("✅ Tags");
            AddBullet("✅ History (search history)");
            AddBullet("✅ Favourite Words");
            AddBullet("✅ Settings");

            AddStep("How to sync:");
            AddBullet("Automatic:");
            AddBullet("• Upon sign-in, data is automatically synced from Google Drive");
            AddBullet("• When adding/editing/deleting words, changes are automatically uploaded to the cloud");
            AddBullet("Manual:");
            AddBullet("1. Go to User Profile (Sidebar → Click avatar)");
            AddBullet("2. Click 'Sync Now'");

            AddStep("Sync status:");
            AddBullet("• ✅ Green: Synced");
            AddBullet("• 🔄 Yellow: Syncing");
            AddBullet("• ❌ Red: Sync failed");

            AddSubHeader("👤 Account Management");
            AddStep("Access: Sidebar → Click on avatar/name");

            AddStep("Displayed information:");
            AddBullet("• Avatar");
            AddBullet("• Account name");
            AddBullet("• Email");
            AddBullet("• Number of saved words");
            AddBullet("• Sync status");

            AddStep("Sign out:");
            AddBullet("1. Click 'Sign Out'");
            AddBullet("2. Confirm");
            AddBullet("3. Local data will remain intact");

            AddSubHeader("💡 Important Notes");
            AddBullet("⚠️ Use only one device at a time to avoid data conflicts");
            AddBullet("⚠️ Ensure a stable Internet connection when syncing");
            AddBullet("⚠️ Data is stored in the BlueBerryDictionary folder on Google Drive");
        }

        // ==================== HELPER METHODS ====================

        private void AddSectionHeader(string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("SectionHeaderStyle")
            };
            ContentPanel.Children.Add(textBlock);
        }

        private void AddSubHeader(string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("SubHeaderStyle")
            };
            ContentPanel.Children.Add(textBlock);
        }

        private void AddBodyText(string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("BodyTextStyle")
            };
            ContentPanel.Children.Add(textBlock);
        }

        private void AddStep(string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("StepTextStyle"),
                FontWeight = FontWeights.SemiBold
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

        /// <summary>
        /// Apply font từ App.Current.Resources
        /// </summary>
        private void ApplyGlobalFont()
        {
            try
            {
                if (Application.Current.Resources.Contains("AppFontFamily"))
                {
                    this.FontFamily = (System.Windows.Media.FontFamily)Application.Current.Resources["AppFontFamily"];
                }

                if (Application.Current.Resources.Contains("AppFontSize"))
                {
                    this.FontSize = (double)Application.Current.Resources["AppFontSize"];
                }

                System.Diagnostics.Debug.WriteLine($"✅ Applied font to {this.GetType().Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font to dialog error: {ex.Message}");
            }
        }

    }
}