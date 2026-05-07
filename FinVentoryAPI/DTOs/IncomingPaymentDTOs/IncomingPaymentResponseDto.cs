namespace FinVentoryAPI.DTOs.IncomingPaymentDTOs
{
    public class IncomingPaymentResponseDto
    {
        public int PaymentId { get; set; }
        public string PaymentNo { get; set; } = string.Empty;
        public int FinYearId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public int BusinessPartnerId { get; set; }
        public string BusinessPartnerName { get; set; } = string.Empty;
        public string BusinessPartnerCode { get; set; } = string.Empty;

        public int DepositAccountId { get; set; }
        public string DepositAccountName { get; set; } = string.Empty;

        public string PaymentMode { get; set; } = string.Empty;
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? BankName { get; set; }
        public string? TransactionRef { get; set; }
        public string? Remarks { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal OnAccountAmount { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<IncomingPaymentAllocationResponseDto> Allocations { get; set; } = new();


    }
}
