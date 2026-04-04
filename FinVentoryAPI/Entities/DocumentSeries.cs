using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class DocumentSeries
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
              
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NextNumber { get; set; } = 1;
      
        public bool IsDefault { get; set; } = false;
        public bool IsManual { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false;
              

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
