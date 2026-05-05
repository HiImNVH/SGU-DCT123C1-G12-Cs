// ViewModels/ScanViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TravelGuide.Models;
using TravelGuide.Models.DTOs;
using TravelGuide.Services;

namespace TravelGuide.ViewModels
{
    public class ScanViewModel : INotifyPropertyChanged
    {
        private readonly QRScannerService _scanner;
        private readonly POIDataService _poiData;
        private readonly TTSPlayerService _tts;
        private readonly AuthService _auth;

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isScanning = true;
        private bool _isLoading;
        private bool _hasResult;
        private bool _hasError;
        private string _errorMessage = "";
        private POIDetailDto? _currentPOI;
        private bool _isTTSPlaying;

        public bool IsScanning { get => _isScanning; set => Set(ref _isScanning, value); }
        public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }
        public bool HasResult { get => _hasResult; set => Set(ref _hasResult, value); }
        public bool HasError { get => _hasError; set => Set(ref _hasError, value); }
        public string ErrorMessage { get => _errorMessage; set => Set(ref _errorMessage, value); }
        public POIDetailDto? CurrentPOI { get => _currentPOI; set => Set(ref _currentPOI, value); }

        public bool IsTTSPlaying
        {
            get => _isTTSPlaying;
            set
            {
                if (Set(ref _isTTSPlaying, value))
                {
                    OnPropertyChanged(nameof(TTSButtonText));
                    (ToggleTTSCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public string TTSButtonText => IsTTSPlaying ? "⏹ Ngừng phát" : "▶ Phát thuyết minh";

        public ICommand ToggleTTSCommand { get; }
        public ICommand ResetCommand { get; }

        public ScanViewModel(QRScannerService scanner, POIDataService poiData, TTSPlayerService tts, AuthService auth)
        {
            _scanner = scanner;
            _poiData = poiData;
            _tts = tts;
            _auth = auth;

            ToggleTTSCommand = new Command(async () => await ToggleTTSAsync(), () => HasResult);
            ResetCommand = new Command(async () => await ResetStateAsync());
        }

        // ── Public API cho ScanPage ──────────────────────────────────────

        /// <summary>
        /// Gọi từ OnOpenCameraClicked và RestartCamera trong page.
        /// Đồng bộ với ResetStateAsync để tránh race condition.
        /// </summary>
        public async Task StartScanningAsync() => await ResetStateAsync();

        /// <summary>
        /// Giữ lại method sync để tương thích với các chỗ gọi cũ nếu có.
        /// </summary>
        public void StartScanning()
        {
            // Gọi async fire-and-forget một cách an toàn hơn _ = ...
            Task.Run(async () => await ResetStateAsync());
        }

        // ── QR Handling ──────────────────────────────────────────────────

        public async Task OnQRScannedAsync(string rawValue)
        {
            Console.WriteLine("[log] - Bat dau xu ly ket qua scan QR");
            IsScanning = false;
            IsLoading = true;
            HasError = false;
            HasResult = false;

            var poiId = _scanner.DecodePoiId(rawValue);
            if (poiId == null)
            {
                Console.WriteLine("[error] - QR khong hop le");
                SetError("Không nhận diện được gian hàng. Vui lòng thử lại.");
                return;
            }

            var lang = _auth.GetCurrentLanguage();
            var (dto, fromCache) = await _poiData.GetPOIByIdAsync(poiId.Value, lang);
            IsLoading = false;

            if (dto == null)
            {
                var msg = Helpers.NetworkHelper.IsConnected
                    ? "Không tìm thấy thông tin gian hàng."
                    : "Ngoại tuyến - không có dữ liệu. Vui lòng kết nối mạng và thử lại.";
                Console.WriteLine("[error] - Khong tai duoc POI");
                SetError(msg);
                return;
            }

            CurrentPOI = dto;
            HasResult = true;
            Console.WriteLine($"[info] - Hien thi POI: {dto.Name} (tu {(fromCache ? "cache" : "API")})");
            await AutoPlayTTSAsync();
        }

        // ── TTS ──────────────────────────────────────────────────────────

        private async Task AutoPlayTTSAsync()
        {
            if (CurrentPOI?.Content == null) return;
            Console.WriteLine("[log] - Tu dong phat TTS sau scan");
            await PlayTTSAsync();
        }

        private async Task ToggleTTSAsync()
        {
            if (IsTTSPlaying)
            {
                Console.WriteLine("[log] - Nguoi dung bam Ngung Phat (Scan)");
                _tts.Stop();
                IsTTSPlaying = false;
            }
            else
            {
                Console.WriteLine("[log] - Nguoi dung bam Phat (Scan)");
                await PlayTTSAsync();
            }
        }

        private async Task PlayTTSAsync()
        {
            if (CurrentPOI?.Content == null) return;

            var text = CurrentPOI.Content.NarrationText;
            var lang = CurrentPOI.Content.LanguageCode;
            if (string.IsNullOrWhiteSpace(text)) return;

            IsTTSPlaying = true;
            await _tts.PlayAsync(text, lang);
            IsTTSPlaying = false;
        }

        public void StopTTS()
        {
            _tts.Stop();
            IsTTSPlaying = false;
        }

        // ── State ────────────────────────────────────────────────────────

        private void SetError(string msg)
        {
            IsLoading = false;
            HasError = true;
            ErrorMessage = msg;
        }

        private Task ResetStateAsync()
        {
            _tts.Stop();
            IsTTSPlaying = false;

            CurrentPOI = null;
            HasResult = false;
            HasError = false;
            ErrorMessage = "";
            IsScanning = true;
            (ToggleTTSCommand as Command)?.ChangeCanExecute();
            return Task.CompletedTask;
        }

        private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Guid? DecodePoiId(string raw)
        {
            return _scanner.DecodePoiId(raw);
        }
    }


}
