namespace TravelGuide.Core.Enums;

/// <summary>
/// Trang thai phat TTS / audio trong MAUI app.
/// </summary>
public enum PlayerState
{
    /// <summary>Chua phat</summary>
    Idle,

    /// <summary>Dang phat</summary>
    Playing,

    /// <summary>Tam dung phat</summary>
    Paused,

    /// <summary>Dung va reset</summary>
    Stopped
}
