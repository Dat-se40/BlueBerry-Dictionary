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
            AddSection("🔍 VỀ TRA CỨU");

            AddQuestion("Q1: Tại sao không tìm thấy từ?");
            AddAnswer("Có một vài lý do khiến bạn không tìm thấy từ:");
            AddBullet("✅ Kiểm tra chính tả - ứng dụng sẽ gợi ý từ tương tự");
            AddBullet("✅ Thử tra từ đơn giản hơn (VD: \"running\" → \"run\")");
            AddBullet("✅ Kiểm tra kết nối Internet (nếu tra online)");
            AddBullet("✅ Một số từ hiếm có thể không có trong database");

            AddQuestion("Q2: Tại sao không phát được âm thanh?");
            AddAnswer("Hãy kiểm tra các nguyên nhân sau:");
            AddBullet("✅ Kiểm tra loa/tai nghe");
            AddBullet("✅ Kiểm tra kết nối Internet (audio stream từ server)");
            AddBullet("✅ Thử phát lại hoặc restart ứng dụng");
            AddBullet("✅ Một số từ hiếm có thể không có audio");

            AddQuestion("Q3: Offline mode hoạt động như thế nào?");
            AddAnswer("Từ được tải về lưu ở: C:\\Users\\[YourName]\\AppData\\Local\\BlueBerryDictionary\\Data\\PersistentStorage\\StoredWord\\");
            AddAnswer("Chỉ từ đã tải mới tra được offline. Không tải trước toàn bộ từ điển vì quá nặng.");
        }

        // ========== MANAGE FAQ ==========
        private void LoadManageFAQ()
        {
            AddSection("📚 VỀ QUẢN LÝ TỪ VỰNG");

            AddQuestion("Q4: Tối đa bao nhiêu từ trong My Words?");
            AddAnswer("Không giới hạn! Nhưng app có thể chậm nếu >10,000 từ.");
            AddAnswer("Khuyến nghị: Dùng tags để phân loại thay vì lưu quá nhiều từ.");

            AddQuestion("Q5: Làm sao để backup dữ liệu?");
            AddAnswer("Cách 1: Đăng nhập Google (Khuyến nghị)");
            AddBullet("• Dữ liệu tự động backup lên Google Drive");
            AddBullet("• An toàn nhất!");
            AddAnswer("Cách 2: Copy thủ công");
            AddBullet("• Vào thư mục: C:\\Users\\[YourName]\\AppData\\Local\\BlueBerryDictionary\\");
            AddBullet("• Copy toàn bộ thư mục Data/");
            AddBullet("• Paste vào máy khác cùng đường dẫn");

            AddQuestion("Q6: Xóa nhầm từ, có thể khôi phục?");
            AddAnswer("❌ Không có tính năng undo");
            AddAnswer("✅ Nếu đã đồng bộ Google Drive:");
            AddBullet("1. Đăng xuất");
            AddBullet("2. Đăng nhập lại");
            AddBullet("3. Chọn \"Giữ dữ liệu trên cloud\"");

            AddQuestion("Q7: Tags có giới hạn không?");
            AddAnswer("Không giới hạn số lượng tags. Mỗi từ có thể có nhiều tags.");
            AddAnswer("Khuyến nghị: Tạo 5-10 tags chính (VD: IELTS, TOEIC, Daily)");
        }

        // ========== THEME FAQ ==========
        private void LoadThemeFAQ()
        {
            AddSection("🎨 VỀ GIAO DIỆN");

            AddQuestion("Q8: Theme tùy chỉnh có lưu khi tắt app?");
            AddAnswer("✅ Có, lưu tự động trong AppSettings.json");
            AddAnswer("✅ Khi restart, theme được load lại");

            AddQuestion("Q9: Làm sao để quay về màu mặc định?");
            AddAnswer("1. Vào Settings");
            AddAnswer("2. Dropdown \"Đổi nền\" → \"Mặc định\"");
            AddAnswer("3. Xác nhận \"Yes\"");

            AddQuestion("Q10: Toggle Light/Dark có ảnh hưởng đến theme tùy chỉnh?");
            AddAnswer("✅ Có! Theme tùy chỉnh sẽ tự động adapt sang Dark mode");
            AddAnswer("Màu sẽ được tối hơn (darken) để phù hợp");

            AddQuestion("Q11: Font chữ có áp dụng cho toàn bộ app?");
            AddAnswer("✅ Có, áp dụng cho tất cả text trong app");
            AddAnswer("⚠️ Một số icon (emoji) không thay đổi");
        }

        // ========== SYNC FAQ ==========
        private void LoadSyncFAQ()
        {
            AddSection("☁️ VỀ ĐỒNG BỘ");

            AddQuestion("Q12: Đồng bộ mất bao lâu?");
            AddAnswer("Lần đầu (merge data): 10-30 giây (tùy số từ)");
            AddAnswer("Lần sau (incremental): 1-5 giây");
            AddAnswer("Upload 1 từ mới: <1 giây");

            AddQuestion("Q13: Dữ liệu lưu ở đâu trên Google Drive?");
            AddAnswer("Thư mục: BlueBerryDictionary/Users/[email]/");
            AddAnswer("Files:");
            AddBullet("• MyWords.json (từ vựng)");
            AddBullet("• Tags.json (nhãn)");
            AddBullet("• Settings.json (cài đặt)");

            AddQuestion("Q14: Có thể dùng nhiều thiết bị?");
            AddAnswer("✅ Có! Đăng nhập cùng Google account");
            AddAnswer("Dữ liệu tự động đồng bộ giữa các thiết bị");
            AddAnswer("⚠️ Chỉ nên dùng 1 thiết bị tại 1 thời điểm (tránh conflict)");

            AddQuestion("Q15: Không có Internet, có dùng được app?");
            AddAnswer("✅ Có thể tra từ (nếu đã tải offline)");
            AddAnswer("✅ Xem My Words, History, Favourite");
            AddAnswer("❌ Không đồng bộ được");
            AddAnswer("❌ Không tra từ mới online");

            AddQuestion("Q16: Lỗi \"Đồng bộ thất bại\", làm sao?");
            AddAnswer("Cách 1: Kiểm tra kết nối");
            AddBullet("• Mở trình duyệt, thử truy cập google.com");
            AddBullet("• Kiểm tra firewall có chặn app không");
            AddAnswer("Cách 2: Đăng xuất/nhập lại");
            AddBullet("1. Đăng xuất");
            AddBullet("2. Restart app");
            AddBullet("3. Đăng nhập lại");
            AddBullet("4. Chọn \"Merge data\" (gộp dữ liệu)");
        }

        // ========== BUGS FAQ ==========
        private void LoadBugsFAQ()
        {
            AddSection("🐛 VỀ LỖI KỸ THUẬT");

            AddQuestion("Q17: App bị crash khi mở");
            AddAnswer("1. Kiểm tra .NET 9.0 Runtime đã cài đúng chưa");
            AddAnswer("2. Xóa file AppSettings.json (app sẽ tạo mới)");
            AddAnswer("3. Reinstall app");

            AddQuestion("Q18: App chạy chậm, lag");
            AddAnswer("Nguyên nhân: Quá nhiều từ trong My Words (>10,000)");
            AddAnswer("Giải pháp:");
            AddBullet("1. Xóa từ cũ không dùng");
            AddBullet("2. Export ra file text, chỉ giữ từ quan trọng");
            AddBullet("3. Sử dụng tính năng lọc thay vì load tất cả");

            AddQuestion("Q19: Không thể đăng nhập Google");
            AddAnswer("1. Kiểm tra trình duyệt mặc định (phải là Chrome/Edge/Firefox)");
            AddAnswer("2. Xóa cookies Google");
            AddAnswer("3. Thử đăng nhập Google trên trình duyệt trước");
            AddAnswer("4. Disable antivirus tạm thời");

            AddQuestion("Q20: Icon/Hình ảnh không hiển thị");
            AddAnswer("• Kiểm tra thư mục Resources/ còn đầy đủ không");
            AddAnswer("• Reinstall app");

            AddQuestion("❓ Vẫn gặp vấn đề?");
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
