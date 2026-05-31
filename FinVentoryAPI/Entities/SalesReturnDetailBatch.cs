namespace FinVentoryAPI.Entities
{
    public class SalesReturnDetailBatch
    {
        public int Id { get; set; }
        public int DetailId { get; set; }
        public int ReturnId { get; set; }
        public int BatchId { get; set; }
        public decimal Qty { get; set; }

        public SalesReturnDetail? Detail { get; set; }
        public ItemBatch? Batch { get; set; }
    }
}
