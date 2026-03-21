namespace FinVentoryAPI.DTOs.ItemDTOs
{
    public class ItemPriceDto
    {
        public string PriceType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public bool IsTaxIncluded { get; set; }
    }
}
