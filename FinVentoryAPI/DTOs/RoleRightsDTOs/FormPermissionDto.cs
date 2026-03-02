namespace FinVentoryAPI.DTOs.RoleRightsDTOs
{
    public class FormPermissionDto
    {
        public int MenuItemId { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPrint { get; set; }
        public bool CanExport { get; set; }
        public bool CanApprove { get; set; }

    }
}
