using System;
using System.Collections.Generic;
using System.Linq;

using PriceGroupWebservice.Dto;

namespace UserGroupsCsvToJson
{
    public class PriceGroupGenerator
    {
        private readonly PriceGroupStore _priceGroupStore;
        private readonly ProductTypeStore _productTypeStore;

        public PriceGroupGenerator(ProductTypeStore productTypeStore, PriceGroupStore priceGroupStore)
        {
            _productTypeStore = productTypeStore;
            _priceGroupStore = priceGroupStore;
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
                        productTypeName = _productTypeStore.GetProductType(g.Key.ProductTypeId);
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
                        Console.WriteLine(ex.Message);
                    }

                    return null;
                }).Where(g => g != null).ToArray();

            return priceGroups;
        }
    }
}