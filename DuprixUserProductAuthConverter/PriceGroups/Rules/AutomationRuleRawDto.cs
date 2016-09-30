using System.Collections.Generic;

namespace UserGroupsCsvToJson.PriceGroups
{
    public class AutomationRuleRawDto
    {
        //public string Buyer { get; set; }
        //public string PriceGroupName { get; set; }
        //public int SubsidiaryId { get; set; }
        //public int ProductTypeId { get; set; }
        //public decimal MinSalesMargin { get; set; }
        //public decimal MaxPriceIncrease { get; set; }
        //public decimal MaxPriceDecrease { get; set; }
        //public decimal MaxTopWeightedSales { get; set; }
        //public decimal MaxPriceIndex { get; set; }
        //public bool CalculationMethodCheck { get; set; }


        public int PriceGroupId { get; set; }
        public string PriceGroupName { get; set; }
        public int PriceRuleId { get; set; }
        public int ProductTypeId { get; set; }
        public string Buyer { get; set; }
        public IEnumerable<int> Subsidiaries { get; set; }
        public bool RoundingRules { get; set; }
        public double MinimumMargin { get; set; }
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
            bool calculationMethodCheck;

            int.TryParse(parameters[0], out priceGroupId);
            PriceGroupName = parameters[1];
            int.TryParse(parameters[2], out priceRuleId);
            int.TryParse(parameters[3], out productTypeId);
            Buyer = parameters[4];

            RoundingRules = bool.Parse(parameters[5]);
            decimal.TryParse(parameters[6], out minMargin);
            CostPlus= bool.Parse(parameters[7]);


            int.TryParse(parameters[3], out productTypeId);
            decimal.TryParse(parameters[4], out minSalesMargin);
            decimal.TryParse(parameters[5], out maxPriceIncrease);
            decimal.TryParse(parameters[6], out maxPriceDecrease);
            decimal.TryParse(parameters[7], out maxTopWeightedSales);
            decimal.TryParse(parameters[8], out maxPriceIndex);

            bool.TryParse(parameters[9], out calculationMethodCheck);

            var splits = parameters[12].Split('|');
            foreach (string s in splits)
            {
                int subsidiaryId;
                if (int.TryParse(s, out subsidiaryId))
                {
                    subsidiaryList.Add(subsidiaryId);
                }
            }

            Subsidiaries = subsidiaryList;            
            ProductTypeId = productTypeId;
            MinSalesMargin = minSalesMargin;
            MaxPriceIncrease = maxPriceIncrease;
            MaxPriceDecrease = maxPriceDecrease;
            MaxTopWeightedSales = maxTopWeightedSales;
            MaxPriceIndex = maxPriceIndex;
            CalculationMethodCheck = calculationMethodCheck;
        }
    }
}