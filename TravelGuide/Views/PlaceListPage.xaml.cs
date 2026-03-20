using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelGuide.Models;
using TravelGuide.Views;

namespace TravelGuide;

public partial class PlaceListPage : ContentPage

{
    public ObservableCollection<Place> Places { get; set; }
    private async void HomePageTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    private async void OnUserTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UserPage());
    }
    public PlaceListPage()
    {
        InitializeComponent();

        Places = new ObservableCollection<Place>
        {
            new Place
            {
                Name = "Bà Nà Hills",
                Description = "Khu du lịch nổi tiếng với cầu Vàng",
                Image = "banahills.jpg"
            },
            new Place
            {
                Name = "Biển Mỹ Khê",
                Description = "Một trong những bãi biển đẹp nhất Việt Nam",
                Image = "beach.jpg"
            },
            new Place
            {
                Name = "Ngũ Hành Sơn",
                Description = "Danh lam thắng cảnh nổi tiếng",
                Image = "mountain.jpg"
            }
        };

        PlaceList.ItemsSource = Places;
    }
}