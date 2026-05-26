namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1DocumentIssuedDto
    {
        public string DocType { get; set; } = string.Empty;   // "Tax Invoice", "Credit Note" etc.
        public string SeriesFrom { get; set; } = string.Empty;
        public string SeriesTo { get; set; } = string.Empty;
        public int TotalIssued { get; set; }
        public int Cancelled { get; set; }
        public int NetIssued => TotalIssued - Cancelled;
    }
}
