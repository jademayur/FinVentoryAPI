namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class Gstr1CdnurSummaryDto
    {
        public string TaxPeriod { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;
        public List<Gstr1CdnurRowDto> Notes { get; set; } = new();

        public int TotalNotes => Notes.Count;
        public decimal TotalNoteValue => Notes.Sum(x => x.NoteValue);
        public decimal TotalIGST => Notes.Sum(x => x.IGSTAmount);
    }
}
