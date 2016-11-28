using System.Collections.Generic;
using System.IO;
using System.Linq;

using ApplicationSettingsWebservice.Dto.Duprix;

using log4net;

using Newtonsoft.Json;

namespace UserGroupsCsvToJson
{
    public class UserSettingsParser
    {
        private const string JSON_OUTPUT_FILE_NAME = "user_settings";
        private const string EXTENSION = ".json";
        private readonly ILog _logger;
        private readonly UserSettingsStore _userSettingsStore;

        public UserSettingsParser(UserSettingsStore userSettingsStore, ILog logger)
        {
            _userSettingsStore = userSettingsStore;
            _logger = logger;
        }

        public void Export(IEnumerable<DuprixSettingsDto> userSettingsList, string directoryName)
        {
            string jsonOutput = JsonConvert.SerializeObject(userSettingsList, Formatting.Indented);
            string userName = userSettingsList.Count() > 1 ? userSettingsList.First().UserName : "";

            var fileName = $"{JSON_OUTPUT_FILE_NAME}_{userName}{EXTENSION}";
            string outputPath = Path.Combine(directoryName ?? "", fileName);

            using(var fs = File.CreateText(outputPath))
                fs.Write(jsonOutput);

            _logger.Info($"File was successfully parsed. Output: {outputPath}");
        }

        public void UploadSettings(IEnumerable<DuprixSettingsDto> userSettingsList)
        {
            foreach(var userSettings in userSettingsList)
            {
                var result = _userSettingsStore.Update(userSettings);
                if(result.Success)
                    _logger.Info($"Saved settings for {userSettings.UserName}");
                else
                {
                    _logger.Info(
                        $"failed to save settings for {userSettings.UserName}.  REASON: {result.FailureReason}");
                }
            }
        }

        public IEnumerable<UserSettingsRawDto> Parse(string filePath, bool isFirstLineHeader)
        {
            var userProductsAuth = new List<UserSettingsRawDto>();

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
                            var userProductAuth = new UserSettingsRawDto();
                            userProductAuth.Parse(productIds);
                            userProductsAuth.Add(userProductAuth);
                        }
                    }
                }
            }
            return userProductsAuth;
        }
    }
}