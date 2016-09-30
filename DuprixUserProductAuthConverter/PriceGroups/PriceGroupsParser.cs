using System.Collections.Generic;
using System.IO;
using System.Linq;

using CsvHelper;

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
            var priceGroupList = priceGroups.ToList();
            for(int i = 0;i < priceGroupList.Count;i++)
            {                
                var result = _priceGroupStore.Save(priceGroupList[i]);
                if(result.Success)
                {
                    priceGroupList[i] = result.Result;
                    _logger.Info($"saved settings for {priceGroupList[i].Name}");
                }
                else
                {
                    _logger.Error(
                        $"failed to save price group '{priceGroupList[i].Name}' for product type {priceGroupList[i].ProductType.Name} and subsidiary {string.Join("|", priceGroupList[i].Subsidiaries)}.  REASON: {result.FailureReason}");
                }
            }

            return priceGroupList;
        }

        public IEnumerable<PriceGroupRawDto> Parse(string filePath, bool isFirstLineHeader)
        {
            var userProductsAuth = new List<PriceGroupRawDto>();

            //using ( var streamReader = new StreamReader(filePath))
            //    using(var csvReader = new CsvReader(streamReader))
            //    {
            //        csvReader.GetRecords<PriceGroupRawDto>();
            //    }            
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (var fileReader = new StreamReader(fs))
                {
                    if (isFirstLineHeader)
                        fileReader.ReadLine();

                    while (!fileReader.EndOfStream)
                    {
                        string line = fileReader.ReadLine();
                        if (line != null)
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