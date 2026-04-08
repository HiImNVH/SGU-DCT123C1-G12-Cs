using System.Collections.ObjectModel;
using TravelGuide.Models;
using TravelGuide.Services;

namespace TravelGuide;

public partial class PlaceListPage : ContentPage
{
    private readonly IPOIService _poiService;
    public ObservableCollection<POI> Places { get; set; } = new();

    public PlaceListPage(IPOIService poiService)
    {
        InitializeComponent();
        _poiService = poiService;
        BindingContext = this;
    }
    private List<POI> _allPlaces = new();

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue?.ToLower() ?? "";

        var filtered = _allPlaces.Where(p =>
            (!string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(keyword)) ||
            (!string.IsNullOrEmpty(p.Category) && p.Category.ToLower().Contains(keyword))
        ).ToList();

        Places.Clear();
        foreach (var p in filtered)
            Places.Add(p);
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var pois = await _poiService.GetAllAsync();
        _allPlaces = pois.ToList();
        Places.Clear();
        foreach (var p in pois)
            Places.Add(p);
    }

    private async void OnItemTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is POI poi)
            await Shell.Current.GoToAsync(nameof(PlaceDetailPage), true,
                new Dictionary<string, object> { { "Place", poi } });
    }
}