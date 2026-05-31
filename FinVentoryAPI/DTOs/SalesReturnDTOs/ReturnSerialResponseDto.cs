namespace FinVentoryAPI.DTOs.SalesReturnDTOs
{
    public class ReturnSerialResponseDto
    {
        public int Id { get; set; }
        public int DetailId { get; set; }
        public int SerialId { get; set; }
        public string? SerialNo { get; set; }
        public string? Status { get; set; }
    }
}
