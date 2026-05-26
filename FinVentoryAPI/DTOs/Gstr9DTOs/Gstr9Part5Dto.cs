namespace FinVentoryAPI.DTOs.Gstr9DTOs
{
    public class Gstr9Part5Dto
    {
        /// <summary>10 — Supplies / tax declared through amendments in April–Sept of next FY</summary>
        public decimal SuppliesDeclaredAmendments { get; set; }

        /// <summary>11 — Reversal of ITC availed in previous FY</summary>
        public decimal ITCReversalPreviousFY { get; set; }
    }
}
