using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.SeriesDTOs
{
    public class UpdateSeriesDto
    {
        public int? CompanyId { get; set; }
        public int? FinancialYearId { get; set; }
        public int? ModuleId { get; set; }

        [Required(ErrorMessage = "Document type is required")]
        [MaxLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? SeriesCode { get; set; }

        [Required(ErrorMessage = "Series name is required")]
        [MaxLength(50)]
        public string SeriesName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prefix is required")]
        [MaxLength(20)]
        [RegularExpression(@"^[A-Za-z0-9/-]+$", ErrorMessage = "Invalid prefix format")]
        public string Prefix { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Suffix { get; set; }

        [MaxLength(100)]
        public string? Format { get; set; }

        [Range(1, 20, ErrorMessage = "Document length must be between 1 and 20")]
        public int DocumentLength { get; set; } = 5;

        [Range(1, int.MaxValue, ErrorMessage = "Start number must be at least 1")]
        public int StartFromNumber { get; set; } = 1;

        public bool IsDefault { get; set; }
        public bool IsManual { get; set; }
        public bool IsActive { get; set; }
    }
}
