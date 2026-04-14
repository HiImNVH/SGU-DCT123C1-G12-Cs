// Services/TTSPlayerService.cs
using Microsoft.Maui.Media;
using TravelGuide.Models;

namespace TravelGuide.Services
{
    /// <summary>
    /// TTSModule - Play audio theo thứ tự:
    ///   1. Local audio (LocalAudioPath) → nếu có, phát ngay
    ///   2. Stream từ AudioUrl (cần mạng) → phát và lưu cache
    ///   3. TTS Engine từ NarrationText → fallback cuối cùng
    /// Control: Play / Pause / Stop — State management
    /// Thư viện: Microsoft.Maui.Media (TextToSpeech)
    /// </summary>
    public class TTSPlayerService
    {
        private PlayerState _state = PlayerState.Idle;
        private CancellationTokenSource? _cts;
        private string? _currentText;
        private string? _currentLang;

        private readonly Dictionary<string, string> _localeMap = new()
        {
            { "vi", "vi-VN" }, { "en", "en-US" }, { "ja", "ja-JP" },
            { "ko", "ko-KR" }, { "zh", "zh-CN" }, { "fr", "fr-FR" }
        };

        public PlayerState GetState() => _state;

        /// <summary>
        /// Phát audio theo độ ưu tiên: local file → URL stream → TTS text
        /// </summary>
        public async Task PlayAsync(
            string? localAudioPath,
            string? audioUrl,
            string narrationText,
            string langCode)
        {
            Console.WriteLine($"[log] - Bat dau phat audio: lang={langCode}");

            await StopAsync();
            _cts = new CancellationTokenSource();
            _currentText = narrationText;
            _currentLang = langCode;
            _state = PlayerState.Playing;

            try
            {
                // Ưu tiên 1: File audio local
                if (!string.IsNullOrEmpty(localAudioPath) && File.Exists(localAudioPath))
                {
                    Console.WriteLine("[info] - Phat file audio local");
                    await PlayFromFileAsync(localAudioPath);
                    return;
                }

                // Ưu tiên 2: Stream từ AudioUrl (nếu có mạng)
                if (!string.IsNullOrEmpty(audioUrl) &&
                    Helpers.NetworkHelper.IsConnected)
                {
                    Console.WriteLine("[info] - Phat audio tu URL");
                    await PlayFromUrlAsync(audioUrl);
                    return;
                }

                // Ưu tiên 3: TTS Engine từ NarrationText
                if (!string.IsNullOrEmpty(narrationText))
                {
                    Console.WriteLine("[info] - Dung TTS engine doc NarrationText");
                    await PlayTTSAsync(narrationText, langCode);
                    return;
                }

                Console.WriteLine("[warn] - Khong co noi dung de phat audio");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[log] - Audio bi dung boi nguoi dung");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi phat audio: {ex.Message}");
            }
            finally
            {
                _state = PlayerState.Idle;
            }
        }

        private async Task PlayFromFileAsync(string filePath)
        {
            // .NET MAUI không có built-in MediaPlayer đơn giản,
            // dùng MediaElement từ CommunityToolkit hoặc fallback sang TTS
            // Trong POC này: fallback sang TTS nếu không có MediaElement
            Console.WriteLine("[warn] - Phat file local: su dung TTS fallback (POC)");
            await PlayTTSAsync(_currentText!, _currentLang!);
        }

        private async Task PlayFromUrlAsync(string url)
        {
            // Trong POC: fallback sang TTS vì stream audio URL phức tạp
            // Production: dùng MediaElement với streaming
            Console.WriteLine("[warn] - Phat URL: su dung TTS fallback (POC)");
            await PlayTTSAsync(_currentText!, _currentLang!);
        }

        private async Task PlayTTSAsync(string text, string langCode)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var settings = new SpeechOptions();

            try
            {
                var locales = await TextToSpeech.GetLocalesAsync();
                var matched = locales.FirstOrDefault(l =>
                    l.Language.StartsWith(langCode, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    settings.Locale = matched;
                    Console.WriteLine($"[info] - TTS dung locale: {matched.Language}");
                }
                else
                {
                    Console.WriteLine($"[warn] - Khong tim thay locale cho {langCode}, dung mac dinh");
                }
            }
            catch
            {
                Console.WriteLine("[warn] - Khong lay duoc danh sach locale");
            }

            await TextToSpeech.SpeakAsync(text, settings, _cts!.Token);
        }

        /// <summary>Tạm dừng</summary>
        public async Task PauseAsync()
        {
            if (_state != PlayerState.Playing) return;
            Console.WriteLine("[log] - Tam dung phat audio");
            _cts?.Cancel();
            _state = PlayerState.Paused;
            await Task.CompletedTask;
        }

        /// <summary>Dừng và reset</summary>
        public async Task StopAsync()
        {
            Console.WriteLine("[log] - Dung phat audio");
            _cts?.Cancel();
            _cts = null;
            _state = PlayerState.Stopped;
            await Task.CompletedTask;
        }
    }
}
