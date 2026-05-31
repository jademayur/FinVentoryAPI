namespace FinVentoryAPI.DTOs.SalesReturnDTOs
{
    public class ReturnBatchDto
    {
        public string BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Qty { get; set; }
    }
}
