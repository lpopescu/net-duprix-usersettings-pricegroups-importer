using ApplicationSettingsWebservice.Client.Repositories;
using ApplicationSettingsWebservice.Dto.Duprix;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson
{
    public class UserSettingsStore
    {
        private readonly DuprixSettingsRepository _repository;

        public UserSettingsStore(DuprixSettingsRepository repository)
        {
            _repository = repository;
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
    }
}