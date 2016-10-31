using Microsoft.VisualStudio.TestTools.UnitTesting;
using UserGroupsCsvToJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using ApplicationSettingsWebservice.Dto.Duprix;

using FluentAssertions;

using MoreLinq;

using Ploeh.AutoFixture;

namespace UserGroupsCsvToJson.Tests
{
    [TestClass()]
    public class UserSettingsStoreTests
    {
        [TestMethod()]
        public void GetTest()
        {
            var _fixture = new Fixture();
            int productTypeId = 2;
            var settingsDtos = _fixture.CreateMany<DuprixSettingsDto>();

            settingsDtos = CreateDuprixSettingsDtos(settingsDtos);

            var sameProductType = settingsDtos.Where(s => s.UserGroups.Exists(u => u.ProductTypeId == productTypeId));
            List<DuprixSettingsDto> list = new List<DuprixSettingsDto>();
            foreach ( var setting in sameProductType)
            {
                var userGroups = setting.UserGroups.Where(u => u.ProductTypeId == productTypeId).ToList();
                userGroups.ForEach(ug =>
                {
                    var users = sameProductType.Where(u => u.UserName != setting.UserName);
                    IEnumerable<DuprixSettingsDto> exists = users.Where(u =>
                                        u.UserGroups.Exists(
                                            usg =>
                                                usg.ProductTypeId == ug.ProductTypeId &&
                                                usg.SubsidiaryId == ug.SubsidiaryId));

                    
                    list.AddRange(exists.Except(list));
                });                
            };

            list.Count().Should().Be(2);
        }

        private static IEnumerable<DuprixSettingsDto> CreateDuprixSettingsDtos(IEnumerable<DuprixSettingsDto> userSettings)
        {
            userSettings = new List<DuprixSettingsDto>
                           {
                               new DuprixSettingsDto
                               {
                                   UserName = "a",
                                   UserGroups = new List<DuprixUserGroupDto>
                                                {
                                                    new DuprixUserGroupDto
                                                    {
                                                        ProductTypeId = 1,
                                                        SubsidiaryId = 1
                                                    },
                                                    new DuprixUserGroupDto
                                                    {
                                                        ProductTypeId = 2,
                                                        SubsidiaryId = 3
                                                    }
                                                }
                               },
                               new DuprixSettingsDto
                               {
                                   UserName = "b",
                                   UserGroups = new List<DuprixUserGroupDto>
                                                {
                                                    new DuprixUserGroupDto
                                                    {
                                                        ProductTypeId = 1,
                                                        SubsidiaryId = 1
                                                    },
                                                    new DuprixUserGroupDto
                                                    {
                                                        ProductTypeId = 2,
                                                        SubsidiaryId = 1
                                                    }
                                                }
                               },
                               new DuprixSettingsDto
                               {
                                   UserName = "c",
                                   UserGroups = new List<DuprixUserGroupDto>
                                                {
                                                    new DuprixUserGroupDto
                                                    {
                                                        ProductTypeId = 1,
                                                        SubsidiaryId = 2
                                                    },
                                                    new DuprixUserGroupDto
                                                    {
                                                        ProductTypeId = 2,
                                                        SubsidiaryId = 3
                                                    }
                                                }
                               }
                           };
            return userSettings;
        }
    }
}