// HomePage.xaml.cs
using System.Collections.ObjectModel;
using TravelGuide.Models;
using TravelGuide.Services;
using TravelGuide.Views;

namespace TravelGuide;

public partial class HomePage : ContentPage
{
    private readonly IPOIService        _poiService;
    private readonly ILocationService   _locationService;
    private readonly INarrationService  _narrationService;
    private readonly IAuthService       _authService;

    private static LocalizationService L => LocalizationService.Instance;

    public ObservableCollection<POI> NearbyPlaces { get; set; } = new();

    public HomePage(
        IPOIService poiService,
        ILocationService locationService,
        INarrationService narrationService,
        IAuthService authService)
    {
        InitializeComponent();
        _poiService       = poiService;
        _locationService  = locationService;
        _narrationService = narrationService;
        _authService      = authService;
        BindingContext    = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadNearbyPOIs();
        await StartGeofencing();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _locationService.StopMonitoringAsync();
        await _narrationService.StopAsync();
    }

    private async Task LoadNearbyPOIs()
    {
        try
        {
            var location = await _locationService.GetCurrentLocationAsync();
            List<POI> pois;

            if (location != null)
                pois = await _poiService.GetNearbyAsync(
                    location.Latitude, location.Longitude, 5000);
            else
                pois = await _poiService.GetAllAsync();

            NearbyPlaces.Clear();
            foreach (var p in pois.Take(5))
                NearbyPlaces.Add(p);
        }
        catch { }
    }

    private async Task StartGeofencing()
    {
        try
        {
            var allPOIs = await _poiService.GetAllAsync();
            if (!allPOIs.Any()) return;

            _locationService.OnEnteredRegion -= OnEnteredPOIRegion;
            _locationService.OnExitedRegion  -= OnExitedPOIRegion;
            _locationService.OnEnteredRegion += OnEnteredPOIRegion;
            _locationService.OnExitedRegion  += OnExitedPOIRegion;

            await _locationService.StartMonitoringAsync(allPOIs);
        }
        catch { }
    }

    private async void OnEnteredPOIRegion(object sender, POIEventArgs e)
    {
        var user = _authService.GetCurrentUser();
        var lang = user?.PreferredLanguage ?? "vi";

        var content = e.POI.Contents?
            .FirstOrDefault(c => c.LanguageCode == lang)
            ?? e.POI.Contents?.FirstOrDefault();

        var text = content?.NarrationText ?? e.POI.Description;
        if (string.IsNullOrEmpty(text)) return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Dùng key localization cho thông báo tự động
            await DisplayAlert(
                $"📍 {e.POI.Name}",
                L["Home_AutoPlay"],
                L["Common_OK"]);

            await _narrationService.SpeakAsync(text, lang);
        });
    }

    private async void OnExitedPOIRegion(object sender, POIEventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await _narrationService.StopAsync();
        });
    }

    private async void OnItemTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is POI poi)
            await Shell.Current.GoToAsync(nameof(PlaceDetailPage), true,
                new Dictionary<string, object> { { "Place", poi } });
    }

    private async void OnMapClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(Views.MapPage));

    private async void OnPlaceTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//places");

    private async void OanProfileTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//profile");
}
