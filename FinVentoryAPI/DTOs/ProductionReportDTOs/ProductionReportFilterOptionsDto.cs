namespace FinVentoryAPI.DTOs.ProductionReportDTOs
{
    public class ProductionReportFilterOptionsDto
    {
        public List<ProductionItemOptionDto> Items { get; set; } = new();
        public List<ProductionStatusOptionDto> Statuses { get; set; } = new();
    }

    public class ProductionItemOptionDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string? ItemCode { get; set; }
    }

    public class ProductionStatusOptionDto
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = "";
    }
}
