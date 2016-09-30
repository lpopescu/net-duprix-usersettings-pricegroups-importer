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

        public bool MinSalesMarginEnabled { get; set; }
        public decimal MinSalesMargin { get; set; }
        public bool MaxPriceIncreaseEnabled { get; set; }
        public decimal MaxPriceIncrease { get; set; }
        public bool MaxPriceDecreaseEnabled { get; set; }
        public decimal MaxPriceDecrease { get; set; }
        public bool MaxTopWeightedSalesEnabled { get; set; }
        public decimal MaxTopWeightedSales { get; set; }
        public bool MaxPriceIndexEnabled { get; set; }
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
            bool calculationMethodCheckEnabled, roundingRules, costPlus, minSalesMarginEnabled, maxPriceIncreaseEnabled
                , maxPriceDecreaseEnabled, maxTopWeightedSalesEnabled, maxPriceIndexEnabled;

            int.TryParse(parameters[0], out priceGroupId);
            PriceGroupName = parameters[1];
            int.TryParse(parameters[2], out priceRuleId);
            int.TryParse(parameters[3], out productTypeId);
            Buyer = parameters[4];

            bool.TryParse(parameters[5], out roundingRules);
            decimal.TryParse(parameters[6], out minMargin);
            bool.TryParse(parameters[7], out costPlus);

            bool.TryParse(parameters[8], out minSalesMarginEnabled);
            decimal.TryParse(parameters[9], out minSalesMargin);

            bool.TryParse(parameters[10], out maxPriceIncreaseEnabled);
            decimal.TryParse(parameters[11], out maxPriceIncrease);

            bool.TryParse(parameters[12], out maxPriceDecreaseEnabled);
            decimal.TryParse(parameters[13], out maxPriceDecrease);

            bool.TryParse(parameters[14], out maxTopWeightedSalesEnabled);
            decimal.TryParse(parameters[15], out maxTopWeightedSales);

            bool.TryParse(parameters[16], out maxPriceIndexEnabled);
            decimal.TryParse(parameters[17], out maxPriceIndex);

            bool.TryParse(parameters[18], out calculationMethodCheckEnabled);

            var splits = parameters[19].Split('|');
            foreach (string s in splits)
            {
                int subsidiaryId;
                if (int.TryParse(s, out subsidiaryId))
                {
                    subsidiaryList.Add(subsidiaryId);
                }
            }
            PriceRuleId = priceRuleId;
            PriceGroupId = priceGroupId;
            Subsidiaries = subsidiaryList;
            ProductTypeId = productTypeId;

            RoundingRules = roundingRules;
            CostPlus = costPlus;
            MinimumMargin = minMargin;

            MinSalesMarginEnabled = minSalesMarginEnabled;
            MaxPriceIndexEnabled = maxPriceIndexEnabled;
            MaxPriceIncreaseEnabled = maxPriceIncreaseEnabled;
            MaxPriceDecreaseEnabled = maxPriceDecreaseEnabled;
            MaxTopWeightedSalesEnabled = maxTopWeightedSalesEnabled;

            MinSalesMargin = minSalesMargin;
            MaxPriceIncrease = maxPriceIncrease;
            MaxPriceDecrease = maxPriceDecrease;
            MaxTopWeightedSales = maxTopWeightedSales;
            MaxPriceIndex = maxPriceIndex;
            CalculationMethodCheckEnabled = calculationMethodCheckEnabled;
        }
    }
}