using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Newtonsoft.Json;

using PriceGroupWebservice.Dto;

namespace UserGroupsCsvToJson
{
    public class PriceGroupsParser
    {
        private readonly PriceGroupStore _priceGroupStore;
        private const string JSON_OUTPUT_FILE_NAME = "pricegroups.json";

        public PriceGroupsParser(PriceGroupStore priceGroupStore)
        {
            _priceGroupStore = priceGroupStore;
        }

        public void Export<T>(IEnumerable<T> priceGroups, string directoryName, string alternativeFileName =null)
        {
            string jsonOutput = JsonConvert.SerializeObject(priceGroups, Formatting.Indented);
            string outputPath = Path.Combine(directoryName ?? "", alternativeFileName ?? JSON_OUTPUT_FILE_NAME);

            using (var fs = File.CreateText(outputPath))
                fs.Write(jsonOutput);

            Console.WriteLine("File was successfully parsed. Output: {0}", outputPath);            
        }    

        public void Upload(IEnumerable<PriceGroupDto> priceGroups)
        {
            foreach (var priceGroup in priceGroups)
            {
                var result = _priceGroupStore.Update(priceGroup);
                if (result.Success)
                    Console.WriteLine($"saved settings for {priceGroup.Name}");
                else
                {
                    Console.WriteLine(
                        $"failed to save price group '{priceGroup.Name}' for product type {priceGroup.ProductType.Name}.  REASON: {result.FailureReason}");
                }
            }
        }

        public IEnumerable<PriceGroupRawDto> Parse(string filePath, bool isFirstLineHeader)
        {
            var userProductsAuth = new List<PriceGroupRawDto>();

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