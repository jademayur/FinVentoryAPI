using FinVentoryAPI.Enums;

namespace FinVentoryAPI.Entities
{
    public class DefaultSeedData
    {
        // (GroupName, GroupType enum, BalanceTo enum, SortOrder)
        public static List<(string Name, GroupType GroupType, BalanceTo BalanceTo, int Sort)> AccountGroups =>
        [
            ("Fixed Assets",    GroupType.Asset,      BalanceTo.BalanceSheet,    1),
        ("Current Assets",      GroupType.Asset,      BalanceTo.BalanceSheet,    2),
        ("Cash & Bank",         GroupType.Asset,      BalanceTo.BalanceSheet,    3),
        ("Loans & Advances",    GroupType.Asset,      BalanceTo.BalanceSheet,    4),
        ("Capital Account",     GroupType.Liability, BalanceTo.BalanceSheet,    5),
        ("Current Liabilities", GroupType.Liability, BalanceTo.BalanceSheet,    6),
        ("Duties & Taxes",      GroupType.Liability, BalanceTo.BalanceSheet,    7),
        ("Sales Accounts",      GroupType.Income,      BalanceTo.ProfitAndLoss,   8),
        ("Other Income",        GroupType.Income,      BalanceTo.ProfitAndLoss,   9),
        ("Purchase Accounts",   GroupType.Expense,    BalanceTo.ProfitAndLoss,  10),
        ("Direct Expenses",     GroupType.Expense,    BalanceTo.ProfitAndLoss,  11),
        ("Indirect Expenses",   GroupType.Expense,    BalanceTo.ProfitAndLoss,  12),
    ];

        // (AccountName, AccountCode, GroupName, AccountType, BookType?, BookSubType?)
        public static List<(string Name, string Code, string GroupName, AccountType Type, BookType? Book, BookSubType? SubBook)> Accounts =>
        [
            ("Cash Account",            "AC001", "Cash & Bank",          AccountType.Head, BookType.CASH,       null),
        ("Bank Account",            "AC002", "Cash & Bank",          AccountType.Head, BookType.BANK,       null),
        ("Accounts Receivable",     "AC003", "Current Assets",       AccountType.General, null,                null),
        ("Stock in Hand",           "AC004", "Current Assets",       AccountType.General, null,                null),
        ("Accounts Payable",        "AC005", "Current Liabilities",  AccountType.General, null,                null),
        ("GST Input Tax Credit",    "AC006", "Duties & Taxes",       AccountType.General, null,                null),
        ("GST Output Tax",          "AC007", "Duties & Taxes",       AccountType.General, null,                null),
        ("IGST Payable",            "AC008", "Duties & Taxes",       AccountType.General, null,                null),
        ("TDS Payable",             "AC009", "Duties & Taxes",       AccountType.General, null,                null),
        ("Capital Account",         "AC010", "Capital Account",      AccountType.General, null,                null),
        ("Retained Earnings",       "AC011", "Capital Account",      AccountType.General, null,                null),
        ("Sales Account",           "AC012", "Sales Accounts",       AccountType.Head, BookType.SALE,       null),
        ("Purchase Account",        "AC013", "Purchase Accounts",    AccountType.Head, BookType.PRCH,       null),
        ("Salary & Wages",          "AC014", "Direct Expenses",      AccountType.General, null,                null),
        ("Freight & Cartage",       "AC015", "Direct Expenses",      AccountType.General, null,                null),
        ("Office Expenses",         "AC016", "Indirect Expenses",    AccountType.General, null,                null),
        ("Round Off",               "AC017", "Indirect Expenses",    AccountType.General, null,                null),
    ];

        // (TaxName, TaxType string, TaxRate, IGST, CGST, SGST)
        public static List<(string Name, string Type, decimal Rate, decimal IGST, decimal CGST, decimal SGST)> Taxes =>
        [
            ("GST Exempt",  "GST",  0m,   0m,   0m,   0m),
        ("GST @ 5%",    "GST",  5m,   5m,   2.5m, 2.5m),
        ("GST @ 12%",   "GST",  12m,  12m,  6m,   6m),
        ("GST @ 18%",   "GST",  18m,  18m,  9m,   9m),
        ("GST @ 28%",   "GST",  28m,  28m,  14m,  14m),
    ];
    }
}
