using System.Net.Http.Headers;

namespace TravelGuide.AdminWeb.Helpers;

/// <summary>
/// Tao HttpClient kem JWT token cho moi request den API
/// </summary>
public static class ApiHelper
{
    public static HttpClient CreateAuthenticatedClient(IHttpClientFactory factory, string token)
    {
        var client = factory.CreateClient("TravelGuideAPI");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }
}
