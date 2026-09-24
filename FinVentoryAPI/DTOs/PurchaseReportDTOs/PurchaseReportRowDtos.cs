namespace FinVentoryAPI.DTOs.PurchaseReportDTOs
{
    public class PurchaseRegisterRowDto
    {
        public string InvoiceNo { get; set; } = "";
        public string SupplierInvoiceNo { get; set; } = "";
        public string InvoiceDate { get; set; } = "";
        public string PartyName { get; set; } = "";
        public string? GstNo { get; set; }
        public string? GstType { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = "";
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal NetTotal { get; set; }
        public string? Remarks { get; set; }
    }

    public class PurchaseRegisterDetailsRowDto : PurchaseRegisterRowDto
    {
        public List<PurchaseRegisterItemLineDto> Items { get; set; } = new();
    }

    public class PurchaseRegisterItemLineDto
    {
        public string ItemName { get; set; } = "";
        public string? ItemCode { get; set; }
        public string? HsnCode { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class ItemWisePurchaseRowDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string? ItemCode { get; set; }
        public string? HsnCode { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTaxable { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalCess { get; set; }
        public decimal TotalNet { get; set; }
    }

    public class PartyWisePurchaseRowDto
    {
        public int BusinessPartnerId { get; set; }
        public string PartyName { get; set; } = "";
        public string? GstNo { get; set; }
        public string? GstType { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalSubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalCess { get; set; }
        public decimal TotalNet { get; set; }
    }

    public class TaxWisePurchaseRowDto
    {
        public string TaxName { get; set; } = "";
        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
    }

    public class MonthlySummaryRowDto
    {
        public string MonthLabel { get; set; } = "";
        public int Year { get; set; }
        public int Month { get; set; }
        public int InvoiceCount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal NetTotal { get; set; }
    }

    public class MonthlyGSTRowDto
    {
        public string MonthLabel { get; set; } = "";
        public int Year { get; set; }
        public int Month { get; set; }
        public string GstType { get; set; } = "";
        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
    }
}
