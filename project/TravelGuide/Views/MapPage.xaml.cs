// Views/MapPage.xaml.cs
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using TravelGuide.Models.DTOs;
using TravelGuide.Services;

namespace TravelGuide.Views
{
    public partial class MapPage : ContentPage
    {
        private readonly POIDataService _poiData;
        private static LocalizationService L => LocalizationService.Instance;

        // ── State ────────────────────────────────────────────────────
        private double _userLat = 10.776889;
        private double _userLng = 106.700806;
        private bool _isNavigating = false;
        private string _navMode = "walking";   // walking | motorcycle
        private POISummaryDto? _navTarget;
        private List<RouteStep> _steps = new();
        private int _currentStep = 0;
        private CancellationTokenSource? _navCts;

        // ── Goong API key ─────────────────────────────────────────────
        // Dùng chung key với map tiles
        private const string GoongMaptileKey = "POdfxjueKxYZ09MjbKKLCKkxeNLfhFpsXaOfT3Rn";
        private const string GoongApiKey = "EP7ZJYCiahp2hKdjd7U8PJ7cvpD02sMqYVHr4cvS";
        private static readonly HttpClient _http = new();

        public MapPage(POIDataService poiData)
        {
            InitializeComponent();
            _poiData = poiData;

            L.PropertyChanged += (_, _) =>
                MainThread.BeginInvokeOnMainThread(RefreshUIText);
        }

        // ── Lifecycle ────────────────────────────────────────────────

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            RefreshUIText();
            Console.WriteLine("[log] - Mo trang Ban do");
            await LoadMapAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopNavigation();
        }

        private void RefreshUIText()
        {
            Title = L["Map_Title"];
        }

        // ── Load bản đồ ───────────────────────────────────────────────

        private async Task LoadMapAsync()
        {
            try
            {
                var pois = await _poiData.GetAllActiveAsync();
                Console.WriteLine($"[info] - Hien thi {pois.Count} POI tren ban do");

                // Lấy vị trí GPS
                try
                {
                    var loc = await Geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Best,
                        Timeout = TimeSpan.FromSeconds(6)
                    });
                    if (loc != null)
                    {
                        _userLat = loc.Latitude;
                        _userLng = loc.Longitude;
                    }
                }
                catch { Console.WriteLine("[warn] - Khong lay duoc GPS"); }

                var lat = _userLat.ToString(CultureInfo.InvariantCulture);
                var lng = _userLng.ToString(CultureInfo.InvariantCulture);
                var poiJs = BuildPOIJson(pois);

                var viewDetailText = L["Map_ViewDetail"];
                var navigateText = L["Map_Navigate"];
                var userLocText = L["Map_UserLocation"];

                // ── HTML với Goong JS + dẫn đường ────────────────────
                string html = $@"
<!DOCTYPE html>
<html>
<head>
<meta name='viewport' content='width=device-width,initial-scale=1,minimum-scale=0.5,maximum-scale=10,user-scalable=yes'>
<script src='https://cdn.jsdelivr.net/npm/@goongmaps/goong-js@1.0.9/dist/goong-js.js'></script>
<link  href='https://cdn.jsdelivr.net/npm/@goongmaps/goong-js@1.0.9/dist/goong-js.css' rel='stylesheet'/>
<style>
  body,html {{ margin:0;padding:0;height:100%; }}
  #map {{ position:absolute;top:0;bottom:0;width:100%; }}

