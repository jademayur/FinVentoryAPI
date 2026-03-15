namespace FinVentoryAPI.Entities
{
    public class Hsn : BaseEntity
    {
        public int HsnId { get; set; }
        public int CompanyId { get; set; }
        public string HsnName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string HSNType { get; set; } = string.Empty;
        public int TaxId { get; set; }
        public decimal? Cess { get; set; }
        public Tax? tax { get; set; }

    }
}
