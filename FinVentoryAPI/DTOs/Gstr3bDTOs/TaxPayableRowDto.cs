namespace FinVentoryAPI.DTOs.Gstr3bDTOs
{
    public class TaxPayableRowDto
    {
        public decimal TaxPayable { get; set; }
        public decimal ITCAvailable { get; set; }
        /// <summary>Cash ledger payment = TaxPayable – ITCAvailable (floor 0)</summary>
        public decimal CashLedgerPayment => Math.Max(0, TaxPayable - ITCAvailable);
    }
}
