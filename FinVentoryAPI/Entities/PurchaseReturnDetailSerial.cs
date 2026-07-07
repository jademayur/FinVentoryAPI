namespace FinVentoryAPI.Entities
{
    public class PurchaseReturnDetailSerial
    {
        public int Id { get; set; }
        public int ReturnId { get; set; }
        public int DetailId { get; set; }
        public int SerialId { get; set; }
        public string SerialNo { get; set; }

        public PurchaseReturnDetail? Detail { get; set; }
        public ItemSerial? Serial { get; set; }
    }
}
