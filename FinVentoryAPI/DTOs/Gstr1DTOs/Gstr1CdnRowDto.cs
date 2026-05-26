namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1CdnRowDto
    {
        public int NoteId { get; set; }
        public string NoteNo { get; set; } = string.Empty;
        public DateTime NoteDate { get; set; }
        public string NoteType { get; set; } = string.Empty; // "Credit" | "Debit"
        public string OriginalInvoiceNo { get; set; } = string.Empty;
        public DateTime? OriginalInvoiceDate { get; set; }
        public string RecipientGSTIN { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public int? PlaceOfSupply { get; set; }
        public bool IsInterState { get; set; }
        public decimal NoteValue { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax => IGSTAmount + CGSTAmount + SGSTAmount + CessAmount;
    }
}
