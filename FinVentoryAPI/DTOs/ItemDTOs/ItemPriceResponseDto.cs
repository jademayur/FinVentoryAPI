namespace FinVentoryAPI.DTOs.ItemDTOs
{
    public class ItemPriceResponseDto
    {
         public int ItemPriceId { get; set; }
        public int ItemId { get; set; }
 
        /// <summary>MRP / Retail / Wholesale / Purchase</summary>
        public string PriceType { get; set; } = string.Empty;
 
        public decimal Rate { get; set; }
        public bool IsTaxIncluded { get; set; }
 
        //public DateTime? FromDate { get; set; }
        //public DateTime? ToDate { get; set; }
    }
}
