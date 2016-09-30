using System.Collections.Generic;

using net_product_webservice.Dto.ProductHierarchy;

namespace UserGroupsCsvToJson
{
    public class PriceGroupRawDto
    {
        public string Name { get; set; }
        public IEnumerable<int> Subsidiaries { get; set; }
        public ProductTypeDto ProductType { get; set; }
        public int PriceRuleId { get; set; }
        public int ProductId { get; set; }

        public void Parse(string[] idStrings)
        {
            int priceRuleId;
            int productId;
            List<int> subsidiaryList = new List<int>();

            Name = idStrings[0];

            var splits = idStrings[1].Split('|');
            foreach(string s in splits)
            {
                int subsidiaryId;
                if(int.TryParse(s, out subsidiaryId))
                {
                    subsidiaryList.Add(subsidiaryId);
                }
            }

            Subsidiaries = subsidiaryList;
            int.TryParse(idStrings[2], out priceRuleId);
            int.TryParse(idStrings[3], out productId);

            PriceRuleId = priceRuleId;            
            ProductId = productId;
        }
    }
}