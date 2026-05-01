namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class SalesInvoiceDetailSerialResponseDto
    {
        public int Id { get; set; }
        public int DetailId { get; set; }
        public int SerialId { get; set; }
        public string? SerialNo { get; set; }
        public string? Status { get; set; }
    }
}
