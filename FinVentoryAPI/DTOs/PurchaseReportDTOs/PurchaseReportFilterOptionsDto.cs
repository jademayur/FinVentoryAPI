namespace FinVentoryAPI.DTOs.PurchaseReportDTOs
{
    public class PurchaseReportFilterOptionsDto
    {
        public List<IdNameDto> BusinessPartners { get; set; } = new();
        public List<ItemOptionDto> Items { get; set; } = new();
        public List<IdNameDto> Locations { get; set; } = new();
        public List<string> GstTypes { get; set; } = new();
        public List<string> Statuses { get; set; } = new();
    }
}
