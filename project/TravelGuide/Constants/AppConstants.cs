namespace TravelGuide.Constants
{
    public static class AppConstants
    {
        // Android Emulator: 10.0.2.2 = localhost của máy host
        // Thiết bị thật: đổi thành IP máy tính, ví dụ "http://192.168.1.x:5171"
        public const string BaseUrl          = "http://10.12.244.253:5171";
        // Note: Thay bằng IPv4 của máy host nếu chạy trên thiết bị thật
        // Endpoints khớp với API thực tế
        public const string ApiPoi           = "/api/poi";
        public const string ApiAuth          = "/api/auth/login";
        public const string ApiRegister = "/api/auth/register";
        public const string ApiUserLanguage  = "/api/user/preference/language";

        // Defaults
        public const string DefaultLanguage  = "vi";
        public const string PrefKeyLanguage  = "preferred_language";
        public const string PrefKeyAuthToken = "auth_token";
        public const string PrefKeyUserId    = "user_id";
        public const int    CacheExpireDays  = 7;
    }
}
