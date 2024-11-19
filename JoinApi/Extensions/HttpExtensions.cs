namespace JoinApi.Extensions
{
    public static class HttpExtensions
    {

        public static void AddMetadataInHeader(this HttpResponse httpResponse, string key, string value)
        {
            httpResponse.Headers.Add("X-Join-Metadata-" + key, value);
        }

    }
}
