using FinVentoryAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.AccountGroupDTOs
{
    public class CreateAccountGroupDto
    {

        [Required(ErrorMessage = "Group name is required.")]
        public string GroupName { get; set; } = string.Empty;
        public int? ParentGroupId { get; set; }
        [Required(ErrorMessage = "Group Type is required.")]
        public GroupType GroupType { get; set; }
        [Required(ErrorMessage = "Balance To is required.")]
        public BalanceTo BalanceTo { get; set; }
        public int SortOrder { get; set; } = 0;
        public int CreatedBy { get; set; }

    }
}
