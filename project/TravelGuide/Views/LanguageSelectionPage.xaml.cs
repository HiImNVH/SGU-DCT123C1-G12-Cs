using TravelGuide.Models;
using TravelGuide.Services;

namespace TravelGuide;

public partial class LanguageSelectionPage : ContentPage
{
    private readonly IAuthService _authService;

    public List<Language> Languages { get; set; } = new()
    {
        new Language { Code = "vi", Name = "Vietnamese", NativeName = "Tiếng Việt", IsActive = true },
        new Language { Code = "en", Name = "English",    NativeName = "English",     IsActive = true },
        new Language { Code = "ja", Name = "Japanese",   NativeName = "日本語",       IsActive = true },
        new Language { Code = "ko", Name = "Korean",     NativeName = "한국어",        IsActive = true },
        new Language { Code = "zh", Name = "Chinese",    NativeName = "中文",         IsActive = true },
        new Language { Code = "fr", Name = "French",     NativeName = "Français",    IsActive = true }
    };

    public LanguageSelectionPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        BindingContext = this;
    }

    private async void OnLanguageSelected(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string code)
        {
            // 1. Lưu vào user preference
            var user = _authService.GetCurrentUser();
            if (user != null)
                user.PreferredLanguage = code;
            Preferences.Set("app_language", code);
            // 2. Đổi ngôn ngữ UI — tự lưu vào Preferences bên trong
            LocalizationService.Instance.SetLanguage(code);

            // 3. Cập nhật tab trực tiếp
            AppShell.Current?.UpdateTabTitles();

            // 4. Điều hướng vào app
            await Shell.Current.GoToAsync("//register");
        }
    }
}