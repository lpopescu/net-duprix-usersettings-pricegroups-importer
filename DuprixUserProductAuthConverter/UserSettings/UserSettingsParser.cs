using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ApplicationSettingsWebservice.Dto.Duprix;

using Newtonsoft.Json;

namespace UserGroupsCsvToJson
{
    public class UserSettingsParser
    {
        private readonly UserSettingsStore _userSettingsStore;
        private readonly UserSettingsGenerator _userSettignsGenerator;
        private const string JSON_OUTPUT_FILE_NAME = "output.json";

        public UserSettingsParser(UserSettingsStore userSettingsStore)
        {
            _userSettingsStore = userSettingsStore;
        }

        public void Export(IEnumerable<DuprixSettingsDto> userSettingsList, string directoryName )
        {
            string jsonOutput = JsonConvert.SerializeObject(userSettingsList, Formatting.Indented);
            string outputPath = Path.Combine(directoryName ?? "", JSON_OUTPUT_FILE_NAME);

            using (var fs = File.CreateText(outputPath))
                fs.Write(jsonOutput);

            Console.WriteLine("File was successfully parsed. Output: {0}", outputPath);

            string sampleJsonOutput = JsonConvert.SerializeObject(userSettingsList.Take(2), Formatting.Indented);
            Console.WriteLine("Output sample: {0}", sampleJsonOutput);  
        }

        public void UploadSettings(IEnumerable<DuprixSettingsDto> userSettingsList)
        {
            foreach (var userSettings in userSettingsList)
            {
                var result = _userSettingsStore.Update(userSettings);
                if (result.Success)
                    Console.WriteLine($"saved settings for {userSettings.UserName}");
                else
                {
                    Console.WriteLine(
                        $"failed to save settings for {userSettings.UserName}.  REASON: {result.FailureReason}");
                }
            }
        }

        public IEnumerable<UserSettingsRawDto> Parse(string filePath, bool isFirstLineHeader)
        {
            var userProductsAuth = new List<UserSettingsRawDto>();

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