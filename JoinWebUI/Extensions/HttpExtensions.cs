using System.Net;
using System.Runtime.CompilerServices;

namespace JoinWebUI.Extensions
{
    public static class HttpExtensions
    {
        public static bool IsSuccessful(this HttpStatusCode httpStatusCode)
        {
            return (int)httpStatusCode is >= 200 and <= 299;
        }
        public static bool IsClientError(this HttpStatusCode httpStatusCode)
        {
            return (int)httpStatusCode is >= 400 and <= 499;
        }
        public static bool IsServerError(this HttpStatusCode httpStatusCode)
        {
            return (int)httpStatusCode is >= 500 and <= 599;
        }
        public static bool IsError(this HttpStatusCode httpStatusCode)
        {
            return (int)httpStatusCode is >= 400 and <= 599;
        }
        public static Dictionary<string,string> GetMetadataInHeader(this HttpResponseMessage httpResponseMessage)
        {
            return httpResponseMessage.Headers
                .Where(h => h.Key.StartsWith("X-Join-Metadata-"))
                .ToDictionary(h => h.Key, h => h.Value.First().Substring("X-Join-Metadata-".Length));
        }
    }
}
