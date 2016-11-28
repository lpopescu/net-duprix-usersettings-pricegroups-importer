using System.Collections.Generic;
using System.IO;
using System.Linq;

using CsvHelper;

using log4net;

using Microsoft.Practices.ObjectBuilder2;

using PriceGroupWebservice.Dto;

namespace UserGroupsCsvToJson
{
    public class PriceGroupsParser
    {
        private readonly ILog _logger;
        private readonly PriceGroupStore _priceGroupStore;

        public PriceGroupsParser(PriceGroupStore priceGroupStore, ILog logger)
        {
            _priceGroupStore = priceGroupStore;
            _logger = logger;
        }

        public IEnumerable<PriceGroupDto> Upload(IEnumerable<PriceGroupDto> priceGroups)
        {
            var updatedPriceGroups = new List<PriceGroupDto>();

            CheckForPriceGroupsWithSameProduct(priceGroups);

            for (int i = 0;i < priceGroups.Count();i++)
            {                
                var result = _priceGroupStore.Save(priceGroups.ElementAt(i));
                if(result.Success)
                {
                    var newPriceGroup = result.Result;
                    updatedPriceGroups.Add(newPriceGroup);
                    _logger.Info($"saved settings for {newPriceGroup.Name} - {newPriceGroup.Id}");
                }
                else
                {
                    var priceGroup = priceGroups.ElementAt(i);
                    _logger.Error(
                        $"failed to save price group '{priceGroup.Name}' for product type {priceGroup.ProductType.Name} and subsidiary {string.Join("|", priceGroup.Subsidiaries)}.  REASON: {result.FailureReason}");
                }
            }

            return updatedPriceGroups;
        }

        private void CheckForPriceGroupsWithSameProduct(IEnumerable<PriceGroupDto> priceGroups)
        {
            var priceGroupResult = _priceGroupStore.GetAll();
            if(!priceGroupResult.Success)
                _logger.Warn("Could not retrieve price groups to check inconsistencies");
            else
            {
                var existingPriceGroups = priceGroupResult.Result;
                foreach(var existingPriceGroup in existingPriceGroups)
                {
                    priceGroups.ForEach(pg =>
                    {
                        IEnumerable<int> repeatedProducts = existingPriceGroup.Products.Where(p => pg.Products.Contains(p));
                        if(repeatedProducts.Any())
                        {
                            _logger.Warn(
                                $"The price groups {pg.Id} - {pg.Name} and {existingPriceGroup.Id} - {existingPriceGroup.Name} contain same products" +
                                $" {string.Join(",", repeatedProducts)}");
                        }
                    });
                }
            }
        }

        public IEnumerable<PriceGroupRawDto> Parse(string filePath, string[] priceGroupsToFilter)
        {
            var priceGroupRawDtos = new List<PriceGroupRawDto>();
            
            using(var fs = new FileStream(filePath, FileMode.Open))
            {
                using(var fileReader = new StreamReader(fs))
                {
                    var csvReader = new CsvReader(fileReader);
                    while(csvReader.Read())
                    {
                        var priceGroupRawDto = new PriceGroupRawDto();
                        
                        priceGroupRawDto.Parse(csvReader);
                        priceGroupRawDtos.Add(priceGroupRawDto);
                    }                    
                }
            }
            if(priceGroupsToFilter.Any(p => !string.IsNullOrWhiteSpace(p)) )
            {                
                priceGroupRawDtos = priceGroupRawDtos.Where(p => priceGroupsToFilter.Contains(p.Name)).ToList();
            }
            return priceGroupRawDtos;
        }
    }
}