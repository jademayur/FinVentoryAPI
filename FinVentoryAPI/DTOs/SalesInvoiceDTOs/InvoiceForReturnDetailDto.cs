namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class InvoiceForReturnDetailDto
    {
        public int DetailId { get; set; }            // ← becomes SourceDetailId on the return
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public string? ItemManageBy { get; set; }
        public int HsnId { get; set; }
        public string? HsnCode { get; set; }
        public string PriceType { get; set; } = string.Empty;

        public decimal OriginalQty { get; set; }
        public decimal AlreadyCopiedQty { get; set; }
        public decimal PendingQty { get; set; }

        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }

        public decimal IgstRate { get; set; }
        public decimal CgstRate { get; set; }
        public decimal SgstRate { get; set; }
        public decimal CessRate { get; set; }

        public int? IgstPostingAccountId { get; set; }
        public int? CgstPostingAccountId { get; set; }
        public int? SgstPostingAccountId { get; set; }
        public int? CessPostingAccountId { get; set; }

        public bool IsManualTax { get; set; }
        public int? ManualTaxId { get; set; }
    }
}
