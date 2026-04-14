// Views/HomePage.xaml.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TravelGuide.Models.DTOs;
using TravelGuide.Services;

namespace TravelGuide.Views
{
    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        private readonly POIDataService _poiData;
        private readonly AuthService _auth;
        private static LocalizationService L => LocalizationService.Instance;

        public ObservableCollection<POISummaryDto> NearbyPlaces { get; } = new();

        private bool _isLoading;
        private bool _isRefreshing;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        public bool IsRefreshing { get => _isRefreshing; set { _isRefreshing = value; OnPropertyChanged(); } }
        public bool IsEmpty => !IsLoading && NearbyPlaces.Count == 0;

        public ICommand RefreshCommand { get; }

        public HomePage(POIDataService poiData, AuthService auth)
        {
            InitializeComponent();
            _poiData = poiData;
            _auth = auth;
            BindingContext = this;
            RefreshCommand = new Command(async () => await LoadPOIsAsync(forceRefresh: true));

            // Đổi ngôn ngữ → refresh UI ngay lập tức
            L.PropertyChanged += (_, _) =>
                MainThread.BeginInvokeOnMainThread(RefreshUIText);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            RefreshUIText();
            UpdateUserGreeting();
            await LoadPOIsAsync();
        }

        // ── Refresh toàn bộ text theo ngôn ngữ hiện tại ─────────────────
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
            UserNameLabel.Text = user?.Username ?? "Du khách";
            GreetingLabel.Text = L["Home_Greeting"];
        }

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

        private async void OnScanTapped(object sender, EventArgs e)
        {
            Console.WriteLine("[log] - Chuyen sang trang Scan QR");
            await Shell.Current.GoToAsync("//scan");
        }

        private async void OnPOICardTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is not POISummaryDto poi) return;
            Console.WriteLine($"[log] - Xem chi tiet POI tu trang chu: {poi.Name}");
            await Shell.Current.GoToAsync(nameof(POIDetailPage),
                new Dictionary<string, object> { { "PoiId", poi.Id.ToString() } });
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
