namespace TravelGuide.Constants
{
    public static class AppConstants
    {
        // Android Emulator: 10.0.2.2 = localhost của máy host
        // Thiết bị thật: đổi thành IP máy tính, ví dụ "http://192.168.1.x:5171"
        public const string BaseUrl         = "http://10.0.2.2:5171";

        // Endpoints
        public const string ApiPoi          = "/api/poi";
        public const string ApiAuth         = "/api/auth/login";
        public const string ApiRegister     = "/api/auth/register";
        public const string ApiUserLanguage = "/api/user/preference/language";

        // Defaults
        public const string DefaultLanguage  = "vi";
        public const string PrefKeyLanguage  = "preferred_language";
        public const string PrefKeyAuthToken = "auth_token";
        public const string PrefKeyUserId    = "user_id";
        public const int    CacheExpireDays  = 7;

        // ── Deep-link / QR ──────────────────────────────────────────
        /// <summary>
        /// Custom URI scheme đăng ký trong AndroidManifest (IntentFilter trên MainActivity).
        /// QR code sẽ chứa URL dạng: travelguide://poi/{poiId}
        /// </summary>
        public const string DeepLinkScheme = "travelguide";

        /// <summary>
        /// GitHub release page — dùng làm fallback khi người dùng chưa cài app.
        /// Trỏ đến trang Releases để tải APK mới nhất.
        /// </summary>
        public const string GitHubReleasesUrl =
            "https://github.com/HiImNVH/SGU-DCT123C1-G12-Cs/releases/latest";

        /// <summary>Tạo deep-link URL cho một POI. Dùng khi sinh QR code.</summary>
        public static string BuildPoiDeepLink(string poiId)
    => $"https://hiimnvh.github.io/SGU-DCT123C1-G12-Cs/qr-redirect.html?poi={poiId}";
    }
}