namespace FinVentoryAPI.DTOs.FinancialYearDTOs
{
    public class CreateFinancialYearDto
    {
        public int CompanyId { get; set; }
        public string YearName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? CreatedBy { get; set; }
    }
}
