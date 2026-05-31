namespace FinVentoryAPI.DTOs.SalesReturnDTOs
{
    public class SalesReturnDetailResponseDto
    {
        public int DetailId { get; set; }
        public int ReturnId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public int HsnId { get; set; }
        public string HsnCode { get; set; } = string.Empty;
        public string? PriceType { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AddisDiscountAmount { get; set; }
        public bool IsTaxIncluded { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? ItemManageBy { get; set; }
        public List<ReturnBatchResponseDto>? Batches { get; set; }
        public List<ReturnSerialResponseDto>? Serials { get; set; }
        public List<SalesReturnTaxDetailResponseDto>? TaxDetails { get; set; }
    }
}
