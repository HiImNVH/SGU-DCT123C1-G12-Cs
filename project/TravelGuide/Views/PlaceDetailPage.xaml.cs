using TravelGuide.Models;
using TravelGuide.Services;

namespace TravelGuide;

[QueryProperty(nameof(Place), "Place")]
[QueryProperty(nameof(PlaceName), "PlaceName")]
public partial class PlaceDetailPage : ContentPage
{
    private readonly INarrationService _narrationService;
    private readonly IAuthService _authService;
    private readonly IPOIService _poiService;
    private POI _currentPOI;
    private int _selectedRating = 0;

    public PlaceDetailPage(
        INarrationService narrationService,
        IAuthService authService,
        IPOIService poiService)
    {
        InitializeComponent();
        _narrationService = narrationService;
        _authService = authService;
        _poiService = poiService;
    }

    public POI Place
    {
        set { _currentPOI = value; BindingContext = value; }
    }

    public string PlaceName
    {
        set { Title = value; }
    }

    // TTS-01
    private async void OnPlayNarrationClicked(object sender, EventArgs e)
    {
        if (_currentPOI == null) return;

        var user = _authService.GetCurrentUser();
        var lang = user?.PreferredLanguage ?? "vi";

        var content = _currentPOI.Contents?
            .FirstOrDefault(c => c.LanguageCode == lang)
            ?? _currentPOI.Contents?.FirstOrDefault();

        var text = content?.NarrationText ?? _currentPOI.Description;

        if (string.IsNullOrEmpty(text))
        {
            await DisplayAlert("Thông báo", "Chưa có nội dung thuyết minh", "OK");
            return;
        }

        PlayBtn.IsEnabled = false;
        StopBtn.IsEnabled = true;
        await _narrationService.SpeakAsync(text, lang);
        PlayBtn.IsEnabled = true;
        StopBtn.IsEnabled = false;
    }

    // TTS-03
    private async void OnStopNarrationClicked(object sender, EventArgs e)
    {
        await _narrationService.StopAsync();
        PlayBtn.IsEnabled = true;
        StopBtn.IsEnabled = false;
    }

    // Chọn số sao
    private void OnStarClicked(object sender, EventArgs e)
    {
        if (sender is Button btn &&
            int.TryParse(btn.CommandParameter?.ToString(), out int star))
        {
            _selectedRating = star;
            // Highlight sao đã chọn
            var stars = new[] { Star1, Star2, Star3, Star4, Star5 };
            for (int i = 0; i < stars.Length; i++)
                stars[i].Opacity = i < star ? 1.0 : 0.3;
        }
    }

    // Gửi đánh giá
    private async void OnSubmitRatingClicked(object sender, EventArgs e)
    {
        if (_selectedRating == 0)
        {
            await DisplayAlert("Thông báo", "Vui lòng chọn số sao", "OK");
            return;
        }

        if (_currentPOI == null) return;

        SubmitRatingBtn.IsEnabled = false;

        var success = await _poiService.RateAsync(
            _currentPOI.Id, _selectedRating, CommentEntry?.Text);

        SubmitRatingBtn.IsEnabled = true;

        if (success)
        {
            RatingMessage.Text = "✅ Cảm ơn bạn đã đánh giá!";
            RatingMessage.IsVisible = true;

            // Reload POI để cập nhật rating mới
            var updated = await _poiService.GetByIdAsync(_currentPOI.Id);
            if (updated != null)
            {
                _currentPOI = updated;
                BindingContext = updated;
            }
        }
        else
        {
            await DisplayAlert("Lỗi", "Không thể gửi đánh giá", "OK");
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _narrationService.StopAsync();
    }
}