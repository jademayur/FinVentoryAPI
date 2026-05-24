namespace FinVentoryAPI.DTOs.Gstr3bDTOs
{
    public class TaxPayableSummaryDto
    {
        public TaxPayableRowDto IGST { get; set; } = new();
        public TaxPayableRowDto CGST { get; set; } = new();
        public TaxPayableRowDto SGST { get; set; } = new();
        public TaxPayableRowDto Cess { get; set; } = new();
    }
}
