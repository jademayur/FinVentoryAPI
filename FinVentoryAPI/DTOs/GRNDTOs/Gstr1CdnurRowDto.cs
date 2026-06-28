namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class Gstr1CdnurRowDto
    {
        public int ReturnId { get; set; }
        public string ReturnNo { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }

        /// <summary>"C" = Credit Note | "D" = Debit Note</summary>
        public string NoteType { get; set; } = string.Empty;

        public string? OriginalInvoiceNo { get; set; }
        public DateTime? OriginalInvoiceDate { get; set; }

        public string PartyName { get; set; } = string.Empty;

        public int? PlaceOfSupply { get; set; }
        public bool IsInterState { get; set; }

        /// <summary>
        /// GSTR-1 CDNUR type:
        /// "B2CL" = inter-state note value > ₹2.5L
        /// "EXPWP" = export with payment
        /// "EXPWOP" = export without payment
        /// </summary>
        public string UrType { get; set; } = "B2CL";

        public decimal NoteValue { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
    }
}
