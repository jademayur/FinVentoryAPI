namespace FinVentoryAPI.DTOs.PurchaseOrderDTOs
{
    public class UpdatePurchaseOrderMainDto
    {
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }

        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }

        public int? PurchaseStateCode { get; set; }
        public int? BillStateCode { get; set; }

        public int? ContactPersonId { get; set; }
      
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }

        public decimal RoundOff { get; set; }
        public string? Remarks { get; set; }

        public List<UpdatePurchaseOrderDetailDto> Details { get; set; } = new();
    }
}
