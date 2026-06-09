namespace FinVentoryAPI.DTOs.PurchaseOrderDTOs
{
    public class PurchaseOrderResponseDto
    {
        public int OrderId { get; set; }
        public int FinYearId { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }

        public int BusinessPartnerId { get; set; }
        public string BusinessPartnerName { get; set; } = string.Empty;
        public string BusinessPartnerCode { get; set; } = string.Empty;

        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;

        public int? PurchaseStateCode { get; set; }
        public string? PurchaseStateName { get; set; }

        public int? BillStateCode { get; set; }
        public string? BillStateName { get; set; }

        public int? ContactPersonId { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonMobile { get; set; }

    
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
        public string? Remarks { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<PurchaseOrderDetailResponseDto> Details { get; set; } = new();
        public List<PurchaseOrderTaxDetailResponseDto> TaxDetails { get; set; } = new();
    }
}
