namespace FinVentoryAPI.DTOs.DocumentSeriesMappingDTOs
{
    public class DocumentSeriesMappingResponseDto
    {
        public int MappingId { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public int SeriesId { get; set; }
        public string SeriesName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public int NextNumber { get; set; }
        public bool IsDefault { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsLocked { get; set; }
    }
}
