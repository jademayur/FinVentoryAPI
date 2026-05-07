namespace FinVentoryAPI.DTOs.IncomingPaymentDTOs
{
    public class IncomingPaymentAllocationResponseDto
    {
        public int AllocationId { get; set; }
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal AmountApplied { get; set; }
    }
}
