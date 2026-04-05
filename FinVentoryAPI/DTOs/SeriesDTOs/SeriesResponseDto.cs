using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.SeriesDTOs
{
    public class SeriesResponseDto
    {
        public int SeriesId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string SeriesName { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        
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