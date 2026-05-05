// Services/TTSPlayerService.cs
using TravelGuide.Models;

namespace TravelGuide.Services
{
    /// <summary>
    /// TTSModule - Phát văn bản bằng TextToSpeech engine.
    /// Flow: Play(text, lang) → đang phát → Stop() hoặc phát xong tự dừng.
    /// Thư viện: Microsoft.Maui.Media (TextToSpeech)
    /// </summary>
    public class TTSPlayerService
    {
        private CancellationTokenSource? _cts;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Phát TTS từ văn bản. Trả về khi phát xong hoặc bị dừng.
        /// </summary>
        public async Task PlayAsync(string narrationText, string langCode)
        {
            if (string.IsNullOrWhiteSpace(narrationText))
            {
                Console.WriteLine("[warn] - Khong co van ban de phat TTS");
                return;
            }

            // Hủy phiên cũ nếu đang phát
            Stop();

            Console.WriteLine($"[log] - Bat dau phat TTS: lang={langCode}");
            _isPlaying = true;

            var cts = new CancellationTokenSource();
            _cts = cts;

            try
            {
                var settings = new SpeechOptions();

                // Tìm locale phù hợp
                var locales = await TextToSpeech.GetLocalesAsync();
                var matched = locales.FirstOrDefault(l =>
                    l.Language.StartsWith(langCode, StringComparison.OrdinalIgnoreCase));

                if (matched != null)
                {
                    settings.Locale = matched;
                    Console.WriteLine($"[info] - TTS locale: {matched.Language}");
                }
                else
                {
                    Console.WriteLine($"[warn] - Khong tim thay locale cho '{langCode}', dung mac dinh");
                }

                await TextToSpeech.Default.SpeakAsync(narrationText, settings, cts.Token);
                Console.WriteLine("[log] - TTS phat xong van ban");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[log] - TTS bi dung boi nguoi dung");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi TTS: {ex.Message}");
            }
            finally
            {
                _isPlaying = false;
                _cts = null;
            }
        }

        /// <summary>Dừng phát TTS ngay lập tức</summary>
        public void Stop()
        {
            var cts = _cts;
            _cts = null;
            _isPlaying = false;

            if (cts != null)
            {
                Console.WriteLine("[log] - Dung phat TTS");
                try
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                catch { /* đã dispose hoặc đã cancel */ }
            }
        }
    }
}
