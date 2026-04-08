using System.Globalization;
using TravelGuide.Models;
using TravelGuide.Services;

namespace TravelGuide.Views;

public partial class MapPage : ContentPage
{
    private readonly IPOIService _poiService;
    private readonly ILocationService _locationService;

    public MapPage(IPOIService poiService, ILocationService locationService)
    {
        InitializeComponent();
        _poiService = poiService;
        _locationService = locationService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMap();
    }

    private async Task LoadMap()
    {
        try
        {
            var pois = await _poiService.GetAllAsync();
            var location = await _locationService.GetCurrentLocationAsync();
            double userLat = location?.Latitude ?? 10.776889;
            double userLng = location?.Longitude ?? 106.700806;

            // Dùng InvariantCulture để đảm bảo dấu chấm thập phân
            var lat = userLat.ToString(CultureInfo.InvariantCulture);
            var lng = userLng.ToString(CultureInfo.InvariantCulture);

            var poiJs = BuildPOIJavaScript(pois);

            string html =
                "<!DOCTYPE html><html><head>" +
                "<meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=5.0, user-scalable=yes'>" +
                "<script src='https://cdn.jsdelivr.net/npm/@goongmaps/goong-js@1.0.9/dist/goong-js.js'></script>" +
                "<link href='https://cdn.jsdelivr.net/npm/@goongmaps/goong-js@1.0.9/dist/goong-js.css' rel='stylesheet'/>" +
                "<style>" +
                "body{margin:0;padding:0;}" +
                "#map{position:absolute;top:0;bottom:0;width:100%;}" +
                ".popup-content{font-family:sans-serif;max-width:180px;}" +
                ".popup-content h4{margin:0 0 4px 0;font-size:14px;color:#2E7D32;}" +
                ".popup-content p{margin:0 0 6px 0;font-size:12px;color:#666;}" +
                ".popup-btn{display:block;width:100%;padding:6px;background:#2E7D32;color:white;border:none;border-radius:6px;font-size:12px;cursor:pointer;}" +
                "</style></head><body>" +
                "<div id='map'></div>" +
                "<script>" +
                "goongjs.accessToken='POdfxjueKxYZ09MjbKKLCKkxeNLfhFpsXaOfT3Rn';" +
                "var map=new goongjs.Map({" +
                "container:'map'," +
                "style:'https://tiles.goong.io/assets/goong_map_web.json?api_key=POdfxjueKxYZ09MjbKKLCKkxeNLfhFpsXaOfT3Rn'," +
                // Dùng biến lat/lng đã convert InvariantCulture
                $"center:[{lng},{lat}]," +
                "zoom:13" +
                "});" +
                "map.on('load',function(){" +
                "var userEl=document.createElement('div');" +
                "userEl.style.cssText='width:16px;height:16px;background:#2196F3;border:3px solid white;border-radius:50%;box-shadow:0 2px 6px rgba(0,0,0,0.4)';" +
                $"new goongjs.Marker({{element:userEl}}).setLngLat([{lng},{lat}]).setPopup(new goongjs.Popup().setHTML('<b>Vi tri cua ban</b>')).addTo(map);" +
                $"var places={poiJs};" +
                "places.forEach(function(p){" +
                "var el=document.createElement('div');" +
                "el.style.cssText='width:32px;height:32px;background:#2E7D32;border:3px solid white;border-radius:50%;display:flex;align-items:center;justify-content:center;box-shadow:0 2px 6px rgba(0,0,0,0.4);cursor:pointer;';" +
                "el.innerHTML='<span style=\"color:white;font-size:16px\">&#128205;</span>';" +
                "var popup=new goongjs.Popup({offset:30}).setHTML(" +
                "'<div class=\"popup-content\">'+" +
                "'<h4>'+p.name+'</h4>'+" +
                "'<p>'+p.category+'</p>'+" +
                "'<p>'+p.description+'</p>'+" +
                "'<button class=\"popup-btn\" onclick=\"window.location=\\'app://detail?id='+p.id+'&name='+encodeURIComponent(p.name)+'\\'\">" +
                "&#128266; Nghe thuyet minh</button>'+" +
                "'</div>');" +
                "new goongjs.Marker({element:el}).setLngLat([p.lng,p.lat]).setPopup(popup).addTo(map);" +
                "});" +
                "});" +
                "</script></body></html>";

            mapWeb.Source = new HtmlWebViewSource { Html = html };
            mapWeb.Navigating += MapWeb_Navigating;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể tải bản đồ: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private string BuildPOIJavaScript(List<POI> pois)
    {
        if (pois == null || !pois.Any()) return "[]";

        // Dùng InvariantCulture cho tất cả số thực
        var items = pois.Select(p =>
            "{" +
            $"id:'{p.Id}'," +
            $"name:'{EscapeJs(p.Name)}'," +
            $"category:'{EscapeJs(p.Category)}'," +
            $"description:'{EscapeJs(p.Description)}'," +
            $"lat:{p.Latitude.ToString(CultureInfo.InvariantCulture)}," +
            $"lng:{p.Longitude.ToString(CultureInfo.InvariantCulture)}," +
            $"radius:{p.Radius}" +
            "}");

        return "[" + string.Join(",", items) + "]";
    }

    private string EscapeJs(string s) =>
        s?.Replace("'", "\\'").Replace("\n", " ").Replace("\r", "") ?? "";

    private async void MapWeb_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("app://detail"))
        {
            e.Cancel = true;

            var uri = new Uri(e.Url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var id = query["id"];
            var name = query["name"];

            var pois = await _poiService.GetAllAsync();
            var poi = pois.FirstOrDefault(p => p.Id.ToString() == id);

            if (poi != null)
                await Shell.Current.GoToAsync(nameof(PlaceDetailPage), true,
                    new Dictionary<string, object> { { "Place", poi } });
            else
                await Shell.Current.GoToAsync(nameof(PlaceDetailPage), true,
                    new Dictionary<string, object> { { "PlaceName", name } });
        }
    }
}