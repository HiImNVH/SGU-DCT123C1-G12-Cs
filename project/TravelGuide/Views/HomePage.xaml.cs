// Views/HomePage.xaml.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TravelGuide.Controls;
using TravelGuide.Models.DTOs;
using TravelGuide.Services;

namespace TravelGuide.Views
{
    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        private readonly POIDataService _poiData;
        private readonly AuthService _auth;
        private readonly ProximityService _proximity;
        private readonly TTSPlayerService _tts;
        private static LocalizationService L => LocalizationService.Instance;

        public ObservableCollection<POISummaryDto> NearbyPlaces { get; } = new();

        private bool _isLoading;
        private bool _isRefreshing;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        public bool IsRefreshing { get => _isRefreshing; set { _isRefreshing = value; OnPropertyChanged(); } }
        public bool IsEmpty => !IsLoading && NearbyPlaces.Count == 0;

        public ICommand RefreshCommand { get; }

        public HomePage(POIDataService poiData, AuthService auth,
                        ProximityService proximity, TTSPlayerService tts)
        {
            InitializeComponent();
            _poiData = poiData;
            _auth = auth;
            _proximity = proximity;
            _tts = tts;
            BindingContext = this;
            RefreshCommand = new Command(async () => await LoadPOIsAsync(forceRefresh: true));

            L.PropertyChanged += (_, _) =>
                MainThread.BeginInvokeOnMainThread(RefreshUIText);

            // ── Kết nối ProximityBanner events ──────────────────────
            ProximityBanner.YesClicked += OnProximityYes;
            ProximityBanner.NoClicked += OnProximityNo;
            ProximityBanner.Dismissed += OnProximityDismissed;

            // ── Kết nối ProximityService events ─────────────────────
            _proximity.AutoPlayTriggered += OnAutoPlayTriggered;
            _proximity.NearbyPOIDetected += OnNearbyPOIDetected;
        }

        // ── Lifecycle ────────────────────────────────────────────────

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            RefreshUIText();
            UpdateUserGreeting();
            await LoadPOIsAsync();

            // Bắt đầu theo dõi vị trí khi page hiển thị
            await _proximity.StartAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Dừng khi user chuyển tab khác
            _proximity.Stop();
        }

        // ── Proximity handlers ───────────────────────────────────────

        /// <summary>
        /// Vào vùng ≤15m → tự phát TTS ngay, không hỏi.
        /// Hiển thị mini-banner thông báo "Đang phát..." rồi tự ẩn sau 3 giây.
        /// </summary>
        private void OnAutoPlayTriggered(POISummaryDto poi)
        {
            Console.WriteLine($"[proximity] - Auto play: {poi.Name}");

            // Load chi tiết và phát TTS
            _ = Task.Run(async () =>
            {
                var lang = _auth.GetCurrentLanguage();
                var (dto, _) = await _poiData.GetPOIByIdAsync(poi.Id, lang);
                if (dto?.Content == null) return;

                await _tts.StopAsync();
                await _tts.PlayAsync(
                    null,
                    dto.Content.AudioUrl,
                    dto.Content.NarrationText,
                    dto.Content.LanguageCode);
            });

            // Hiện mini-banner tự tắt sau 3 giây (thông báo "Đang phát")
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                ProximityBanner.Show(poi, 10); // hiện với text đặc biệt
                // Sau 3 giây tự ẩn — không cần user tương tác
                await Task.Delay(3000);
                ProximityBanner.Hide();
            });
        }

        /// <summary>
        /// Vào vùng 15–40m → hiện banner hỏi user.
        /// </summary>
        private void OnNearbyPOIDetected(POISummaryDto poi)
        {
            Console.WriteLine($"[proximity] - Nearby notify: {poi.Name}");

            var dist = _proximity.LastLocation != null
                ? ProximityService.GetDistance(_proximity.LastLocation, poi)
                : 0;

            MainThread.BeginInvokeOnMainThread(() =>
                ProximityBanner.Show(poi, dist));
        }

        /// <summary>User bấm "Có" → load POI chi tiết và phát TTS</summary>
        private void OnProximityYes(POISummaryDto poi)
        {
            Console.WriteLine($"[proximity] - User chon CO: {poi.Name}");

            _ = Task.Run(async () =>
            {
                var lang = _auth.GetCurrentLanguage();
                var (dto, _) = await _poiData.GetPOIByIdAsync(poi.Id, lang);
                if (dto?.Content == null) return;

                await _tts.StopAsync();
                await _tts.PlayAsync(
                    null,
                    dto.Content.AudioUrl,
                    dto.Content.NarrationText,
                    dto.Content.LanguageCode);
            });
        }

        /// <summary>User bấm "Không" → reset trigger để không hỏi lại ngay</summary>
        private void OnProximityNo(POISummaryDto poi)
        {
            Console.WriteLine($"[proximity] - User chon KHONG: {poi.Name}");
            // Reset để POI này không notify lại cho đến khi user đi ra xa rồi quay lại
            _proximity.ResetPOI(poi.Id);
        }

        /// <summary>Banner tự tắt sau 8 giây không tương tác → không làm gì</summary>
        private void OnProximityDismissed(POISummaryDto poi)
        {
            Console.WriteLine($"[proximity] - Banner tu tat: {poi.Name}");
            // Reset để có thể notify lại nếu user vẫn ở đây
            // Không reset _notified → user phải đi ra rồi vào lại mới notify
        }

        // ── UI refresh ───────────────────────────────────────────────

        private void RefreshUIText()
        {
            if (GreetingLabel != null) GreetingLabel.Text = L["Home_Greeting"];
            if (ScanCardTitleLabel != null) ScanCardTitleLabel.Text = L["Scan_Title"];
            if (ScanCardHintLabel != null) ScanCardHintLabel.Text = L["Home_ScanHint"];
            if (NearbyTitleLabel != null) NearbyTitleLabel.Text = L["Home_Nearby"];
            if (EmptyLabel != null) EmptyLabel.Text = L["Home_Empty"];
        }

        private void UpdateUserGreeting()
        {
            var user = _auth.GetCurrentUser();
            if (UserNameLabel != null)
                UserNameLabel.Text = user?.Username ?? "Du khách";
            if (GreetingLabel != null)
                GreetingLabel.Text = L["Home_Greeting"];
        }

        // ── Load POIs ────────────────────────────────────────────────

        private async Task LoadPOIsAsync(bool forceRefresh = false)
        {
            Console.WriteLine("[log] - Tai danh sach POI cho trang chu");
            IsLoading = true;
            OnPropertyChanged(nameof(IsEmpty));

            try
            {
                var list = await _poiData.GetAllActiveAsync();
                NearbyPlaces.Clear();
                foreach (var p in list.Take(10))
                    NearbyPlaces.Add(p);

                // Cập nhật danh sách POI cho ProximityService
                _proximity.UpdatePOIs(list);

                Console.WriteLine($"[info] - Da tai {NearbyPlaces.Count} POI");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi tai POI: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        // ── Navigation ───────────────────────────────────────────────

        private async void OnScanTapped(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//scan");

        private async void OnPOICardTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is not POISummaryDto poi) return;
            await Shell.Current.GoToAsync(nameof(POIDetailPage),
                new Dictionary<string, object> { { "PoiId", poi.Id.ToString() } });
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
