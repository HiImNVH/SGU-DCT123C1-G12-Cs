using Microsoft.Maui.Media;

namespace TravelGuide.Services
{
    public class NarrationService : INarrationService
    {
        public bool IsSpeaking { get; private set; }
        private CancellationTokenSource _cts;

        private readonly Dictionary<string, string> _localeMap = new()
        {
            { "vi", "vi-VN" }, { "en", "en-US" }, { "ja", "ja-JP" },
            { "ko", "ko-KR" }, { "zh", "zh-CN" }, { "fr", "fr-FR" }
        };

        public async Task SpeakAsync(string text, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            await StopAsync();
            _cts = new CancellationTokenSource();
            IsSpeaking = true;

            var settings = new SpeechOptions();
            var locales = await TextToSpeech.GetLocalesAsync();
            var matched = locales.FirstOrDefault(l =>
                l.Language.StartsWith(languageCode, StringComparison.OrdinalIgnoreCase));
            if (matched != null)
                settings.Locale = matched;

            await TextToSpeech.SpeakAsync(text, settings, _cts.Token);
            IsSpeaking = false;
        }

        public async Task StopAsync()
        {
            _cts?.Cancel();
            _cts = null;
            IsSpeaking = false;
            await Task.CompletedTask;
        }

        public async Task PauseAsync() => await StopAsync();
        public async Task ResumeAsync() => await Task.CompletedTask;
    }
}