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
        private PlayerState _playerState = PlayerState.Idle;

        public bool IsScanning { get => _isScanning; set => Set(ref _isScanning, value); }
        public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }
        public bool HasResult { get => _hasResult; set => Set(ref _hasResult, value); }
        public bool HasError { get => _hasError; set => Set(ref _hasError, value); }
        public string ErrorMessage { get => _errorMessage; set => Set(ref _errorMessage, value); }
        public POIDetailDto? CurrentPOI { get => _currentPOI; set => Set(ref _currentPOI, value); }
        public PlayerState PlayerState { get => _playerState; set => Set(ref _playerState, value); }

        public bool IsPlaying => PlayerState == PlayerState.Playing;
        public bool IsPaused => PlayerState == PlayerState.Paused;
        public bool CanPlay => HasResult && !IsPlaying && !IsPaused;
        public bool CanResume => HasResult && IsPaused;

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ResetCommand { get; }

        public ScanViewModel(QRScannerService scanner, POIDataService poiData, TTSPlayerService tts, AuthService auth)
        {
            _scanner = scanner;
            _poiData = poiData;
            _tts = tts;
            _auth = auth;

            PlayCommand = new Command(async () => await PlayTTSAsync(), () => CanPlay || CanResume);
            PauseCommand = new Command(async () => await PauseAsync(), () => IsPlaying);
            StopCommand = new Command(async () => await StopTTSAsync(), () => IsPlaying || IsPaused);

            /*
             * FIX race condition TTS: ResetCommand giờ gọi async method
             * để đảm bảo TTS.StopAsync() hoàn thành trước khi reset state.
             * Trước đây dùng _ = _tts.StopAsync() (fire-and-forget) → TTS
             * có thể vẫn đang chạy trong khi camera đã bật lại.
             */
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

        private async Task PlayTTSAsync()
        {
            if (CurrentPOI?.Content == null) return;
            PlayerState = PlayerState.Playing;
            RefreshCommands();

            /*
             * PlayAsync là blocking call (chờ TTS đọc xong mới return).
             * Chạy trên Task.Run để không block UI thread trong khi đọc.
             */
            await Task.Run(async () =>
            {
                await _tts.PlayAsync(
                    null,
                    CurrentPOI.Content.AudioUrl,
                    CurrentPOI.Content.NarrationText,
                    CurrentPOI.Content.LanguageCode);
            });

            // Cập nhật state sau khi TTS xong (hoặc bị cancel)
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                PlayerState = _tts.GetState();
                RefreshCommands();
            });
        }

        private async Task PauseAsync()
        {
            await _tts.PauseAsync();
            PlayerState = PlayerState.Paused;
            RefreshCommands();
        }

        public async Task StopTTSAsync()
        {
            await _tts.StopAsync();
            PlayerState = PlayerState.Stopped;
            RefreshCommands();
        }

        // ── State ────────────────────────────────────────────────────────

        private void SetError(string msg)
        {
            IsLoading = false;
            HasError = true;
            ErrorMessage = msg;
        }

        private async Task ResetStateAsync()
        {
            /*
             * FIX: await StopAsync trước khi reset → đảm bảo TTS dừng hẳn
             * trước khi IsScanning = true và camera bật lại.
             * Trước đây dùng _ = _tts.StopAsync() không await → race condition.
             */
            await _tts.StopAsync();

            CurrentPOI = null;
            HasResult = false;
            HasError = false;
            ErrorMessage = "";
            PlayerState = PlayerState.Idle;
            IsScanning = true;
            RefreshCommands();
        }

        private void RefreshCommands()
        {
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsPaused));
            OnPropertyChanged(nameof(CanPlay));
            OnPropertyChanged(nameof(CanResume));
            (PlayCommand as Command)?.ChangeCanExecute();
            (PauseCommand as Command)?.ChangeCanExecute();
            (StopCommand as Command)?.ChangeCanExecute();
        }

        private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(name);
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Guid? DecodePoiId(string raw)
        {
            return _scanner.DecodePoiId(raw);
        }
    }


}
