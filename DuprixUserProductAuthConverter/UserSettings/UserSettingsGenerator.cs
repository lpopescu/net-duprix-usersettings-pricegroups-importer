using System.Collections.Generic;
using System.Linq;

using ApplicationSettingsWebservice.Dto.Duprix;

namespace UserGroupsCsvToJson
{
    public class UserSettingsGenerator
    {
        public IEnumerable<DuprixSettingsDto> Generate(IEnumerable<UserSettingsRawDto> userProductAuths)
        {
            var userSettings = userProductAuths
                .GroupBy(u => new {u.UserName, u.UserRole})
                .Select(g => new DuprixSettingsDto
                             {
                                 UserName = g.Key.UserName,
                                 UserRole = g.Key.UserRole,
                                 UserGroups = g.Select(upa => upa.UserGroup).ToList()
                             }).ToArray();

            return userSettings;
        }
    }
}