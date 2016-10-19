using System.Collections.Generic;
using System.IO;
using System.Linq;

using log4net;

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

            for(int i = 0;i < priceGroups.Count();i++)
            {
                var result = _priceGroupStore.Save(priceGroups.ElementAt(i));
                if(result.Success)
                {
                    updatedPriceGroups.Add(result.Result);
                    _logger.Info($"saved settings for {updatedPriceGroups[i].Name} - {updatedPriceGroups[i].Id}");
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

        public IEnumerable<PriceGroupRawDto> Parse(string filePath, bool isFirstLineHeader)
        {
            var userProductsAuth = new List<PriceGroupRawDto>();

            using(var fs = new FileStream(filePath, FileMode.Open))
            {
                using(var fileReader = new StreamReader(fs))
                {
                    if(isFirstLineHeader)
                        fileReader.ReadLine();

                    while(!fileReader.EndOfStream)
                    {
                        string line = fileReader.ReadLine();
                        if(line != null)
                        {
                            var productIds = line.Split('\t', ',');
                            var priceGroupRawDto = new PriceGroupRawDto();

                            priceGroupRawDto.Parse(productIds);
                            userProductsAuth.Add(priceGroupRawDto);
                        }
                    }
                }
            }
            return userProductsAuth;
        }
    }
}