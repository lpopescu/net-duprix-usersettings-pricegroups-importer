using System.Threading.Tasks;

using PriceGroupWebservice.Client;
using PriceGroupWebservice.Dto;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson.PriceGroups
{
    public class AutomationRuleStore
    {
        private readonly IAutomationRuleSettingRepository _automationRuleRepository;        

        public AutomationRuleStore(IAutomationRuleSettingRepository automationRuleRepository)
        {
            _automationRuleRepository = automationRuleRepository;
        }

        public async Task<RepositoryResult<AutomationRuleSettingDto>> SaveAsync(AutomationRuleSettingDto automationRule)
        {
            return await _automationRuleRepository.PostAsync(automationRule);
        }
    }
}