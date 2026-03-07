using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Enums
{
    public enum GroupType
    {
        Asset = 1,
        Liability = 2,
        Income = 3,
        Expense = 4
        
    }

    public enum BalanceTo
    {
        Trading = 1,
        [Display(Name = "Profit And Loss")]
        ProfitAndLoss = 2,
        [Display(Name = "Balance Sheet")]
        BalanceSheet = 3
    }

    public enum VoucherType
    {
       [Display(Name = "NO BOOK")]
        NO  = 0,
        [Display(Name = "CASH BOOK")]
        CASH  = 1,
        [Display(Name = "BANK BOOK")]
        BANK  = 2,
        [Display(Name = "SALES BOOK")]
        SALE  = 3,
        [Display(Name = "PURCHASE BOOK")]
        PRCH  = 4,
        [Display(Name = "J.V BOOK")]
        JV  = 5    	
    }
    
}
