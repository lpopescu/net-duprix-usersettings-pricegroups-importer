using CsvHelper.Configuration;

namespace UserGroupsCsvToJson
{
    public class SubsidiaryMap : CsvClassMap<PriceGroupRuleDto>
    {
        public SubsidiaryMap()
        {
            AutoMap();
            Map(m => m.Subsidiaries).TypeConverter<SubsidiaryConverter>();
        }
    }
}