namespace FinVentoryAPI.DTOs.OpeningBalanceDTOs
{
    public class OpeningBalanceResponseDto
    {
        public int TotalAccounts { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }
}
