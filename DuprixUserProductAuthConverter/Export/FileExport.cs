using System.Collections.Generic;
using System.IO;

using CsvHelper;

using log4net;

using Newtonsoft.Json;

namespace UserGroupsCsvToJson
{
    public class FileExport
    {
        private const string JSON_OUTPUT_FILE_NAME = "pricegroups.json";
        private readonly ILog _logger;

        public FileExport(ILog logger)
        {
            _logger = logger;
        }

        public void ExportToJson<T>(IEnumerable<T> collection, string directoryName, string alternativeFileName = null)
        {
            string jsonOutput = JsonConvert.SerializeObject(collection, Formatting.Indented);
            string outputPath = Path.Combine(directoryName ?? "", alternativeFileName ?? JSON_OUTPUT_FILE_NAME);

            using(var fs = File.CreateText(outputPath))
                fs.Write(jsonOutput);

            _logger.Info($"Data was successfully exported to json. Output: {outputPath}");
        }

        public void ExportToCsv<T>(IEnumerable<T> collection, string directoryName, string fileName)
        {
            string outputPath = Path.Combine(directoryName ?? "", fileName);

            using(var textwriter = new StreamWriter(fileName))
            {
                using(var csvWriter = new CsvWriter(textwriter))
                {
                    csvWriter.Configuration.RegisterClassMap<SubsidiaryMap>();
                    csvWriter.WriteHeader<T>();

                    csvWriter.WriteRecords(collection);
                }
            }

            _logger.Info($"Data was successfully exported to csv. Output: {outputPath}");
        }
    }
}