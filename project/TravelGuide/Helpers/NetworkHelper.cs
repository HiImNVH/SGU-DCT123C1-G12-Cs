// Helpers/NetworkHelper.cs
namespace TravelGuide.Helpers
{
    public static class NetworkHelper
    {
        public static bool IsConnected =>
            Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
    }
}
