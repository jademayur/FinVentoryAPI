using FinVentoryAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class OpeningBalance
    {
        public int OpeningBalanceId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }   // Always positive value
        public BalanceType BalanceType { get; set; } // Dr / Cr
        public Account Account { get; set; }
    }

}

