using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using TravelGuide;

namespace TravelGuide.Views;
public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();
        LoadMap();
    }

    private void LoadMap()
    {
        string html = @"
<!DOCTYPE html>
<html>
<head>
<script src='https://cdn.jsdelivr.net/npm/@goongmaps/goong-js@1.0.9/dist/goong-js.js'></script>
<link href='https://cdn.jsdelivr.net/npm/@goongmaps/goong-js@1.0.9/dist/goong-js.css' rel='stylesheet' />
<style>
body { margin:0; }
#map { position:absolute; top:0; bottom:0; width:100%; }
.popup-btn {
    margin-top:5px;
    padding:5px 10px;
    background:#4CAF50;
    color:white;
    border:none;
    border-radius:5px;
}
</style>
</head>
<body>
<div id='map'></div>

<script>
goongjs.accessToken = 'POdfxjueKxYZ09MjbKKLCKkxeNLfhFpsXaOfT3Rn';

var map = new goongjs.Map({
    container: 'map',
    style: 'https://tiles.goong.io/assets/goong_map_web.json?api_key=POdfxjueKxYZ09MjbKKLCKkxeNLfhFpsXaOfT3Rn',
    center: [106.700806, 10.776889],
    zoom: 12
});

map.on('load', function () {
var places = [
    {
        name: 'Chợ Bến Thành',
        lat: 10.772,
        lng: 106.698,
    },
    {
        name: 'Nhà thờ Đức Bà',
        lat: 10.779,
        lng: 106.699,

    },
    {
        name: 'Dinh Độc Lập',
        lat: 10.7769,
        lng: 106.6953,

    },
    {
        name: 'Phố đi bộ Nguyễn Huệ',
        lat: 10.7745,
        lng: 106.7030,

    },
    {
        name: 'Landmark 81',
        lat: 10.794,
        lng: 106.721,

    }
];

    places.forEach(p => {

        var popup = new goongjs.Popup({ offset: 25 }).setHTML(`
            <b>${p.name}</b><br/>
            ${p.desc}<br/>
        `);

        new goongjs.Marker()
            .setLngLat([p.lng, p.lat])
            .setPopup(popup)
            .addTo(map);
    });
});

//gửi dữ liệu về C#

</script>

</body>
</html>";

        mapWeb.Source = new HtmlWebViewSource { Html = html };

        mapWeb.Navigating += MapWeb_Navigating;
    }
    private async void MapWeb_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("app://detail"))
        {
            e.Cancel = true;

            var uri = new Uri(e.Url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var name = query["name"];

            await Shell.Current.GoToAsync(nameof(PlaceDetailPage), true,
                new Dictionary<string, object>
                {
                { "PlaceName", name }
                });
        }
    }
}