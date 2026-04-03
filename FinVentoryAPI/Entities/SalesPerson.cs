using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class SalesPerson : BaseEntity
    {
        [Key]
        public int SalesPersonId { get; set; }
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SalesPersonCode { get; set; }

        [Required]
        [MaxLength(150)]
        public string SalesPersonName { get; set; }

        [MaxLength(15)]
        public string? Mobile { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        public decimal? CommissionPct { get; set; }  // e.g. 2.5 = 2.5%
            
       
    }
}
