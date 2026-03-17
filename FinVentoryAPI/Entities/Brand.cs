namespace FinVentoryAPI.Entities
{
    public class Brand : BaseEntity
    {
        public int BrandId { get; set; }
        public int CompanyId { get; set; }
        public string BrandName { get; set; } = string.Empty;

    }
}
