namespace FinVentoryAPI.DTOs.ProductionReportDTOs
{
    public class ProductionRegisterRowDto
    {
        public int ProductionOrderId { get; set; }
        public string? OrderNo { get; set; }
        public string? OrderDate { get; set; }
        public string ItemName { get; set; } = "";
        public string? ItemCode { get; set; }
        public string? BomName { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public string Status { get; set; } = "";
        public int StatusId { get; set; }
        public string? PlannedStartDate { get; set; }
        public string? PlannedEndDate { get; set; }
        public string? ActualCompletionDate { get; set; }
        public int? DaysTaken { get; set; }
        public string? Notes { get; set; }
    }

    public class ProductionSummaryRowDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string? ItemCode { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalPlanned { get; set; }
        public decimal TotalActual { get; set; }
        public decimal AvgYield { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class MaterialConsumptionRowDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string? ItemCode { get; set; }
        public decimal TotalPlannedConsumption { get; set; }
        public decimal TotalActualConsumption { get; set; }
        public decimal Variance { get; set; }
        public decimal? WastagePercent { get; set; }
    }

    public class StatusSummaryRowDto
    {
        public string StatusName { get; set; } = "";
        public int StatusId { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class MonthlyProductionRowDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthLabel { get; set; } = "";
        public int OrderCount { get; set; }
        public int CompletedCount { get; set; }
        public decimal TotalPlanned { get; set; }
        public decimal TotalActual { get; set; }
    }

    public class DailyProductionRowDto
    {
        public string Date { get; set; } = "";
        public int OrderCount { get; set; }
        public int CompletedCount { get; set; }
        public decimal TotalPlanned { get; set; }
        public decimal TotalActual { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class DailyProductionDetailsRowDto : DailyProductionRowDto
    {
        public List<ProductionOrderDetailLineDto> Orders { get; set; } = new();
    }

    public class MonthlyProductionDetailsRowDto : MonthlyProductionRowDto
    {
        public List<ProductionOrderDetailLineDto> Orders { get; set; } = new();
    }

    public class ProductionOrderDetailLineDto
    {
        public int ProductionOrderId { get; set; }
        public string? OrderNo { get; set; }
        public string? OrderDate { get; set; }
        public string ItemName { get; set; } = "";
        public string? ItemCode { get; set; }
        public string Status { get; set; } = "";
        public int StatusId { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public string? PlannedStartDate { get; set; }
        public string? PlannedEndDate { get; set; }
        public string? ActualCompletionDate { get; set; }
        public int? DaysTaken { get; set; }
    }
}
