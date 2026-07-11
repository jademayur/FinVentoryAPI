namespace FinVentoryAPI.DTOs.Dashboard
{
    public class PendingDocsDto
    {
        public int PendingGRNCount { get; set; }
        public int PendingDeliveryCount { get; set; }
        public int PendingPurchaseOrderCount { get; set; }
        public int PendingSalesOrderCount { get; set; }
    }
}
