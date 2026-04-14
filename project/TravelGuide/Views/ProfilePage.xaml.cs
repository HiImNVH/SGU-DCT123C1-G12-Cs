// Views/ProfilePage.xaml.cs
using TravelGuide.Services;

namespace TravelGuide.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly AuthService _auth;
        private static LocalizationService L => LocalizationService.Instance;

        private readonly Dictionary<string, string> _langNames = new()
        {
            { "vi", "Tiếng Việt" }, { "en", "English" }, { "ja", "日本語" },
            { "ko", "한국어" },       { "zh", "中文" },     { "fr", "Français" }
        };

        public ProfilePage(AuthService auth)
        {
            InitializeComponent();
            _auth = auth;

            // Đổi ngôn ngữ → refresh UI ngay lập tức
            L.PropertyChanged += (_, _) =>
                MainThread.BeginInvokeOnMainThread(RefreshUIText);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine("[log] - Mo trang Ho so");
            RefreshUIText();
            LoadProfile();
        }

        // ── Refresh toàn bộ text theo ngôn ngữ hiện tại ─────────────────
        private void RefreshUIText()
        {
            Title = L["Profile_Title"];

            if (LanguageSectionLabel != null) LanguageSectionLabel.Text = "Cài đặt ngôn ngữ";
            if (LanguageNarrLabel != null) LanguageNarrLabel.Text = L["Profile_Language"];
            if (ChangeLanguageBtn != null) ChangeLanguageBtn.Text = L["Profile_ChangeLanguage"];
            if (AccountInfoLabel != null) AccountInfoLabel.Text = "Thông tin tài khoản";
            if (UsernameFieldLabel != null) UsernameFieldLabel.Text = "Tên đăng nhập";
            if (RoleFieldLabel != null) RoleFieldLabel.Text = "Quyền";
            if (LogoutBtn != null) LogoutBtn.Text = L["Profile_Logout"];

            // Guest frame
            if (GuestModeLabel != null) GuestModeLabel.Text = L["Profile_Guest"];
            if (GuestHintLabel != null) GuestHintLabel.Text = "Đăng nhập để lưu ngôn ngữ trên server";
            if (LoginPromptBtn != null) LoginPromptBtn.Text = "Đăng nhập ngay";

            // Role label
            var user = _auth.GetCurrentUser();
            if (RoleLabel != null)
                RoleLabel.Text = user?.Role == Models.UserRole.Admin
                    ? L["Profile_Admin"]
                    : (user != null ? L["Profile_Tourist"] : L["Profile_Guest"]);

            // Ngôn ngữ hiện tại
            var lang = _auth.GetCurrentLanguage();
            if (CurrentLanguageLabel != null)
                CurrentLanguageLabel.Text = _langNames.GetValueOrDefault(lang, lang);
        }

        // ── Load thông tin user ──────────────────────────────────────────
        private void LoadProfile()
        {
            var user = _auth.GetCurrentUser();
            var isLoggedIn = _auth.IsAuthenticated() && user != null;
            var lang = _auth.GetCurrentLanguage();

            CurrentLanguageLabel.Text = _langNames.GetValueOrDefault(lang, lang);

            if (isLoggedIn && user != null)
            {
                UserNameLabel.Text = user.Username;
                RoleLabel.Text = user.Role == Models.UserRole.Admin
                    ? L["Profile_Admin"]
                    : L["Profile_Tourist"];
                InfoUsername.Text = user.Username;
                InfoRole.Text = user.Role == Models.UserRole.Admin ? "Admin" : "User";

                AccountFrame.IsVisible = true;
                LogoutBtn.IsVisible = true;
                GuestFrame.IsVisible = false;
                Console.WriteLine("[info] - Hien thi ho so nguoi dung da dang nhap");
            }
            else
            {
                UserNameLabel.Text = "Du khách";
                RoleLabel.Text = L["Profile_Guest"];
                AccountFrame.IsVisible = false;
                LogoutBtn.IsVisible = false;
                GuestFrame.IsVisible = true;
                Console.WriteLine("[info] - Hien thi trang ho so khach (Guest)");
            }
        }

        // ── Đổi ngôn ngữ ────────────────────────────────────────────────
        private async void OnChangeLanguageClicked(object sender, EventArgs e)
        {
            var options = _langNames.Values.ToArray();
            var codes = _langNames.Keys.ToArray();

            var choice = await DisplayActionSheet(
                L["Lang_Select"],
                L["Common_Cancel"],
                null,
                options);

            if (string.IsNullOrEmpty(choice) || choice == L["Common_Cancel"]) return;

            var idx = Array.IndexOf(options, choice);
            if (idx < 0) return;

            var selectedCode = codes[idx];
            Console.WriteLine($"[info] - Doi ngon ngu: {selectedCode}");

            // SetLanguage → tự động trigger PropertyChanged → RefreshUIText() chạy ngay
            L.SetLanguage(selectedCode);

            // Cập nhật server nếu đã login
            if (_auth.IsAuthenticated())
                await _auth.UpdateLanguageAsync(selectedCode);

            await DisplayAlert(L["Common_Success"],
                $"{L["Profile_ChangeLanguage"]}: {choice}",
                L["Common_OK"]);
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                L["Profile_Logout"],
                L["Profile_LogoutConfirm"],
                L["Profile_Logout"],
                L["Common_Cancel"]);

            if (!confirm) return;

            Console.WriteLine("[log] - Nguoi dung dang xuat");
            await _auth.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//login");
        }
    }
}
