namespace FinVentoryAPI.DTOs.GoodsDeliveryDTOs
{
    public class LinkedOrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
    }
}
