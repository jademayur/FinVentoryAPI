namespace FinVentoryAPI.DTOs.SalesOrderDTOs
{
    public class CreateSalesOrderMainDto
    {
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Optional — if user selected a quotation
        public int? QuotationId { get; set; }
        public DateTime? QuotationDate { get; set; }
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }


        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int? ContactPersonId { get; set; }
        public int? SalesPersonId { get; set; }
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }
        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }

        public decimal RoundOff { get; set; }
        public string? Remarks { get; set; }

        public List<CreateSalesOrderDetailDto> Details { get; set; } = new();
    }
}
