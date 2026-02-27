namespace FinVentoryAPI.DTOs.MenuGroupDTOs
{
    public class MenuGroupResponseDto
    {
        public int MenuGroupId { get; set; }
        public int ModuleId { get; set; }
        public string MenuGroupName { get; set; } 
        public int SortOrder { get; set; } 
        public string? Icon { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
