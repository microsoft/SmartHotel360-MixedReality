#if !UNITY_EDITOR
using System.Net.Http;

namespace SmartHotelMR
{
    public static class HttpClientExtensions
    {
        public static void AddApiKeyHeader(this HttpClient client, string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey))
                client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        }
    }
}
#endif