// Views/LoginPage.xaml.cs
using TravelGuide.Models.DTOs;
using TravelGuide.Services;

namespace TravelGuide.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _auth;
        private static LocalizationService L => LocalizationService.Instance;

        public LoginPage(AuthService auth)
        {
            InitializeComponent();
            _auth = auth;

            L.PropertyChanged += (_, _) =>
                MainThread.BeginInvokeOnMainThread(RefreshUIText);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshUIText();
        }

        // ── Refresh toàn bộ text theo ngôn ngữ ──────────────────────
        private void RefreshUIText()
        {
            if (LoginSubtitleLabel != null) LoginSubtitleLabel.Text = L["Login_Title"];
            if (UsernameEntry != null) UsernameEntry.Placeholder = L["Login_Username"];
            if (PasswordEntry != null) PasswordEntry.Placeholder = L["Login_Password"];
            if (LoginBtn != null) LoginBtn.Text = L["Login_Button"];
            if (GuestBtn != null) GuestBtn.Text = L["Login_Guest"];
            if (OrLabel != null) OrLabel.Text = L["Login_OrLabel"];
            if (FirstTimeLabel != null) FirstTimeLabel.Text = L["Login_FirstTime"];
            if (SelectLanguageBtn != null) SelectLanguageBtn.Text = L["Lang_Select"];
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry?.Text?.Trim();
            var password = PasswordEntry?.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert(L["Common_Error"], L["Login_EmptyFields"], L["Common_OK"]);
                return;
            }

            Console.WriteLine("[log] - Bat dau dang nhap");
            if (LoginBtn != null) LoginBtn.IsEnabled = false;
            if (LoadingIndicator != null) { LoadingIndicator.IsVisible = true; LoadingIndicator.IsRunning = true; }

            var result = await _auth.LoginAsync(new LoginRequest
            {
                Username = username,
                Password = password
            });

            if (LoginBtn != null) LoginBtn.IsEnabled = true;
            if (LoadingIndicator != null) { LoadingIndicator.IsVisible = false; LoadingIndicator.IsRunning = false; }

            if (result.Success)
            {
                Console.WriteLine("[info] - Dang nhap thanh cong");
                var lang = result.User?.PreferredLanguage ?? "vi";
                LocalizationService.Instance.InitFromUser(lang);
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                Console.WriteLine($"[error] - Dang nhap that bai: {result.ErrorMessage}");
                await DisplayAlert(L["Login_Failed"], result.ErrorMessage, L["Common_OK"]);
            }
        }

        private async void OnGuestClicked(object sender, EventArgs e)
        {
            Console.WriteLine("[log] - Nguoi dung chon che do khach");
            await Shell.Current.GoToAsync("//main");
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//register");

        private async void OnSelectLanguageClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//language");
    }
}
