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
    private readonly DeviceTrackingService _trackingService;
    public ScanPage(ScanViewModel vm, DeviceTrackingService trackingService)
    {
        InitializeComponent();
        _vm = vm;
        _trackingService = trackingService;
        BindingContext = vm;

        L.PropertyChanged += OnLanguageChanged;
    }

    // ── Lifecycle ────────────────────────────────────────────────────
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

    // ── Camera (fix recreate) ────────────────────────────────────────
    private void RecreateCameraView()
    {
        DestroyCameraView();
        _cameraView = new CameraBarcodeReaderView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Options = new BarcodeReaderOptions
            {
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

    // ── Localization ─────────────────────────────────────────────────
    private void OnLanguageChanged(object? sender, PropertyChangedEventArgs e)
        => MainThread.BeginInvokeOnMainThread(RefreshUIText);

    private void RefreshUIText()
    {
        if (ScanTitleLabel != null) ScanTitleLabel.Text = L["Scan_Title"];
        if (ScanInstructionLabel != null) ScanInstructionLabel.Text = L["Scan_Instruction"];
        if (LoadingLabel != null) LoadingLabel.Text = L["Scan_Loading"];
        if (RetryBtn != null) RetryBtn.Text = L["Common_Retry"];
        if (PlayBtn != null) PlayBtn.Text = L["TTS_Play"];
        if (PauseBtn != null) PauseBtn.Text = L["TTS_Pause"];
        if (TTSSectionLabel != null) TTSSectionLabel.Text = L["TTS_Section"];
        if (ContentSectionLabel != null) ContentSectionLabel.Text = L["Content_Section"];
        if (ScanAgainBtn != null) ScanAgainBtn.Text = L["Scan_Again"];
        if (OpenCameraBtn != null) OpenCameraBtn.Text = L["Scan_OpenCamera"];
    }

    // ── Scan event ───────────────────────────────────────────────────
    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (!_isActive || _isProcessing) return;
        var result = e.Results?.FirstOrDefault();
        if (result == null || string.IsNullOrWhiteSpace(result.Value)) return;
        _isProcessing = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                DestroyCameraView();
                var poiId = _vm.DecodePoiId(result.Value);
                if (poiId == null)
                {
                    await DisplayAlert(L["Common_Error"], L["Scan_InvalidQR"], L["Common_OK"]);
                    _isProcessing = false;
                    return;
                }
                await Shell.Current.GoToAsync($"{nameof(POIDetailPage)}?PoiId={poiId}");
                _trackingService.RecordScan();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _isProcessing = false;
            }
        });
    }

    // ── Buttons ──────────────────────────────────────────────────────
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
