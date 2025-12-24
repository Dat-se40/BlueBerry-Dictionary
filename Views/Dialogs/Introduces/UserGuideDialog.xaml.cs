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
            AddSectionHeader("🎯 GIỚI THIỆU");

            AddBodyText("BlueBerry Dictionary là một ứng dụng từ điển tiếng Anh thông minh được phát triển với mục tiêu mang đến trải nghiệm tra cứu và học từ vựng hiệu quả nhất cho người dùng. Ứng dụng không chỉ đơn thuần là một công cụ tra từ, mà còn là người bạn đồng hành đắc lực giúp bạn xây dựng và quản lý kho từ vựng cá nhân một cách khoa học và hiệu quả.");

            AddBodyText("Với giao diện thân thiện, hiện đại và nhiều tính năng nâng cao, BlueBerry Dictionary phù hợp cho mọi đối tượng người học - từ học sinh, sinh viên đang chuẩn bị cho các kỳ thi IELTS, TOEIC, đến những người đi làm cần nâng cao vốn từ vựng chuyên ngành. Đặc biệt, hệ thống đồng bộ dữ liệu qua Google Drive giúp bạn có thể học mọi lúc mọi nơi mà không lo mất dữ liệu.");

            AddSubHeader("✨ Tính năng nổi bật");
            AddBullet("✅ Tra cứu từ với nhiều nguồn dữ liệu uy tín");
            AddBullet("✅ Phát âm chuẩn US 🇺🇸 và UK 🇬🇧");
            AddBullet("✅ Quản lý từ vựng với hệ thống Tags (nhãn)");
            AddBullet("✅ Lưu trữ lịch sử và từ yêu thích");
            AddBullet("✅ Tùy chỉnh giao diện (23 themes + custom)");
            AddBullet("✅ Đồng bộ dữ liệu qua Google Drive");
            AddBullet("✅ Hỗ trợ offline mode");

            AddSubHeader("👥 Đội ngũ phát triển");
            AddBodyText("Môn học: Lập trình trực quan");
            AddBodyText("Giảng viên: ThS. Mai Trọng Khang");
            AddBodyText("Học kỳ: 1 - Năm 2024-2025");
            AddBodyText("Thành viên:");
            AddBullet("• Nguyễn Tấn Đạt");
            AddBullet("• Võ Nguyễn Thanh Hương");
            AddBullet("• Phan Thế Phong");
        }

        /// <summary>
        /// Load Tra cứu content
        /// </summary>
        private void LoadSearchContent()
        {
            AddSectionHeader("🔍 TRA CỨU TỪ VỰNG");

            AddSubHeader("📖 Tra từ cơ bản");
            AddBodyText("Tính năng tra cứu từ vựng là trái tim của BlueBerry Dictionary. Khi bạn mở ứng dụng, thanh tìm kiếm nổi bật ở trung tâm màn hình sẵn sàng đón bạn.");

            AddStep("Bước 1: Nhập từ cần tra vào thanh tìm kiếm");
            AddStep("Bước 2: Chọn từ gợi ý hoặc nhấn Enter");
            AddBullet("• Ứng dụng hiển thị gợi ý trong khi bạn gõ");
            AddBullet("• Gợi ý dựa trên độ tương đồng với từ bạn nhập");

            AddStep("Bước 3: Xem thông tin từ vựng");
            AddBullet("✅ Phiên âm: US 🇺🇸 và UK 🇬🇧");
            AddBullet("✅ Nghĩa: Tất cả nghĩa của từ (danh từ, động từ, tính từ...)");
            AddBullet("✅ Ví dụ: Câu ví dụ minh họa");
            AddBullet("✅ Từ đồng nghĩa/Trái nghĩa (nếu có)");

            AddSubHeader("🔊 Phát âm chuẩn");
            AddBodyText("BlueBerry Dictionary cung cấp cả hai giọng phát âm Mỹ và Anh, giúp bạn có thể lựa chọn accent phù hợp với mục tiêu học tập.");
            AddStep("Cách 1: Click icon loa 🔊 bên cạnh phiên âm");
            AddStep("Cách 2: Phím tắt");
            AddBullet("• Ctrl + U: Phát âm US");
            AddBullet("• Ctrl + K: Phát âm UK");

            AddSubHeader("💾 Lưu từ vựng");
            AddBodyText("Sau khi tra cứu một từ hữu ích, bạn có thể lưu lại để ôn tập sau này.");

            AddStep("Cách 1: Lưu toàn bộ từ");
            AddBullet("1. Click nút 'Lưu từ' (💾) ở góc trên bên phải");
            AddBullet("2. Từ sẽ được lưu vào My Words với tất cả nghĩa");

            AddStep("Cách 2: Lưu nghĩa cụ thể (Khuyến nghị)");
            AddBullet("1. Click nút 'Lưu từ' (💾)");
            AddBullet("2. Chọn các nghĩa bạn muốn lưu");
            AddBullet("3. (Tùy chọn) Gắn nhãn (tags) cho từ");
            AddBullet("4. Click 'Lưu'");

            AddSubHeader("❤️ Đánh dấu yêu thích");
            AddBullet("• Click icon ❤️ để thêm/bỏ từ khỏi danh sách yêu thích");
            AddBullet("• Trái tim đỏ = Đã yêu thích");
            AddBullet("• Trái tim xám = Chưa yêu thích");

            AddSubHeader("🌐 Chế độ Offline");
            AddBodyText("Một trong những ưu điểm lớn của BlueBerry Dictionary là khả năng hoạt động offline.");

            AddStep("Tải từ về máy:");
            AddBullet("1. Tra từ online lần đầu");
            AddBullet("2. Click nút 'Tải về' (📥)");
            AddBullet("3. Từ sẽ được lưu vào máy");

            AddStep("Sử dụng offline:");
            AddBullet("• Lần sau tra từ, ứng dụng tự động dùng bản offline (nếu có)");
            AddBullet("• Icon 📡: Xanh = Online, Xám = Offline");
        }

        /// <summary>
        /// Load Quản lý từ content
        /// </summary>
        private void LoadManageContent()
        {
            AddSectionHeader("📚 QUẢN LÝ TỪ VỰNG CÁ NHÂN");

            AddSubHeader("📖 My Words - Kho từ vựng của bạn");
            AddBodyText("My Words là tính năng trung tâm của BlueBerry Dictionary, nơi bạn xây dựng và quản lý kho từ vựng cá nhân của mình.");
            AddStep("Truy cập: Sidebar → My Words");

            AddSubHeader("🏷️ Tạo và quản lý Tags (Nhãn)");
            AddBodyText("Tags là công cụ mạnh mẽ giúp bạn tổ chức từ vựng theo cách của riêng mình.");

            AddStep("Bước 1: Tạo nhãn mới");
            AddBullet("1. Click nút '🏷️ Tạo nhãn mới'");
            AddBullet("2. Điền thông tin:");
            AddBullet("   • Tên nhãn: VD 'IELTS', 'Business English'");
            AddBullet("   • Chọn icon: Click vào icon mẫu hoặc nhập emoji");
            AddBullet("   • Chọn màu: Click vào bảng màu");
            AddBullet("3. Click 'Tạo'");

            AddStep("Bước 2: Gắn nhãn cho từ");
            AddBullet("Cách 1: Khi lưu từ");
            AddBullet("• Chọn nhãn từ dropdown trong dialog 'Chọn nghĩa để lưu'");
            AddBullet("Cách 2: Gắn sau khi đã lưu");
            AddBullet("1. Vào My Words");
            AddBullet("2. Click vào từ cần gắn nhãn");
            AddBullet("3. Click 'Gắn nhãn'");
            AddBullet("4. Chọn nhãn từ danh sách");

            AddStep("Xóa nhãn:");
            AddBullet("1. Click icon ⚙️ trên thẻ nhãn");
            AddBullet("2. Chọn các nhãn muốn xóa");
            AddBullet("3. Xác nhận");

            AddSubHeader("🔍 Lọc từ vựng");
            AddBodyText("Với hàng trăm hoặc hàng nghìn từ trong My Words, việc tìm kiếm và lọc trở nên cực kỳ quan trọng.");

            AddStep("Lọc theo chữ cái:");
            AddBullet("• Click vào chữ cái (A-Z) ở thanh bên trên");
            AddBullet("• Chọn 'Tất cả' để bỏ lọc");

            AddStep("Lọc theo loại từ:");
            AddBullet("• Dropdown 'Loại từ' → Chọn:");
            AddBullet("   - Tất cả");
            AddBullet("   - Danh từ (noun)");
            AddBullet("   - Động từ (verb)");
            AddBullet("   - Tính từ (adjective)");
            AddBullet("   - Trạng từ (adverb)");

            AddStep("Lọc theo nhãn:");
            AddBullet("• Click vào thẻ nhãn ở thanh bên trên");
            AddBullet("• Chỉ hiển thị từ có nhãn đó");

            AddStep("Tìm kiếm nhanh:");
            AddBullet("• Nhập từ vào ô 'Tìm trong từ đã lưu...'");
            AddBullet("• Kết quả hiển thị realtime");

            AddSubHeader("📊 Thống kê từ vựng");
            AddBodyText("Ở góc trên bên phải, bạn sẽ thấy thống kê hữu ích:");
            AddBullet("• 📚 Tổng số từ: Tổng số từ đã lưu");
            AddBullet("• 🏷️ Nhãn: Số nhãn đã được tạo");
            AddBullet("• 🆕 Từ mới tuần này: Từ đượcthêm trong 7 ngày gần nhất");
            AddBullet("• 📅 Từ mới tháng này: Từ được thêm trong 30 ngày gần nhất");

            AddSubHeader("✏️ Chỉnh sửa/Xóa từ");
            AddStep("Chỉnh sửa:");
            AddBullet("1. Click vào từ trong My Words");
            AddBullet("2. Click 'Chỉnh sửa'");
            AddBullet("3. Sửa nghĩa, gắn/bỏ nhãn");
            AddBullet("4. Click 'Lưu'");

            AddStep("Xóa:");
            AddBullet("1. Click vào từ");
            AddBullet("2. Click 'Xóa'");
            AddBullet("3. Xác nhận");
        }

        /// <summary>
        /// Load Lịch sử content
        /// </summary>
        private void LoadHistoryContent()
        {
            AddSectionHeader("📜 LỊCH SỬ TRA CỨU");

            AddBodyText("Trang History ghi lại toàn bộ lịch sử tra cứu của bạn, tạo thành một timeline về hành trình học từ vựng. Mỗi lần bạn tra một từ, nó sẽ được tự động thêm vào History với timestamp chính xác.");

            AddStep("Truy cập: Sidebar → History");

            AddSubHeader("✨ Tính năng");
            AddBullet("✅ Xem tất cả từ đã tra (100 từ gần nhất)");
            AddBullet("✅ Hiển thị thời gian tra cứu");
            AddBullet("✅ Click vào từ để xem lại chi tiết");
            AddBullet("✅ Xóa từng từ hoặc xóa toàn bộ lịch sử");

            AddSubHeader("🗑️ Xóa lịch sử");
            AddStep("Xóa một từ:");
            AddBullet("• Hover vào từ → Click 🗑️");

            AddStep("Xóa toàn bộ:");
            AddBullet("• Click 'Xóa tất cả lịch sử' → Xác nhận");

            AddSubHeader("💡 Mẹo sử dụng");
            AddBodyText("Lịch sử này rất hữu ích khi bạn muốn tìm lại một từ mà mình đã tra nhưng quên mất không lưu. Thay vì phải tra lại từ đầu, bạn chỉ cần vào History, scroll xuống hoặc dùng tìm kiếm để tìm lại từ đó nhanh chóng.");
        }

        /// <summary>
        /// Load Yêu thích content
        /// </summary>
        private void LoadFavouriteContent()
        {
            AddSectionHeader("❤️ TỪ YÊU THÍCH");

            AddBodyText("Favourite Words là nơi lưu trữ những từ đặc biệt quan trọng với bạn - có thể là những từ khó nhớ nhất, những từ bạn yêu thích nhất, hoặc những từ bạn muốn ôn tập thường xuyên hơn. Đây như một 'shortlist' trong kho từ vựng lớn của bạn.");

            AddStep("Truy cập: Sidebar → Favourite Words");

            AddSubHeader("✨ Tính năng");
            AddBullet("✅ Xem tất cả từ đã đánh dấu ❤️");
            AddBullet("✅ Lọc theo chữ cái A-Z");
            AddBullet("✅ Lọc theo loại từ (noun, verb, adjective...)");
            AddBullet("✅ Tìm kiếm nhanh");
            AddBullet("✅ Click để xem chi tiết");

            AddSubHeader("📊 Giới hạn");
            AddBodyText("Số lượng từ yêu thích có giới hạn mặc định là 1000 từ. Tuy nhiên, bạn có thể thay đổi con số này trong Settings:");
            AddBullet("• 500 từ (Tiết kiệm dung lượng)");
            AddBullet("• 1000 từ (Mặc định)");
            AddBullet("• 5000 từ (Cho người học nhiều)");
            AddBullet("• Unlimited (Không giới hạn)");

            AddSubHeader("💡 Mẹo sử dụng hiệu quả");
            AddBodyText("Hai tính năng My Words và Favourite nên được sử dụng song song nhưng với mục đích khác nhau:");
            AddBullet("• My Words: Kho từ chính, lưu trữ toàn bộ từ vựng đã học");
            AddBullet("• Favourite: Danh sách ưu tiên, chỉ chứa từ khó nhớ cần ôn nhiều");

            AddBodyText("Mỗi ngày trước khi bắt đầu học, hãy mở Favourite và review nhanh các từ ở đó. Khi bạn cảm thấy đã thuộc một từ, có thể bỏ nó khỏi Favourite - nhưng nó vẫn ở trong My Words nếu bạn cần tra lại sau này.");
        }

        /// <summary>
        /// Load Giao diện content
        /// </summary>
        private void LoadThemeContent()
        {
            AddSectionHeader("🎨 TÙY CHỈNH GIAO DIỆN");

            AddBodyText("BlueBerry Dictionary cung cấp nhiều tùy chọn để bạn có thể cá nhân hóa giao diện theo sở thích và môi trường học tập của mình.");

            AddStep("Truy cập: Sidebar → ⚙️ Settings");

            AddSubHeader("🌓 Chế độ Sáng/Tối (Light/Dark Mode)");
            AddBodyText("BlueBerry Dictionary cung cấp cả chế độ Light (sáng) và Dark (tối) để bạn có thể lựa chọn phù hợp nhất với mình.");

            AddStep("Cách 1: Từ Settings");
            AddBullet("1. Vào Settings");
            AddBullet("2. Dropdown 'Chế độ hiển thị'");
            AddBullet("3. Chọn:");
            AddBullet("   • Light (Sáng) - Phù hợp ban ngày");
            AddBullet("   • Dark (Tối) - Phù hợp ban đêm");
            AddBullet("   • Auto (Tự động theo hệ thống)");

            AddStep("Cách 2: Toggle nhanh");
            AddBullet("• Click nút 🌙/☀️ ở góc trên bên phải");
            AddBullet("• Chuyển đổi nhanh giữa Light và Dark");

            AddSubHeader("🎨 Thay đổi màu sắc giao diện");
            AddBodyText("Ngoài việc chọn Light hay Dark, bạn còn có thể thay đổi hoàn toàn bảng màu của giao diện.");

            AddStep("Option 1: Màu mặc định");
            AddBullet("• Chọn 'Mặc định' trong dropdown 'Đổi nền'");
            AddBullet("• Màu xanh pastel (Blue Gradient)");

            AddStep("Option 2: Chọn theme có sẵn");
            AddBullet("1. Dropdown 'Đổi nền' → 'Chọn theme có sẵn...'");
            AddBullet("2. Chọn 1 trong 23 theme:");
            AddBullet("   • Pastel Dream (Hồng nhạt)");
            AddBullet("   • Lavender Mist (Tím lavender)");
            AddBullet("   • Aqua Fresh (Xanh nước biển)");
            AddBullet("   • Ocean Gradient (Xanh dương gradient)");
            AddBullet("   • ...và 19 theme khác");
            AddBullet("3. Click 'Áp dụng'");

            AddStep("Option 3: Tùy chỉnh màu riêng");
            AddBullet("1. Dropdown 'Đổi nền' → 'Chọn màu tùy chỉnh...'");
            AddBullet("2. Chọn 3 màu:");
            AddBullet("   • Primary: Màu chính (navbar, buttons)");
            AddBullet("   • Secondary: Màu phụ (backgrounds)");
            AddBullet("   • Accent: Màu nhấn (text, icons)");
            AddBullet("3. Xem preview realtime");
            AddBullet("4. Click 'Áp dụng'");

            AddStep("Quay về mặc định:");
            AddBullet("• Chọn 'Mặc định' → Xác nhận 'Yes'");

            AddSubHeader("🔤 Thay đổi font chữ");
            AddBodyText("Font chữ cũng ảnh hưởng rất lớn đến trải nghiệm sử dụng. BlueBerry Dictionary cho phép bạn thay đổi font chữ cho toàn bộ ứng dụng.");

            AddStep("Chọn font có sẵn:");
            AddBullet("1. Dropdown 'Font chữ' → 'Chọn font...'");
            AddBullet("2. Chọn font từ danh sách (Arial, Calibri, Times New Roman...)");
            AddBullet("3. Kéo slider để điều chỉnh kích thước (10-24pt)");
            AddBullet("4. Xem preview");
            AddBullet("5. Click 'Áp dụng'");

            AddStep("Reset về mặc định:");
            AddBullet("• Chọn 'Mặc định' (Segoe UI 14pt)");

            AddSubHeader("📊 Giới hạn số từ yêu thích");
            AddBullet("• 500 từ (Tiết kiệm dung lượng)");
            AddBullet("• 1000 từ (Mặc định)");
            AddBullet("• 5000 từ (Cho người học nhiều)");
            AddBullet("• Unlimited (Không giới hạn)");

            AddSubHeader("💾 Tự động lưu");
            AddStep("Toggle 'Tự động lưu':");
            AddBullet("• Bật: Tự động lưu settings mỗi khi thay đổi");
            AddBullet("• Tắt: Phải click 'Lưu cài đặt' thủ công");
        }

        /// <summary>
        /// Load Đồng bộ content
        /// </summary>
        private void LoadSyncContent()
        {
            AddSectionHeader("🔐 ĐĂNG NHẬP & ĐỒNG BỘ");

            AddSubHeader("🌟 Tại sao nên đăng nhập?");
            AddBodyText("Một trong những tính năng mạnh mẽ nhất của BlueBerry Dictionary là khả năng đồng bộ dữ liệu qua Google Drive. Điều này có nghĩa là tất cả từ vựng, tags, lịch sử, và cài đặt của bạn sẽ được backup an toàn trên cloud và đồng bộ giữa nhiều thiết bị.");

            AddBodyText("Hãy tưởng tượng bạn đang học trên máy tính ở nhà, đã lưu được 500 từ vựng. Hôm sau, bạn mang laptop đến trường để ôn bài. Chỉ cần đăng nhập bằng cùng tài khoản Google, tất cả dữ liệu đó sẽ tự động được tải về laptop, giống hệt như bạn đang dùng máy tính ở nhà.");

            AddSubHeader("🔑 Cách đăng nhập");
            AddStep("Lần đầu sử dụng:");
            AddBullet("1. Mở ứng dụng");
            AddBullet("2. Màn hình đăng nhập xuất hiện");
            AddBullet("3. Click 'Đăng nhập với Google'");
            AddBullet("4. Chọn tài khoản Google");
            AddBullet("5. Cho phép quyền truy cập Google Drive");

            AddStep("Chế độ Guest:");
            AddBullet("• Click 'Tiếp tục với Guest'");
            AddBullet("• Không đồng bộ, dữ liệu chỉ lư");

            AddSubHeader("☁️ Đồng bộ dữ liệu");
            AddBodyText("Sau khi đăng nhập, mọi thao tác thêm, sửa, xóa từ vựng của bạn sẽ tự động được đồng bộ lên Google Drive trong vài giây.");

            AddStep("Dữ liệu được đồng bộ:");
            AddBullet("✅ My Words (từ vựng đã lưu)");
            AddBullet("✅ Tags (nhãn)");
            AddBullet("✅ History (lịch sử tra cứu)");
            AddBullet("✅ Favourite Words (từ yêu thích)");
            AddBullet("✅ Settings (cài đặt)");

            AddStep("Cách đồng bộ:");
            AddBullet("Tự động:");
            AddBullet("• Khi đăng nhập, dữ liệu tự động đồng bộ từ Google Drive");
            AddBullet("• Khi thêm/sửa/xóa từ, tự động upload lên cloud");
            AddBullet("Thủ công:");
            AddBullet("1. Vào User Profile (Sidebar → Click avatar)");
            AddBullet("2. Click 'Đồng bộ ngay'");

            AddStep("Trạng thái đồng bộ:");
            AddBullet("• ✅ Xanh: Đã đồng bộ");
            AddBullet("• 🔄 Vàng: Đang đồng bộ");
            AddBullet("• ❌ Đỏ: Lỗi đồng bộ");

            AddSubHeader("👤 Quản lý tài khoản");
            AddStep("Truy cập: Sidebar → Click vào avatar/tên");

            AddStep("Thông tin hiển thị:");
            AddBullet("• Avatar");
            AddBullet("• Tên tài khoản");
            AddBullet("• Email");
            AddBullet("• Số từ đã lưu");
            AddBullet("• Trạng thái đồng bộ");

            AddStep("Đăng xuất:");
            AddBullet("1. Click 'Đăng xuất'");
            AddBullet("2. Xác nhận");
            AddBullet("3. Dữ liệu local vẫn được giữ lại");

            AddSubHeader("💡 Lưu ý quan trọng");
            AddBullet("⚠️ Chỉ nên dùng 1 thiết bị tại 1 thời điểm để tránh xung đột dữ liệu");
            AddBullet("⚠️ Đảm bảo có kết nối Internet ổn định khi đồng bộ");
            AddBullet("⚠️ Dữ liệu được lưu trong thư mục BlueBerryDictionary trên Google Drive");
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