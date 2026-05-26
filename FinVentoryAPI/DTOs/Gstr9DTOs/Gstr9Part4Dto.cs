namespace FinVentoryAPI.DTOs.Gstr9DTOs
{
    public class Gstr9Part4Dto
    {
        /// <summary>9 — Tax paid through ITC and cash</summary>
        public TaxPaidRowDto IGST { get; set; } = new();
        public TaxPaidRowDto CGST { get; set; } = new();
        public TaxPaidRowDto SGST { get; set; } = new();
        public TaxPaidRowDto Cess { get; set; } = new();
    }
}
