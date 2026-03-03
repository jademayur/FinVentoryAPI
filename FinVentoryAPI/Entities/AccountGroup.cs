using FinVentoryAPI.Enums;

namespace FinVentoryAPI.Entities
{
    public class AccountGroup : BaseEntity
    {
        public int AccountGroupId { get; set; }
        public int CompanyId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int? ParentGroupId { get; set; } 
        public GroupType GroupType { get; set; }
        public BalanceTo BalanceTo { get; set; }
        public int SortOrder { get; set; } = 0;

        public virtual AccountGroup? ParentGroup { get; set; }
        public virtual ICollection<AccountGroup> ChildGroups { get; set; } = new List<AccountGroup>();

    }
}
