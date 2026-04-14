using System.ComponentModel;
using TravelGuide.Services;
using TravelGuide.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace TravelGuide.Views;

public partial class ScanPage : ContentPage
{
    private CameraBarcodeReaderView _cameraView;

    private readonly ScanViewModel _vm;

    private bool _isProcessing = false;
    private bool _isActive = false;

    private static LocalizationService L => LocalizationService.Instance;

    public ScanPage(ScanViewModel vm)
    {
        InitializeComponent();

        _vm = vm;
        BindingContext = vm;

        L.PropertyChanged += OnLanguageChanged;
    }

    // =========================
    // LIFECYCLE
    // =========================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _isActive = true;
        _isProcessing = false;

        RefreshUIText();

        await _vm.StartScanningAsync();

        RecreateCameraView();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _isActive = false;

        DestroyCameraView();

        L.PropertyChanged -= OnLanguageChanged;
    }

    // =========================
    // 🔥 CAMERA FIX (SCANBOT)
    // =========================

    private void RecreateCameraView()
    {
        DestroyCameraView();

        _cameraView = new CameraBarcodeReaderView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Options = new BarcodeReaderOptions
            {
                // 🔥 FIX AMBIGUOUS
                Formats = BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false
            }
        };

        _cameraView.BarcodesDetected += OnBarcodesDetected;

        CameraContainer.Children.Add(_cameraView);
    }

    private void DestroyCameraView()
    {
        if (_cameraView != null)
        {
            _cameraView.BarcodesDetected -= OnBarcodesDetected;
            CameraContainer.Children.Remove(_cameraView);
            _cameraView = null;
        }
    }

    // =========================
    // LOCALIZATION (BẮT BUỘC)
    // =========================

    private void OnLanguageChanged(object? sender, PropertyChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(RefreshUIText);
    }

    private void RefreshUIText()
    {
        ScanTitleLabel.Text = L["Scan_Title"];
        ScanInstructionLabel.Text = L["Scan_Instruction"];
        LoadingLabel.Text = L["Loading"];
        RetryBtn.Text = L["Retry"];
        PlayBtn.Text = L["Play"];
        PauseBtn.Text = L["Pause"];
        TTSSectionLabel.Text = L["TTS"];
        ContentSectionLabel.Text = L["Content"];
        ScanAgainBtn.Text = L["Scan_Again"];
        OpenCameraBtn.Text = L["Open_Camera"];
    }

    // =========================
    // SCAN EVENT
    // =========================

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (!_isActive || _isProcessing) return;

        var result = e.Results?.FirstOrDefault();
        if (result == null || string.IsNullOrWhiteSpace(result.Value))
            return;

        _isProcessing = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                DestroyCameraView();

                var poiId = _vm.DecodePoiId(result.Value);

                if (poiId == null)
                {
                    await DisplayAlert("Error", "QR không hợp lệ", "OK");
                    _isProcessing = false;
                    return;
                }

                await Shell.Current.GoToAsync($"{nameof(POIDetailPage)}?PoiId={poiId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _isProcessing = false;
            }
        });
    }

    // =========================
    // BUTTON
    // =========================

    private async void OnScanAgainClicked(object sender, EventArgs e)
    {
        _isProcessing = false;

        await _vm.StartScanningAsync();

        RecreateCameraView();
    }

    private async void OnOpenCameraClicked(object sender, EventArgs e)
    {
        _isProcessing = false;

        await _vm.StartScanningAsync();

        RecreateCameraView();
    }
}