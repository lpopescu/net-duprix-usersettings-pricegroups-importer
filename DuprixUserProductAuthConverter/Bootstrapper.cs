using ApplicationSettingsWebservice.Client;

using AutoMapper;

using CsvHelper;

using log4net;
using log4net.Config;

using Microsoft.Practices.Unity;

using net_product_webservice.Client;

using PriceGroupWebservice.Client;
using PriceGroupWebservice.Dto;

namespace UserGroupsCsvToJson
{
    public class Bootstrapper : UnityContainerExtension
    {
        protected override void Initialize()
        {
            XmlConfigurator.Configure();

            var logger = LogManager.GetLogger(typeof(Program));
            Container.RegisterInstance(logger);

            Container.RegisterType<ICsvParser, CsvParser>();
            Container.RegisterType<ICsvReader, CsvReader>();
            Container.RegisterType<ICsvWriter, CsvWriter>();

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
            Container.RegisterType<FileExport>();

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PriceGroupDto, PriceGroupRuleDto>()
                   .ForMember(dst => dst.PriceGroupId,
                       opt => opt.MapFrom(src => src.Id))
                   .ForMember(dst => dst.PriceGroupName,
                       opt => opt.MapFrom(src => src.Name))
                   .ForMember(dst => dst.PriceRuleId,
                       opt => opt.MapFrom(src => src.PriceRule.Id))
                       .ForMember(dst => dst.ProductTypeId,
                       opt => opt.MapFrom(src => src.ProductType.Id));
                cfg.CreateMissingTypeMaps = true;
            });

            Container.RegisterInstance(mapperConfiguration.CreateMapper());
        }
    }
}