  /* Popup */
  .popup {{ font-family:sans-serif;min-width:180px; }}
  .popup h4 {{ margin:0 0 4px;font-size:15px;color:#1B5E20;font-weight:700; }}
  .popup p  {{ margin:0 0 8px;font-size:12px;color:#666; }}
  .popup-btns {{ display:flex;gap:6px; }}
  .btn-detail  {{ flex:1;padding:8px 4px;background:#2E7D32;color:white;border:none;border-radius:8px;font-size:12px;cursor:pointer; }}
  .btn-nav     {{ flex:1;padding:8px 4px;background:#1976D2;color:white;border:none;border-radius:8px;font-size:12px;cursor:pointer; }}

  /* Route line sẽ được thêm qua Goong layer */
  .nav-step-marker {{
    background:#1976D2;color:white;border:2px solid white;
    border-radius:50%;width:28px;height:28px;
    display:flex;align-items:center;justify-content:center;
    font-size:14px;box-shadow:0 2px 6px rgba(0,0,0,0.3);
  }}
</style>
</head>
<body>
<div id='map'></div>
<script>
  goongjs.accessToken = '{GoongMaptileKey}'

  var userLat = {lat};
  var userLng = {lng};

  var map = new goongjs.Map({{
    container: 'map',
    style: 'https://tiles.goong.io/assets/goong_map_web.json?api_key={GoongMaptileKey}',
    center: [userLng, userLat],
    zoom: 16
  }});

  // ── Marker người dùng ──────────────────────────────────────
  var userEl = document.createElement('div');
  userEl.style.cssText = 'width:20px;height:20px;background:#2196F3;border:3px solid white;border-radius:50%;box-shadow:0 2px 8px rgba(33,150,243,0.5)';
  var userMarker = new goongjs.Marker({{element:userEl}})
    .setLngLat([userLng,userLat])
    .setPopup(new goongjs.Popup().setHTML('<b>📍 {userLocText}</b>'))
    .addTo(map);

  // ── POI markers ────────────────────────────────────────────
  var places = {poiJs};
  var popups = {{}};

  places.forEach(function(p) {{
    var el = document.createElement('div');
    el.style.cssText = 'width:38px;height:38px;background:#2E7D32;border:3px solid white;border-radius:50%;display:flex;align-items:center;justify-content:center;box-shadow:0 3px 10px rgba(0,0,0,0.3);cursor:pointer;font-size:18px';
    el.innerHTML = '📍';

    var popup = new goongjs.Popup({{offset:36, maxWidth:'220px'}}).setHTML(
      '<div class=""popup""><h4>' + p.name + '</h4>' +
      '<p>' + p.category + '</p>' +
      '<div class=""popup-btns"">' +
        '<button class=""btn-detail"" onclick=""openDetail(\'' + p.id + '\')""><b>{viewDetailText}</b></button>' +
        '<button class=""btn-nav""    onclick=""startNav(\'' + p.id + '\',' + p.lat + ',' + p.lng + ',\'' + p.name + '\')"">🧭 {navigateText}</button>' +
      '</div></div>'
    );

    popups[p.id] = popup;
    new goongjs.Marker({{element:el}})
      .setLngLat([p.lng, p.lat])
      .setPopup(popup)
      .addTo(map);
  }});

  // ── Route layer ────────────────────────────────────────────
  map.on('load', function() {{
    map.addSource('route', {{
      type: 'geojson',
      data: {{ type:'Feature', geometry:{{ type:'LineString', coordinates:[] }} }}
    }});

    // Viền trắng (outline) để nổi bật
    map.addLayer({{
      id: 'route-outline',
      type: 'line',
      source: 'route',
      layout: {{ 'line-join':'round', 'line-cap':'round' }},
      paint: {{ 'line-color':'#FFFFFF', 'line-width':8, 'line-opacity':0.8 }}
    }});

    // Đường route màu xanh
    map.addLayer({{
      id: 'route-line',
      type: 'line',
      source: 'route',
      layout: {{ 'line-join':'round', 'line-cap':'round' }},
      paint: {{ 'line-color':'#1976D2', 'line-width':5, 'line-opacity':0.9 }}
    }});

    // Mũi tên chỉ hướng dọc theo đường
    map.addLayer({{
      id: 'route-arrows',
      type: 'symbol',
      source: 'route',
      layout: {{
        'symbol-placement':'line',
        'symbol-spacing':80,
        'icon-image':'arrow',
        'icon-rotation-alignment':'map',
        'icon-allow-overlap':true
      }}
    }});
  }});

  // Step markers container
  var stepMarkers = [];

  // ── Hàm vẽ route từ coordinates ───────────────────────────
  function drawRoute(coordinates) {{
    map.getSource('route').setData({{
      type: 'Feature',
      geometry: {{ type:'LineString', coordinates:coordinates }}
    }});
  }}

  // ── Vẽ step markers ────────────────────────────────────────
  function drawStepMarkers(steps) {{
    // Xoá markers cũ
    stepMarkers.forEach(function(m) {{ m.remove(); }});
    stepMarkers = [];

    steps.forEach(function(step, i) {{
      var el = document.createElement('div');
      el.className = 'nav-step-marker';
      el.textContent = (i+1);
      var m = new goongjs.Marker({{element:el}})
        .setLngLat([step.lng, step.lat])
        .addTo(map);
      stepMarkers.push(m);
    }});
  }}

  // ── Xoá route ──────────────────────────────────────────────
  function clearRoute() {{
    if (map.getSource('route')) {{
      map.getSource('route').setData({{
        type:'Feature',
        geometry:{{ type:'LineString', coordinates:[] }}
      }});
    }}
    stepMarkers.forEach(function(m) {{ m.remove(); }});
    stepMarkers = [];
  }}

  // ── Cập nhật vị trí user real-time ────────────────────────
  function updateUserLocation(lat, lng) {{
    userLat = lat; userLng = lng;
    userMarker.setLngLat([lng, lat]);
  }}

  // ── Zoom về vị trí user ────────────────────────────────────
  function locateMe() {{
    map.flyTo({{ center:[userLng, userLat], zoom:17, duration:800 }});
  }}

  // ── Highlight bước hiện tại ────────────────────────────────
  function highlightStep(idx) {{
    if (idx < stepMarkers.length) {{
      var lnglat = stepMarkers[idx].getLngLat();
      map.easeTo({{ center:lnglat, zoom:17, duration:500 }});
    }}
  }}

  // ── Fit bounds để thấy cả route ───────────────────────────
  function fitRouteBounds(coords) {{
    if (!coords || coords.length < 2) return;
    var bounds = coords.reduce(function(b, c) {{ return b.extend(c); }},
      new goongjs.LngLatBounds(coords[0], coords[0]));
    map.fitBounds(bounds, {{ padding:60, duration:1000 }});
  }}

  // ── Deep link handlers ─────────────────────────────────────
  function openDetail(id) {{
    window.location = 'app://detail?id=' + id;
  }}

  function startNav(id, lat, lng, name) {{
    window.location = 'app://navigate?id=' + id + '&lat=' + lat + '&lng=' + lng + '&name=' + encodeURIComponent(name);
  }}
</script>
</body>
</html>";

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

        // ── Xử lý deep link từ WebView ────────────────────────────────

        private async void MapWeb_Navigating(object? sender, WebNavigatingEventArgs e)
        {
            if (!e.Url.StartsWith("app://")) return;
            e.Cancel = true;

            var uri = new Uri(e.Url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

            // app://detail?id=...
            if (e.Url.StartsWith("app://detail"))
            {
                var id = query["id"];
                if (!string.IsNullOrEmpty(id))
                    await Shell.Current.GoToAsync(nameof(POIDetailPage),
                        new Dictionary<string, object> { { "PoiId", id } });
                return;
            }

            // app://navigate?id=...&lat=...&lng=...&name=...
            if (e.Url.StartsWith("app://navigate"))
            {
                var id = query["id"];
                var lat = query["lat"];
                var lng = query["lng"];
                var name = query["name"];

                if (!string.IsNullOrEmpty(id) && double.TryParse(lat, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var destLat)
                    && double.TryParse(lng, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var destLng))
                {
                    var poi = new POISummaryDto
                    {
                        Id = Guid.Parse(id),
                        Name = Uri.UnescapeDataString(name ?? ""),
                        Latitude = destLat,
                        Longitude = destLng
                    };
                    await StartNavigationAsync(poi);
                }
            }
        }

        // ── Navigation logic ──────────────────────────────────────────

        private async Task StartNavigationAsync(POISummaryDto destination)
        {
            Console.WriteLine($"[nav] - Bat dau dan duong den: {destination.Name}");

            _navTarget = destination;
            _isNavigating = true;
            _currentStep = 0;

            // Hiện panel
            NavPanel.IsVisible = true;
            NavDestLabel.Text = destination.Name;
            NavInstructionLabel.Text = "Đang tính đường đi...";
            NavDistanceLabel.Text = "";

            // Gọi Goong Directions API
            var route = await GetRouteAsync(_userLat, _userLng,
                destination.Latitude, destination.Longitude, _navMode);

            if (route == null)
            {
                await DisplayAlert(L["Common_Error"], "Không thể tính đường đi.", L["Common_OK"]);
                StopNavigation();
                return;
            }

            _steps = route.Steps;

            // Vẽ route trên map
            var coords = route.Geometry
                .Select(p => $"[{p.Lng.ToString(CultureInfo.InvariantCulture)},{p.Lat.ToString(CultureInfo.InvariantCulture)}]");
            var coordsJs = "[" + string.Join(",", coords) + "]";

            var stepsJs = "[" + string.Join(",",
                _steps.Select(s =>
                    $"{{lat:{s.Location.Lat.ToString(CultureInfo.InvariantCulture)}," +
                    $"lng:{s.Location.Lng.ToString(CultureInfo.InvariantCulture)}}}")) + "]";

            await mapWeb.EvaluateJavaScriptAsync($"drawRoute({coordsJs})");
            await mapWeb.EvaluateJavaScriptAsync($"drawStepMarkers({stepsJs})");
            await mapWeb.EvaluateJavaScriptAsync($"fitRouteBounds({coordsJs})");

            // Cập nhật tổng hợp
            NavTotalDistLabel.Text = FormatDistance(route.TotalDistanceM);
            NavEtaLabel.Text = ((int)Math.Ceiling(route.TotalDurationS / 60)).ToString();

            // Bắt đầu bước đầu tiên
            ShowStep(0);

            // Bắt đầu theo dõi GPS để cập nhật
            _navCts = new CancellationTokenSource();
            _ = TrackNavigationAsync(_navCts.Token);
        }

        private void ShowStep(int index)
        {
            if (index >= _steps.Count) return;
            _currentStep = index;
            var step = _steps[index];

            NavInstructionLabel.Text = step.Instruction;
            NavDistanceLabel.Text = $"Sau {FormatDistance(step.DistanceM)}";
            DirectionIcon.Text = GetManeuverIcon(step.Maneuver);

            // Pan map đến step nếu gần
            _ = mapWeb.EvaluateJavaScriptAsync($"highlightStep({index})");
        }

        private async Task TrackNavigationAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _isNavigating)
            {
                try
                {
                    var loc = await Geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Best,
                        Timeout = TimeSpan.FromSeconds(5)
                    }, ct);

                    if (loc != null)
                    {
                        _userLat = loc.Latitude;
                        _userLng = loc.Longitude;

                        // Cập nhật vị trí user trên map
                        var latStr = _userLat.ToString(CultureInfo.InvariantCulture);
                        var lngStr = _userLng.ToString(CultureInfo.InvariantCulture);
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                            await mapWeb.EvaluateJavaScriptAsync(
                                $"updateUserLocation({latStr},{lngStr})"));

                        // Kiểm tra đã qua step chưa (trong vòng 15m)
                        await MainThread.InvokeOnMainThreadAsync(() => CheckStepProgress(loc));

                        // Kiểm tra đã đến nơi chưa
                        if (_navTarget != null)
                        {
                            var dist = CalculateDistance(loc.Latitude, loc.Longitude,
                                _navTarget.Latitude, _navTarget.Longitude);

                            // Cập nhật khoảng cách còn lại
                            await MainThread.InvokeOnMainThreadAsync(() =>
                                NavTotalDistLabel.Text = FormatDistance(dist));

                            if (dist < 15)
                            {
                                await MainThread.InvokeOnMainThreadAsync(async () =>
                                {
                                    await DisplayAlert("🎉 Đã đến nơi!",
                                        $"Bạn đã đến {_navTarget.Name}", "OK");
                                    StopNavigation();
                                });
                                break;
                            }
                        }
                    }

                    await Task.Delay(3000, ct);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[nav] - Loi GPS: {ex.Message}");
                    await Task.Delay(5000, ct);
                }
            }
        }

        private void CheckStepProgress(Location userLoc)
        {
            if (_currentStep >= _steps.Count) return;

            var step = _steps[_currentStep];
            var dist = CalculateDistance(userLoc.Latitude, userLoc.Longitude,
                step.Location.Lat, step.Location.Lng);

            // Nếu đã đến gần step hiện tại → chuyển sang step tiếp theo
            if (dist < 15 && _currentStep < _steps.Count - 1)
            {
                ShowStep(_currentStep + 1);
            }
        }

        private void StopNavigation()
        {
            _isNavigating = false;
            _navCts?.Cancel();
            _navCts = null;
            _navTarget = null;
            _steps.Clear();
            _currentStep = 0;

            NavPanel.IsVisible = false;
            _ = mapWeb.EvaluateJavaScriptAsync("clearRoute()");
        }

        // ── Goong Directions API ──────────────────────────────────────

        private async Task<RouteResult?> GetRouteAsync(
            double fromLat, double fromLng,
            double toLat, double toLng,
            string mode)
        {
            try
            {
                // ✅ FIX vehicle
                var vehicle = mode == "walking" ? "bike" : "car";

                var url = $"https://rsapi.goong.io/Direction" +
                          $"?origin={fromLat.ToString(CultureInfo.InvariantCulture)}," +
                          $"{fromLng.ToString(CultureInfo.InvariantCulture)}" +
                          $"&destination={toLat.ToString(CultureInfo.InvariantCulture)}," +
                          $"{toLng.ToString(CultureInfo.InvariantCulture)}" +
                          $"&vehicle={vehicle}" +
                          $"&api_key={GoongApiKey}";

                Console.WriteLine($"[nav] URL: {url}");

                var response = await _http.GetAsync(url);

                // ✅ FIX: check status code
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[nav] HTTP ERROR: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[nav] RESPONSE: {json}");

                // ✅ FIX: check empty response
                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine("[nav] Empty response");
                    return null;
                }

                using var doc = JsonDocument.Parse(json);

                // ✅ FIX: check error field
                if (doc.RootElement.TryGetProperty("error", out var error))
                {
                    Console.WriteLine($"[nav] API ERROR: {error}");
                    return null;
                }

                // ✅ FIX: check routes tồn tại
                if (!doc.RootElement.TryGetProperty("routes", out var routes) ||
                    routes.GetArrayLength() == 0)
                {
                    Console.WriteLine("[nav] No routes found");
                    return null;
                }

                return ParseGoongDirections(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[nav] Exception: {ex.Message}");
                return null;
            }
        }

        private static RouteResult? ParseGoongDirections(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var routes = doc.RootElement.GetProperty("routes");
                if (routes.GetArrayLength() == 0) return null;

                var route = routes[0];
                var legs = route.GetProperty("legs");
                if (legs.GetArrayLength() == 0) return null;

                var leg = legs[0];

                // Tổng khoảng cách và thời gian
                var totalDistM = leg.GetProperty("distance").GetProperty("value").GetDouble();
                var totalDurS = leg.GetProperty("duration").GetProperty("value").GetDouble();

                // Parse các bước
                var steps = new List<RouteStep>();
                var stepsEl = leg.GetProperty("steps");

                foreach (var stepEl in stepsEl.EnumerateArray())
                {
                    var startLoc = stepEl.GetProperty("start_location");
                    var htmlInst = stepEl.TryGetProperty("html_instructions", out var instrEl)
                        ? StripHtml(instrEl.GetString() ?? "")
                        : "";
                    var distM = stepEl.GetProperty("distance").GetProperty("value").GetDouble();
                    var maneuver = stepEl.TryGetProperty("maneuver", out var manEl)
                        ? manEl.GetString() ?? ""
                        : "";

                    steps.Add(new RouteStep
                    {
                        Instruction = htmlInst,
                        DistanceM = distM,
                        Maneuver = maneuver,
                        Location = new LatLng(
                            startLoc.GetProperty("lat").GetDouble(),
                            startLoc.GetProperty("lng").GetDouble())
                    });
                }

                // Decode polyline geometry (overview_polyline)
                var polyline = route.GetProperty("overview_polyline")
                                    .GetProperty("points").GetString() ?? "";
                var geometry = DecodePolyline(polyline);

                return new RouteResult
                {
                    Steps = steps,
                    Geometry = geometry,
                    TotalDistanceM = totalDistM,
                    TotalDurationS = totalDurS
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[nav] - Loi parse route: {ex.Message}");
                return null;
            }
        }

        // ── Button handlers ───────────────────────────────────────────

        private async void OnLocateMeClicked(object sender, EventArgs e)
        {
            try
            {
                var loc = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(5)
                });
                if (loc != null)
                {
                    _userLat = loc.Latitude;
                    _userLng = loc.Longitude;
                    await mapWeb.EvaluateJavaScriptAsync("locateMe()");
                }
            }
            catch { }
        }

        private void OnStopNavigationClicked(object sender, EventArgs e)
            => StopNavigation();

        private async void OnWalkModeClicked(object sender, EventArgs e)
        {
            _navMode = "walking";
            WalkModeBtn.BackgroundColor = Color.FromArgb("#E8F5E9");
            BikeModeBtn.BackgroundColor = Color.FromArgb("#F5F5F5");

            if (_isNavigating && _navTarget != null)
                await StartNavigationAsync(_navTarget);
        }

        private async void OnBikeModeClicked(object sender, EventArgs e)
        {
            _navMode = "motorcycle";
            BikeModeBtn.BackgroundColor = Color.FromArgb("#E8F5E9");
            WalkModeBtn.BackgroundColor = Color.FromArgb("#F5F5F5");

            if (_isNavigating && _navTarget != null)
                await StartNavigationAsync(_navTarget);
        }

        // ── Utilities ─────────────────────────────────────────────────

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

        private static double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371000;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLng = (lng2 - lng1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
                     * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static string FormatDistance(double meters)
        {
            if (meters < 1000) return $"{(int)Math.Round(meters)} m";
            return $"{(meters / 1000):F1} km";
        }

        private static string GetManeuverIcon(string maneuver) => maneuver switch
        {
            "turn-left" => "↰",
            "turn-right" => "↱",
            "turn-sharp-left" => "↺",
            "turn-sharp-right" => "↻",
            "turn-slight-left" => "↖",
            "turn-slight-right" => "↗",
            "uturn-left" => "⬅",
            "uturn-right" => "➡",
            "roundabout-left" => "↺",
            "roundabout-right" => "↻",
            "ramp-left" => "↰",
            "ramp-right" => "↱",
            "merge" => "⬆",
            "straight" => "⬆",
            _ => "▶"
        };

        private static string StripHtml(string html)
        {
            // Loại bỏ HTML tags đơn giản
            var result = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ");
            result = System.Net.WebUtility.HtmlDecode(result);
            return result.Trim();
        }

        /// <summary>Decode Google Encoded Polyline Algorithm</summary>
        private static List<LatLng> DecodePolyline(string encoded)
        {
            var points = new List<LatLng>();
            int index = 0, lat = 0, lng = 0;

            while (index < encoded.Length)
            {
                int b, shift = 0, result = 0;
                do { b = encoded[index++] - 63; result |= (b & 0x1f) << shift; shift += 5; }
                while (b >= 0x20);
                lat += (result & 1) != 0 ? ~(result >> 1) : result >> 1;

                shift = 0; result = 0;
                do { b = encoded[index++] - 63; result |= (b & 0x1f) << shift; shift += 5; }
                while (b >= 0x20);
                lng += (result & 1) != 0 ? ~(result >> 1) : result >> 1;

                points.Add(new LatLng(lat / 1e5, lng / 1e5));
            }
            return points;
        }
    }

    // ── Data models ───────────────────────────────────────────────────

    record LatLng(double Lat, double Lng);

    record RouteStep
    {
        public string Instruction { get; init; } = "";
        public double DistanceM { get; init; }
        public string Maneuver { get; init; } = "";
        public LatLng Location { get; init; } = new(0, 0);
    }

    record RouteResult
    {
        public List<RouteStep> Steps { get; init; } = new();
        public List<LatLng> Geometry { get; init; } = new();
        public double TotalDistanceM { get; init; }
        public double TotalDurationS { get; init; }
    }
}
