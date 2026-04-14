// Views/MapPage.xaml.cs
using System.Globalization;
using TravelGuide.Models.DTOs;
using TravelGuide.Services;

namespace TravelGuide.Views
{
    public partial class MapPage : ContentPage
    {
        private readonly POIDataService _poiData;
        private static LocalizationService L => LocalizationService.Instance;

        public MapPage(POIDataService poiData)
        {
            InitializeComponent();
            _poiData = poiData;

            // Đổi ngôn ngữ → refresh title ngay lập tức
            L.PropertyChanged += (_, _) =>
                MainThread.BeginInvokeOnMainThread(RefreshUIText);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            RefreshUIText();
            Console.WriteLine("[log] - Mo trang Ban do");
            await LoadMapAsync();
        }

        private void RefreshUIText()
        {
            Title = L["Map_Title"];
        }

        private async Task LoadMapAsync()
        {
            try
            {
                var pois = await _poiData.GetAllActiveAsync();
                Console.WriteLine($"[info] - Hien thi {pois.Count} POI tren ban do");

                double userLat = 10.776889;
                double userLng = 106.700806;

                try
                {
                    var loc = await Geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Low,
                        Timeout = TimeSpan.FromSeconds(5)
                    });
                    if (loc != null)
                    {
                        userLat = loc.Latitude;
                        userLng = loc.Longitude;
                    }
                }
                catch
                {
                    Console.WriteLine("[warn] - Khong lay duoc vi tri, dung vi tri mac dinh");
                }

                var lat = userLat.ToString(CultureInfo.InvariantCulture);
                var lng = userLng.ToString(CultureInfo.InvariantCulture);
                var poiJs = BuildPOIJson(pois);

                // Lấy text nút "Xem chi tiết" theo ngôn ngữ hiện tại
                var viewDetailText = L["Map_ViewDetail"];
                var userLocText = L["Map_UserLocation"];

                string html =
                    "<!DOCTYPE html><html><head>" +
                    "<meta name='viewport' content='width=device-width, initial-scale=1.0, minimum-scale=0.5, maximum-scale=10.0, user-scalable=yes'>" +
                    "<script src='https://cdn.jsdelivr.net/npm/@goongmaps/goong-js@1.0.9/dist/goong-js.js'></script>" +
                    "<link href='https://cdn.jsdelivr.net/npm/@goongmaps/goong-js@1.0.9/dist/goong-js.css' rel='stylesheet'/>" +
                    "<style>" +
                    "body{margin:0;padding:0;}" +
                    "#map{position:absolute;top:0;bottom:0;width:100%;}" +
                    ".popup-content{font-family:sans-serif;max-width:200px;}" +
                    ".popup-content h4{margin:0 0 4px;font-size:14px;color:#2E7D32;}" +
                    ".popup-content p{margin:0 0 6px;font-size:12px;color:#666;}" +
                    ".popup-btn{display:block;width:100%;padding:8px;background:#2E7D32;color:white;border:none;border-radius:8px;font-size:13px;cursor:pointer;}" +
                    "</style></head><body>" +
                    "<div id='map'></div>" +
                    "<script>" +
                    "goongjs.accessToken='POdfxjueKxYZ09MjbKKLCKkxeNLfhFpsXaOfT3Rn';" +
                    "var map=new goongjs.Map({" +
                    "container:'map'," +
                    "style:'https://tiles.goong.io/assets/goong_map_web.json?api_key=POdfxjueKxYZ09MjbKKLCKkxeNLfhFpsXaOfT3Rn'," +
                    $"center:[{lng},{lat}]," +
                    "zoom:14" +
                    "});" +
                    "map.on('load',function(){" +
                    "var userEl=document.createElement('div');" +
                    "userEl.style.cssText='width:18px;height:18px;background:#2196F3;border:3px solid white;border-radius:50%;box-shadow:0 2px 8px rgba(0,0,0,0.4)';" +
                    $"new goongjs.Marker({{element:userEl}}).setLngLat([{lng},{lat}]).setPopup(new goongjs.Popup().setHTML('<b>{userLocText}</b>')).addTo(map);" +
                    $"var places={poiJs};" +
                    "places.forEach(function(p){" +
                    "var el=document.createElement('div');" +
                    "el.style.cssText='width:36px;height:36px;background:#2E7D32;border:3px solid white;border-radius:50%;display:flex;align-items:center;justify-content:center;box-shadow:0 2px 8px rgba(0,0,0,0.4);cursor:pointer;';" +
                    "el.innerHTML='<span style=\"color:white;font-size:18px\">📍</span>';" +
                    "var popup=new goongjs.Popup({offset:34}).setHTML(" +
                    "'<div class=\"popup-content\">'+" +
                    "'<h4>'+p.name+'</h4>'+" +
                    "'<p>'+p.category+'</p>'+" +
                    $"'<button class=\"popup-btn\" onclick=\"window.location=\\'app://detail?id='+p.id+'\\'\">{{viewDetailText}}</button>'+" +
                    "'</div>');" +
                    "new goongjs.Marker({element:el}).setLngLat([p.lng,p.lat]).setPopup(popup).addTo(map);" +
                    "});" +
                    "});" +
                    "</script></body></html>";

                // Fix string interpolation cho viewDetailText trong JS
                html = html.Replace("{viewDetailText}", viewDetailText);

                mapWeb.Source = new HtmlWebViewSource { Html = html };
                mapWeb.Navigating += MapWeb_Navigating;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi tai ban do: {ex.Message}");
                await DisplayAlert(L["Common_Error"], L["Map_ErrorLoad"], L["Common_OK"]);
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private string BuildPOIJson(List<POISummaryDto> pois)
        {
            if (pois == null || !pois.Any()) return "[]";
            var items = pois.Select(p =>
                "{" +
                $"id:'{p.Id}'," +
                $"name:'{Esc(p.Name)}'," +
                $"category:'{Esc(p.Category)}'," +
                $"lat:{p.Latitude.ToString(CultureInfo.InvariantCulture)}," +
                $"lng:{p.Longitude.ToString(CultureInfo.InvariantCulture)}" +
                "}");
            return "[" + string.Join(",", items) + "]";
        }

        private string Esc(string? s) =>
            s?.Replace("'", "\\'").Replace("\n", " ").Replace("\r", "") ?? "";

        private async void MapWeb_Navigating(object? sender, WebNavigatingEventArgs e)
        {
            if (!e.Url.StartsWith("app://detail")) return;
            e.Cancel = true;

            var uri = new Uri(e.Url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var id = query["id"];

            if (!string.IsNullOrEmpty(id))
            {
                Console.WriteLine($"[log] - Mo chi tiet POI tu ban do: {id}");
                await Shell.Current.GoToAsync(nameof(POIDetailPage),
                    new Dictionary<string, object> { { "PoiId", id } });
            }
        }
    }
}
