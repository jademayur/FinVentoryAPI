namespace FinVentoryAPI.DTOs.SalesReturnDTOs
{
    public class UpdateSalesReturnDetailDto
    {
        public int ItemId { get; set; }
        public string? PriceType { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }
        public int SourceDetailId { get; set; }
        public decimal OriginalQty { get; set; }
        public List<ReturnBatchDto>? Batches { get; set; }
        public List<ReturnSerialDto>? Serials { get; set; }
    }
}
