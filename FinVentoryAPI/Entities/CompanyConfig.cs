namespace FinVentoryAPI.Entities
{
    public class CompanyConfig
    {
        public int ConfigId { get; set; }
        public int CompanyId { get; set; }
        public string ConfigKey { get; set; } = string.Empty;
        public string? ConfigValue { get; set; }
        public string ConfigType { get; set; } = "string";
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
