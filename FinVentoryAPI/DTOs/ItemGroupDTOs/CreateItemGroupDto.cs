using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.ItemGroupDTOs
{
    public class CreateItemGroupDto
    {

        [Required(ErrorMessage = "Item Group Name is required.")]
        public string ItemGroupName { get; set; } = string.Empty;
        public int? ParentGroupId { get; set; }
        public string? GroupCode { get; set; }
    }
}
