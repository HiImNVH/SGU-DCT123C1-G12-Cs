using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelGuide.Views;

namespace TravelGuide;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }
    private async void OnMapClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MapPage());
    }
    private async void OnPlaceTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PlaceListPage());
    }
    private async void OnUserTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserPage());
    }
}
