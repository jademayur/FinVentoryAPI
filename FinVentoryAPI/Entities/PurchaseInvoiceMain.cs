using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class PurchaseInvoiceMain:BaseEntity
    {
        public int InvoiceId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;  // Auto-generated
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int PurchaseAccountId { get; set; }
        public decimal SubTotal { get; set; }       
        public decimal TaxAmount { get; set; }       
        public decimal CessAmount { get; set; }      
        public decimal RoundOff { get; set; }       
        public decimal NetTotal { get; set; }      
        public string? Remarks { get; set; }
        public string Status { get; set; } = "Draft";
        public int? SalesStateCode { get; set; }     
        public int? BillStateCode { get; set; }     
        public int? ContactPersonId { get; set; }   
        public int? SalesPersonId { get; set; }    
        public int? BillAddressId { get; set; }      
        public int? ShipAddressId { get; set; }     
        public string? TransportName { get; set; }
        public string? VehicleNo { get; set; }
        public string? LrNo { get; set; }
        public DateTime? LrDate { get; set; }
        public string? RefNo { get; set; }
        public string? RefDate { get; set; }
        public BusinessPartner? BusinessPartner { get; set; }
        public Location? Location { get; set; }

        [ForeignKey(nameof(PurchaseAccountId))]
        public Account? PurchaseAccount { get; set; }
        public BusinessPartnerContact? ContactPerson { get; set; }
        public SalesPerson? SalesPerson { get; set; }
        public BusinessPartnerAddress? BillAddress { get; set; }
        public BusinessPartnerAddress? ShipAddress { get; set; }
     //   public ICollection<PurchaseInvoiceDetail>? Details { get; set; }
      //  public ICollection<PurchaseInvoiceTaxDetail>? TaxDetails { get; set; }

    }
}
