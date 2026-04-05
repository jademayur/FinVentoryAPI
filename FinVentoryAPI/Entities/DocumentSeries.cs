using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class DocumentSeries : BaseEntity
    {
        [Key]
        public int SeriesId { get; set; }
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DocumentType { get; set; } = string.Empty; // Invoice, Order

        [MaxLength(50)]
        public string? SeriesName { get; set; } // Primary, Export

        [MaxLength(20)]
        public string Prefix { get; set; } = "INV";

        [Range(1, int.MaxValue, ErrorMessage = "Start number must be at least 1")]
        public int StartFromNumber { get; set; } = 1;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NextNumber { get; set; } = 1;      
        public bool IsDefault { get; set; } = false;
        public bool IsManual { get; set; } = false;       
        public bool IsLocked { get; set; } = false;           
       

    }
}
