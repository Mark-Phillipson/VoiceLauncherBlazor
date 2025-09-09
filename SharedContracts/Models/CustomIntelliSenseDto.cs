namespace SharedContracts.Models
{
    public class CustomIntelliSenseDto
    {
        public int Id { get; set; }
        public LanguageDto? Language { get; set; }
        public string? DisplayValue { get; set; }
        public string? SendKeysValue { get; set; }
        public string? Remarks { get; set; }
        public string? DeliveryType { get; set; }
    }
}
