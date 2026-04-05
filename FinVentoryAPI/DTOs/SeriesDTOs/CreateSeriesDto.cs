using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.SeriesDTOs
{
    public class CreateSeriesDto
    {
        [Required(ErrorMessage = "Document type is required")]
        [MaxLength(50)]
        public string DocumentType { get; set; } = "Invoice";

        [Required(ErrorMessage = "Series name is required")]
        [MaxLength(50)]
        public string SeriesName { get; set; } = "Default";

        [Required(ErrorMessage = "Prefix is required")]
        [MaxLength(20)]
        [RegularExpression(@"^[A-Za-z0-9/-]+$", ErrorMessage = "Prefix can contain only letters, numbers, '/' and '-'")]
        public string Prefix { get; set; } = "INV";
        [Range(1, int.MaxValue, ErrorMessage = "Start number must be at least 1")]
        public int StartFromNumber { get; set; } = 1;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsManual { get; set; } = false;
    }
}
