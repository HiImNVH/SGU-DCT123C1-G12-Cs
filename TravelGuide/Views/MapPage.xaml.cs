using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace TravelGuide;
public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();

        mapWeb.Source = new HtmlWebViewSource
        {
            Html = @"
            <!DOCTYPE html>
            <html>
            <head>
            <link rel='stylesheet' href='https://unpkg.com/leaflet/dist/leaflet.css'/>
            <script src='https://unpkg.com/leaflet/dist/leaflet.js'></script>
            </head>

            <body>
            <div id='map' style='height:100vh'></div>

            <script>
            var map = L.map('map').setView([16.047079,108.206230], 13);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',{
            maxZoom:19
            }).addTo(map);

            L.marker([16.047079,108.206230]).addTo(map)
            .bindPopup('Đà Nẵng');
            </script>

            </body>
            </html>"
        };
    }
}