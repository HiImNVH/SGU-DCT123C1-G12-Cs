// RegisterPage.xaml.cs
using TravelGuide.Services;
using TravelGuide.Models.DTOs;

namespace TravelGuide;

public partial class RegisterPage : ContentPage
{
    private readonly IAuthService _authService;
    private static LocalizationService L => LocalizationService.Instance;

    public RegisterPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry?.Text?.Trim();
        var password = PasswordEntry?.Text;
        var email    = EmailEntry?.Text?.Trim();
        var fullName = FullNameEntry?.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert(L["Common_Error"], L["Register_EmptyFields"], L["Common_OK"]);
            return;
        }

        RegisterBtn.IsEnabled = false;

        var result = await _authService.RegisterAsync(new RegisterRequest
        {
            Username = username,
            Password = password,
            Email = email,
            FullName = fullName,
            PreferredLanguage = Preferences.Get("app_language", "vi")
        });

        RegisterBtn.IsEnabled = true;

        if (result.Success)
            await Shell.Current.GoToAsync("//main");
        else
            await DisplayAlert(L["Register_Failed"], result.ErrorMessage, L["Common_OK"]);
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///login");
    }
}
