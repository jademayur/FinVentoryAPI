
namespace FinVentoryAPI.DTOs.OpeningItemBalanceDTOs
{
    public class OpeningBalanceMatItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }  
        public decimal Amount { get; set; }

    }
}
