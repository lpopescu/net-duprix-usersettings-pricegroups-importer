using ApplicationSettingsWebservice.Client;

using AutoMapper;

using CsvHelper;

using log4net;
using log4net.Config;

using Microsoft.Practices.Unity;

using net_product_webservice.Client;

using PriceGroupWebservice.Client;
using PriceGroupWebservice.Dto;

using UserGroupsCsvToJson.PriceGroups;

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
                cfg.CreateMap<PriceGroupDto, AutomationRuleRawDto>()
                   .ForMember(dst => dst.PriceGroupId,
                       opt => opt.MapFrom(src => src.Id))
                   .ForMember(dst => dst.PriceGroupName,
                       opt => opt.MapFrom(src => src.Name))
                   .ForMember(dst => dst.PriceRuleId,
                       opt => opt.MapFrom(src => src.PriceRule.Id))
                       .ForMember(dst => dst.ProductTypeId,
                       opt => opt.MapFrom(src => src.ProductType.Id));

                cfg.CreateMap<AutomationRuleRawDto, AutomationRuleSettingDto>()
                   .ForMember(dst => dst.IsPriceRuleCheckEnabled,
                       opt => opt.MapFrom(src => src.CalculationMethodCheckEnabled))
                   .ForMember(dst => dst.MaximumNegativePriceDifferencePercentage,
                       opt => opt.MapFrom(src => src.MaxPriceDecrease))
                   .ForMember(dst => dst.MaximumPositivePriceDifferencePercentage,
                       opt => opt.MapFrom(src => src.MaxPriceIncrease))
                   .ForMember(dst => dst.MaximumPriceIndex, opt => opt.MapFrom(src => src.MaxPriceIndex))
                   .ForMember(dst => dst.MaximumToppedWeightedSales, opt => opt.MapFrom(src => src.MaxTopWeightedSales))
                   .ForMember(dst => dst.MinimumSalesMarginPercentage, opt => opt.MapFrom(src => src.MinSalesMargin))

                   .ForMember(dst => dst.PriceGroupId, opt => opt.MapFrom(src => src.PriceGroupId));

                cfg.CreateMap<AutomationRuleRawDto, PriceGroupDto>()
                   .ForMember(dst => dst.Id,
                       opt => opt.MapFrom(src => src.PriceGroupId))
                   .ForMember(dst => dst.Name,
                       opt => opt.MapFrom(src => src.PriceGroupName))
                   .ForMember(dst => dst.CostPlus,
                       opt => opt.MapFrom(src => src.CostPlus))
                       .ForMember(dst => dst.MinimumMargin,
                       opt => opt.MapFrom(src => src.MinimumMargin))
                       .ForMember(dst => dst.RoundingRules,
                       opt => opt.MapFrom(src => src.RoundingRules))
                       .ForMember(dst => dst.Subsidiaries,
                       opt => opt.MapFrom(src => src.Subsidiaries));

                cfg.CreateMissingTypeMaps = true;
            });

            Container.RegisterInstance(mapperConfiguration.CreateMapper());
        }
    }
}