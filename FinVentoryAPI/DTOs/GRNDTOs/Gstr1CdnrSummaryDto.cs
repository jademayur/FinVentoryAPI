namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class Gstr1CdnrSummaryDto
    {
        public string TaxPeriod { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;
        public List<Gstr1CdnrRowDto> CreditNotes { get; set; } = new();
        public List<Gstr1CdnrRowDto> DebitNotes { get; set; } = new();

        public int TotalCreditNotes => CreditNotes.Count;
        public int TotalDebitNotes => DebitNotes.Count;
        public decimal TotalCreditNoteValue => CreditNotes.Sum(x => x.NoteValue);
        public decimal TotalDebitNoteValue => DebitNotes.Sum(x => x.NoteValue);
    }
}
