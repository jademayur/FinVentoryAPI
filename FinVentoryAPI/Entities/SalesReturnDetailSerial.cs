namespace FinVentoryAPI.Entities
{
    public class SalesReturnDetailSerial
    {
        public int ReturnId { get; set; }
        public int DetailId { get; set; }
        public int SerialId { get; set; }
        public int Id { get; set; }

        public SalesReturnDetail? Detail { get; set; }
        public ItemSerial? Serial { get; set; }
    }
}
