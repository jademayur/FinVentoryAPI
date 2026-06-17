namespace FinVentoryAPI.DTOs.GoodsDeliveryDTOs
{
    public class UpdateGoodsDeliveryMainDto
    {
        public DateTime DeliveryDate { get; set; }
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string? Remarks { get; set; }

        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int? ContactPersonId { get; set; }
        public int? SalesPersonId { get; set; }
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }
        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public decimal RoundOff { get; set; }

        public List<UpdateGoodsDeliveryDetailDto> Details { get; set; } = new();
    }
}
