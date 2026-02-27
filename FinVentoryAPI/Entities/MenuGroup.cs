namespace FinVentoryAPI.Entities
{
    public class MenuGroup
    {
        public int MenuGroupId { get; set; }
        public int ModuleId { get; set; }
        public string MenuGroupName { get; set; } = string.Empty;
        public int SortOrder { get; set; } = 0;
        public string? Icon { get; set; }
        public bool IsActive { get; set; } = false;

        public virtual Module Module { get; set; } = null!;
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();



    }
}
