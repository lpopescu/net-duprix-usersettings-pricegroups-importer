using System;
using System.Collections.Generic;
using System.Linq;

using ApplicationSettingsWebservice.Client.Repositories;
using ApplicationSettingsWebservice.Dto.Duprix;

using log4net;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson
{
    public class UserSettingsStore
    {
        private readonly DuprixSettingsRepository _repository;
        private readonly ILog _logger;

        public UserSettingsStore(DuprixSettingsRepository repository, ILog logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public RepositoryResult<DuprixSettingsDto> Update(DuprixSettingsDto userSettings)
        {
            var result = _repository.StoreSettingsAsync(userSettings).Result;
            return result;
        }

        public RepositoryResult<DuprixSettingsDto> Get(string userName)
        {
            return _repository.GetSettingsAsync(userName).Result;
        }

        public IEnumerable<DuprixSettingsDto> Get(int productTypeId)
        {
            var userSettings = Enumerable.Empty<DuprixSettingsDto>();
            //RepositoryResult<IEnumerable<DuprixSettingsDto>> repositoryResult = _repository.GetAllAsync();

            var settings = new List<DuprixSettingsDto>()
                           {
                               new DuprixSettingsDto { UserName = "a", UserGroups = new List<DuprixUserGroupDto>
                                                                                    {
                                                                                        new DuprixUserGroupDto {ProductTypeId = 2365, SubsidiaryId = 1 },
                                                                                        new DuprixUserGroupDto {ProductTypeId = 2253, SubsidiaryId = 1 }
                                                                                    } },
                               new DuprixSettingsDto { UserName = "b", UserGroups = new List<DuprixUserGroupDto>
                                                                                    {
                                                                                        new DuprixUserGroupDto {ProductTypeId = 2069, SubsidiaryId = 1 },
                                                                                        new DuprixUserGroupDto {ProductTypeId = 2287, SubsidiaryId = 1 }
                                                                                    }},
                               new DuprixSettingsDto { UserName = "c", UserGroups = new List<DuprixUserGroupDto>
                                                                                    {
                                                                                        new DuprixUserGroupDto {ProductTypeId = 2043, SubsidiaryId = 1 },
                                                                                        new DuprixUserGroupDto {ProductTypeId = 2515, SubsidiaryId = 1 }
                                                                                    }}

                           };
            var repositoryResult = new RepositoryResult<IEnumerable<DuprixSettingsDto>>(settings);

            if (repositoryResult.Success)
            {
                userSettings = repositoryResult.Result
                    .Where(s => s.UserGroups.Exists(u => u.ProductTypeId == productTypeId));

                if(userSettings.Count() > 1)
                {
                    string userNames = string.Join("|", userSettings.Select(u => u.UserName));
                    _logger.Warn($"More than one user was found with product type id {productTypeId}. Users: {userNames}");
                }
            }
            else if(repositoryResult.FailureReason == RepositoryFailureReason.ResourceNotFound)
            {
                _logger.Error($"No user was found with product type id {productTypeId}");
            }
            else if (repositoryResult.FailureReason == RepositoryFailureReason.InternalServerError)
            {
                _logger.Error($"There was an internal server error.");
            }

            return userSettings;
        }
    }
}