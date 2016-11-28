using System.Collections.Generic;
using System.IO;

using CsvHelper;

using log4net;

using Newtonsoft.Json;

using UserGroupsCsvToJson.PriceGroups;

namespace UserGroupsCsvToJson
{
    public class AutomationRuleParser
    {
        private const string JSON_OUTPUT_FILE_NAME = "automationRules.json";
        private readonly AutomationRuleStore _automationRuleStore;
        private readonly ILog _logger;

        public AutomationRuleParser(AutomationRuleStore automationRuleStore, ILog logger)
        {
            _automationRuleStore = automationRuleStore;
            _logger = logger;
        }

        public void Export<T>(IEnumerable<T> automationRules, string directoryName, string alternativeFileName = null)
        {
            string jsonOutput = JsonConvert.SerializeObject(automationRules, Formatting.Indented);
            string outputPath = Path.Combine(directoryName ?? "", alternativeFileName ?? JSON_OUTPUT_FILE_NAME);

            using(var fs = File.CreateText(outputPath))
                fs.Write(jsonOutput);

            _logger.Info($"File was successfully parsed. Output: {outputPath}");
        }

        public void Upload(IEnumerable<AutomationRule> automationRules)
        {
            foreach(var automationRule in automationRules)
            {
                var result = _automationRuleStore.SaveAsync(automationRule.SettingDto).Result;
                if(result.Success)
                {
                    _logger.Info(
                        $"saved automation rule settings for price group: {automationRule.SettingDto.PriceGroupId}-{automationRule.RawDto.PriceGroupName}, buyer: {automationRule.RawDto.Buyer}");
                }
                else
                {
                    _logger.Error(
                        $"failed to save automation rule settings for price group {automationRule.SettingDto.PriceGroupId}-{automationRule.RawDto.PriceGroupName}," +
                        $" subsidiary {string.Join("|", automationRule.RawDto.Subsidiaries)}, productType: {automationRule.RawDto.ProductTypeId}, buyer: {automationRule.RawDto.Buyer}." +
                        $"  REASON: {result.FailureReason}");
                }
            }
        }

        public IEnumerable<AutomationRuleRawDto> Parse(string filePath, bool isFirstLineHeader)
        {
            var ruleRawDtos = new List<AutomationRuleRawDto>();

            using(var fs = new FileStream(filePath, FileMode.Open))
            {
                using(var fileReader = new StreamReader(fs))
                {
                    var csvReader = new CsvReader(fileReader);
                    while(csvReader.Read())
                    {
                        var automationRuleRawDto = new AutomationRuleRawDto();
                        automationRuleRawDto.Parse(csvReader);
                        ruleRawDtos.Add(automationRuleRawDto);
                    }
                }
            }
            return ruleRawDtos;
        }
    }
}