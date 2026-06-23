namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class DeliveryPickerDto
    {
        public int DeliveryId { get; set; }
        public string DeliveryNo { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public decimal NetTotal { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }

        public List<DeliveryPickerDetailDto> Details { get; set; } = new();
    }
}
