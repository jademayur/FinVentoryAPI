using FinVentoryAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class OpeningItemBalance
    {
        [Key]
        public int OpeningBalanceId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }   // Always positive value
        public decimal Amount { get; set; }
        public Item Item { get; set; }

    }
}
