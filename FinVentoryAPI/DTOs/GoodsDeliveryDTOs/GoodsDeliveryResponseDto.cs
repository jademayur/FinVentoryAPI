namespace FinVentoryAPI.DTOs.GoodsDeliveryDTOs
{
    public class GoodsDeliveryResponseDto
    {
        public int DeliveryId { get; set; }
        public int FinYearId { get; set; }
        public string DeliveryNo { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string? Remarks { get; set; }

        public int BusinessPartnerId { get; set; }
        public string BusinessPartnerName { get; set; } = string.Empty;
        public string BusinessPartnerCode { get; set; } = string.Empty;

        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;

        public int? SalesStateCode { get; set; }
        public string? SalesStateName { get; set; }
        public int? BillStateCode { get; set; }
        public string? BillStateName { get; set; }

        public int? ContactPersonId { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonMobile { get; set; }

        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }

        public int? BillAddressId { get; set; }
        public string? BillAddressLine { get; set; }
        public int? ShipAddressId { get; set; }
        public string? ShipAddressLine { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }
        public decimal Discount { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        /// <summary>Summary of linked source orders.</summary>
        public List<LinkedOrderSummaryDto> LinkedOrders { get; set; } = new();

        public List<GoodsDeliveryDetailResponseDto> Details { get; set; } = new();
        public List<GoodsDeliveryTaxDetailResponseDto> TaxDetails { get; set; } = new();
    }
}
