namespace FinVentoryAPI.Entities
{
    public class RoleRight
    {
        public int RoleRightId { get; set; }
        public int  RoleId { get; set; }

        public int ModuleId { get; set; }
        public int MenuItemId { get; set; }

        public bool CanView { get; set; } = false;
        public bool CanAdd  { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanPrint    { get; set; } = false;
        public bool CanExport { get; set; } = false;
        public bool CanApprove { get; set; } = false;

        public int GrantedBy { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        public virtual Role Role { get; set; } = null!;
        public virtual Module Module { get; set; } = null!;
        public virtual MenuItem MenuItem { get; set; } = null!;


    }
}
