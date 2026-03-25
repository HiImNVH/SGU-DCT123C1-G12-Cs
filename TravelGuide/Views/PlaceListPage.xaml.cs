using System.Collections.ObjectModel;
using TravelGuide.Models;

namespace TravelGuide;

public partial class PlaceListPage : ContentPage
{
    public ObservableCollection<Place> Places { get; set; }

    public PlaceListPage()
    {
        InitializeComponent();

        Places = new ObservableCollection<Place>
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
            },
            new Place
            {
                location_name = "Dinh Độc Lập",
                city = "TP.HCM",
                description = "Di tích lịch sử gắn liền với ngày 30/4/1975",
                image_url = "dinhdoclap.jpg",
                Latitude = 10.7769,
                Longitude = 106.6953
            },
            new Place
            {
                location_name = "Phố đi bộ Nguyễn Huệ",
                city = "TP.HCM",
                description = "Khu phố sầm uất, nơi diễn ra nhiều hoạt động giải trí",
                image_url = "nguyenhue.jpg",
                Latitude = 10.7745,
                Longitude = 106.7030
            }
        };

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
}