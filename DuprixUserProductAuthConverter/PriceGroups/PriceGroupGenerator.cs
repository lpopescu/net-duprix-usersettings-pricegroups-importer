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
            var priceGroups = priceGroupsRawDtos
                .GroupBy(p => new {p.Name, p.SubsidiaryId, p.ProductTypeId, p.PriceRuleId})
                .Select(g =>
                {
                    string productTypeName = null;
                    PriceRuleDto priceRule = null;
                    try
                    {
                        productTypeName = _productTypeStore.GetProductTypeName(g.Key.ProductTypeId);
                        priceRule = _priceGroupStore.GetPriceRule(g.Key.PriceRuleId);

                        return new PriceGroupDto
                               {
                                   Name = g.Key.Name,
                                   Subsidiaries = new[] {g.Key.SubsidiaryId},
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
                        _logger.Error($"{ex.Message} type: {g.Key.ProductTypeId} {g.Key.Name}, subsidiary {g.Key.SubsidiaryId}");
                    }

                    return null;
                }).Where(g => g != null).ToArray();

            return priceGroups;
        }
    }
}