using System.Collections.Generic;

namespace UserGroupsCsvToJson.PriceGroups
{
    public class AutomationRuleRawDto
    {
        public int PriceGroupId { get; set; }
        public string PriceGroupName { get; set; }
        public int PriceRuleId { get; set; }
        public int ProductTypeId { get; set; }
        public string Buyer { get; set; }
        public IEnumerable<int> Subsidiaries { get; set; }
        public bool RoundingRules { get; set; }
        public decimal MinimumMargin { get; set; }
        public bool CostPlus { get; set; }

        public decimal MinSalesMargin { get; set; }
        public decimal MaxPriceIncrease { get; set; }
        public decimal MaxPriceDecrease { get; set; }
        public decimal MaxTopWeightedSales { get; set; }
        public decimal MaxPriceIndex { get; set; }
        public bool CalculationMethodCheckEnabled { get; set; }

        public void Parse(string[] parameters)
        {
            int priceGroupId;
            List<int> subsidiaryList = new List<int>();
            int productTypeId;
            int priceRuleId;
            decimal minMargin;
            decimal minSalesMargin;
            decimal maxPriceIncrease;
            decimal maxPriceDecrease;
            decimal maxTopWeightedSales;
            decimal maxPriceIndex;
            bool calculationMethodCheckEnabled, roundingRules, costPlus;

            int.TryParse(parameters[0], out priceGroupId);
            PriceGroupName = parameters[1];
            int.TryParse(parameters[2], out priceRuleId);
            int.TryParse(parameters[3], out productTypeId);
            Buyer = parameters[4];

            var splits = parameters[5].Split('|');
            foreach (string s in splits)
            {
                int subsidiaryId;
                if (int.TryParse(s, out subsidiaryId))
                {
                    subsidiaryList.Add(subsidiaryId);
                }
            }

            bool.TryParse(parameters[6], out roundingRules);
            decimal.TryParse(parameters[7], out minMargin);
            bool.TryParse(parameters[8], out costPlus);

            decimal.TryParse(parameters[9], out minSalesMargin);
            decimal.TryParse(parameters[10], out maxPriceIncrease);
            decimal.TryParse(parameters[11], out maxPriceDecrease);
            decimal.TryParse(parameters[12], out maxTopWeightedSales);
            decimal.TryParse(parameters[13], out maxPriceIndex);

            bool.TryParse(parameters[14], out calculationMethodCheckEnabled);

            PriceRuleId = priceRuleId;
            PriceGroupId = priceGroupId;
            Subsidiaries = subsidiaryList;
            ProductTypeId = productTypeId;

            RoundingRules = roundingRules;
            CostPlus = costPlus;
            MinimumMargin = minMargin;

            MinSalesMargin = minSalesMargin;
            MaxPriceIncrease = maxPriceIncrease;
            MaxPriceDecrease = maxPriceDecrease;
            MaxTopWeightedSales = maxTopWeightedSales;
            MaxPriceIndex = maxPriceIndex;
            
            CalculationMethodCheckEnabled = calculationMethodCheckEnabled;
        }
    }
}