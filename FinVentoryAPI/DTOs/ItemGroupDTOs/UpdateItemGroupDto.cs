using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.ItemGroupDTOs
{
    public class UpdateItemGroupDto
    {
        public int ItemGroupId { get; set; }
        [Required(ErrorMessage = "Item Group name is required.")]
        public string ItemGroupName { get; set; } = string.Empty;
        public int? ParentGroupId { get; set; }
        public string? GroupCode { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
