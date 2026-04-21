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

        // ── Refresh toàn bộ text theo ngôn ngữ ──────────────────────
        private void RefreshUIText()
        {
            Title = L["Profile_Title"];

            // Section labels — dùng resx keys
            if (LanguageSectionLabel != null) LanguageSectionLabel.Text = L["Profile_LanguageSection"];
            if (LanguageNarrLabel != null) LanguageNarrLabel.Text = L["Profile_Language"];
            if (ChangeLanguageBtn != null) ChangeLanguageBtn.Text = L["Profile_ChangeLanguage"];
            if (AccountInfoLabel != null) AccountInfoLabel.Text = L["Profile_AccountInfo"];
            if (UsernameFieldLabel != null) UsernameFieldLabel.Text = L["Profile_UsernameField"];
            if (RoleFieldLabel != null) RoleFieldLabel.Text = L["Profile_RoleField"];
            if (LogoutBtn != null) LogoutBtn.Text = L["Profile_Logout"];

            // Guest frame
            if (GuestModeLabel != null) GuestModeLabel.Text = L["Profile_Guest"];
            if (GuestHintLabel != null) GuestHintLabel.Text = L["Profile_LoginHint"];
            if (LoginPromptBtn != null) LoginPromptBtn.Text = L["Profile_LoginPrompt"];

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

        // ── Load thông tin user ──────────────────────────────────────
        private void LoadProfile()
        {
            var user = _auth.GetCurrentUser();
            var isLoggedIn = _auth.IsAuthenticated() && user != null;
            var lang = _auth.GetCurrentLanguage();

            if (CurrentLanguageLabel != null)
                CurrentLanguageLabel.Text = _langNames.GetValueOrDefault(lang, lang);

            if (isLoggedIn && user != null)
            {
                if (UserNameLabel != null) UserNameLabel.Text = user.Username;
                if (RoleLabel != null) RoleLabel.Text = user.Role == Models.UserRole.Admin
                    ? L["Profile_Admin"] : L["Profile_Tourist"];
                if (InfoUsername != null) InfoUsername.Text = user.Username;
                if (InfoRole != null) InfoRole.Text = user.Role == Models.UserRole.Admin ? "Admin" : "User";

                if (AccountFrame != null) AccountFrame.IsVisible = true;
                if (LogoutBtn != null) LogoutBtn.IsVisible = true;
                if (GuestFrame != null) GuestFrame.IsVisible = false;
            }
            else
            {
                if (UserNameLabel != null) UserNameLabel.Text = "Du khách";
                if (RoleLabel != null) RoleLabel.Text = L["Profile_Guest"];

                if (AccountFrame != null) AccountFrame.IsVisible = false;
                if (LogoutBtn != null) LogoutBtn.IsVisible = false;
                if (GuestFrame != null) GuestFrame.IsVisible = true;
            }
        }

        // ── Đổi ngôn ngữ ────────────────────────────────────────────
        private async void OnChangeLanguageClicked(object sender, EventArgs e)
        {
            var options = _langNames.Values.ToArray();
            var codes = _langNames.Keys.ToArray();

            var choice = await DisplayActionSheet(
                L["Lang_Select"], L["Common_Cancel"], null, options);

            if (string.IsNullOrEmpty(choice) || choice == L["Common_Cancel"]) return;

            var idx = Array.IndexOf(options, choice);
            if (idx < 0) return;

            var selectedCode = codes[idx];
            Console.WriteLine($"[info] - Doi ngon ngu: {selectedCode}");

            // Đổi ngôn ngữ → tự động trigger PropertyChanged → RefreshUIText()
            L.SetLanguage(selectedCode);

            if (_auth.IsAuthenticated())
                await _auth.UpdateLanguageAsync(selectedCode);

            await DisplayAlert(L["Common_Success"],
                $"{L["Profile_ChangeLanguage"]}: {choice}", L["Common_OK"]);
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                L["Profile_Logout"], L["Profile_LogoutConfirm"],
                L["Profile_Logout"], L["Common_Cancel"]);

            if (!confirm) return;

            Console.WriteLine("[log] - Nguoi dung dang xuat");
            await _auth.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }

        private async void OnLoginClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//login");
    }
}
