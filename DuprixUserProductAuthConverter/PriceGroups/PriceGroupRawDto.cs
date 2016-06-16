namespace UserGroupsCsvToJson
{
    public class PriceGroupRawDto
    {
        public string Name { get; set; }
        public int SubsidiaryId { get; set; }
        public int ProductTypeId { get; set; }
        public int PriceRuleId { get; set; }
        public int ProductId { get; set; }

        public void Parse(string[] idStrings)
        {
            int priceRuleId;
            int productTypeId;
            int subsidiaryId;
            int productId;

            Name = idStrings[0];

            int.TryParse(idStrings[1], out subsidiaryId);
            int.TryParse(idStrings[2], out productTypeId);
            int.TryParse(idStrings[3], out priceRuleId);
            int.TryParse(idStrings[4], out productId);

            SubsidiaryId = subsidiaryId;
            PriceRuleId = priceRuleId;
            ProductTypeId = productTypeId;
            ProductId = productId;
        }
    }
}