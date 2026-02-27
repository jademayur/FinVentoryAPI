namespace FinVentoryAPI.DTOs.MenuItemDTOs
{
    public class MenuItemCreateDto
    {
        public int MenuItemId { get; set; }
        public int MenuGroupId { get; set; }
        public int ModuleId { get; set; }
        public string MenuName { get; set; }
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        
    }
}
