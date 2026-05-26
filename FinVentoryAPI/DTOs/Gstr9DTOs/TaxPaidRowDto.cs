namespace FinVentoryAPI.DTOs.Gstr9DTOs
{
    public class TaxPaidRowDto
    {
        public decimal TaxPayable { get; set; }
        public decimal PaidViaITC { get; set; }
        public decimal PaidViaCash { get; set; }
        public decimal TotalPaid => PaidViaITC + PaidViaCash;
    }
}
