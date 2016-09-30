using System.Collections.Generic;

using AutoMapper;

using log4net;

using PriceGroupWebservice.Dto;

using UserGroupsCsvToJson.PriceGroups;

namespace UserGroupsCsvToJson
{
    internal class AutomationRuleGenerator
    {
        private readonly ILog _logger;
        private readonly IMapper _mapper;
        private readonly PriceGroupStore _priceGroupStore;

        public AutomationRuleGenerator(PriceGroupStore priceGroupStore, IMapper mapper, ILog logger)
        {
            _priceGroupStore = priceGroupStore;
            _mapper = mapper;
            _logger = logger;
        }

        public List<AutomationRule> Generate(IEnumerable<AutomationRuleRawDto> automationRuleRawDtos)
        {
            List<AutomationRule> automationRules = new List<AutomationRule>();

            foreach(var automationRuleRawDto in automationRuleRawDtos)
            {
                var priceGroup = _priceGroupStore.Get(automationRuleRawDto.PriceGroupId);
                if(priceGroup.Success)
                {
                    var automationRuleDto = _mapper.Map<AutomationRuleSettingDto>(automationRuleRawDto);
                    var automationRule = new AutomationRule
                                         {
                                             RawDto = automationRuleRawDto,
                                             SettingDto = automationRuleDto
                                         };
                    automationRules.Add(automationRule);
                }
                else
                {
                    _logger.Error(
                        $"There is no price group {automationRuleRawDto.PriceGroupName} for buyer {automationRuleRawDto.Buyer}.");
                }
            }

            return automationRules;
        }
    }
}