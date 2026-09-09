using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.DocumentTypeDTOs
{
    public class UpdateDocumentTypeDto
    {
        public int DocumentTypeId { get; set; }

        [Required(ErrorMessage = "Document type name is required.")]
        public string TypeName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
