using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class DocumentSeries : BaseEntity
    {
        [Key]
        public int SeriesId { get; set; }
        public int CompanyId { get; set; }
        public int? FinancialYearId { get; set; }
        public int? ModuleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? SeriesCode { get; set; }

        [MaxLength(50)]
        public string? SeriesName { get; set; }

        [MaxLength(20)]
        public string Prefix { get; set; } = "INV";

        [MaxLength(20)]
        public string? Suffix { get; set; }

        [MaxLength(100)]
        public string? Format { get; set; }

        public int DocumentLength { get; set; } = 5;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Start number must be at least 1")]
        public int StartFromNumber { get; set; } = 1;

        public int NextNumber { get; set; } = 1;

        public bool IsDefault { get; set; } = false;
        public bool IsManual { get; set; } = false;
        public bool IsLocked { get; set; } = false;
    }
}
