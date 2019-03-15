using UnityEngine.Networking;

namespace SmartHotelMR
{
    public static class UnityWebRequestExtensions
    {
        public static void AddApiKeyHeader(this UnityWebRequest request, string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey))
                request.SetRequestHeader("X-API-KEY", apiKey);
        }
    }
}
