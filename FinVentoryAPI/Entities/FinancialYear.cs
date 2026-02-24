using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class FinancialYear
    {
        [Key]
        public int FinancialYearId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(20)]
        public string YearName { get; set; }  // Example: 2025-2026

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsClosed { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
