namespace FinVentoryAPI.DTOs.Dashboard
{
    public class TodaySummaryDto
    {
        public decimal TodaySales { get; set; }
        public decimal TodayPurchase { get; set; }
        public decimal MonthSales { get; set; }
        public decimal MonthPurchase { get; set; }
        public int TodaySalesInvoiceCount { get; set; }
        public int TodayPurchaseInvoiceCount { get; set; }
    }
}
