using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.HsnDTOs
{
    public class CreateHsnDto
    {
        [Required(ErrorMessage = "Account name is required.")]
        public string HsnName { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required(ErrorMessage = "HSN Type is required.")]
        public string HsnType { get; set;} = string.Empty;
        [Required(ErrorMessage = "Tax is required.")]
        public int TaxId { get; set;} 
        public decimal? Cess { get; set; }



    }
}
