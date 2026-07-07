using FinVentoryAPI.DTOs.SalesReturnDTOs;

namespace FinVentoryAPI.DTOs.PurchaseReturnDTOs
{
    public class UpdatePurchaseReturnDetailDto
    {
        public int ItemId { get; set; }
        public string? PriceType { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }
        public List<ReturnBatchDto>? Batches { get; set; }
        public List<ReturnSerialDto>? Serials { get; set; }
    }
}
