// Views/LanguageSelectionPage.xaml.cs
using TravelGuide.Models;
using TravelGuide.Services;

namespace TravelGuide.Views
{
    public partial class LanguageSelectionPage : ContentPage
    {
        private readonly AuthService _auth;
        private static LocalizationService L => LocalizationService.Instance;

        public List<LanguageItem> Languages { get; } = new()
        {
            new("vi", "Tiếng Việt (mặc định)", "Vietnamese", "🇻🇳"),
            new("en", "English",               "English",    "🇺🇸"),
            new("zh", "中文",                   "Chinese",    "🇨🇳"),
            new("ja", "日本語",                  "Japanese",   "🇯🇵"),
            new("ko", "한국어",                  "Korean",     "🇰🇷"),
            new("fr", "Français",              "French",     "🇫🇷"),
        };

        public LanguageSelectionPage(AuthService auth)
        {
            InitializeComponent();
            _auth = auth;
            BindingContext = this;

            // Lắng nghe đổi ngôn ngữ → refresh text tĩnh ngay lập tức
            L.PropertyChanged += (_, _) =>
                MainThread.BeginInvokeOnMainThread(RefreshUIText);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshUIText();
        }

        // ── Refresh text tĩnh theo ngôn ngữ hiện tại ────────────────────
        private void RefreshUIText()
        {
            // Các label tĩnh có x:Name trong XAML
            if (TitleLabel != null)
                TitleLabel.Text = L["Lang_Select"];

            if (SkipBtn != null)
                SkipBtn.Text = L["Lang_Skip"];

            Console.WriteLine($"[info] - Da refresh UI LanguageSelectionPage: {L.CurrentLanguageCode}");
        }

        // ── Chọn ngôn ngữ ────────────────────────────────────────────────
        private async void OnLanguageTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is not string code) return;
            Console.WriteLine($"[info] - Nguoi dung chon ngon ngu: {code}");
            await ApplyLanguageAndNavigate(code);
        }

        private async void OnSkipClicked(object sender, EventArgs e)
        {
            Console.WriteLine("[log] - Bo qua chon ngon ngu, dung mac dinh: vi");
            await ApplyLanguageAndNavigate("vi");
        }

        private async Task ApplyLanguageAndNavigate(string code)
        {
            // 1. Lưu preference
            Preferences.Set("preferred_language", code);

            // 2. Đổi ngôn ngữ → tự động trigger PropertyChanged → RefreshUIText()
            //    Tất cả page đang lắng nghe đều refresh ngay lập tức
            L.SetLanguage(code);

            // 3. Cập nhật server nếu đã login
            if (_auth.IsAuthenticated())
                await _auth.UpdateLanguageAsync(code);

            Console.WriteLine($"[info] - Da chon ngon ngu: {code}, chuyen vao app");

            // 4. Điều hướng vào app
            await Shell.Current.GoToAsync("//main");
        }
    }

    public record LanguageItem(string Code, string NativeName, string Name, string Flag);
}
