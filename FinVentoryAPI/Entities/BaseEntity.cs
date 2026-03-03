namespace FinVentoryAPI.Entities
{
    public abstract class BaseEntity
    {
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
