// FinVentoryAPI/Services/Implementations/DashboardService.cs
using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.Dashboard;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public DashboardService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════════
        // SINGLE COMBINED ENDPOINT - Returns all dashboard data in one call
        // ════════════════════════════════════════════════════
        public async Task<DashboardDataDto> GetDashboardDataAsync(int months = 6)
        {
            return new DashboardDataDto
            {
                TodaySummary = await GetTodaySummaryAsync(),
                MonthlyTrend = await GetMonthlyTrendAsync(months),
                OverdueReceivables = await GetOverdueReceivablesAsync(),
                OverduePayables = await GetOverduePayablesAsync(),
                CashBankBalances = await GetCashBankBalancesAsync(),
                LowStockItems = await GetLowStockItemsAsync(),
                PendingDocs = await GetPendingDocsAsync()
            };
        }

        // ════════════════════════════════════════════════════
        // TODAY / MONTH SUMMARY CARDS
        // ════════════════════════════════════════════════════
        public async Task<TodaySummaryDto> GetTodaySummaryAsync()
        {
            var companyId = _common.GetCompanyId();
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var salesQuery = _context.SalesInvoiceMains
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.Status != "Cancelled" && !x.IsDeleted);

            var purchaseQuery = _context.PurchaseInvoiceMains
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.Status != "Cancelled" && !x.IsDeleted);

            var todaySales = await salesQuery
                .Where(x => x.InvoiceDate.Date == today)
                .SumAsync(x => (decimal?)x.NetTotal) ?? 0;

            var todaySalesCount = await salesQuery
                .Where(x => x.InvoiceDate.Date == today)
                .CountAsync();

            var monthSales = await salesQuery
                .Where(x => x.InvoiceDate >= monthStart)
                .SumAsync(x => (decimal?)x.NetTotal) ?? 0;

            var todayPurchase = await purchaseQuery
                .Where(x => x.InvoiceDate.Date == today)
                .SumAsync(x => (decimal?)x.NetTotal) ?? 0;

            var todayPurchaseCount = await purchaseQuery
                .Where(x => x.InvoiceDate.Date == today)
                .CountAsync();

            var monthPurchase = await purchaseQuery
                .Where(x => x.InvoiceDate >= monthStart)
                .SumAsync(x => (decimal?)x.NetTotal) ?? 0;

            return new TodaySummaryDto
            {
                TodaySales = todaySales,
                TodayPurchase = todayPurchase,
                MonthSales = monthSales,
                MonthPurchase = monthPurchase,
                TodaySalesInvoiceCount = todaySalesCount,
                TodayPurchaseInvoiceCount = todayPurchaseCount
            };
        }

        // ════════════════════════════════════════════════════
        // MONTH-WISE SALES VS PURCHASE TREND
        // ════════════════════════════════════════════════════
        public async Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(int months)
        {
            var companyId = _common.GetCompanyId();
            var startDate = DateTime.Today.AddMonths(-(months - 1));
            startDate = new DateTime(startDate.Year, startDate.Month, 1);

            var salesRaw = await _context.SalesInvoiceMains
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.InvoiceDate >= startDate
                            && x.Status != "Cancelled" && !x.IsDeleted)
                .GroupBy(x => new { x.InvoiceDate.Year, x.InvoiceDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.NetTotal) })
                .ToListAsync();

            var purchaseRaw = await _context.PurchaseInvoiceMains
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.InvoiceDate >= startDate
                            && x.Status != "Cancelled" && !x.IsDeleted)
                .GroupBy(x => new { x.InvoiceDate.Year, x.InvoiceDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.NetTotal) })
                .ToListAsync();

            var result = new List<MonthlyTrendDto>();
            for (var i = 0; i < months; i++)
            {
                var d = startDate.AddMonths(i);
                var sales = salesRaw.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0;
                var purchase = purchaseRaw.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0;

                result.Add(new MonthlyTrendDto
                {
                    Year = d.Year,
                    Month = d.Month,
                    MonthLabel = d.ToString("MMM yyyy"),
                    SalesTotal = sales,
                    PurchaseTotal = purchase
                });
            }
            return result;
        }

        // ════════════════════════════════════════════════════
        // OVERDUE SALES BILLS (RECEIVABLES)
        // ⚠ assumes SalesInvoiceMain has DueDate; adjust if it's
        // derived instead from BusinessPartner credit days
        // ════════════════════════════════════════════════════
        public async Task<List<OverdueReceivableDto>> GetOverdueReceivablesAsync()
        {
            var companyId = _common.GetCompanyId();
            var today = DateTime.Today;

            var invoices = await _context.SalesInvoiceMains.AsNoTracking()
                .Where(x => x.CompanyId == companyId
                            && x.Status != "Cancelled"
                            && !x.IsDeleted
                            && x.DueDate < today)
                .ToListAsync();

            if (!invoices.Any()) return new List<OverdueReceivableDto>();

            var partnerIds = invoices.Select(x => x.BusinessPartnerId).Distinct().ToList();
            var partners = await _context.BusinessPartners
                .Where(bp => partnerIds.Contains(bp.BusinessPartnerId))
                .ToDictionaryAsync(bp => bp.BusinessPartnerId);

            var invoiceIds = invoices.Select(x => x.InvoiceId).ToList();
            var paidTotals = await _context.IncomingPaymentAllocations.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.InvoiceId))
                .GroupBy(x => x.InvoiceId)
                .Select(g => new { InvoiceId = g.Key, Paid = g.Sum(x => x.AmountApplied) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.Paid);

            return invoices
                .Where(inv => (inv.NetTotal - paidTotals.GetValueOrDefault(inv.InvoiceId, 0)) > 0)
                .Select(inv => new OverdueReceivableDto
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceNo = inv.InvoiceNo,
                    InvoiceDate = inv.InvoiceDate,
                    DueDate = inv.DueDate,
                    OverdueDays = (today - inv.DueDate).Days,
                    BusinessPartnerName = partners.GetValueOrDefault(inv.BusinessPartnerId)?.BusinessPartnerName ?? "",
                    NetTotal = inv.NetTotal,
                    PaidAmount = paidTotals.GetValueOrDefault(inv.InvoiceId, 0),
                    OutstandingAmount = inv.NetTotal - paidTotals.GetValueOrDefault(inv.InvoiceId, 0)
                })
                .OrderByDescending(x => x.OverdueDays)
                .ToList();
        }

        // ════════════════════════════════════════════════════
        // OVERDUE PURCHASE BILLS (PAYABLES)
        // ════════════════════════════════════════════════════
        public async Task<List<OverduePayableDto>> GetOverduePayablesAsync()
        {
            var companyId = _common.GetCompanyId();
            var today = DateTime.Today;

            var invoices = await _context.PurchaseInvoiceMains.AsNoTracking()
                .Where(x => x.CompanyId == companyId
                            && x.Status != "Cancelled"
                            && !x.IsDeleted
                            && x.DueDate < today)
                .ToListAsync();

            if (!invoices.Any()) return new List<OverduePayableDto>();

            var partnerIds = invoices.Select(x => x.BusinessPartnerId).Distinct().ToList();
            var partners = await _context.BusinessPartners
                .Where(bp => partnerIds.Contains(bp.BusinessPartnerId))
                .ToDictionaryAsync(bp => bp.BusinessPartnerId);

            var invoiceIds = invoices.Select(x => x.InvoiceId).ToList();
            var paidTotals = await _context.OutgoingPaymentAllocations.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.BillId))
                .GroupBy(x => x.BillId)
                .Select(g => new { BillId = g.Key, Paid = g.Sum(x => x.AmountApplied) })
                .ToDictionaryAsync(x => x.BillId, x => x.Paid);

            return invoices
                .Where(inv => (inv.NetTotal - paidTotals.GetValueOrDefault(inv.InvoiceId, 0)) > 0)
                .Select(inv => new OverduePayableDto
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceNo = inv.InvoiceNo,
                    InvoiceDate = inv.InvoiceDate,
                    DueDate = inv.DueDate,
                    OverdueDays = (today - inv.DueDate).Days,
                    BusinessPartnerName = partners.GetValueOrDefault(inv.BusinessPartnerId)?.BusinessPartnerName ?? "",
                    NetTotal = inv.NetTotal,
                    PaidAmount = paidTotals.GetValueOrDefault(inv.InvoiceId, 0),
                    OutstandingAmount = inv.NetTotal - paidTotals.GetValueOrDefault(inv.InvoiceId, 0)
                })
                .OrderByDescending(x => x.OverdueDays)
                .ToList();
        }

        // ════════════════════════════════════════════════════
        // CASH & BANK BALANCES
        // ════════════════════════════════════════════════════
        public async Task<List<CashBankBalanceDto>> GetCashBankBalancesAsync()
        {
            var companyId = _common.GetCompanyId();

            // Pull all Cash/Bank accounts for this company
            var accounts = await _context.Accounts
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId
                            && (x.BookType == Enums.BookType.CASH || x.BookType == Enums.BookType.BANK))
                .ToListAsync();

            var accountIds = accounts.Select(a => a.AccountId).ToList();

            var balances = await _context.AccountLedgerPostings
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && accountIds.Contains(x.AccountId))
                .GroupBy(x => x.AccountId)
                .Select(g => new { AccountId = g.Key, Balance = g.Sum(x => x.Debit - x.Credit) })
                .ToListAsync();

            return accounts
                .Select(a => new CashBankBalanceDto
                {
                    AccountId = a.AccountId,
                    AccountName = a.AccountName,
                    AccountType = a.BookType == Enums.BookType.CASH ? "Cash" : "Bank",
                    BankName = a.BookType == Enums.BookType.BANK ? a.AccountName : null,
                    AccountNumber = null, // Account entity has no account number field
                    Balance = balances.FirstOrDefault(b => b.AccountId == a.AccountId)?.Balance ?? 0
                })
                .OrderBy(x => x.AccountType)
                .ToList();
        }

        // ════════════════════════════════════════════════════
        // LOW STOCK LIST
        // ════════════════════════════════════════════════════
        public async Task<List<LowStockItemDto>> GetLowStockItemsAsync()
        {
            var companyId = _common.GetCompanyId();

            var stockByItem = await _context.StockLedgers
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId)
                .GroupBy(x => x.ItemId)
                .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.Qty) }) // ✅ fixed: Qty is already signed
                .ToListAsync();

            var items = await _context.Items
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.ReorderLevel != null && x.ReorderLevel > 0)
                .Select(x => new { x.ItemId, x.ItemCode, x.ItemName, x.ReorderLevel })
                .ToListAsync();

            return items
                .Join(stockByItem, i => i.ItemId, s => s.ItemId, (i, s) => new LowStockItemDto
                {
                    ItemId = i.ItemId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    AvailableQty = s.Qty,
                    ReorderLevel = i.ReorderLevel,
                    //Unit = i.Unit
                })
                .Where(x => x.ReorderLevel != null && x.AvailableQty <= x.ReorderLevel)
                .OrderBy(x => x.AvailableQty)
                .ToList();
        }

        // ════════════════════════════════════════════════════
        // PENDING DOCS COUNTS
        // ════════════════════════════════════
        public async Task<PendingDocsDto> GetPendingDocsAsync()
        {
            var companyId = _common.GetCompanyId();

            var pendingGRN = await _context.GRNMains
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.Status != "Invoiced" && x.Status != "Closed" && !x.IsDeleted)
                .CountAsync();

            var pendingDelivery = await _context.GoodsDeliveryMains
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.Status != "Invoiced" && x.Status != "Closed" && !x.IsDeleted)
                .CountAsync();

            var pendingPO = await _context.PurchaseOrderMains
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.Status != "Closed" && x.Status != "Cancelled" && !x.IsDeleted)
                .CountAsync();

            var pendingSO = await _context.SalesOrderMains
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.Status != "Closed" && x.Status != "Cancelled" && !x.IsDeleted)
                .CountAsync();

            return new PendingDocsDto
            {
                PendingGRNCount = pendingGRN,
                PendingDeliveryCount = pendingDelivery,
                PendingPurchaseOrderCount = pendingPO,
                PendingSalesOrderCount = pendingSO
            };
        }
    }
}