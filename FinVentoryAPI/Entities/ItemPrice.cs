namespace FinVentoryAPI.Entities
{
    public class ItemPrice
    {
        public int ItemPriceId { get; set; }
        public int ItemId { get; set; } 
        public Item Item { get; set; }
        public string PriceType { get; set; } = string.Empty;
        // MRP / Retail / Wholesale / Purchase
        public decimal Rate { get; set; }
        public bool IsTaxIncluded { get; set; }
      
    }
}
