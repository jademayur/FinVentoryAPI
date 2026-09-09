using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class DocType : BaseEntity
    {
        [Key]
        public int DocumentTypeId { get; set; }
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;
    }
}
