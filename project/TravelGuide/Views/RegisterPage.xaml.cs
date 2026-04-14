using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private void RefreshUIText()
        {
            SubtitleLabel.Text = "Tạo tài khoản mới";
            UsernameLbl.Text = L["Login_Username"];
            PasswordLbl.Text = L["Login_Password"];
            ConfirmPasswordLbl.Text = "Xác nhận mật khẩu";
            RegisterBtn.Text = "Tạo tài khoản";
            AlreadyHaveAccountLabel.Text = "Đã có tài khoản?";
            BackToLoginBtn.Text = L["Login_Button"];

            UsernameEntry.Placeholder = "Nhập tên đăng nhập";
            PasswordEntry.Placeholder = "Ít nhất 6 ký tự";
            ConfirmPasswordEntry.Placeholder = "Nhập lại mật khẩu";
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry?.Text?.Trim();
            var password = PasswordEntry?.Text;
            var confirmPassword = ConfirmPasswordEntry?.Text;

            // Validate
            if (string.IsNullOrEmpty(username))
            {
                await DisplayAlert(L["Common_Error"], "Vui lòng nhập tên đăng nhập.", L["Common_OK"]);
                return;
            }

            if (username.Length < 3)
            {
                await DisplayAlert(L["Common_Error"], "Tên đăng nhập phải có ít nhất 3 ký tự.", L["Common_OK"]);
                return;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                await DisplayAlert(L["Common_Error"], "Mật khẩu phải có ít nhất 6 ký tự.", L["Common_OK"]);
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert(L["Common_Error"], "Mật khẩu xác nhận không khớp.", L["Common_OK"]);
                return;
            }

            Console.WriteLine($"[log] - Bat dau dang ky tai khoan: {username}");

            RegisterBtn.IsEnabled = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var lang = L.CurrentLanguageCode;
            var result = await _auth.RegisterAsync(new RegisterRequest
            {
                Username = username,
                Password = password,
                PreferredLanguage = lang
            });

            RegisterBtn.IsEnabled = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            if (result.Success)
            {
                Console.WriteLine("[info] - Dang ky thanh cong, chuyen vao app");
                await DisplayAlert(L["Common_Success"], "Tạo tài khoản thành công!", L["Common_OK"]);
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                Console.WriteLine($"[error] - Dang ky that bai: {result.ErrorMessage}");
                await DisplayAlert(L["Common_Error"], result.ErrorMessage, L["Common_OK"]);
            }
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//login");
        }
    }
}
