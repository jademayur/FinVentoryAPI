using FinVentoryAPI.DTOs.OpeningItemBalanceDTOs;

namespace FinVentoryAPI.DTOs.OpeningItemBalanceDTOs
{
    public class OpeningBalanceItemDto
    {
        public List<OpeningBalanceMatItemDto> Items { get; set; } = new();
    }
}
