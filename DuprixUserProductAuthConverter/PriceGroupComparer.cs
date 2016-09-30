using System.Collections.Generic;

using PriceGroupWebservice.Dto;

namespace UserGroupsCsvToJson
{
    public class PriceGroupComparer : IEqualityComparer<PriceGroupDto>
    {
        public bool Equals(PriceGroupDto x, PriceGroupDto y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(PriceGroupDto obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}