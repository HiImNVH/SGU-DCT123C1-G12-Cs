using Microsoft.Maui.Controls;

namespace TravelGuide.Views
{
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
<meta charset='utf-8' />
<meta name='viewport' content='initial-scale=1,maximum-scale=1,user-scalable=no' />

<script src='https://cdn.jsdelivr.net/npm/@goongmaps/goong-js@1.0.9/dist/goong-js.js'></script>
<link href='https://cdn.jsdelivr.net/npm/@goongmaps/goong-js@1.0.9/dist/goong-js.css' rel='stylesheet' />

<style>
body { margin: 0; padding: 0; }
#map { position: absolute; top: 0; bottom: 0; width: 100%; }
</style>
</head>

<body>

<div id='map'></div>

<script>

goongjs.accessToken = 'POdfxjueKxYZ09MjbKKLCKkxeNLfhFpsXaOfT3Rn';

var map = new goongjs.Map({
    container: 'map',
    style: 'https://tiles.goong.io/assets/goong_map_web.json',
    center: [106.700806, 10.776889],
    zoom: 10
});

</script>

</body>
</html>";

            mapWeb.Source = new HtmlWebViewSource
            {
                Html = html
            };
            mapWeb.HandlerChanged += (s, e) =>
            {
#if ANDROID
                if (mapWeb.Handler.PlatformView is Android.Webkit.WebView webView)
                {
                    webView.Settings.JavaScriptEnabled = true;
                    webView.Settings.DomStorageEnabled = true;
                }
#endif
            };
        }
    }
}