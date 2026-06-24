namespace FinVentoryAPI.DTOs.SalesReturnDTOs
{
    public class CreateSalesReturnDetailDto
    {
        public int ItemId { get; set; }
        public string? PriceType { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }
        public int SourceDetailId { get; set; }   // 0 = not from copy
        public decimal OriginalQty { get; set; }   // 0 = not from copy
        public List<ReturnBatchDto>? Batches { get; set; }
        public List<ReturnSerialDto>? Serials { get; set; }

        public int? ManualTaxId { get; set; }
        public decimal? ManualIgstRate { get; set; }
        public decimal? ManualCgstRate { get; set; }
        public decimal? ManualSgstRate { get; set; }
        public decimal? ManualCessRate { get; set; }
    }
}
