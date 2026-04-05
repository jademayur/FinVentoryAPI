using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.SeriesDTOs
{
    public class UpdateSeriesDto
    {
        [Required(ErrorMessage = "Document type is required")]
        [MaxLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Series name is required")]
        [MaxLength(50)]
        public string SeriesName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prefix is required")]
        [MaxLength(20)]
        [RegularExpression(@"^[A-Za-z0-9/-]+$", ErrorMessage = "Invalid prefix format")]
        public string Prefix { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "Start number must be at least 1")]
        public int StartFromNumber { get; set; } = 1;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsManual { get; set; }
        public bool IsActive { get; set; }

        public int Modife { get; set; }


    }
}
