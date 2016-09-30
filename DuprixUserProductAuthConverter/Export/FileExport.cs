using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

using log4net;

using Microsoft.Practices.ObjectBuilder2;

using Newtonsoft.Json;

namespace UserGroupsCsvToJson
{
    public class FileExport
    {
        private readonly ILog _logger;
        private const string JSON_OUTPUT_FILE_NAME = "pricegroups.json";

        public FileExport(ILog logger)
        {
            _logger = logger;            
        }

        public void ExportToJson<T>(IEnumerable<T> collection, string directoryName, string alternativeFileName = null)
        {
            string jsonOutput = JsonConvert.SerializeObject(collection, Formatting.Indented);
            string outputPath = Path.Combine(directoryName ?? "", alternativeFileName ?? JSON_OUTPUT_FILE_NAME);

            using (var fs = File.CreateText(outputPath))
                fs.Write(jsonOutput);

            _logger.Info($"Data was successfully exported to json. Output: {outputPath}");
        }

        public void ExportToCsv<T>(IEnumerable<T> collection, string directoryName, string fileName)
        {
            string outputPath = Path.Combine(directoryName ?? "", fileName);

            using ( var textwriter = new StreamWriter(fileName))
                using(var csvWriter = new CsvWriter(textwriter))
                {                
                    csvWriter.Configuration.RegisterClassMap<SubsidiaryMap>();
                    csvWriter.WriteHeader<T>();

                    csvWriter.WriteRecords(collection);
                }

            _logger.Info($"Data was successfully exported to csv. Output: {outputPath}");
        }
    }

    public class SubsidiaryMap : CsvClassMap<PriceGroupRuleDto>
    {
        public SubsidiaryMap()
        {
            AutoMap();
            Map(m => m.Subsidiaries).TypeConverter<SubsidiaryConverter>();
        }
    }

    public class SubsidiaryConverter : ITypeConverter
    {
        public string ConvertToString(TypeConverterOptions options, object value)
        {
            var intList = (IEnumerable<int>)value;
            return string.Join("|", intList);
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            throw new System.NotImplementedException();
        }

        public bool CanConvertTo(Type type)
        {
            return true;
        }

        public bool CanConvertFrom(Type type)
        {
            throw new System.NotImplementedException();
        }
    }
}