using System.Configuration;

namespace UserGroupsCsvToJson
{
    public class AppSettings
    {
        private const string SETTINGS_WEBSERVICE_URI = "ApplicationSettingsWebserviceUri";
        private const string PRICEGROUP_WEBSERVICE_URI = "PriceGroupWebserviceUri";
        private const string PRODUCT_WEBSERVICE_URI = "ProductWebserviceUri";

        public string SettingsWebserviceUri { get; private set; }
        public string PriceGroupWebserviceUri { get; private set; }
        public string ProductWebserviceUri { get; set; }

        public void Load()
        {
            SettingsWebserviceUri = ConfigurationManager.AppSettings[SETTINGS_WEBSERVICE_URI];
            PriceGroupWebserviceUri = ConfigurationManager.AppSettings[PRICEGROUP_WEBSERVICE_URI];
            ProductWebserviceUri = ConfigurationManager.AppSettings[PRODUCT_WEBSERVICE_URI];
        }
    }
}