using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelGuide.Models;

namespace TravelGuide;

[QueryProperty(nameof(Place), "Place")]
public partial class PlaceDetailPage : ContentPage
{
    public PlaceDetailPage()
    {
        InitializeComponent();
    }

    public Place Place
    {
        set { BindingContext = value; }
    }
    public string PlaceName
    {
        set
        {
            Title = value;
        }
    }
}