namespace FinVentoryAPI.DTOs.SeriesDTOs
{
    public class SeriesResponseDto
    {
        public int SeriesId { get; set; }
        public int CompanyId { get; set; }
        public int? FinancialYearId { get; set; }
        public string? YearName { get; set; }
        public int? ModuleId { get; set; }
        public string? ModuleName { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string? SeriesCode { get; set; }
        public string SeriesName { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string? Suffix { get; set; }
        public string? Format { get; set; }
        public int DocumentLength { get; set; } = 5;
        public int StartFromNumber { get; set; } = 1;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NextNumber { get; set; }
        public bool IsDefault { get; set; }
        public bool IsManual { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
    }
}
