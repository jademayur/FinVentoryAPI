namespace FinVentoryAPI.DTOs.FinancialYearDTOs
{
    public class UpdateFinancialYearDto
    {
        public int FinancialYearId { get; set; }
        public string YearName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
