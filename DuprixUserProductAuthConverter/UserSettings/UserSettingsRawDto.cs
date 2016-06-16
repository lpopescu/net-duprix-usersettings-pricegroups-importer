using ApplicationSettingsWebservice.Dto.Duprix;

namespace UserGroupsCsvToJson
{
    public class UserSettingsRawDto
    {
        public string UserName { get; set; }
        public DuprixUserGroupDto UserGroup { get; set; }        
        public UserRole UserRole { get; }
        
        public UserSettingsRawDto()
        {
            UserRole = UserRole.Purchaser;
        }

        public void Parse(string[] idStrings)
        {
            int productGroupId;
            int productTypeId;
            int subsidiaryId;

            UserName = idStrings[0];

            int.TryParse(idStrings[1], out productTypeId);
            int.TryParse(idStrings[2], out productGroupId);
            int.TryParse(idStrings[3], out subsidiaryId);

            UserGroup = new DuprixUserGroupDto
                        {
                            ProductGroupId = productGroupId,
                            ProductTypeId = productTypeId,
                            SubsidiaryId = subsidiaryId
                        };
        }
    }
}