using FinVentoryAPI.Enums;

namespace FinVentoryAPI.DTOs.OpeningBalanceDTOs
{
    public class OpeningBalanceItemDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public BalanceType BalanceType { get; set; }
    }
}
