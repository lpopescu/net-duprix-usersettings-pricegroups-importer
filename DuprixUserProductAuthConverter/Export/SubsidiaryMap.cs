using CsvHelper.Configuration;

using UserGroupsCsvToJson.PriceGroups;

namespace UserGroupsCsvToJson
{
    public class SubsidiaryMap : CsvClassMap<AutomationRuleRawDto>
    {
        public SubsidiaryMap()
        {
            Map(m => m.PriceGroupId);
            Map(m => m.PriceGroupName);
            Map(m => m.PriceRuleId);
            Map(m => m.ProductTypeId);
            Map(m => m.Buyer);
            Map(m => m.Subsidiaries).TypeConverter<SubsidiaryConverter>();
            Map(m => m.RoundingRules);
            Map(m => m.MinimumMargin);
            Map(m => m.CostPlus);
            Map(m => m.MinSalesMarginEnabled);
            Map(m => m.MinSalesMargin);
            Map(m => m.MaxPriceIncreaseEnabled);
            Map(m => m.MaxPriceIncrease);
            Map(m => m.MaxPriceDecreaseEnabled);
            Map(m => m.MaxPriceDecrease);
            Map(m => m.MaxTopWeightedSalesEnabled);
            Map(m => m.MaxTopWeightedSales);
            Map(m => m.MaxPriceIndexEnabled);
            Map(m => m.MaxPriceIndex);
            Map(m => m.CalculationMethodCheckEnabled);
        }
    }
}