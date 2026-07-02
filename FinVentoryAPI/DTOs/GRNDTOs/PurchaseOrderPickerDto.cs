namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class PurchaseOrderPickerDto
    {
        public int PurchaseOrderId { get; set; }
        public string PurchaseOrderNo { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public decimal NetTotal { get; set; }      
        public bool IsFullyReceived { get; set; }
        public List<PurchaseOrderPickerDetailDto> Details { get; set; } = new();
    }
}
