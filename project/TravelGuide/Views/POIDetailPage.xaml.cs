// Views/POIDetailPage.xaml.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TravelGuide.Models;
using TravelGuide.Models.DTOs;
using TravelGuide.Services;

namespace TravelGuide.Views
{
    [QueryProperty(nameof(PoiId), "PoiId")]
    public partial class POIDetailPage : ContentPage, INotifyPropertyChanged
    {
        private readonly POIDataService _poiData;
        private readonly TTSPlayerService _tts;
        private readonly AuthService _auth;
        private static LocalizationService L => LocalizationService.Instance;

        // ── State ──────────────────────────────────────────────────────
        private POISummaryDto? _poi;
        private bool _isLoading;
        private bool _hasContent;
        private string _narrationText = "";
        private string _languageFlag = "🌐";
        private PlayerState _playerState = PlayerState.Idle;

        public POISummaryDto? POI { get => _poi; set => Set(ref _poi, value); }
        public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }
        public bool HasContent { get => _hasContent; set => Set(ref _hasContent, value); }
        public bool HasNoContent => !HasContent && !IsLoading;
        public string NarrationText { get => _narrationText; set => Set(ref _narrationText, value); }
        public string LanguageFlag { get => _languageFlag; set => Set(ref _languageFlag, value); }
        public PlayerState PlayerState { get => _playerState; set => Set(ref _playerState, value); }

        public bool IsPlaying => PlayerState == PlayerState.Playing;
        public bool IsPaused => PlayerState == PlayerState.Paused;
        public bool CanPlay => HasContent && !IsPlaying && !IsPaused;

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand BackCommand { get; }

        private POIDetailDto? _detailDto;

        private string? _poiId;
        public string? PoiId
        {
            get => _poiId;
            set
            {
                _poiId = value;
                if (!string.IsNullOrEmpty(value))
                    _ = LoadPOIAsync(Guid.Parse(value));
            }
        }

        public POIDetailPage(POIDataService poiData, TTSPlayerService tts, AuthService auth)
        {
            InitializeComponent();
            _poiData = poiData;
            _tts = tts;
            _auth = auth;
            BindingContext = this;

            PlayCommand = new Command(async () => await PlayAsync(), () => CanPlay);
            PauseCommand = new Command(async () => await PauseAsync(), () => IsPlaying);
            ResumeCommand = new Command(async () => await ResumeAsync(), () => IsPaused);
            StopCommand = new Command(async () => await StopAsync(), () => IsPlaying || IsPaused);
            BackCommand = new Command(async () =>
            {
                await StopAsync();
                await Shell.Current.GoToAsync("..");
            });

            // Đổi ngôn ngữ → refresh UI ngay lập tức
            L.PropertyChanged += (_, _) =>
                MainThread.BeginInvokeOnMainThread(RefreshUIText);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshUIText();
        }

        // ── Refresh toàn bộ text theo ngôn ngữ hiện tại ─────────────────
        private void RefreshUIText()
        {
            if (TTSSectionLabel != null) TTSSectionLabel.Text = L["TTS_Section"];
            if (PlayBtn != null) PlayBtn.Text = L["TTS_Play"];
            if (PauseBtn != null) PauseBtn.Text = L["TTS_Pause"];
            if (ResumeBtn != null) ResumeBtn.Text = L["TTS_Resume"];
            if (PlayingLabel != null) PlayingLabel.Text = L["TTS_Playing"];
            if (ContentLabel != null) ContentLabel.Text = "Nội dung";
            if (NoContentLabel != null) NoContentLabel.Text = L["TTS_NoContent"];
        }

        private async Task LoadPOIAsync(Guid poiId)
        {
            Console.WriteLine($"[log] - Tai chi tiet POI: {poiId}");
            IsLoading = true;

            var lang = _auth.GetCurrentLanguage();
            var (dto, _) = await _poiData.GetPOIByIdAsync(poiId, lang);

            IsLoading = false;

            if (dto == null)
            {
                Console.WriteLine("[error] - Khong tai duoc chi tiet POI");
                HasContent = false;
                OnPropertyChanged(nameof(HasNoContent));
                return;
            }

            _detailDto = dto;
            POI = new POISummaryDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Category = dto.Category,
                ImageUrl = dto.ImageUrl,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };
            Title = dto.Name;

            if (dto.Content != null)
            {
                NarrationText = dto.Content.NarrationText;
                LanguageFlag = GetLangFlag(dto.Content.LanguageCode);
                HasContent = true;
                Console.WriteLine("[info] - Co noi dung thuyet minh, tu dong phat TTS");
                await PlayAsync();
            }
            else
            {
                HasContent = false;
                Console.WriteLine("[warn] - Khong co noi dung thuyet minh");
            }

            OnPropertyChanged(nameof(HasNoContent));
        }

        private async Task PlayAsync()
        {
            if (_detailDto?.Content == null) return;
            PlayerState = PlayerState.Playing;
            RefreshCommands();

            await _tts.PlayAsync(
                localAudioPath: null,
                audioUrl: _detailDto.Content.AudioUrl,
                narrationText: _detailDto.Content.NarrationText,
                langCode: _detailDto.Content.LanguageCode);

            PlayerState = _tts.GetState();
            RefreshCommands();
        }

        private async Task PauseAsync()
        {
            await _tts.PauseAsync();
            PlayerState = PlayerState.Paused;
            RefreshCommands();
        }

        private async Task ResumeAsync() => await PlayAsync();

        private async Task StopAsync()
        {
            await _tts.StopAsync();
            PlayerState = PlayerState.Stopped;
            RefreshCommands();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await StopAsync();
        }

        private void RefreshCommands()
        {
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsPaused));
            OnPropertyChanged(nameof(CanPlay));
            (PlayCommand as Command)?.ChangeCanExecute();
            (PauseCommand as Command)?.ChangeCanExecute();
            (ResumeCommand as Command)?.ChangeCanExecute();
            (StopCommand as Command)?.ChangeCanExecute();
        }

        private static string GetLangFlag(string code) => code switch
        {
            "vi" => "🇻🇳",
            "en" => "🇺🇸",
            "ja" => "🇯🇵",
            "ko" => "🇰🇷",
            "zh" => "🇨🇳",
            "fr" => "🇫🇷",
            _ => "🌐"
        };

        private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(name);
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
