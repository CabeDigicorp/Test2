#nullable disable
namespace JoinApi.Settings
{
    public class MongoSettings
    {
        public string ConnectionURI { get; set; }
        public string DatabaseName { get; set; }
        public bool UseTls { get; set; }
        public bool ResetDatabase { get; set; }
        public string AdminUser { get; set; }
        public string AdminPassword { get; set; }
        public bool CreateAdminUserOnStartup { get; set; }
        public string CaClientPath { get; set; }
        public string KeyClientPath { get; set; }
    }
}
