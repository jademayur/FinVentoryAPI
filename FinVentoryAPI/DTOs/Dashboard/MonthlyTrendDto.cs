namespace FinVentoryAPI.DTOs.Dashboard
{
    public class MonthlyTrendDto
    {
        public string MonthLabel { get; set; }   // e.g. "Jul 2026"
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal PurchaseTotal { get; set; }
    }
}
