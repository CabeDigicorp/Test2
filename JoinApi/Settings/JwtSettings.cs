#nullable disable
namespace JoinApi.Settings
{
    public class JwtSettings
    {
        public string ValidAudience { get; set; }
        public string ValidIssuer { get; set; }
        public string SecretKey { get; set; }
    }
}
