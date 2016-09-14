using System.Collections.Generic;
using System.IO;

using log4net;

using Newtonsoft.Json;

using PriceGroupWebservice.Dto;

namespace UserGroupsCsvToJson
{
    public class PriceGroupsParser
    {
        private const string JSON_OUTPUT_FILE_NAME = "pricegroups.json";
        private readonly ILog _logger;
        private readonly PriceGroupStore _priceGroupStore;

        public PriceGroupsParser(PriceGroupStore priceGroupStore, ILog logger)
        {
            _priceGroupStore = priceGroupStore;
            _logger = logger;
        }

        public void Export<T>(IEnumerable<T> priceGroups, string directoryName, string alternativeFileName = null)
        {
            string jsonOutput = JsonConvert.SerializeObject(priceGroups, Formatting.Indented);
            string outputPath = Path.Combine(directoryName ?? "", alternativeFileName ?? JSON_OUTPUT_FILE_NAME);

            using(var fs = File.CreateText(outputPath))
                fs.Write(jsonOutput);

            _logger.Info($"File was successfully parsed. Output: {outputPath}");
        }

        public void Upload(IEnumerable<PriceGroupDto> priceGroups)
        {
            foreach(var priceGroup in priceGroups)
            {
                var result = _priceGroupStore.Save(priceGroup);
                if(result.Success)
                    _logger.Info($"saved settings for {priceGroup.Name}");
                else
                {
                    _logger.Error(
                        $"failed to save price group '{priceGroup.Name}' for product type {priceGroup.ProductType.Name} and subsidiary {string.Join("|", priceGroup.Subsidiaries)}.  REASON: {result.FailureReason}");
                }
            }
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