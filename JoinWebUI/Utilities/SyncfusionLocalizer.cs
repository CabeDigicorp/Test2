using Syncfusion.Blazor;

namespace JoinWebUI.Utilities
{
        public class SyncfusionLocalizer : ISyncfusionStringLocalizer
    {
        public string GetText(string key)
        {
            return this.ResourceManager.GetString(key);
        }

        public System.Resources.ResourceManager ResourceManager
        {
            get
            {
                // Replace the ApplicationNamespace with your application name.
                return JoinWebUI.Resources.SfResources.ResourceManager;
            }
        }
    }
}
