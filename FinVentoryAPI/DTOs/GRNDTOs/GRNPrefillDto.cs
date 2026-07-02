namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class GRNPrefillDto
    {
        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int? ContactPersonId { get; set; }      
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }
        public int? PurchaseStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public List<GRNPrefillDetailDto> Details { get; set; } = new();
    }
}
