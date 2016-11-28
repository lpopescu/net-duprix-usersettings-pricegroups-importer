using System.Collections.Generic;
using System.Globalization;

using CsvHelper;

using PriceGroupWebservice.Dto;

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

        internal void Parse(CsvReader csvReader)
        {
            List<int> subsidiaryList = new List<int>();
            csvReader.Configuration.CultureInfo = CultureInfo.GetCultureInfo("en-GB");
            PriceGroupId = csvReader.GetField<int>(0);
            PriceGroupName = csvReader.GetField(1);
            PriceRuleId = csvReader.GetField<int>(2);
            ProductTypeId = csvReader.GetField<int>(3);
            Buyer = csvReader.GetField(4);

            var splits = csvReader.GetField(5).Split('|');
            foreach(string s in splits)
            {
                int subsidiaryId;
                if(int.TryParse(s, out subsidiaryId))
                {
                    subsidiaryList.Add(subsidiaryId);
                }
            }

            Subsidiaries = subsidiaryList;

            RoundingRules = csvReader.GetField<bool>(6);
            MinimumMargin = csvReader.GetField<decimal>(7);
            CostPlus = csvReader.GetField<bool>(8);
            MinSalesMargin = csvReader.GetField<decimal>(9);
            MaxPriceIncrease = csvReader.GetField<decimal>(10);
            MaxPriceDecrease = csvReader.GetField<decimal>(11);
            MaxTopWeightedSales = csvReader.GetField<decimal>(12);
            MaxPriceIndex = csvReader.GetField<decimal>(13);
            CalculationMethodCheckEnabled = csvReader.GetField<bool>(14);
        }

    }

    public class AutomationRuleComparer
    {
        public bool Equals(AutomationRuleRawDto x, DefaultAutomationRuleSettingDto y)
        {
            return y.IsPriceRuleCheckEnabled == x.CalculationMethodCheckEnabled &&
                   y.MaximumNegativePriceDifferencePercentage == x.MaxPriceDecrease &&
                   y.MaximumPositivePriceDifferencePercentage == x.MaxPriceIncrease &&
                   y.MaximumPriceIndex == x.MaxPriceIndex &&
                   y.MaximumToppedWeightedSales == x.MaxTopWeightedSales &&
                   y.MinimumSalesMarginPercentage == x.MinSalesMargin;
        }

        public bool Equals(AutomationRuleSettingDto x, DefaultAutomationRuleSettingDto y)
        {
            return y.IsPriceRuleCheckEnabled == x.IsPriceRuleCheckEnabled &&
                   y.MaximumNegativePriceDifferencePercentage == x.MaximumNegativePriceDifferencePercentage &&
                   y.MaximumPositivePriceDifferencePercentage == x.MaximumPositivePriceDifferencePercentage &&
                   y.MaximumPriceIndex == x.MaximumPriceIndex &&
                   y.MaximumToppedWeightedSales == x.MaximumToppedWeightedSales &&
                   y.MinimumSalesMarginPercentage == x.MinimumSalesMarginPercentage;
        }

    }
}