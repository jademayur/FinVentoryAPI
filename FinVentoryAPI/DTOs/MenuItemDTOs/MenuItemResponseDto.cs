using System.Reflection.Metadata.Ecma335;

namespace FinVentoryAPI.DTOs.MenuItemDTOs
{
    public class MenuItemResponseDto
    {
        public int MenuItemId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string? MenuItemIcon { get; set; }
        public int MenuItemSortOrder { get; set; }
        public bool MenuItemIsActive { get; set; }

        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string? ModuleIcon { get; set; }
        public int ModuleSortOrder { get; set; }
        public bool ModuleIsActive { get; set; }

        public int MenuGroupId { get; set; }
        public string MenuGroupName { get; set; } = string.Empty;
        public string? MenuGroupIcon { get; set; }
        public int MenuGroupSortOrder { get; set; }
        public bool MenuGroupIsActive { get; set; }

        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
    }
}
