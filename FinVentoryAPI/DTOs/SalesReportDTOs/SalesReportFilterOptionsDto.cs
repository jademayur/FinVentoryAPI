namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class SalesReportFilterOptionsDto
    {
        public List<IdNameDto> BusinessPartners { get; set; } = new();
        public List<ItemOptionDto> Items { get; set; } = new();
        public List<IdNameDto> SalesPersons { get; set; } = new();
        public List<IdNameDto> Locations { get; set; } = new();
        public List<string> GstTypes { get; set; } = new();
        public List<string> Statuses { get; set; } = new();
    }
}
