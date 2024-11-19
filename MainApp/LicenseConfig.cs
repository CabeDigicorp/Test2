using LicenseSpring;

internal static class Config
{
    // Provide your LicenseSpring credentials here, please keep them safe
    //private static readonly string ApiKey = "d925fe93-3b15-4922-a677-e49e4504bab7";
    //private static readonly string SharedKey = "UQmBKgaoR9gmc36Nou8hb-Ab7VmUxeWRQQcKGbppwos";
    //private static readonly string ProductCode = "jn";

    //JoinByKey
    private static readonly string ApiKey = "d925fe93-3b15-4922-a677-e49e4504bab7";
    private static readonly string SharedKey = "UQmBKgaoR9gmc36Nou8hb-Ab7VmUxeWRQQcKGbppwos";
    private static readonly string ProductCode = "jnk";//"jnu";

    public static LicenseSpringConfiguration CreateLicenseSpringConfiguration()
    {
        string licenseFilePath = ""; // use default license file path, but you can provide your desired path
        string hardwareID = ""; // use default HardwareID, but you can calculate device fingerprint by your own

        var extOptions = CreateExtendedOptions( licenseFilePath, hardwareID );

        var configuration = new LicenseSpringConfiguration(
            apiKey: ApiKey,
            sharedKey: SharedKey,
            productCode: ProductCode,
            appName: System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString(),
            appVersion: System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
            extendedOptions: extOptions );

        return configuration;
    }

    private static LicenseSpringExtendedOptions CreateExtendedOptions( string licenseFilePath, string hardwareID, bool enableLogging = false )
    {
        // Attention: license logging is private and might be useful for developers, but it should BE DISABLED on end user computers!
        var configurationFactory = new LicenseSpringConfigurationFactory();
        return configurationFactory.CreateExtendedOptions( licenseFilePath, hardwareID, enableLogging ); // see also other arguments
    }
}
