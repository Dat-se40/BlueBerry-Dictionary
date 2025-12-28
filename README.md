# BlueBerry Dictionary 📚

## Tổng quan đồ án

BlueBerry Dictionary là ứng dụng từ điển tiếng Anh được xây dựng bằng WPF .NET, cung cấp trải nghiệm tra cứu từ vựng toàn diện với giao diện hiện đại và nhiều tính năng nâng cao.

### Thông tin đồ án
- **Môn học**: Lập trình trực quan
- **Giảng viên**: ThS. Mai Trọng Khang
- **Học kỳ**: 1 - Năm 2024-2025
- **Thành viên nhóm**:
  - Nguyễn Tấn Đạt 
  - Võ Nguyễn Thanh Hương 
  - Phan Thế Phong 

### Công nghệ
- **Framework**: WPF .NET
- **Runtime**: Microsoft.NETCore.App 9.0.8
- **Pattern**: MVVM (Model-View-ViewModel)
### Build .exe file
```
dotnet clean
dotnet publish -c Release -r win-x64 --self-contained true
cd bin\Release\net9.0-windows\win-x64\publish
.\BlueBerryDictionary.exe
```
---

## Chức năng chính

### 1. Tra cứu từ vựng
- **Multi-source API**: Tích hợp 3 nguồn từ điển:
  - Free Dictionary API (nguồn chính)
  - Merriam-Webster Dictionary & Thesaurus
  - Cambridge Audio
- **Autocomplete**: Gợi ý từ thông minh với thuật toán Levenshtein Distance
- **Phát âm**: Hỗ trợ cả phát âm US 🇺🇸 và UK 🇬🇧
- **Cache thông minh**: Lưu cache 100 từ gần nhất, tối ưu tốc độ tra cứu

### 2. Quản lý từ vựng cá nhân
- ***HomePage**: Hiện thị các câu quote random, tránh gây nhàm chán cho người dùng
- **My Words**: Lưu trữ từ vựng với khả năng:
  - Chọn nghĩa cụ thể khi lưu từ
  - Gắn nhãn (tags) tùy chỉnh với icon và màu sắc
  - Lọc theo chữ cái, loại từ, nhãn
  - Thống kê từ vựng (tổng số từ, từ mới tuần/tháng)
- **Favourite Words**: Đánh dấu từ yêu thích
- **History**: Lịch sử tra cứu với timestamp

### 3. Offline Mode
- Tải từ về máy để sử dụng không cần internet
- Hỗ trợ ~3000+ từ vựng phổ biến sẵn có

### 4. Giao diện
- **Theme Toggle**: Chuyển đổi Light/Dark mode mượt mà
- **Responsive Design**: Tự động điều chỉnh kích thước
- **Animations**: Hiệu ứng chuyển trang, hover, click
- **Navigation**: Sidebar + Toolbar đầy đủ
- **SettingPage** điều chỉnh phù hợp cá nhân hóa
### 5. Hỗ trợ đăng nhập
- Có thể sử dụng từ nhiều các thiết bị khác nhau
- Nhiều người dùng khác nhau có thể sài chung một thiết bị

### 5. Đăng nhập, đồng bộ thông tin tài khoản người dùng
---

## Cách sử dụng + User Flow

### Flow 1: Tra cứu từ cơ bản
```
1. Nhập từ vào search bar
2. Chọn từ gợi ý (hoặc nhấn Enter)
3. Xem định nghĩa, phát âm, ví dụ
4. Click 🔊 để nghe phát âm US/UK
5. Lưu từ vào My Words (💾) hoặc Favourite (❤️)
```

### Flow 2: Quản lý từ vựng
```
1. Vào My Words từ sidebar
2. Tạo nhãn mới (🏷️ Tạo nhãn mới):
   - Chọn icon và màu
   - Đặt tên nhãn (VD: IELTS, Business)
3. Lưu từ:
   - Chọn nghĩa muốn lưu
   - Gắn nhãn (tùy chọn)
4. Lọc từ:
   - Theo chữ cái (A-Z)
   - Theo loại từ (noun, verb, adjective)
   - Theo nhãn
```

