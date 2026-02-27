namespace FinVentoryAPI.Entities
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        public int MenuGroupId { get; set; }
        public int ModuleId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public virtual MenuGroup MenuGroup { get; set; } = null!;
        public virtual Module Module { get;set;  } = null!;

        public virtual ICollection<RoleRight> RoleRights { get; set; } = new List<RoleRight>();


    }
}
