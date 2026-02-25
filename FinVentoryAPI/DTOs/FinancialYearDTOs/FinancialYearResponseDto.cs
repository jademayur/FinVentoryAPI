namespace FinVentoryAPI.DTOs.FinancialYearDTOs
{
    public class FinancialYearResponseDto
    {
        public int FinancialYearId { get; set; }
        public int CompanyId { get; set; }
        public string YearName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsClosed { get; set; }
    }
}
