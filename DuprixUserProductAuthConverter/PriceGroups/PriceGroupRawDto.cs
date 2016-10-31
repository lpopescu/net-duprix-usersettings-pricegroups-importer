using System;
using System.Collections.Generic;
using CsvHelper;
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

        internal void Parse(CsvReader csvReader)
        {            
            List<int> subsidiaryList = new List<int>();

            Name = csvReader.GetField(0);
            var splits = csvReader.GetField(1).Split('|');
            foreach (string s in splits)
            {
                int subsidiaryId;
                if (int.TryParse(s, out subsidiaryId))
                {
                    subsidiaryList.Add(subsidiaryId);
                }
            }

            Subsidiaries = subsidiaryList;
            PriceRuleId = csvReader.GetField<int>(2);
            ProductId = csvReader.GetField<int>(3);
        }
    }
}