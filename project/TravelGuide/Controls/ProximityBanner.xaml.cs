// Controls/ProximityBanner.xaml.cs
using TravelGuide.Models.DTOs;
using TravelGuide.Services;

namespace TravelGuide.Controls
{
    public partial class ProximityBanner : ContentView
    {
        // ── Events cho parent page xử lý ────────────────────────────
        public event Action<POISummaryDto>? YesClicked;
        public event Action<POISummaryDto>? NoClicked;
        public event Action<POISummaryDto>? Dismissed;

        // ── Config ───────────────────────────────────────────────────
        private const int AUTO_DISMISS_SECONDS = 8; // tự tắt sau 8 giây

        // ── State ────────────────────────────────────────────────────
        private POISummaryDto? _currentPOI;
        private CancellationTokenSource? _autoDismissCts;
        private static LocalizationService L => LocalizationService.Instance;

        public ProximityBanner()
        {
            InitializeComponent();
        }

        // ── Public API ───────────────────────────────────────────────

        /// <summary>
        /// Hiển thị banner với thông tin POI.
        /// Tự động dismiss sau AUTO_DISMISS_SECONDS giây nếu user không tương tác.
        /// </summary>
        public void Show(POISummaryDto poi, double distanceMeters)
        {
            // Hủy timer của banner cũ nếu có
            _autoDismissCts?.Cancel();
            _currentPOI = poi;

            // Cập nhật nội dung
            POINameLabel.Text  = poi.Name;
            DistanceLabel.Text = $"📍 {FormatDistance(distanceMeters)}";
            QuestionLabel.Text = "Bạn có muốn nghe thuyết minh không?";
            YesBtn.Text        = "Có";
            NoBtn.Text         = "Không";

            // Hiển thị với animation slide-in từ trên
            IsVisible = true;
            _ = SlideInAsync();

            // Bắt đầu đếm ngược tự dismiss
            _autoDismissCts = new CancellationTokenSource();
            _ = AutoDismissAsync(_autoDismissCts.Token);
        }

        /// <summary>Ẩn banner ngay lập tức</summary>
        public void Hide()
        {
            _autoDismissCts?.Cancel();
            _ = SlideOutAndHideAsync();
        }

        // ── Handlers ─────────────────────────────────────────────────

        private void OnYesClicked(object sender, EventArgs e)
        {
            _autoDismissCts?.Cancel();
            var poi = _currentPOI;
            _ = SlideOutAndHideAsync();
            if (poi != null) YesClicked?.Invoke(poi);
        }

        private void OnNoClicked(object sender, EventArgs e)
        {
            _autoDismissCts?.Cancel();
            var poi = _currentPOI;
            _ = SlideOutAndHideAsync();
            if (poi != null) NoClicked?.Invoke(poi);
        }

        private void OnDismissClicked(object sender, EventArgs e)
        {
            _autoDismissCts?.Cancel();
            var poi = _currentPOI;
            _ = SlideOutAndHideAsync();
            if (poi != null) Dismissed?.Invoke(poi);
        }

        // ── Auto-dismiss ─────────────────────────────────────────────

        private async Task AutoDismissAsync(CancellationToken ct)
        {
            try
            {
                // Chạy progress bar đếm ngược
                var steps   = AUTO_DISMISS_SECONDS * 10; // cập nhật 10 lần/giây
                var interval = 100; // ms

                for (int i = steps; i >= 0; i--)
                {
                    ct.ThrowIfCancellationRequested();
                    var progress = (double)i / steps;
                    await MainThread.InvokeOnMainThreadAsync(() =>
                        CountdownBar.Progress = progress);
                    await Task.Delay(interval, ct);
                }

                // Hết giờ → dismiss
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var poi = _currentPOI;
                    _ = SlideOutAndHideAsync();
                    if (poi != null) Dismissed?.Invoke(poi);
                });
            }
            catch (OperationCanceledException) { /* user đã tương tác */ }
        }

        // ── Animations ───────────────────────────────────────────────

        private async Task SlideInAsync()
        {
            // Bắt đầu từ trên màn hình (TranslationY âm)
            BannerFrame.TranslationY = -120;
            BannerFrame.Opacity      = 0;

            await Task.WhenAll(
                BannerFrame.TranslateTo(0, 0, 300, Easing.CubicOut),
                BannerFrame.FadeTo(1, 250)
            );
        }

        private async Task SlideOutAndHideAsync()
        {
            await Task.WhenAll(
                BannerFrame.TranslateTo(0, -120, 250, Easing.CubicIn),
                BannerFrame.FadeTo(0, 200)
            );
            IsVisible = false;
            BannerFrame.TranslationY = 0;
            BannerFrame.Opacity      = 1;
        }

        // ── Utils ────────────────────────────────────────────────────

        private static string FormatDistance(double meters)
        {
            if (meters < 1000)
                return $"{(int)Math.Round(meters)} m";
            return $"{(meters / 1000):F1} km";
        }
    }
}
