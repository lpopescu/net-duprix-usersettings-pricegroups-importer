using ApplicationSettingsWebservice.Dto.Duprix;

namespace UserGroupsCsvToJson
{
    public class UserSettings
    {
        public string UserName { get; set; }

        public DuprixUserGroupDto[] UserGroups { get; set; }

        public UserRole UserRole { get; set; }
    }
}