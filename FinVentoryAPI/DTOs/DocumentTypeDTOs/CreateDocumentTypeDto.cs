using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.DocumentTypeDTOs
{
    public class CreateDocumentTypeDto
    {
        [Required(ErrorMessage = "Document type name is required.")]
        public string TypeName { get; set; } = string.Empty;
    }
}
