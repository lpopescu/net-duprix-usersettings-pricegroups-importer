﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

using PriceGroupWebservice.Dto;

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
            if(args[1] == null)
            {
                PrintParametersUsage("The file path was not specified.");
                return;
            }

            var filePath = args[1];
            bool isFirstLineHeader = true;
            string parameter = args[0];

            if(!File.Exists(filePath))
                Console.WriteLine("The file path '{0}' does not exist.", filePath);
            
            var fileInfo = new FileInfo(filePath);
            var container = new UnityContainer();

            container.AddNewExtension<Bootstrapper>();

            if(parameter == "-u")
            {
                var userSettingsParser = container.Resolve<UserSettingsParser>();
                var settingsRawDtos = userSettingsParser.Parse(filePath, isFirstLineHeader);

                var settingsGenerator = new UserSettingsGenerator();
                var userSettingsList = settingsGenerator.Generate(settingsRawDtos);

                userSettingsParser.Export(userSettingsList, fileInfo.DirectoryName);
                userSettingsParser.UploadSettings(userSettingsList);
            }
            else if(parameter == "-purr")
            {                
                var priceGroupStore = container.Resolve<PriceGroupStore>();

                RepositoryResult<IEnumerable<PriceGroupDto>> priceGroupsRepositoryResult = priceGroupStore.GetAll();
                if(priceGroupsRepositoryResult.Success)
                {
                    foreach (var priceGroup in priceGroupsRepositoryResult.Result)
                    {
                        priceGroup.RoundingRules = true;
                        var updateResult = priceGroupStore.Update(priceGroup);

                        if (updateResult.Success)
                            Console.WriteLine($"Updated price group {priceGroup.Id} - {priceGroup.Name}");
                        else                        
                            Console.WriteLine($"FAILED to updated price group {priceGroup.Id} - {priceGroup.Name}");   
                    }
                }
            }
            else if(parameter == "-p")
            {
                var priceGroupsParser = container.Resolve<PriceGroupsParser>();
                var priceGroupDtos = priceGroupsParser.Parse(filePath, isFirstLineHeader);

                List<PriceGroupRawDto> tempPriceGroups = priceGroupDtos.ToList();

                List<DuplicatePriceGroup> duplicates = new List<DuplicatePriceGroup>();
                foreach (var pg in priceGroupDtos)
                {
                    foreach(var tpg in tempPriceGroups)
                    {
                        if(pg.Name != tpg.Name && pg.ProductId == tpg.ProductId && pg.SubsidiaryId == tpg.SubsidiaryId)
                        {
                            duplicates.Add( new DuplicatePriceGroup { Duplicate = tpg, FirstOccurrence = pg} );
                        }
                    }
                }                

                var priceGroupGenerator = container.Resolve<PriceGroupGenerator>();
                var priceGroups = priceGroupGenerator.Generate(priceGroupDtos);

                priceGroupsParser.Export(priceGroups, fileInfo.DirectoryName);
                priceGroupsParser.Export(duplicates, fileInfo.DirectoryName, "duplicates.json");
                priceGroupsParser.Upload(priceGroups);
            }
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
