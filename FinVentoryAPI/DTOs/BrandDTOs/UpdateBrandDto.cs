using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.BrandDTOs
{
    public class UpdateBrandDto
    {
        public int BrandId { get; set; }
        [Required(ErrorMessage = "Brand name is required.")]
        public string BrandName { get; set; } = string.Empty;
        public bool  IsActive { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

    }
}
