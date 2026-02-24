namespace FinVentoryAPI.DTOs.CompanySelectionDtos
{
    public interface CompanySelectionDto
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
    }
}
