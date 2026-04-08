using TravelGuide.Services;
using TravelGuide.Models;
namespace TravelGuide;

public partial class ProfilePage : ContentPage
{
    private readonly IAuthService _authService;
    private static LocalizationService L => LocalizationService.Instance;

    private readonly Dictionary<string, string> _languageNames = new()
    {
        { "vi", "Tiếng Việt" },
        { "en", "English" },
        { "ja", "日本語" },
        { "ko", "한국어" },
        { "zh", "中文" },
        { "fr", "Français" }
    };

    public ProfilePage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProfile();
    }

    private void LoadProfile()
    {
        var user = _authService.GetCurrentUser();
        if (user == null) return;

        FullNameLabel.Text = user.FullName ?? user.Username;
        EmailLabel.Text = user.Email ?? "";

        RoleLabel.Text = user.Role.ToString() == "Admin"
            ? L["Profile_Admin"]
            : L["Profile_Tourist"];

        InfoFullName.Text = user.FullName ?? user.Username;
        InfoEmail.Text = user.Email ?? "";

        var lang = user.PreferredLanguage ?? "vi";
        InfoLanguage.Text = _languageNames.GetValueOrDefault(lang, lang);
        CurrentLanguageLabel.Text = _languageNames.GetValueOrDefault(lang, lang);
    }

    private async void OnChangeLanguageClicked(object sender, EventArgs e)
    {
        var options = _languageNames.Values.ToArray();
        var codes = _languageNames.Keys.ToArray();
        var cancelText = L["Common_Cancel"];

        var choice = await DisplayActionSheet(
            L["Lang_Select"],
            cancelText,
            null,
            options);

        if (string.IsNullOrEmpty(choice) || choice == cancelText) return;

        var idx = Array.IndexOf(options, choice);
        if (idx < 0) return;

        var user = _authService.GetCurrentUser();
        if (user == null) return;

        var selectedCode = codes[idx];

        // 1. Lưu preference
        user.PreferredLanguage = selectedCode;

        // 2. Đổi ngôn ngữ UI — tự lưu vào Preferences bên trong
        L.SetLanguage(selectedCode);

        // 3. Cập nhật tab trực tiếp
        AppShell.Current?.UpdateTabTitles();

        // 4. Cập nhật labels
        CurrentLanguageLabel.Text = options[idx];
        InfoLanguage.Text = options[idx];

        RoleLabel.Text = user.Role.ToString() == "Admin"
            ? L["Profile_Admin"]
            : L["Profile_Tourist"];

        await DisplayAlert(
            L["Common_Success"],
            $"{L["Profile_LanguageChanged"]} {choice}",
            L["Common_OK"]);
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        var user = _authService.GetCurrentUser();
        if (user == null) return;

        var newName = await DisplayPromptAsync(
            L["Profile_EditTitle"],
            L["Profile_EditPrompt"],
            initialValue: user.FullName ?? "");

        if (string.IsNullOrEmpty(newName)) return;

        user.FullName = newName;
        FullNameLabel.Text = newName;
        InfoFullName.Text = newName;

        await DisplayAlert(L["Common_Success"], L["Profile_UpdateSuccess"], L["Common_OK"]);
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            L["Profile_LogoutTitle"],
            L["Profile_LogoutConfirm"],
            L["Profile_Logout"],
            L["Common_Cancel"]);

        if (!confirm) return;

        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}