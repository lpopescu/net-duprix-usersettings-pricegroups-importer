using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using ApplicationSettingsWebservice.Client.Repositories;

using AutoMapper;

using log4net;

using PriceGroupWebservice.Client;
using PriceGroupWebservice.Dto;

using UserGroupsCsvToJson.PriceGroups;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson
{
    public class PriceGroupStore
    {
        private readonly PriceGroupRepository _priceGroupRepository;
        private readonly PriceRuleRepository _priceRuleRepository;
        private readonly DuprixSettingsRepository _duprixSettingsRepository;
        private readonly AutomationRuleSettingRepository _automationRules;
        private readonly ProductTypeStore _productTypeStore;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public PriceGroupStore(PriceGroupRepository priceGroupRepository, PriceRuleRepository priceRuleRepository,
            DuprixSettingsRepository duprixSettingsRepository,
            AutomationRuleSettingRepository automationRules,
            ProductTypeStore productTypeStore,
            IMapper mapper,
            ILog logger)
        {
            _priceGroupRepository = priceGroupRepository;
            _priceRuleRepository = priceRuleRepository;
            _duprixSettingsRepository = duprixSettingsRepository;
            _automationRules = automationRules;
            _productTypeStore = productTypeStore;
            _mapper = mapper;
            _logger = logger;
        }

        public RepositoryResult<PriceGroupDto> Save(PriceGroupDto priceGroup)
        {
            var result = _priceGroupRepository.PostAsync(priceGroup).Result;
            return result;
        }

        public RepositoryResult<PriceGroupDto> Update(PriceGroupDto priceGroup)
        {
            var result = _priceGroupRepository.PutAsync(priceGroup).Result;
            return result;
        }

        public RepositoryResult<PriceGroupDto> UpdateFrom(AutomationRuleRawDto automationRuleRawDto)
        {
            var priceGroupResult = new RepositoryResult<PriceGroupDto>(null, HttpStatusCode.NotFound);
            RepositoryResult<PriceGroupDto> priceGroupsRepositoryResult =
                _priceGroupRepository.GetAsync(automationRuleRawDto.PriceGroupId).Result;

            if(!priceGroupsRepositoryResult.Success)
                return priceGroupResult;

            PriceGroupDto existingPriceGroup = priceGroupsRepositoryResult.Result;
            var priceGroup = _mapper.Map<PriceGroupDto>(automationRuleRawDto);

            priceGroup.Products = existingPriceGroup.Products;
            SetPriceRule(automationRuleRawDto, existingPriceGroup, priceGroup);
            SetProductType(automationRuleRawDto, existingPriceGroup, priceGroup);

            var automationRuleComparer = new AutomationRuleComparer();
            DefaultAutomationRuleSettingDto automationRuleDefaults = _automationRules.GetDefaultSettingsAsync().Result.Result;
            bool areDefaultRules = !automationRuleComparer.Equals(automationRuleRawDto, automationRuleDefaults);

            priceGroup.CustomRulesAppliedFlag = areDefaultRules;
            priceGroup.ShouldBeAutomated = existingPriceGroup.ShouldBeAutomated;

            RepositoryResult<PriceGroupDto> updateResult = _priceGroupRepository.PutAsync(priceGroup).Result;

            priceGroupResult = updateResult;
            return priceGroupResult;
        }

        private void SetProductType(AutomationRuleRawDto automationRuleRawDto,
                                    PriceGroupDto existingPriceGroup,
                                    PriceGroupDto priceGroup)
        {
            if(existingPriceGroup.ProductType.Id != automationRuleRawDto.ProductTypeId)
            {
                var productHierarchyProductType =
                    _productTypeStore.GetProductType(automationRuleRawDto.ProductTypeId).Result;
                priceGroup.ProductType = new ProductTypeDto
                                         {
                                             Id = productHierarchyProductType.Id,
                                             Name = productHierarchyProductType.Name
                                         };
            }
            else
                priceGroup.ProductType = existingPriceGroup.ProductType;
        }

        private void SetPriceRule(AutomationRuleRawDto automationRuleRawDto,
                                  PriceGroupDto existingPriceGroup,
                                  PriceGroupDto priceGroup)
        {
            if(existingPriceGroup.PriceRule.Id != automationRuleRawDto.PriceRuleId)
            {
                var priceRuleDto = _priceRuleRepository.GetAsync(automationRuleRawDto.PriceRuleId).Result.Result;
                priceGroup.PriceRule = priceRuleDto;
            }
            else
                priceGroup.PriceRule = existingPriceGroup.PriceRule;
        }

        public RepositoryResult<PriceGroupDto> Get(int priceGroupid)
        {
            return _priceGroupRepository.GetAsync(priceGroupid).Result;
        }

        public RepositoryResult<IEnumerable<PriceGroupDto>> GetAll()
        {
            return _priceGroupRepository.GetAllAsync().Result;
        }

        public RepositoryResult<PriceGroupDto> Delete(int priceGroupId)
        {
            return _priceGroupRepository.DeleteAsync(priceGroupId).Result;
        }

        public PriceRuleDto GetPriceRule(int id)
        {
            RepositoryResult<PriceRuleDto> result = _priceRuleRepository.GetAsync(id).Result;
            if (result.Success)
                return result.Result;

            throw new Exception($"Failed to retrieve price rule for {id}");
        }

        public void DeleteAutomationRulesFor(PriceGroupDto priceGroup)
        {
            var automationRules = _automationRules.GetAllAsync().Result;
            if (!automationRules.Success)
            {
                _logger.Error($"failed to retrieve automation rules for pricegroup {priceGroup.Id} {priceGroup.Name}");
            }
            var priceGroupAutomationRules = automationRules
                .Result
                .Where(ar => ar.PriceGroupId == priceGroup.Id);

            foreach (var automationRule in priceGroupAutomationRules)
            {
                var delete = _automationRules.DeleteAsync(automationRule.Id).Result;
                if(!delete.Success)
                {
                    _logger.Error($"Failed to delete automation rule {automationRule.Id}");
                }
            }
        }

        public IEnumerable<PriceGroupDto> GetAllFor(string userName)
        {
            IEnumerable<PriceGroupDto> userPriceGroups = Enumerable.Empty<PriceGroupDto>();
            var duprixSettings = _duprixSettingsRepository.GetSettingsAsync(userName).Result;

            if(duprixSettings.Success)
            {
                var productTypesPerSubsidiary =
                    duprixSettings.Result.UserGroups.Select(ug => new {ug.ProductTypeId, ug.SubsidiaryId});

                var priceGroups = GetAll();
                if(priceGroups.Success)
                {
                    userPriceGroups =
                        priceGroups.Result.Where(
                            pg =>
                                productTypesPerSubsidiary.Any(
                                    pt =>
                                        pt.ProductTypeId == pg.ProductType.Id &&
                                        pg.Subsidiaries.Contains(pt.SubsidiaryId)));
                }
            }

            return userPriceGroups;
        }

        public IEnumerable<PriceGroupDto> DeleteFor(string userName)
        {
            List<PriceGroupDto> deletedPriceGroups = new List<PriceGroupDto>();
            var duprixSettings = _duprixSettingsRepository.GetSettingsAsync(userName).Result;

            if (duprixSettings.Success)
            {
                var productTypesPerSubsidiary = duprixSettings.Result.UserGroups.Select(ug => new { ug.ProductTypeId, ug.SubsidiaryId });

                var priceGroups = GetAll();
                if (priceGroups.Success)
                {
                    var userPriceGroups = priceGroups.Result.Where(pg => productTypesPerSubsidiary.Any(pt => pt.ProductTypeId == pg.ProductType.Id && pg.Subsidiaries.Contains(pt.SubsidiaryId)));

                    _logger.Info($"Deleting price groups for {userName}");
                    foreach (var userPriceGroup in userPriceGroups)
                    {
                        string message =
                            $"Deleting {userPriceGroup.Id} : {userPriceGroup.Name}, subsidiary {string.Join("|", userPriceGroup.Subsidiaries)}, productType: {userPriceGroup.ProductType.Id} : {userPriceGroup.ProductType.Name}";

                        var deletedPriceGroup = _priceGroupRepository.DeleteAsync(userPriceGroup.Id).Result;
                        if (deletedPriceGroup.Success)
                        {
                            _logger.Info(message);
                            deletedPriceGroups.Add(userPriceGroup);
                        }
                        else
                        {
                            _logger.Error($"Failed to delete {userPriceGroup.Id} - {userPriceGroup.Name}");
                        }
                    }
                }
            }

            return deletedPriceGroups;
        }
    }
}