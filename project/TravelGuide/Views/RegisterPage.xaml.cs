// Views/RegisterPage.xaml.cs
using TravelGuide.Models.DTOs;
using TravelGuide.Services;

namespace TravelGuide.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly AuthService _auth;
        private static LocalizationService L => LocalizationService.Instance;

        public RegisterPage(AuthService auth)
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
            if (SubtitleLabel != null) SubtitleLabel.Text = L["Register_Subtitle"];
            if (UsernameLbl != null) UsernameLbl.Text = L["Login_Username"];
            if (PasswordLbl != null) PasswordLbl.Text = L["Login_Password"];
            if (ConfirmPasswordLbl != null) ConfirmPasswordLbl.Text = L["Register_ConfirmPassword"];
            if (RegisterBtn != null) RegisterBtn.Text = L["Register_Button"];
            if (AlreadyHaveAccountLabel != null) AlreadyHaveAccountLabel.Text = L["Register_AlreadyHave"];
            if (BackToLoginBtn != null) BackToLoginBtn.Text = L["Login_Button"];

            if (UsernameEntry != null) UsernameEntry.Placeholder = L["Register_Placeholder_Username"];
            if (PasswordEntry != null) PasswordEntry.Placeholder = L["Register_Placeholder_Password"];
            if (ConfirmPasswordEntry != null) ConfirmPasswordEntry.Placeholder = L["Register_Placeholder_Confirm"];
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry?.Text?.Trim();
            var password = PasswordEntry?.Text;
            var confirmPassword = ConfirmPasswordEntry?.Text;

            if (string.IsNullOrEmpty(username))
            {
                await DisplayAlert(L["Common_Error"], L["Login_EmptyFields"], L["Common_OK"]);
                return;
            }
            if (username.Length < 3)
            {
                await DisplayAlert(L["Common_Error"], L["Login_EmptyFields"], L["Common_OK"]);
                return;
            }
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                await DisplayAlert(L["Common_Error"], L["Login_EmptyFields"], L["Common_OK"]);
                return;
            }
            if (password != confirmPassword)
            {
                await DisplayAlert(L["Common_Error"], L["Login_EmptyFields"], L["Common_OK"]);
                return;
            }

            Console.WriteLine($"[log] - Bat dau dang ky: {username}");

            if (RegisterBtn != null) RegisterBtn.IsEnabled = false;
            if (LoadingIndicator != null) { LoadingIndicator.IsVisible = true; LoadingIndicator.IsRunning = true; }

            var result = await _auth.RegisterAsync(new RegisterRequest
            {
                Username = username,
                Password = password,
                PreferredLanguage = L.CurrentLanguageCode
            });

            if (RegisterBtn != null) RegisterBtn.IsEnabled = true;
            if (LoadingIndicator != null) { LoadingIndicator.IsVisible = false; LoadingIndicator.IsRunning = false; }

            if (result.Success)
            {
                Console.WriteLine("[info] - Dang ky thanh cong");
                await DisplayAlert(L["Common_Success"], L["Register_Success"], L["Common_OK"]);
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                Console.WriteLine($"[error] - Dang ky that bai: {result.ErrorMessage}");
                await DisplayAlert(L["Common_Error"], result.ErrorMessage, L["Common_OK"]);
            }
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//login");
    }
}
