using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelGuide.Views;
using TravelGuide.Models;
namespace TravelGuide;

public partial class HomePage : ContentPage
{
    public ObservableCollection<Place> NearbyPlaces { get; set; }

    public HomePage()
    {
        InitializeComponent();
        var allPlaces = new List<Place>
        {
            new Place
            {
                location_name = "Chợ Bến Thành",
                city = "TP.HCM",
                description = "Chợ nổi tiếng",
                image_url = "benthanh.jpg",
                Latitude = 10.772,
                Longitude = 106.698
            },
            new Place
            {
                location_name = "Nhà thờ Đức Bà",
                city = "TP.HCM",
                description = "Kiến trúc Pháp",
                image_url = "nhathoducba.jpg",
                Latitude = 10.779,
                Longitude = 106.699
            },
            new Place
            {
                location_name = "Landmark 81",
                city = "TP.HCM",
                description = "Tòa nhà cao nhất",
                image_url = "landmark81.jpg",
                Latitude = 10.794,
                Longitude = 106.721
            }
        };

        // 👉 vị trí giả của user
        double userLat = 10.775;
        double userLng = 106.700;

        // 👉 lọc 3 địa điểm gần nhất
        var nearby = allPlaces
            .OrderBy(p => Distance(userLat, userLng, p.Latitude, p.Longitude))
            .Take(3);

        NearbyPlaces = new ObservableCollection<Place>(nearby);

        BindingContext = this;
    }
    private async void OnItemTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is Place place)
        {
            await Shell.Current.GoToAsync(nameof(PlaceDetailPage), true,
                new Dictionary<string, object>
                {
                { "Place", place }
                });
        }
    }

    // tính khoảng cách đơn giản
    private double Distance(double lat1, double lon1, double lat2, double lon2)
    {
        return Math.Sqrt(Math.Pow(lat1 - lat2, 2) + Math.Pow(lon1 - lon2, 2));
    }

    private async void OnMapClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MapPage));
    }

    private async void OnPlaceTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//places");
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//profile");
    }

}