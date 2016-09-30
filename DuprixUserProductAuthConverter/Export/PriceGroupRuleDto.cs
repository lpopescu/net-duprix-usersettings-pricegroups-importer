using System.Collections.Generic;

namespace UserGroupsCsvToJson
{
    public class PriceGroupRuleDto
    {
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
    }
}