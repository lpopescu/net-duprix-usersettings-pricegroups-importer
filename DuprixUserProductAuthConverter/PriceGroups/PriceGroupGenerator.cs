using System;
using System.Collections.Generic;
using System.Linq;

using log4net;

using PriceGroupWebservice.Dto;

namespace UserGroupsCsvToJson
{
    public class PriceGroupGenerator
    {
        private readonly PriceGroupStore _priceGroupStore;
        private readonly ILog _logger;
        private readonly ProductTypeStore _productTypeStore;

        public PriceGroupGenerator(ProductTypeStore productTypeStore, PriceGroupStore priceGroupStore, ILog logger)
        {
            _productTypeStore = productTypeStore;
            _priceGroupStore = priceGroupStore;
            _logger = logger;
        }

        public IEnumerable<PriceGroupDto> Generate(IEnumerable<PriceGroupRawDto> priceGroupsRawDtos)
        {
            var priceGroupRawList = FillProductTypes(priceGroupsRawDtos.ToList());
            var groupedPriceGroups = priceGroupRawList
                .GroupBy(p =>
                {
                    if(p.ProductType == null)
                    {
                        _logger.Warn($"The product {p.ProductId}-{p.Name} does not have a product type.");
                        return new { p.Name, ProductTypeId = -1, p.PriceRuleId };
                    }
                    return new {p.Name, ProductTypeId = p.ProductType.Id, p.PriceRuleId};
                });

                var priceGroups = groupedPriceGroups.Select(g =>
                {
                    string productTypeName = null;
                    PriceRuleDto priceRule = null;
                    var subsidiaries = g.SelectMany(p => p.Subsidiaries).Distinct();
                    try
                    {
                        productTypeName = _productTypeStore.GetProductTypeName(g.Key.ProductTypeId);
                        priceRule = _priceGroupStore.GetPriceRule(g.Key.PriceRuleId);

                        return new PriceGroupDto
                               {
                                   Name = g.Key.Name,
                                   Subsidiaries = subsidiaries,
                                   Products = g.Select(pg => pg.ProductId).ToList(),
                                   ProductType = new ProductTypeDto
                                                 {
                                                     Id = g.Key.ProductTypeId,
                                                     Name = productTypeName
                                                 },
                                   PriceRule = priceRule
                               };
                    }
                    catch(Exception ex)
                    {
                        _logger.Error($"{ex.Message} type: {g.Key.ProductTypeId} {g.Key.Name}, subsidiary {string.Join( "|", subsidiaries)}");
                    }

                    return null;
                }).Where(g => g != null).ToArray();

            return priceGroups;
        }

        private IEnumerable<PriceGroupRawDto> FillProductTypes(List<PriceGroupRawDto> priceGroupsRawDtos)
        {
            var productIds = priceGroupsRawDtos.Select(p => p.ProductId);
            var productTypeProductDtos = _productTypeStore.GetProductTypesFor(productIds);
            
            foreach (var productTypeProductDto in productTypeProductDtos)
            {
                var priceGroupRaw = priceGroupsRawDtos.First(pg => pg.ProductId == productTypeProductDto.ProductId);
                priceGroupRaw.ProductType = productTypeProductDto.ProductType;
                yield return priceGroupRaw;
            }
        }
    }
}