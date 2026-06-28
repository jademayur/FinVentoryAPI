namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class Gstr1CdnrRowDto
    {
        public int ReturnId { get; set; }
        public string ReturnNo { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }

        /// <summary>"C" = Credit Note | "D" = Debit Note</summary>
        public string NoteType { get; set; } = string.Empty;

        public string? OriginalInvoiceNo { get; set; }
        public DateTime? OriginalInvoiceDate { get; set; }

        public string RecipientGSTIN { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;

        public int? PlaceOfSupply { get; set; }
        public bool IsInterState { get; set; }
        public bool IsReverseCharge { get; set; }

        public decimal NoteValue { get; set; }        // NetTotal
        public decimal TaxableValue { get; set; }     // SubTotal
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
    }
}
