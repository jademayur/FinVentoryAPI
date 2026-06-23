namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1DocSeriesRowDto
    {
        /// <summary>Nature of the document (e.g. Tax Invoice, Credit Note)</summary>
        public string NatureOfDocument { get; set; } = string.Empty;

        /// <summary>Lowest serial number issued in the period</summary>
        public string? SerialNoFrom { get; set; }

        /// <summary>Highest serial number issued in the period</summary>
        public string? SerialNoTo { get; set; }

        /// <summary>Total documents issued (including cancelled)</summary>
        public int TotalIssued { get; set; }

        /// <summary>Total cancelled documents</summary>
        public int TotalCancelled { get; set; }

        /// <summary>Net documents = TotalIssued - TotalCancelled</summary>
        public int NetIssued => TotalIssued - TotalCancelled;
    }
}
