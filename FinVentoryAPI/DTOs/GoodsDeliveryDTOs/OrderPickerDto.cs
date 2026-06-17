namespace FinVentoryAPI.DTOs.GoodsDeliveryDTOs
{
    public class OrderPickerDto
    {
        public int OrderId { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal NetTotal { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }

        /// <summary>True when every line on this order has been fully delivered.</summary>
        public bool IsFullyDelivered { get; set; }

        public List<OrderPickerDetailDto> Details { get; set; } = new();
    }
}
