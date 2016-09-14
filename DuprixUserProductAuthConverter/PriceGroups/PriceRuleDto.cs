namespace UserGroupsCsvToJson.PriceGroups
{
    public class AutomationRuleRawDto
    {
        public string Buyer { get; set; }
        public string PriceGroupName { get; set; }
        public int SubsidiaryId { get; set; }
        public int ProductTypeId { get; set; }
        public decimal MinSalesMargin { get; set; }
        public decimal MaxPriceIncrease { get; set; }
        public decimal MaxPriceDecrease { get; set; }
        public decimal MaxTopWeightedSales { get; set; }
        public decimal MaxPriceIndex { get; set; }
        public bool CalculationMethodCheck { get; set; }

        public void Parse(string[] parameters)
        {
            int subsidiaryId;
            int productTypeId;
            decimal minSalesMargin;
            decimal maxPriceIncrease;
            decimal maxPriceDecrease;
            decimal maxTopWeightedSales;
            decimal maxPriceIndex;
            bool calculationMethodCheck;

            Buyer = parameters[0];
            PriceGroupName = parameters[1];

            int.TryParse(parameters[2], out subsidiaryId);
            int.TryParse(parameters[3], out productTypeId);
            decimal.TryParse(parameters[4], out minSalesMargin);
            decimal.TryParse(parameters[5], out maxPriceIncrease);
            decimal.TryParse(parameters[6], out maxPriceDecrease);
            decimal.TryParse(parameters[7], out maxTopWeightedSales);
            decimal.TryParse(parameters[8], out maxPriceIndex);

            bool.TryParse(parameters[9], out calculationMethodCheck);

            SubsidiaryId = subsidiaryId;
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