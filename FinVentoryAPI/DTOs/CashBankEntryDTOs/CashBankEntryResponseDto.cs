namespace FinVentoryAPI.DTOs.CashBankEntryDTOs
{
    public class CashBankEntryResponseDto
    {
        public int CashBankEntryId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public string EntryNumber { get; set; } = null!;

        public int BookTypeId { get; set; }
        public string BookType { get; set; } = null!;

        public DateOnly EntryDate { get; set; }

        public int EntryTypeId { get; set; }
        public string EntryType { get; set; } = null!;

        public int HeadAccountId { get; set; }
        public string HeadAccountName { get; set; } = null!;
        public string? HeadAccountCode { get; set; }

        public string AccountDrCr { get; set; } = null!;

        public decimal TotalAmount { get; set; }
        public string? PaymentMode { get; set; }
        public string? ReferenceNo { get; set; }
        public DateOnly? ReferenceDate { get; set; }
        public string? Narration { get; set; }
        public string Status { get; set; } = null!;

        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<CashBankEntryLineResponseDto> Lines { get; set; } = new();
    }
}
