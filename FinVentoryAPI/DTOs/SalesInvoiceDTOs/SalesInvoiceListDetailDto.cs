namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class SalesInvoiceListDetailDto
    {
        public int DetailId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
    }
}
