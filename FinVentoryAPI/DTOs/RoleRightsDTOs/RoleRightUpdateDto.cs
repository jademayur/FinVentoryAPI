namespace FinVentoryAPI.DTOs.RoleRightsDTOs
{
    public class RoleRightUpdateDto
    {
        public int RoleRightId { get; set; }
        public int RoleId { get; set; }

        public int ModuleId { get; set; }
        public int MenuItemId { get; set; }

        public bool CanView { get; set; } = false;
        public bool CanAdd { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanPrint { get; set; } = false;
        public bool CanExport { get; set; } = false;
        public bool CanApprove { get; set; } = false;

        public int GrantedBy { get; set; }
        //public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    }
}
