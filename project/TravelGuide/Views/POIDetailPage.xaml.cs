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
    public partial class POIDetailPage : ContentPage
    {
        private readonly POIDataService _poiData;
        private readonly TTSPlayerService _tts;
        private readonly AuthService _auth;
        private static LocalizationService L => LocalizationService.Instance;

        private POISummaryDto? _poi;
        private bool _isLoading;
        private bool _hasContent;
        private string _narrationText = "";
        private string _languageFlag = "🌐";
        private bool _isTTSPlaying;

        public POISummaryDto? POI { get => _poi; set => Set(ref _poi, value); }
        public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }
        public bool HasContent { get => _hasContent; set => Set(ref _hasContent, value); }
        public bool HasNoContent => !HasContent && !IsLoading;
        public string NarrationText { get => _narrationText; set => Set(ref _narrationText, value); }
        public string LanguageFlag { get => _languageFlag; set => Set(ref _languageFlag, value); }

        public ICommand BackCommand { get; }

        private POIDetailDto? _detailDto;

        private string? _poiId;
        public string? PoiId
        {
            get => _poiId;
            set
            {
                _poiId = value;
                if (Guid.TryParse(value, out var id))
                    _ = LoadPOIAsync(id);
            }
        }

        public POIDetailPage(POIDataService poiData, TTSPlayerService tts, AuthService auth)
        {
            InitializeComponent();
            _poiData = poiData;
            _tts = tts;
            _auth = auth;
            BindingContext = this;

            BackCommand = new Command(async () =>
            {
                _tts.Stop();
                await Shell.Current.GoToAsync("..");
            });

            L.PropertyChanged += OnLanguageChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshUIText();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _tts.Stop();
            _isTTSPlaying = false;
            L.PropertyChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(object? sender, PropertyChangedEventArgs e)
            => MainThread.BeginInvokeOnMainThread(RefreshUIText);

        // ── Refresh UI ───────────────────────────────────────────────
        private void RefreshUIText()
        {
            UpdateTTSButton();
            if (ContentLabel != null) ContentLabel.Text = L["Content_Section"];
            if (NoContentLabel != null) NoContentLabel.Text = L["TTS_NoContent"];
        }

        /// <summary>Cập nhật text và màu nút TTS trực tiếp</summary>
        private void UpdateTTSButton()
        {
            if (TTSToggleBtn == null) return;
            if (_isTTSPlaying)
            {
                TTSToggleBtn.Text = L["TTS_Stop"];
                TTSToggleBtn.BackgroundColor = Color.FromArgb("#F57C00");
            }
            else
            {
                TTSToggleBtn.Text = L["TTS_Play"];
                TTSToggleBtn.BackgroundColor = Color.FromArgb("#2E7D32");
            }
        }

        // ── Load POI ─────────────────────────────────────────────────
        private async Task LoadPOIAsync(Guid poiId)
        {
            IsLoading = true;
            var lang = _auth.GetCurrentLanguage();
            Console.WriteLine($"[log] - Load POI: {poiId}, lang={lang}");

            var (dto, fromCache) = await _poiData.GetPOIByIdAsync(poiId, lang);
            IsLoading = false;

            if (dto == null)
            {
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
                ImageUrl = dto.ImageUrl
            };
            Title = dto.Name;

            if (dto.Content != null && !string.IsNullOrWhiteSpace(dto.Content.NarrationText))
            {
                NarrationText = dto.Content.NarrationText;
                LanguageFlag = GetLangFlag(dto.Content.LanguageCode);
                HasContent = true;

                Console.WriteLine($"[info] - Co noi dung TTS ({dto.Content.NarrationText.Length} ky tu), tu dong phat");
                UpdateTTSButton();

                // Tự động phát TTS khi vào trang
                await PlayTTSAsync();
            }
            else
            {
                HasContent = false;
                Console.WriteLine("[warn] - Khong co noi dung van ban cho TTS");
            }
            OnPropertyChanged(nameof(HasNoContent));
        }

        // ── TTS: Clicked event handler ──────────────────────────────
        private async void OnTTSToggleClicked(object? sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"[log] - NUT DUOC BAM! isTTSPlaying={_isTTSPlaying}");

                if (_isTTSPlaying)
                {
                    Console.WriteLine("[log] - Nguoi dung bam Ngung Phat");
                    _tts.Stop();
                    _isTTSPlaying = false;
                    UpdateTTSButton();
                }
                else
                {
                    Console.WriteLine("[log] - Nguoi dung bam Phat");
                    await PlayTTSAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - OnTTSToggleClicked: {ex}");
            }
        }

        /// <summary>
        /// Phát TTS từ NarrationText trong database.
        /// </summary>
        private async Task PlayTTSAsync()
        {
            if (_detailDto?.Content == null) return;

            var text = _detailDto.Content.NarrationText;
            var lang = _detailDto.Content.LanguageCode;

            if (string.IsNullOrWhiteSpace(text)) return;

            _isTTSPlaying = true;
            UpdateTTSButton();

            try
            {
                Console.WriteLine($"[log] - Bat dau PlayAsync: {text.Length} ky tu");
                await _tts.PlayAsync(text, lang);
                Console.WriteLine("[log] - PlayAsync xong");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - PlayTTSAsync: {ex}");
            }
            finally
            {
                _isTTSPlaying = false;
                UpdateTTSButton();
            }
        }

        // ── Helpers ──────────────────────────────────────────────────
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

        private new void OnPropertyChanged([CallerMemberName] string? name = null)
            => base.OnPropertyChanged(name);
        private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
