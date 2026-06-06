namespace FinVentoryAPI.DTOs.SalesOrderDTOs
{
    public class UpdateSalesOrderDetailDto
    {
        public int ItemId { get; set; }
        public string? PriceType { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }
    }
}