### Flow 3: Sử dụng Offline
```
1. Tra từ online lần đầu
2. Click icon Download (📥)
3. Lần sau tra từ tự động dùng bản offline
```

---

## Cách cài đặt

### Yêu cầu hệ thống
- Windows 10/11
- .NET 9.0 Runtime
- Visual Studio 2022 (để build từ source)

### Cài đặt từ Source Code

1. **Clone repository**
```bash
git clone https://github.com/Dat-se40/BlueBerry-Dictionary.git
cd BlueBerry-Dictionary
```

2. **Cấu hình API Keys**
   - Mở file `ApiClient/Configuration/appsettings.json`
   - Thay thế API keys (hoặc sử dụng keys mặc định đã có):
```json
{
  "ApiKeys": {
    "MerriamWebsterDictionary": "YOUR_KEY_HERE",
    "MerriamWebsterThesaurus": "YOUR_KEY_HERE",
    "Pixabay": "YOUR_KEY_HERE"
  }
}
```

3. **Restore NuGet Packages**
```bash
dotnet restore
```

4. **Build & Run**
```bash
dotnet build
dotnet run
```

### Cài đặt từ Release (nếu có)
1. Tải file `.exe` từ [Releases](https://github.com/Dat-se40/BlueBerry-Dictionary/releases)
2. Chạy file installer
3. Mở ứng dụng và sử dụng

---

## Tổng quan Kiến trúc & Kỹ Thuật

### Cấu trúc Project

```
BlueBerryDictionary/
├── ApiClient/              # API integration layer
│   ├── Client/
│   │   ├── Audio.cs       # Audio playback service
│   │   └── MerriamWebster.cs
│   ├── Configuration/
│   │   ├── Config.cs      # Singleton config manager
│   │   └── appsettings.json
│   └── DictionaryApiClient.cs  # Main API client
│
├── Data/                   # Data access layer
│   ├── FileStorage.cs     # JSON file I/O
│   └── PersistentStorage/ # Stored words, quotes
│
├── Models/                 # Data models
│   ├── Word.cs            # Main word model
│   ├── WordShortened.cs   # Lightweight word card
│   ├── Tag.cs             # Tag model
│   ├── Quote.cs           # Quote of the day
│   └── MerriamWebsterModels.cs
│
├── Services/               # Business logic
│   ├── NavigationService.cs    # Page navigation
│   ├── TagService.cs           # Tag & word management
│   ├── WordCacheManager.cs     # LRU cache
│   └── WordSearchService.cs    # Search logic
│
├── ViewModels/             # MVVM ViewModels
│   ├── SearchViewModel.cs
│   └── MyWordsViewModel.cs
│
├── Views/                  # UI layer
│   ├── Pages/
│   │   ├── HomePage.xaml
│   │   ├── DetailsPage.xaml
│   │   ├── MyWordsPage.xaml
│   │   ├── HistoryPage.xaml
│   │   └── FavouriteWordsPage.xaml
│   ├── Dialogs/
│   │   ├── MeaningSelectorDialog.xaml
│   │   └── TagPickerDialog.xaml
│   └── UserControls/
|       └── WordItem.xaml # Hiện quote random
│       └── WordDefinitionCard.xaml
│
├── Resources/              # Styles & Resources
│   └── Styles/
│       ├── Colors.xaml    # Theme colors
│       ├── ButtonStyles.xaml
│       └── ControlStyles.xaml
│
└── MainWindow.xaml         # Main window
```

---

## Kiến trúc Chi tiết

### 1. API Integration (ApiClient/)

**DictionaryApiClient.cs** - Fallback chain pattern:
```csharp
public async Task<List<Word>> SearchWordAsync(string word)
{
    // Try 1: Cache (instant)
    if (cache.Contains(word)) return cache.Get(word);
    
    // Try 2: Local storage (fast)
    if (localStorage.Exists(word)) return localStorage.Load(word);
    
    // Try 3: Merriam-Webster (3s timeout)
    try { return await MerriamAPI.Fetch(word); }
    catch (TimeoutException) {}
    
    // Try 4: Free Dictionary (3s timeout)
    try { return await FreeAPI.Fetch(word); }
    catch { return null; }
}
```

**Audio.cs** - Multi-source audio:
```csharp
public async Task<(string us, string uk)> FetchAudioAsync(string word)
{
    // Priority: Cambridge > Free Dict > Merriam-Webster
    // Cambridge: Highest quality, constructed URL
    // Free Dict: Direct URLs from response
    // Merriam-Webster: Fallback option
}
```

### 2. Data Layer (Data/ + Models/)

**FileStorage.cs** - JSON persistence:
```csharp
// Paths
_storedWordPath: ..\Data\PersistentStorage\StoredWord\{word}.json
_storedQuotePath: ..\Data\PersistentStorage\StoredQuote\quote_{id}.json
_listFile: ..\Data\PersistentStorage\AvailableWordList.txt
```

**TagService.cs** - Singleton pattern:
```csharp
// Thread-safe singleton
private static readonly object _lock = new object();
public static TagService Instance { get; }

// Data structure
Dictionary<string, Tag> _tags;          // tagId -> Tag
Dictionary<string, WordShortened> _words;  // word -> WordShortened
```

### 3. Navigation System (Services/NavigationService.cs)

**Custom navigation** với back/forward stack:
```csharp
private Stack<string> _backStack;
private Stack<string> _forwardStack;

public void NavigateTo(string pageTag, Page customPage = null)
{
    // Save current to back stack
    _backStack.Push(_currentPage);
    _forwardStack.Clear();
    
    // Create fresh page instance (no caching)
    var page = CreatePage(pageTag);
    _frame.Navigate(page);
}
```

### 4. Caching Strategy (Services/WordCacheManager.cs)

**LRU Cache** với 100 entries:
```csharp
private ConcurrentDictionary<string, CacheEntry> _memoryCache;
private int _maxCacheSize = 100;

public void AddToCache(string word, List<Word> words)
{
    if (_memoryCache.Count >= _maxCacheSize)
    {
        // Remove oldest entry
        var oldest = _memoryCache.OrderBy(x => x.Value._lastAccessed).First();
        _memoryCache.TryRemove(oldest.Key, out _);
    }
    _memoryCache.TryAdd(word, new CacheEntry { ... });
}
```

### 5. Search Features (Services/WordSearchService.cs)

**Autocomplete** với Levenshtein Distance:
```csharp
public List<string> GetSuggestions(string term, int maxResults = 5)
{
    // Step 1: Exact prefix matches (O(n))
    var exactMatches = _dictionary
        .Where(w => w.StartsWith(term))
        .OrderBy(w => w.Length)
        .Take(maxResults);
    
    // Step 2: Fuzzy matches if not enough (O(n²))
    if (exactMatches.Count < maxResults)
    {
        var fuzzyMatches = _dictionary
            .Select(w => new { Word = w, Distance = LevenshteinDistance(term, w) })
            .Where(x => x.Distance <= Math.Max(2, term.Length / 2))
            .OrderBy(x => x.Distance);
    }
}
```

---

## Cách thêm trang mới (Hướng dẫn mở rộng)

### Bước 1: Tạo Page mới

1. **Tạo file XAML** (`Views/Pages/NewPage.xaml`):
```xml
<local:WordListPageBase x:Class="BlueBerryDictionary.Views.Pages.NewPage"
      xmlns:local="clr-namespace:BlueBerryDictionary.Views.Pages"
      Background="{DynamicResource MainBackground}">
    
    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <Grid Margin="30">
            <!-- Nội dung page -->
        </Grid>
    </ScrollViewer>
</local:WordListPageBase>
```

2. **Tạo Code-behind** (`Views/Pages/NewPage.xaml.cs`):
```csharp
public partial class NewPage : WordListPageBase
{
    public NewPage(Action<string> onWordClick) : base(onWordClick)
    {
        InitializeComponent();
        LoadData();
    }
    
    public override void LoadData()
    {
        // Load dữ liệu cho page
    }
}
```

### Bước 2: Đăng ký Navigation

**Thêm vào `NavigationService.cs`**:
```csharp
private Page CreatePage(string pageTag)
{
    Page page = pageTag switch
    {
        "Home" => new HomePage(_onWordClick, _sidebarNavigate),
        "History" => new HistoryPage(_onWordClick),
        "NewPage" => new NewPage(_onWordClick),  // ← Thêm dòng này
        _ => new HomePage(_onWordClick, _sidebarNavigate)
    };
    
    if (page is WordListPageBase basePage)
    {
        basePage.LoadData();
    }
    
    return page;
}
```

### Bước 3: Thêm vào Sidebar/Toolbar

**Sidebar** (`MainWindow.xaml`):
```xml
<Button Style="{StaticResource SidebarButtonStyle}"
        Click="SidebarItem_Click"
        Tag="NewPage">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="🆕" FontSize="20" Margin="0,0,15,0"/>
        <TextBlock Text="New Page" FontSize="15"/>
    </StackPanel>
</Button>
```

**Toolbar** (`HomePage.xaml`):
```xml
<Button Style="{StaticResource ToolButtonStyle}"
        Tag="NewPage"
        Click="ButtnNavigate_Click">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="🆕" FontSize="18" Margin="0,0,8,0"/>
        <TextBlock Text="New Page"/>
    </StackPanel>
</Button>
```

### Bước 4: Tạo ViewModel (nếu cần)

```csharp
public partial class NewPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "New Page";
    
    [ObservableProperty]
    private ObservableCollection<string> _items;
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        // Load data logic
    }
}
```

**Bind vào Page**:
```csharp
public NewPage(Action<string> onWordClick) : base(onWordClick)
{
    InitializeComponent();
    
    var viewModel = new NewPageViewModel();
    DataContext = viewModel;
    
    LoadData();
}
```

### Bước 5: Sử dụng Styles có sẵn

**Card style**:
```xml
<Border Style="{StaticResource CardStyle}">
    <StackPanel>
        <!-- Nội dung -->
    </StackPanel>
</Border>
```

**Button styles**:
```xml
<!-- Primary button -->
<Button Style="{StaticResource PrimaryButtonStyle}" Content="Save"/>

<!-- Filter chip -->
<Button Style="{StaticResource FilterChipStyle}" Content="All"/>

<!-- Clear button -->
<Button Style="{StaticResource ClearButtonStyle}" Content="Clear All"/>
```

---

## Best Practices

### 1. Navigation
- **Luôn dùng** `NavigationService.NavigateTo()` thay vì `Frame.Navigate()`
- **Không cache pages** - tạo mới mỗi lần navigate để tránh memory leak

### 2. Theme Support
- **Sử dụng DynamicResource** cho tất cả colors:
```xml
Foreground="{DynamicResource TextColor}"
Background="{DynamicResource CardBackground}"
```

### 3. Word Click Handling
- **Kế thừa từ** `WordListPageBase` để tự động có `OnWordClicked`
- **Truyền callback** qua constructor:
```csharp
public NewPage(Action<string> onWordClick) : base(onWordClick)
```

### 4. Data Loading
- **Override** `LoadData()` cho async operations
- **Gọi** `LoadData()` trong constructor hoặc từ `NavigationService`

---

## Troubleshooting

### Lỗi thường gặp

**1. API Timeout**
```
Solution: Tăng timeout trong DictionaryApiClient:
var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
cts.CancelAfter(TimeSpan.FromSeconds(5)); // Tăng từ 3s lên 5s
```

**2. Navigation không hoạt động**
```
Check: NavigationService có được inject đúng không?
Check: Tag của button có match với CreatePage() switch case?
```

**3. Theme không đổi**
```
Check: Có dùng DynamicResource thay vì StaticResource?
Check: ApplyLightMode/ApplyDarkMode có được gọi?
```

---

## Contributing

Contributions are welcome! Please:
1. Fork repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

---

## License

Distributed under the MIT License. See `LICENSE` for more information.

---

## Contact

- **Email**: 24520280@gm.uit.edu.vn
- **GitHub**: [https://github.com/Dat-se40/BlueBerry-Dictionary](https://github.com/Dat-se40/BlueBerry-Dictionary)

---

## Acknowledgments

- [Free Dictionary API](https://dictionaryapi.dev/)
- [Merriam-Webster API](https://dictionaryapi.com/)
- [Cambridge Dictionary](https://dictionary.cambridge.org/)
- [Pixabay API](https://pixabay.com/api/docs/)
- WPF Community
