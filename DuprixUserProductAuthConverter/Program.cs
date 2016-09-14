using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using log4net;

using Microsoft.Practices.Unity;

using PriceGroupWebservice.Dto;

using UserGroupsCsvToJson.PriceGroups;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson
{
    internal class Program
    {        
        private static void Main(string[] args)
        {
            if(args[0] == null)
            {
                PrintParametersUsage("The parameter was not specified.");
                return;
            }

            string parameter = args[0];
            var container = new UnityContainer();
            container.AddNewExtension<Bootstrapper>();

            if(parameter == "-deletepg")
            {
                DeletePriceGroupCommand(args, container);
            }

            else if(parameter == "-purr")
            {
                //update price groups
                UpdatePriceGroupCommand(container);
            }
            else
            {
                if(args[1] == null)
                {
                    PrintParametersUsage("The file path was not specified.");
                    return;
                }

                var filePath = args[1];
                bool isFirstLineHeader = true;

                if(!File.Exists(filePath))
                    Console.WriteLine("The file path '{0}' does not exist.", filePath);

                var fileInfo = new FileInfo(filePath);

                if(parameter == "-u")
                {
                    //import userSettings
                    ImportUserSettingsCommand(container, filePath, isFirstLineHeader, fileInfo);
                }
                else if(parameter == "-p")
                {
                    //check for duplicate price groups
                    ImportPriceGroupsCommand(container, filePath, isFirstLineHeader, fileInfo);
                }
                else if(parameter == "-ar")
                {
                    ImportAutomationRulesCommand(container, filePath, isFirstLineHeader, fileInfo);
                }
            }
        }

        private static void ImportAutomationRulesCommand(UnityContainer container, string filePath, bool isFirstLineHeader, FileInfo fileInfo)
        {
            var automationRuleParser = container.Resolve<AutomationRuleParser>();
            var automationRuleRawDtos = automationRuleParser.Parse(filePath, isFirstLineHeader);

            var automationRuleGenerator = container.Resolve<AutomationRuleGenerator>();            
            List<AutomationRule> automationRules = automationRuleGenerator.Generate(automationRuleRawDtos);

            var automationRuleDtos = automationRules.Select(a => a.SettingDto);
            automationRuleParser.Export(automationRuleDtos, fileInfo.DirectoryName);

            automationRuleParser.Upload(automationRules);
        }

        private static void ImportPriceGroupsCommand(UnityContainer container,
                                                     string filePath,
                                                     bool isFirstLineHeader,
                                                     FileInfo fileInfo)
        {
            var priceGroupsParser = container.Resolve<PriceGroupsParser>();
            var priceGroupDtos = priceGroupsParser.Parse(filePath, isFirstLineHeader);

            List<PriceGroupRawDto> tempPriceGroups = priceGroupDtos.ToList();

            List<DuplicatePriceGroup> duplicates = new List<DuplicatePriceGroup>();
            foreach(var pg in priceGroupDtos)
            {
                foreach(var tpg in tempPriceGroups)
                {
                    if(pg.Name != tpg.Name && pg.ProductId == tpg.ProductId &&
                       pg.SubsidiaryId == tpg.SubsidiaryId)
                        duplicates.Add(new DuplicatePriceGroup {Duplicate = tpg, FirstOccurrence = pg});
                }
            }

            var priceGroupGenerator = container.Resolve<PriceGroupGenerator>();
            var priceGroups = priceGroupGenerator.Generate(priceGroupDtos);

            priceGroupsParser.Export(priceGroups, fileInfo.DirectoryName);
            priceGroupsParser.Export(duplicates, fileInfo.DirectoryName, "duplicates.json");
            Console.WriteLine("uploading price groups");
            priceGroupsParser.Upload(priceGroups);
            Console.WriteLine("Finished!");
        }

        private static void ImportUserSettingsCommand(UnityContainer container,
                                                      string filePath,
                                                      bool isFirstLineHeader,
                                                      FileInfo fileInfo)
        {
            var userSettingsParser = container.Resolve<UserSettingsParser>();
            var settingsRawDtos = userSettingsParser.Parse(filePath, isFirstLineHeader);

            var settingsGenerator = new UserSettingsGenerator();
            var userSettingsList = settingsGenerator.Generate(settingsRawDtos);

            userSettingsParser.Export(userSettingsList, fileInfo.DirectoryName);
            userSettingsParser.UploadSettings(userSettingsList);
        }

        private static void UpdatePriceGroupCommand(UnityContainer container)
        {
            var priceGroupStore = container.Resolve<PriceGroupStore>();

            RepositoryResult<IEnumerable<PriceGroupDto>> priceGroupsRepositoryResult = priceGroupStore.GetAll();
            if(priceGroupsRepositoryResult.Success)
            {
                var logger = container.Resolve<ILog>();
                logger.Info($"Retrieved {priceGroupsRepositoryResult.Result.Count()} price groups");

                foreach(var priceGroup in priceGroupsRepositoryResult.Result)
                {
                    priceGroup.RoundingRules = true;
                    var updateResult = priceGroupStore.Update(priceGroup);

                    if(updateResult.Success)
                        logger.Info($"Updated price group {priceGroup.Id} - {priceGroup.Name}");
                    else
                        logger.Error($"FAILED to updated price group {priceGroup.Id} - {priceGroup.Name}");
                }
            }
        }

        private static void DeletePriceGroupCommand(string[] args, UnityContainer container)
        {
            var priceGroupStore = container.Resolve<PriceGroupStore>();

            if(args[1] == null)
            {
                PrintParametersUsage("The user name was not specified.");
                return;
            }

            string userName = args[1];
            priceGroupStore.DeleteFor(userName);
        }

        private static void PrintParametersUsage(string errorMsg)
        {
            Console.WriteLine(errorMsg);
            Console.WriteLine("usage: userproductauthcsvconverter -[p/u] [file_path]");
            Console.WriteLine("   -p: process price groups file");
            Console.WriteLine("   -u: process user settings file");
            Console.WriteLine("   -purr: updates price groups rounding rules to true");
        }
    }

    public class DuplicatePriceGroup
    {
        public PriceGroupRawDto Duplicate { get; set; }
        public PriceGroupRawDto FirstOccurrence { get; set; }
    }
}