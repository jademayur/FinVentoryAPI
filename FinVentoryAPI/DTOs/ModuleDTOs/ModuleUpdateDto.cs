namespace FinVentoryAPI.DTOs.ModuleDTOs
{
    public class ModuleUpdateDto
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } 
        public string? Icon { get; set; }
        public int SortOrder { get; set; } 
        public bool IsActive { get; set; } 
    }
}
