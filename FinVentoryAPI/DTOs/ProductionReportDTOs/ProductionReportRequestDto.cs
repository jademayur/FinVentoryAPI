namespace FinVentoryAPI.DTOs.ProductionReportDTOs
{
    public class ProductionReportRequestDto
    {
        public string ReportType { get; set; } = "ProductionRegister";
        // "ProductionRegister" | "ProductionSummary" | "MaterialConsumption"
        // "StatusSummary" | "MonthlyProduction"

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public List<int>? ItemIds { get; set; }
        public List<int>? Statuses { get; set; } // int values of ProductionOrderStatus enum

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
