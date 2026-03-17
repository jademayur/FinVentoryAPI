using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.BrandDTOs
{
    public class CreateBrandDto
    {

        [Required(ErrorMessage = "Brand name is required.")]
        public string BrandName { get; set; } = string.Empty;
    }
}
