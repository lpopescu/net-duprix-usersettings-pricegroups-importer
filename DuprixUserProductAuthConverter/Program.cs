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
        private static void PrintParametersUsage(string errorMsg)
        {
            Console.WriteLine(errorMsg);
            Console.WriteLine("usage: usergroupscsvtojson -[options] [file_path]/[user_name]");
            Console.WriteLine("   -purr: updates all price groups rounding rules to true");
            Console.WriteLine(" file_path options:");
            Console.WriteLine("   -p: process price groups file");
            Console.WriteLine("   -u: process user settings file");
            Console.WriteLine("   -ar: imports automation rules file");
            Console.WriteLine(" user_name options:");
            Console.WriteLine("   -deletepg: deletes price groups for user");
            Console.WriteLine("   -delete_excl_pg: deletes price groups except for these user names");
            Console.WriteLine("   -upar: updates automation rules flags to true");
        }

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
            else if (parameter == "-delete_excl_pg")
            {
                if(args[1] == null)
                {
                    PrintParametersUsage("The user name comma separated list was not specified.");
                    return;
                }
                string userNamesCsv = args[1];
                string[] userNames = userNamesCsv.Split(new[] {','});

                DeleteAllPriceGroupsExceptCommand(container, userNames);
            }
            else if(parameter == "-upar")
            {
                UpdateAutomationRuleFlagsCommand(args, container);
            }
            else if(parameter == "-purr")
            {             
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
                    ImportUserSettingsCommand(container, filePath, isFirstLineHeader, fileInfo);
                }
                else if(parameter == "-p")
                {                 
                    ImportPriceGroupsCommand(container, filePath, isFirstLineHeader, fileInfo);
                }
                else if(parameter == "-ar")
                {
                    ImportAutomationRulesCommand(container, filePath, isFirstLineHeader, fileInfo);
                }
            }
        }

        private static void DeleteAllPriceGroupsExceptCommand(UnityContainer container, string[] userNames)
        {
            var logger = container.Resolve<ILog>();
            var priceGroupStore = container.Resolve<PriceGroupStore>();
            
            List<PriceGroupDto> excludedPricegroups = new List<PriceGroupDto>();
            foreach (string userName in userNames)
            {
                logger.Info($"Getting price groups for {userName}");
                var priceGroups = priceGroupStore.GetAllFor(userName);

                excludedPricegroups.AddRange(priceGroups);
            }
            var comparer = new PriceGroupComparer();
            var allPriceGroups = priceGroupStore.GetAll().Result;
            var toDeletePriceGroups = allPriceGroups.Except(excludedPricegroups, comparer);

            foreach(var priceGroup in toDeletePriceGroups)
            {
                if(priceGroupStore.Delete(priceGroup.Id).Success)
                {
                    logger.Info(
                        $"Deleted price group {priceGroup.Id} {priceGroup.Name} for type {priceGroup.ProductType.Id} {priceGroup.ProductType.Name}");
                }
                else
                {
                    logger.Error($"Failed to delete price group {priceGroup.Id} {priceGroup.Name}");
                }
            }
        }

        private static void UpdateAutomationRuleFlagsCommand(string[] args, UnityContainer container)
        {
            var logger = container.Resolve<ILog>();
            if(args[1] == null)
            {
                PrintParametersUsage("The user name was not specified.");
                return;
            }

            var automationRuleStore = container.Resolve<AutomationRuleStore>();
            var priceGroupStore = container.Resolve<PriceGroupStore>();

            string userName = args[1];
            var userPriceGroups = priceGroupStore.GetAllFor(userName);
            var userAutomationRules = automationRuleStore.GetAll(userPriceGroups);

            foreach(var automationRule in userAutomationRules)
            {
                automationRule.IsMaximumNegativePriceDifferencePercentageRuleEnabled = true;
                automationRule.IsMaximumPositivePriceDifferencePercentageRuleEnabled = true;
                automationRule.IsMaximumToppedWeightedSalesRuleEnabled = true;
                automationRule.IsMaximumPriceIndexRuleEnabled = true;
                automationRule.IsMinimumSalesMarginPercentageRuleEnabled = true;

                var result = automationRuleStore.UpdateAsync(automationRule).Result;
                if(result.Success)
                    logger.Info($"Updated automation rule {automationRule.Id} for price group {automationRule.PriceGroupId}");
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
            if (args[1] == null)
            {
                PrintParametersUsage("The user name was not specified.");
                return;
            }

            var logger = container.Resolve<ILog>();
            string userName = args[1];
            var priceGroups = priceGroupStore.DeleteFor(userName);

            logger.Info($"For {userName}");
            Console.WriteLine("Delete also automation rules for user price groups? (y/n)");
            if(Console.ReadKey().Key == ConsoleKey.Y)
            {
                foreach (var priceGroup in priceGroups)
                {
                    logger.Info($"  deleting automation rules for pricegroup {priceGroup.Id} {priceGroup.Name}");
                    priceGroupStore.DeleteAutomationRulesFor(priceGroup);
                }

            }            
        }

    }

    public class DuplicatePriceGroup
    {
        public PriceGroupRawDto Duplicate { get; set; }
        public PriceGroupRawDto FirstOccurrence { get; set; }
    }

    public class PriceGroupComparer : IEqualityComparer<PriceGroupDto>
    {
        public bool Equals(PriceGroupDto x, PriceGroupDto y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(PriceGroupDto obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}