namespace FinVentoryAPI.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Icon { get; set; }

        public virtual ICollection<RoleRight> RoleRights { get; set; }
              = new List<RoleRight>();

        //public virtual ICollection<RoleGeneralRight> GeneralRights { get; set; }     = new List<RoleGeneralRight>();
             

    }
}
