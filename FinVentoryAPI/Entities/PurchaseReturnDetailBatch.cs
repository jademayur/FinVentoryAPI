namespace FinVentoryAPI.Entities
{
    public class PurchaseReturnDetailBatch
    {
        public int Id { get; set; }
        public int DetailId { get; set; }
        public int ReturnId { get; set; }
        public int BatchId { get; set; }
        public string BatchNo { get; set; }
        public decimal Qty { get; set; }

        public PurchaseReturnDetail? Detail { get; set; }
        public ItemBatch? Batch { get; set; }
    }
}
