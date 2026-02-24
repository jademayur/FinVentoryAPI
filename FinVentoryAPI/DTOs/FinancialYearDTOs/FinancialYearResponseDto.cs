namespace FinVentoryAPI.DTOs.FinancialYearDTOs
{
    public class FinancialYearResponseDto
    {
        public int FinancialYearId { get; set; }
        public string YearName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; }
        public bool IsActive { get; set; }
    }
}
