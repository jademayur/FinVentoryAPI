
namespace FinVentoryAPI.DTOs.OpeningItemBalanceDTOs
{
    public class OpeningBalanceMatItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }  
        public decimal Amount { get; set; }

        // Required when Item.ItemManageBy == Batch
        // Sum of all Batch.Qty must equal Quantity
        public List<OpeningBatchDto>? Batches { get; set; }

        // Required when Item.ItemManageBy == Serial
        // Count must equal Quantity
        public List<OpeningSerialDto>? Serials { get; set; }

    }
}
