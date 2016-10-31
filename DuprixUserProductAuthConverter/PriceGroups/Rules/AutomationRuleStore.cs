using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using log4net;

using PriceGroupWebservice.Client;
using PriceGroupWebservice.Dto;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson.PriceGroups
{
    public class AutomationRuleStore
    {
        private readonly ILog _logger;
        private readonly IAutomationRuleSettingRepository _automationRuleRepository;        

        public AutomationRuleStore(IAutomationRuleSettingRepository automationRuleRepository, ILog logger)
        {
            _logger = logger;
            _automationRuleRepository = automationRuleRepository;
        }

        public async Task<RepositoryResult<AutomationRuleSettingDto>> SaveAsync(AutomationRuleSettingDto automationRule)
        {
            return await _automationRuleRepository.PostAsync(automationRule);
        }

        public async Task<RepositoryResult<AutomationRuleSettingDto>> UpdateAsync(AutomationRuleSettingDto automationRule)
        {
            return await _automationRuleRepository.PutAsync(automationRule);
        }

        public async Task<RepositoryResult<DefaultAutomationRuleSettingDto>> GetDefaultRules()
        {
            return await _automationRuleRepository.GetDefaultSettingsAsync();
        }

        public IEnumerable<AutomationRuleSettingDto> GetAll()
        {
            var userAutomationRules = Enumerable.Empty<AutomationRuleSettingDto>();
            var repositoryResult = _automationRuleRepository.GetAllAsync().Result;
            if(repositoryResult.Success)
            {
                userAutomationRules = repositoryResult.Result;
            }
            else
            {
                _logger.Error($"Retrieval of automation rules failed. {repositoryResult.HttpStatusCode} - {repositoryResult.FailureReason}");
            }
            return userAutomationRules;
        }

        public IEnumerable<AutomationRuleSettingDto> GetAll(IEnumerable<PriceGroupDto> priceGroups)
        {
            RepositoryResult<IEnumerable<AutomationRuleSettingDto>> rules = _automationRuleRepository.GetAllAsync().Result;
            var userAutomationRules = new List<AutomationRuleSettingDto>();

            if (rules.Success)
            {
                foreach(var priceGroupDto in priceGroups)
                {
                    userAutomationRules.AddRange( rules.Result.Where(r => r.PriceGroupId == priceGroupDto.Id) );
                }    
            }

            return userAutomationRules;
        }
    }
}