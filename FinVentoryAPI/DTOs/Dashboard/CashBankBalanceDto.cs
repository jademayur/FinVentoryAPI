namespace FinVentoryAPI.DTOs.Dashboard
{
    public class CashBankBalanceDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }   // Bank name or "Cash"
        public string AccountType { get; set; }   // "Cash" | "Bank"
        public string BankName { get; set; }      // null for cash
        public string AccountNumber { get; set; } // null for cash
        public decimal Balance { get; set; }
    }
}
