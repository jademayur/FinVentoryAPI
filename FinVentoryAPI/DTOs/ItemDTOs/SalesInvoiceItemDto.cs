namespace FinVentoryAPI.DTOs.ItemDTOs
{
    public class SalesInvoiceItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }

        // HSN
        public int HsnId { get; set; }
        public string HsnCode { get; set; } = string.Empty;

        // Tax — from Hsn.Tax
        public int TaxId { get; set; }
        public string TaxName { get; set; } = string.Empty;
        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal CessRate { get; set; }

        // Posting Accounts
        public int? IGSTPostingAccountId { get; set; }
        public int? CGSTPostingAccountId { get; set; }
        public int? SGSTPostingAccountId { get; set; }
        public int? CessPostingAccountId { get; set; }

        // Prices
        public List<SalesInvoiceItemPriceDto> Prices { get; set; } = new();
    }

    public class SalesInvoiceItemPriceDto
    {
        public string PriceType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public bool IsTaxIncluded { get; set; }
    }
}
