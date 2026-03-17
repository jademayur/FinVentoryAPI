using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class ItemGroup : BaseEntity
    {

        [Required(ErrorMessage = "Item Group Name is required.")]
        public int ItemGroupId { get; set; }
        public int CompanyId { get; set; }
        public string ItemGroupName { get; set; } = string.Empty;
        public int? ParentGroupId { get; set; }
        public string? GroupCode { get; set; }

        public virtual ItemGroup? ParentGroup { get; set; }
        public virtual ICollection<ItemGroup> ChildGroups { get; set; } = new List<ItemGroup>();

    }
}
