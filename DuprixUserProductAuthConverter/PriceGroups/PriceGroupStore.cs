using System;

using PriceGroupWebservice.Client;
using PriceGroupWebservice.Dto;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson
{
    public class PriceGroupStore
    {
        private readonly PriceGroupRepository _priceGroupRepository;
        private readonly PriceRuleRepository _priceRuleRepository;

        public PriceGroupStore(PriceGroupRepository priceGroupRepository, PriceRuleRepository priceRuleRepository)
        {
            _priceGroupRepository = priceGroupRepository;
            _priceRuleRepository = priceRuleRepository;
        }

        public RepositoryResult<PriceGroupDto> Update(PriceGroupDto priceGroup)
        {
            var result = _priceGroupRepository.PostAsync(priceGroup).Result;
            return result;
        }

        public PriceRuleDto GetPriceRule(int id)
        {
            RepositoryResult<PriceRuleDto> result = _priceRuleRepository.GetAsync(id).Result;
            if (result.Success)
                return result.Result;

            throw new Exception($"Failed to retrieve price rule for {id}");
        }
    }
}