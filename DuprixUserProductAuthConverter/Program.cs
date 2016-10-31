using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using AutoMapper;

using CsvHelper;

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
                DeletePriceGroupCommand(args, container);
            else if(parameter == "-delete_excl_pg")
            {
                if(args[1] == null)
                {
                    PrintParametersUsage("The user name comma separated list was not specified.");
                    return;
                }
                string userNamesCsv = args[1];
                string[] userNames = userNamesCsv.Split(',');

                DeleteAllPriceGroupsExceptCommand(container, userNames);
            }
            else if(parameter == "-expar")
                ExportAutomationRulesCommand(args, container);
            else if(parameter == "-purr")
                UpdatePriceGroupCommand(container);            
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
                    ImportUserSettingsCommand(container, filePath, isFirstLineHeader, fileInfo);
                else if(parameter == "-p")
                {
                    string priceGroupsToFilter = args[2];
                    ImportPriceGroupsCommand(container, filePath, priceGroupsToFilter, fileInfo);
                }
                else if(parameter == "-ar")
                    ImportAutomationRulesCommand(container, filePath, isFirstLineHeader, fileInfo);
                else if(parameter == "-getpg")
                    GetPriceGroupsCommand(container, filePath, fileInfo);

            }
        }

        private static void RemoveDoubleSubsidiaryPriceGroupAndUpdateRemainingPriceGroups(UnityContainer container,
                                                                                          string userName)
        {
            var logger = container.Resolve<ILog>();
            var priceGroupStore = container.Resolve<PriceGroupStore>();

            logger.Info($"Getting price groups for {userName}");

            var userPriceGroups = priceGroupStore.GetAllFor(userName);
            var toDeletePriceGroups = userPriceGroups
                .Where(pg => pg.Subsidiaries.Count() == 1 && pg.Subsidiaries.Contains(3));

            foreach(var priceGroup in toDeletePriceGroups)
            {
                if(priceGroupStore.Delete(priceGroup.Id).Success)
                {
                    logger.Info(
                        $"Deleted price group {priceGroup.Id} {priceGroup.Name} for type {priceGroup.ProductType.Id} {priceGroup.ProductType.Name}");
                }
                else
                    logger.Error($"Failed to delete price group {priceGroup.Id} {priceGroup.Name}");
            }

            var updatePgs = userPriceGroups.Where(pg => pg.Subsidiaries.Count() == 1 && pg.Subsidiaries.Contains(1));
            foreach(var priceGroup in updatePgs)
            {
                priceGroup.Subsidiaries = priceGroup.Subsidiaries.Concat(new[] {3});
                var updateResult = priceGroupStore.Update(priceGroup);

                if(updateResult.Success)
                    logger.Info($"Updated price group {priceGroup.Id} - {priceGroup.Name}");
                else
                    logger.Error($"FAILED to updated price group {priceGroup.Id} - {priceGroup.Name}");
            }
        }

        private static void DeleteAllPriceGroupsExceptCommand(UnityContainer container, string[] userNames)
        {
            var logger = container.Resolve<ILog>();
            var priceGroupStore = container.Resolve<PriceGroupStore>();

            List<PriceGroupDto> excludedPricegroups = new List<PriceGroupDto>();

            foreach(string userName in userNames)
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
                    logger.Error($"Failed to delete price group {priceGroup.Id} {priceGroup.Name}");
            }
        }

        private static void ExportAutomationRulesCommand(string[] args, UnityContainer container)
        {
            var logger = container.Resolve<ILog>();
            if(args[1] == null)
            {
                PrintParametersUsage("The user name was not specified.");
                return;
            }

            if(args[2] == null)
            {
                PrintParametersUsage("The export path was not specified.");
                return;
            }

            var automationRuleStore = container.Resolve<AutomationRuleStore>();
            var priceGroupStore = container.Resolve<PriceGroupStore>();
            var mapper = container.Resolve<IMapper>();
            var fileExporter = container.Resolve<FileExport>();

            string exportpath = args[2];
            string userName = args[1];
            var userPriceGroups = priceGroupStore.GetAllFor(userName);
            var userAutomationRules = automationRuleStore.GetAll(userPriceGroups);
            var list = new List<AutomationRuleRawDto>();

            var priceGroupsWithDefaultRules =
                userPriceGroups.Where(pg => userAutomationRules.All(ar => ar.PriceGroupId != pg.Id));
            var automationRuleDefaults = automationRuleStore.GetDefaultRules().Result.Result;

            foreach(var priceGroupWithDefaultRule in priceGroupsWithDefaultRules)
            {
                var automationRuleRawDto = mapper.Map<AutomationRuleRawDto>(automationRuleDefaults);
                automationRuleRawDto.PriceGroupId = priceGroupWithDefaultRule.Id;
                automationRuleRawDto.ProductTypeId = priceGroupWithDefaultRule.ProductType.Id;

                SetAutomationRuleMembers(automationRuleRawDto, priceGroupWithDefaultRule, userName);

                list.Add(automationRuleRawDto);
            }

            foreach (var automationRule in userAutomationRules)
            {
                var automationRuleRawDto = mapper.Map<AutomationRuleRawDto>(automationRule);
                var priceGroup = userPriceGroups.First(pg => pg.Id == automationRule.PriceGroupId);

                SetAutomationRuleMembers(automationRuleRawDto, priceGroup, userName);
                
                list.Add(automationRuleRawDto);
            }
            fileExporter.ExportToCsv(list, exportpath, $"automation_rules_export_{userName}.csv");
            logger.Info($"Exported automation rules");
        }

        private static void SetAutomationRuleMembers(AutomationRuleRawDto automationRuleRawDto,
                                                     PriceGroupDto priceGroup,
                                                     string userName)
        {
            automationRuleRawDto.Subsidiaries = priceGroup.Subsidiaries;
            automationRuleRawDto.Buyer = userName;
            automationRuleRawDto.PriceGroupName = priceGroup.Name;
            automationRuleRawDto.CostPlus = priceGroup.CostPlus;
            automationRuleRawDto.RoundingRules = priceGroup.RoundingRules;
            automationRuleRawDto.PriceRuleId = priceGroup.PriceRule.Id;
            automationRuleRawDto.MinimumMargin = (decimal)priceGroup.MinimumMargin;
        }

        private static void ImportAutomationRulesCommand(UnityContainer container,
                                                         string filePath,
                                                         bool isFirstLineHeader,
                                                         FileInfo fileInfo)
        {
            var automationRuleParser = container.Resolve<AutomationRuleParser>();
            var priceGroupStore = container.Resolve<PriceGroupStore>();
            var logger = container.Resolve<ILog>();

            var automationRuleRawDtos = automationRuleParser.Parse(filePath, isFirstLineHeader);

            var automationRuleGenerator = container.Resolve<AutomationRuleGenerator>();
            List<AutomationRule> automationRules = automationRuleGenerator.Generate(automationRuleRawDtos);

            var automationRuleDtos = automationRules.Select(a => a.SettingDto);
            automationRuleParser.Export(automationRuleDtos, fileInfo.DirectoryName);

            automationRuleParser.Upload(automationRules);

            foreach(var ar in automationRuleRawDtos)
            {
                var updateResult = priceGroupStore.UpdateFrom(ar);

                if(updateResult.Success)
                    logger.Info($"Updated price group {ar.PriceGroupId} - {ar.PriceGroupName}");
                else
                    logger.Error($"FAILED to updated price group {ar.PriceGroupId} - {ar.PriceGroupName}");
            }
        }

        private static void ImportPriceGroupsCommand(UnityContainer container,
                                                     string filePath,
                                                     string priceGroupsToFilter,
                                                     FileInfo fileInfo)
        {
            var priceGroupsParser = container.Resolve<PriceGroupsParser>();

            string[] priceGroupNames = priceGroupsToFilter.Split(',');
            var priceGroupDtos = priceGroupsParser.Parse(filePath, priceGroupNames);
            var userSettingsStore = container.Resolve<UserSettingsStore>();

            List<PriceGroupRawDto> tempPriceGroups = priceGroupDtos.ToList();

            List<DuplicatePriceGroup> duplicates = new List<DuplicatePriceGroup>();
            foreach(var pg in priceGroupDtos)
            {
                foreach(var tpg in tempPriceGroups)
                {
                    if(pg.Name != tpg.Name && pg.ProductId == tpg.ProductId &&
                       !pg.Subsidiaries.Except(tpg.Subsidiaries).Any())
                        duplicates.Add(new DuplicatePriceGroup {Duplicate = tpg, FirstOccurrence = pg});
                }
            }

            var priceGroupGenerator = container.Resolve<PriceGroupGenerator>();
            var priceGroups = priceGroupGenerator.Generate(priceGroupDtos);

            var fileExporter = container.Resolve<FileExport>();
            var mapper = container.Resolve<IMapper>();

            fileExporter.ExportToJson(priceGroups, fileInfo.DirectoryName);
            fileExporter.ExportToJson(duplicates, fileInfo.DirectoryName, "duplicates.json");
            Console.WriteLine("uploading price groups");
            var updatedPriceGroups = priceGroupsParser.Upload(priceGroups);

            var priceGroupRulesTemplate = mapper.Map<IEnumerable<AutomationRuleRawDto>>(updatedPriceGroups)
                                                .ToList();
            
            foreach(var pg in priceGroupRulesTemplate)
            {
                var userSettings = userSettingsStore.Get(pg.ProductTypeId);

                pg.Buyer = userSettings.FirstOrDefault()?.UserName ?? "";
            }

            fileExporter.ExportToCsv(priceGroupRulesTemplate, fileInfo.DirectoryName, "price_groups_rule_template.csv");
            Console.WriteLine("Finished!");
        }

        private static void ImportPriceGroupsForUserCommand(UnityContainer container,
                                                     string filePath,
                                                     string userName,
                                                     FileInfo fileInfo)
        {
            var priceGroupsParser = container.Resolve<PriceGroupsParser>();            
            var userSettingsStore = container.Resolve<UserSettingsStore>();

            //var userSettings = userSettingsStore.Get()

            var priceGroupDtos = priceGroupsParser.Parse(filePath, new string[] { });
            List<PriceGroupRawDto> tempPriceGroups = priceGroupDtos.ToList();

            List<DuplicatePriceGroup> duplicates = new List<DuplicatePriceGroup>();
            foreach (var pg in priceGroupDtos)
            {
                foreach (var tpg in tempPriceGroups)
                {
                    if (pg.Name != tpg.Name && pg.ProductId == tpg.ProductId &&
                       !pg.Subsidiaries.Except(tpg.Subsidiaries).Any())
                        duplicates.Add(new DuplicatePriceGroup { Duplicate = tpg, FirstOccurrence = pg });
                }
            }

            var priceGroupGenerator = container.Resolve<PriceGroupGenerator>();
            var priceGroups = priceGroupGenerator.Generate(priceGroupDtos);

            var fileExporter = container.Resolve<FileExport>();
            var mapper = container.Resolve<IMapper>();

            fileExporter.ExportToJson(priceGroups, fileInfo.DirectoryName);
            fileExporter.ExportToJson(duplicates, fileInfo.DirectoryName, "duplicates.json");
            Console.WriteLine("uploading price groups");
            var updatedPriceGroups = priceGroupsParser.Upload(priceGroups);

            var priceGroupRulesTemplate = mapper.Map<IEnumerable<AutomationRuleRawDto>>(updatedPriceGroups)
                                                .ToList();

            foreach (var pg in priceGroupRulesTemplate)
            {
                var userSettings = userSettingsStore.Get(pg.ProductTypeId);

                pg.Buyer = userSettings.FirstOrDefault()?.UserName ?? "";
            }

            fileExporter.ExportToCsv(priceGroupRulesTemplate, fileInfo.DirectoryName, "price_groups_rule_template.csv");
            Console.WriteLine("Finished!");
        }

        private static void GetPriceGroupsCommand(UnityContainer container,
                                                     string filePath,                                                     
                                                     FileInfo fileInfo)
        {            
            var priceGroupStore = container.Resolve<PriceGroupStore>();

            var priceGroups =  new List<int>();
            using (var fs = new StreamReader(filePath))
            {
                var csvReader = new CsvReader(fs);
                while(csvReader.Read())
                {
                    priceGroups.Add(csvReader.GetField<int>(0));
                }
            }
            var userSettingsStore = container.Resolve<UserSettingsStore>();
            var logger = container.Resolve<ILog>();

            var fileExporter = container.Resolve<FileExport>();
            var mapper = container.Resolve<IMapper>();

            Console.WriteLine("retrieving price groups");
            var updatedPriceGroups = new List<PriceGroupDto>();
            foreach (var priceGroupId in priceGroups)
            {
                RepositoryResult<PriceGroupDto> priceGroupRepositoryResult = priceGroupStore.Get(priceGroupId);
                if(priceGroupRepositoryResult.Success)
                {
                    updatedPriceGroups.Add(priceGroupRepositoryResult.Result);
                }
                else
                {
                    logger.Error(
                        $"Could not find price group with Id {priceGroupId}");
                }
            }
            var priceGroupRulesTemplate = mapper.Map<IEnumerable<AutomationRuleRawDto>>(updatedPriceGroups)
                                                .ToList();

            foreach (var pg in priceGroupRulesTemplate)
            {
                var userSettings = userSettingsStore.Get(pg.ProductTypeId);

                pg.Buyer = userSettings.FirstOrDefault()?.UserName ?? "";
            }

            fileExporter.ExportToCsv(priceGroupRulesTemplate, fileInfo.DirectoryName, "price_groups_rule_template.csv");
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

        private void UpdateUserPriceGroupsCommand(UnityContainer container, string userName)
        {
            var logger = container.Resolve<ILog>();
            var priceGroupStore = container.Resolve<PriceGroupStore>();

            logger.Info($"Getting price groups for {userName}");

            var userPriceGroups = priceGroupStore.GetAllFor(userName);
            foreach(var priceGroup in userPriceGroups)
            {
                
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

            var logger = container.Resolve<ILog>();
            string userName = args[1];
            var priceGroups = priceGroupStore.DeleteFor(userName);

            logger.Info($"For {userName}");
            Console.WriteLine("Delete also automation rules for user price groups? (y/n)");
            if(Console.ReadKey().Key == ConsoleKey.Y)
            {
                foreach(var priceGroup in priceGroups)
                {
                    logger.Info($"  deleting automation rules for pricegroup {priceGroup.Id} {priceGroup.Name}");
                    priceGroupStore.DeleteAutomationRulesFor(priceGroup);
                }
            }
        }
    }
}