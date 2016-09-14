﻿using ApplicationSettingsWebservice.Client;

using log4net;
using log4net.Config;

using Microsoft.Practices.Unity;

using net_product_webservice.Client;

using PriceGroupWebservice.Client;

namespace UserGroupsCsvToJson
{
    public class Bootstrapper : UnityContainerExtension
    {
        protected override void Initialize()
        {
            XmlConfigurator.Configure();
            var logger = LogManager.GetLogger(typeof(Program));

            Container.RegisterInstance(logger);

            Container.RegisterInstance(new AppSettings());
            var appSettings = Container.Resolve<AppSettings>();
            appSettings.Load();

            Container.RegisterType<ApplicationSettingsWebserviceClientBootstrapper>(
                new InjectionConstructor(appSettings.SettingsWebserviceUri)
                );
            Container.RegisterType<PriceGroupWebserviceClientBootstrapper>(
                new InjectionConstructor(appSettings.PriceGroupWebserviceUri)
                );
            Container.RegisterType<ProductWebServiceClientBootstrapper>(
                new InjectionConstructor(appSettings.ProductWebserviceUri)
                );
            Container.AddNewExtension<ApplicationSettingsWebserviceClientBootstrapper>();
            Container.AddNewExtension<PriceGroupWebserviceClientBootstrapper>();
            Container.AddNewExtension<ProductWebServiceClientBootstrapper>();
            Container.RegisterType<UserSettingsStore>();
            Container.RegisterType<UserSettingsParser>();
            Container.RegisterType<PriceGroupsParser>();
            Container.RegisterType<PriceGroupStore>();
        }
    }
}