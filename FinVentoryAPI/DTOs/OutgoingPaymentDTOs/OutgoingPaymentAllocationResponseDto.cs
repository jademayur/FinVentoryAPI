namespace FinVentoryAPI.DTOs.OutgoingPaymentDTOs
{
    public class OutgoingPaymentAllocationResponseDto
    {
        public int AllocationId { get; set; }
        public int PaymentId { get; set; }
        public int BillId { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public decimal BillTotal { get; set; }
        public decimal AmountApplied { get; set; }
    }
}
