using System;
using System.Collections.Generic;
using System.Linq;

using log4net;

using PriceGroupWebservice.Dto;

using UserGroupsCsvToJson.PriceGroups;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson
{
    internal class AutomationRuleGenerator
    {
        private readonly ILog _logger;
        private readonly PriceGroupStore _priceGroupStore;
        private readonly ProductTypeStore _productTypeStore;
        const int defaultPriceRuleId = 3;

        public AutomationRuleGenerator(ProductTypeStore productTypeStore, PriceGroupStore priceGroupStore, ILog logger)
        {
            _productTypeStore = productTypeStore;
            _priceGroupStore = priceGroupStore;
            _logger = logger;
        }

        public List<AutomationRule> Generate(IEnumerable<AutomationRuleRawDto> automationRuleRawDtos)
        {
            List<AutomationRule> automationRules = new List<AutomationRule>();
            var priceGroups = _priceGroupStore.GetAll();

            foreach(var automationRuleRawDto in automationRuleRawDtos)
            {
                IEnumerable<PriceGroupDto> existingPriceGroups
                    =
                    priceGroups.Result.Where(
                        pg =>
                            pg.Name == automationRuleRawDto.PriceGroupName &&
                            pg.Subsidiaries.Contains(automationRuleRawDto.SubsidiaryId)
                            && pg.ProductType.Id == automationRuleRawDto.ProductTypeId);

                if(!existingPriceGroups.Any())
                {
                    var priceGroupDtoResult = ImportIncompletePriceGroup(automationRuleRawDto);

                    if (priceGroupDtoResult.Success)
                    {
                        existingPriceGroups = new[] { priceGroupDtoResult.Result };
                        priceGroups.Result = ConsolidatePriceGroups(priceGroups.Result, existingPriceGroups);
                    }
                }

                if(existingPriceGroups.Any())
                {
                    foreach(var priceGroupDto in existingPriceGroups)
                    {
                        var automationRuleDto = new AutomationRuleSettingDto
                                                {
                                                    IsPriceRuleCheckEnabled =
                                                        automationRuleRawDto.CalculationMethodCheck,
                                                    MaximumNegativePriceDifferencePercentage =
                                                        automationRuleRawDto.MaxPriceDecrease,
                                                    MaximumPositivePriceDifferencePercentage =
                                                        automationRuleRawDto.MaxPriceIncrease,
                                                    MaximumPriceIndex = automationRuleRawDto.MaxPriceIndex,
                                                    MaximumToppedWeightedSales =
                                                        automationRuleRawDto.MaxTopWeightedSales,
                                                    MinimumSalesMarginPercentage =
                                                        automationRuleRawDto.MinSalesMargin,
                                                    PriceGroupId = priceGroupDto.Id
                                                };

                        var automationRule = new AutomationRule
                                             {
                                                 RawDto = automationRuleRawDto,
                                                 SettingDto = automationRuleDto
                                             };
                        automationRules.Add(automationRule);
                    }
                }
                else
                {
                    _logger.Error(
                        $"There is no price group {automationRuleRawDto.PriceGroupName} for buyer {automationRuleRawDto.Buyer}.");
                }
            }

            return automationRules;
        }

        private IEnumerable<PriceGroupDto> ConsolidatePriceGroups(IEnumerable<PriceGroupDto> allPriceGroups, IEnumerable<PriceGroupDto> existingPriceGroups)
        {
            allPriceGroups = allPriceGroups.Concat(existingPriceGroups);
            return allPriceGroups;

        }

        private RepositoryResult<PriceGroupDto> ImportIncompletePriceGroup(AutomationRuleRawDto automationRuleRawDto)
        {

            var productTypeDto = _productTypeStore.GetProductType(automationRuleRawDto.ProductTypeId);

            if(productTypeDto.Success)
            {
                
                var priceGroupRepositoryResult = _priceGroupStore
                    .Save(new PriceGroupDto
                          {
                              Name = automationRuleRawDto.PriceGroupName,
                              ProductType =
                                  new ProductTypeDto
                                  {
                                      Id = productTypeDto.Result.Id,
                                      Name = productTypeDto.Result.Name
                                  },
                              Subsidiaries =
                                  new[] {automationRuleRawDto.SubsidiaryId},
                              PriceRule = _priceGroupStore.GetPriceRule(defaultPriceRuleId)
                          });

                return priceGroupRepositoryResult;
            }

            _logger.Error($"Failed to retrieve product type {automationRuleRawDto.ProductTypeId}");
            throw new Exception($"Failed to retrieve product type {automationRuleRawDto.ProductTypeId}");
        }
    }
}