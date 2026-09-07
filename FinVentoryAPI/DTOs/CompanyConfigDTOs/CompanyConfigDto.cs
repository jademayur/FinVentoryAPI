namespace FinVentoryAPI.DTOs.CompanyConfigDTOs
{
    public class CompanyConfigDto
    {
        public int ConfigId { get; set; }
        public string ConfigKey { get; set; } = string.Empty;
        public string? ConfigValue { get; set; }
        public string ConfigType { get; set; } = "string";
        public string? Description { get; set; }
    }

    public class UpsertCompanyConfigDto
    {
        public string ConfigKey { get; set; } = string.Empty;
        public string? ConfigValue { get; set; }
        public string ConfigType { get; set; } = "string";
        public string? Description { get; set; }
    }

    public class BulkUpsertCompanyConfigDto
    {
        public List<UpsertCompanyConfigDto> Configs { get; set; } = new();
    }
}
