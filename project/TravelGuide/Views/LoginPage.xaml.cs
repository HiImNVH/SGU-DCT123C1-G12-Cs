using TravelGuide.Services;
using TravelGuide.Models.DTOs;

namespace TravelGuide;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;
    private static LocalizationService L => LocalizationService.Instance;

    public LoginPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
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

        LoginBtn.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Username = username,
            Password = password
        });

        LoginBtn.IsEnabled = true;
        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;

        if (result.Success)
        {
            // KHÔNG gọi InitFromUser — LocalizationService tự đọc từ Preferences
            await Shell.Current.GoToAsync("//main");
        }
        else
        {
            await DisplayAlert(L["Login_Failed"], result.ErrorMessage, L["Common_OK"]);
        }
    }

    private async void OnSignupClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///language");
    }
}