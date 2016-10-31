using System;
using System.Collections.Generic;
using System.Linq;

using ApplicationSettingsWebservice.Client.Repositories;
using ApplicationSettingsWebservice.Dto.Duprix;

using log4net;

using MoreLinq;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson
{
    public class UserSettingsStore
    {
        private readonly ILog _logger;
        private readonly IDuprixSettingsRepository _repository;

        public UserSettingsStore(IDuprixSettingsRepository repository, ILog logger)
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
            RepositoryResult<IEnumerable<DuprixSettingsDto>> repositoryResult = _repository.GetAllAsync().Result;

            if(repositoryResult.Success)
            {
                userSettings = repositoryResult.Result
                                               .Where(s => s.UserGroups.Exists(u => u.ProductTypeId == productTypeId));
                
                var usersWithSameProductTypeSubsidiary = GetUsersWithSameProductTypeAndSubsidiary(repositoryResult.Result, productTypeId);

                if (usersWithSameProductTypeSubsidiary.Any())
                {
                    string userNames = string.Join("|", userSettings.Select(u => u.UserName));
                    _logger.Warn(
                        $"More than one user was found with product type id {productTypeId}. Users: {userNames}");
                }                
            }
            else if(repositoryResult.FailureReason == RepositoryFailureReason.ResourceNotFound)
                _logger.Error($"No user was found with product type id {productTypeId}");
            else if(repositoryResult.FailureReason == RepositoryFailureReason.InternalServerError)
                _logger.Error($"There was an internal server error.");

            return userSettings;
        }

        private IEnumerable<DuprixSettingsDto> GetUsersWithSameProductTypeAndSubsidiary(IEnumerable<DuprixSettingsDto> userSettings, int productTypeId)
        {
            List<DuprixSettingsDto> list = new List<DuprixSettingsDto>();
            foreach (var setting in userSettings)
            {
                var userGroups = setting.UserGroups.Where(u => u.ProductTypeId == productTypeId).ToList();
                userGroups.ForEach(ug =>
                {
                    var users = userSettings.Where(u => u.UserName != setting.UserName);
                    IEnumerable<DuprixSettingsDto> exists = users.Where(u =>
                                        u.UserGroups.Exists(
                                            usg =>
                                                usg.ProductTypeId == ug.ProductTypeId &&
                                                usg.SubsidiaryId == ug.SubsidiaryId));


                    list.AddRange(exists.Except(list));
                });
            };
            return list;
        }
    }
}