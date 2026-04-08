using System.ComponentModel;
using System.Globalization;
using TravelGuide.Services.Localization;

namespace TravelGuide.Services
{
    public class LocalizationService : INotifyPropertyChanged
    {
        public static LocalizationService Instance { get; } = new();

        private LocalizationService()
        {
            var saved = Preferences.Get("preferred_language", "vi");
            ApplyCulture(saved);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string this[string key]
        {
            get
            {
                var value = AppResources.ResourceManager
                    .GetString(key, AppResources.Culture);
                return value ?? $"[{key}]";
            }
        }

        public void SetLanguage(string languageCode)
        {
            try
            {
                ApplyCulture(languageCode);
                Preferences.Set("preferred_language", languageCode);

                // Chỉ fire event — KHÔNG tạo AppShell mới
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
            catch (CultureNotFoundException)
            {
                SetLanguage("vi");
            }
        }

        private void ApplyCulture(string languageCode)
        {
            var culture = new CultureInfo(languageCode);
            AppResources.Culture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public string CurrentLanguageCode =>
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    

        public void InitFromUser(string preferredLanguage)
        {
            // Chỉ áp dụng nếu chưa có ngôn ngữ được lưu trên thiết bị
            var saved = Preferences.Get("preferred_language", "");
            if (string.IsNullOrEmpty(saved) && !string.IsNullOrEmpty(preferredLanguage))
                SetLanguage(preferredLanguage);
        }
    }
}