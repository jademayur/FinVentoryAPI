namespace FinVentoryAPI.DTOs.PurchaseReturnDTOs
{
    public class PurchaseReturnTaxDetailResponseDto
    {
        public int TaxDetailId { get; set; }
        public int DetailId { get; set; }
        public int TaxId { get; set; }
        public string TaxName { get; set; } = string.Empty;
        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal CessRate { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
    }
}
