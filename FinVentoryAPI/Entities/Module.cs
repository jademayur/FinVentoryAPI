namespace FinVentoryAPI.Entities
{
    public class Module
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public virtual ICollection<MenuGroup> MenuGroups { get; set; } = new List<MenuGroup>();
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();


    }
}
