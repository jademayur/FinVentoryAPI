using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.FinancialReportDTOs;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class FinancialReportService : IFinancialReportService
    {

        private readonly AppDbContext _context;
        private readonly Common _common;

        public FinancialReportService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<List<BalanceGroupDto>> GetOpeningTrialBalanceAsync()
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            var data = await _context.OpeningBalances
                .Include(x => x.Account)
                .ThenInclude(a => a.AccountGroup)
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId)
                .ToListAsync();

            var result = data
                .GroupBy(x => x.Account.AccountGroup.GroupName)
                .Select(g => new BalanceGroupDto
                {
                    GroupName = g.Key,
                    Items = g.Select(x => new BalanceDto
                    {
                        AccountId = x.Account.AccountId,              // ✅ ADDED
                        AccountName = x.Account.AccountName,
                        Debit = x.BalanceType == BalanceType.Dr ? x.Amount : 0,
                        Credit = x.BalanceType == BalanceType.Cr ? x.Amount : 0
                    }).ToList()
                })
                .OrderBy(x => x.GroupName)
                .ToList();

            return result;
        }


        public async Task<List<BalanceGroupDto>> GetAsOnDateTrialBalanceAsync(DateTime asOnDate)
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            // ── 1. Opening Balances ───────────────────────────────────────────
            var openingBalances = await _context.OpeningBalances
                .Include(x => x.Account)
                    .ThenInclude(a => a.AccountGroup)
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.FinancialYearId == yearId)
                .ToListAsync();

            // accountId → net opening amount (Dr = +, Cr = −)
            var openingMap = openingBalances
                .GroupBy(x => x.AccountId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.BalanceType == BalanceType.Dr ? x.Amount : -x.Amount)
                );

            // ── 2. Ledger Postings up to asOnDate ─────────────────────────────
            var postings = await _context.AccountLedgerPostings
                .Include(p => p.Account)
                    .ThenInclude(a => a.AccountGroup)
                .Where(p =>
                    p.CompanyId == companyId &&
                    p.FinancialYearId == yearId &&
                    p.Date <= asOnDate &&
                    !p.IsDeleted)
                .ToListAsync();

            // accountId → (totalDebit, totalCredit)
            var postingMap = postings
                .GroupBy(p => p.AccountId)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Debit: g.Sum(p => p.Debit),
                        Credit: g.Sum(p => p.Credit)
                    )
                );

            // ── 3. Merge: collect all distinct accounts across both sources ────
            var allAccounts = await _context.Accounts
                .Include(a => a.AccountGroup)
                .Where(a =>
                    a.CompanyId == companyId &&
                    !a.IsDeleted &&
                    (openingMap.Keys.Contains(a.AccountId) ||
                     postingMap.Keys.Contains(a.AccountId)))
                .ToListAsync();

            // ── 4. Compute final Dr/Cr per account ────────────────────────────
            var balanceDtos = allAccounts
                .Select(a =>
                {
                    openingMap.TryGetValue(a.AccountId, out var openingNet);
                    postingMap.TryGetValue(a.AccountId, out var txn);

                    decimal net = openingNet + txn.Debit - txn.Credit;

                    return new
                    {
                        Account = a,
                        GroupName = a.AccountGroup?.GroupName ?? "Ungrouped",
                        Debit = net > 0 ? net : 0,
                        Credit = net < 0 ? -net : 0
                    };
                })
                .Where(x => x.Debit != 0 || x.Credit != 0)
                .ToList();

            // ── 5. Group by Account Group ──────────────────────────────────────
            var result = balanceDtos
                .GroupBy(x => x.GroupName)
                .Select(g => new BalanceGroupDto
                {
                    GroupName = g.Key,
                    Items = g.Select(x => new BalanceDto
                    {
                        AccountId = x.Account.AccountId,              // ✅ ADDED
                        AccountName = x.Account.AccountName,
                        Debit = x.Debit,
                        Credit = x.Credit
                    }).ToList()
                })
                .OrderBy(x => x.GroupName)
                .ToList();

            return result;
        }

        // ════════════════════════════════════════════════
        // TRADING ACCOUNT
        // ════════════════════════════════════════════════
        public async Task<TradingAccountDto> GetTradingAccountAsync(DateTime asOnDate)
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            var groups = await GetGroupedBalancesAsync(
                companyId, yearId, asOnDate, BalanceTo.Trading);

            var incomeGroups = groups
                .Where(g =>
                    _context.AccountGroups.Any(ag =>
                        ag.GroupName == g.GroupName &&
                        ag.CompanyId == companyId &&
                        ag.GroupType == GroupType.Income))
                .ToList();

            var expenseGroups = groups
                .Where(g =>
                    _context.AccountGroups.Any(ag =>
                        ag.GroupName == g.GroupName &&
                        ag.CompanyId == companyId &&
                        ag.GroupType == GroupType.Expense))
                .ToList();

            decimal totalIncome = incomeGroups.SelectMany(g => g.Items).Sum(i => i.Credit);
            decimal totalExpense = expenseGroups.SelectMany(g => g.Items).Sum(i => i.Debit);
            decimal grossProfit = totalIncome - totalExpense;

            return new TradingAccountDto
            {
                IncomeGroups = incomeGroups,
                ExpenseGroups = expenseGroups,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                GrossProfit = grossProfit
            };
        }

        // ════════════════════════════════════════════════
        // PROFIT & LOSS
        // ════════════════════════════════════════════════
        public async Task<ProfitAndLossDto> GetProfitAndLossAsync(DateTime asOnDate)
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            var trading = await GetTradingAccountAsync(asOnDate);
            decimal grossProfit = trading.GrossProfit;

            var groups = await GetGroupedBalancesAsync(
                companyId, yearId, asOnDate, BalanceTo.ProfitAndLoss);

            var incomeGroups = groups
                .Where(g => _context.AccountGroups.Any(ag =>
                    ag.GroupName == g.GroupName &&
                    ag.CompanyId == companyId &&
                    ag.GroupType == GroupType.Income))
                .ToList();

            var expenseGroups = groups
                .Where(g => _context.AccountGroups.Any(ag =>
                    ag.GroupName == g.GroupName &&
                    ag.CompanyId == companyId &&
                    ag.GroupType == GroupType.Expense))
                .ToList();

            decimal indirectIncome = incomeGroups.SelectMany(g => g.Items).Sum(i => i.Credit);
            decimal indirectExpense = expenseGroups.SelectMany(g => g.Items).Sum(i => i.Debit);
            decimal netProfit = grossProfit + indirectIncome - indirectExpense;

            return new ProfitAndLossDto
            {
                GrossProfit = grossProfit,
                IncomeGroups = incomeGroups,
                ExpenseGroups = expenseGroups,
                IndirectIncome = indirectIncome,
                IndirectExpense = indirectExpense,
                NetProfit = netProfit
            };
        }

        // ════════════════════════════════════════════════
        // BALANCE SHEET
        // ════════════════════════════════════════════════
        public async Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime asOnDate)
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            var pnl = await GetProfitAndLossAsync(asOnDate);

            var groups = await GetGroupedBalancesAsync(
                companyId, yearId, asOnDate, BalanceTo.BalanceSheet);

            var assetGroups = groups
                .Where(g => _context.AccountGroups.Any(ag =>
                    ag.GroupName == g.GroupName &&
                    ag.CompanyId == companyId &&
                    ag.GroupType == GroupType.Asset))
                .ToList();

            var liabilityGroups = groups
                .Where(g => _context.AccountGroups.Any(ag =>
                    ag.GroupName == g.GroupName &&
                    ag.CompanyId == companyId &&
                    ag.GroupType == GroupType.Liability))
                .ToList();

            decimal totalAssets = assetGroups.SelectMany(g => g.Items).Sum(i => i.Debit);
            decimal totalLiabilities = liabilityGroups.SelectMany(g => g.Items).Sum(i => i.Credit)
                                       + pnl.NetProfit;

            return new BalanceSheetDto
            {
                AssetGroups = assetGroups,
                LiabilityGroups = liabilityGroups,
                NetProfit = pnl.NetProfit,
                TotalAssets = totalAssets,
                TotalLiabilities = totalLiabilities
            };
        }

        // ════════════════════════════════════════════════
        // ACCOUNT LEDGER DRILL-DOWN
        // ════════════════════════════════════════════════
        public async Task<AccountLedgerDto> GetAccountLedgerAsync(int accountId, DateTime asOnDate)
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            // ── Account name ──────────────────────────────────────────────────
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.AccountId == accountId &&
                    a.CompanyId == companyId &&
                    !a.IsDeleted);

            if (account is null)
                throw new KeyNotFoundException($"Account {accountId} not found.");

            // ── Opening Balance ───────────────────────────────────────────────
            var openingRows = await _context.OpeningBalances
                .Where(x =>
                    x.AccountId == accountId &&
                    x.CompanyId == companyId &&
                    x.FinancialYearId == yearId)
                .ToListAsync();

            // +ve = Dr opening, -ve = Cr opening
            decimal openingNet = openingRows
                .Sum(x => x.BalanceType == BalanceType.Dr ? x.Amount : -x.Amount);

            // ── Ledger Postings up to asOnDate ────────────────────────────────
            var postings = await _context.AccountLedgerPostings
                .Where(p =>
                    p.AccountId == accountId &&
                    p.CompanyId == companyId &&
                    p.FinancialYearId == yearId &&
                    p.Date <= asOnDate &&
                    !p.IsDeleted)
                .OrderBy(p => p.Date)
                .ThenBy(p => p.PostingId)
                .ToListAsync();

            // ── Build entries with running balance ────────────────────────────
            var entries = new List<LedgerEntryDto>();
            decimal running = openingNet;

            // Row 0 — Opening Balance
            entries.Add(new LedgerEntryDto
            {
                Date = null,
                Particulars = "Opening Balance",
                VoucherType = string.Empty,
                VoucherNo = string.Empty,
                Debit = openingNet > 0 ? openingNet : 0,
                Credit = openingNet < 0 ? -openingNet : 0,
                RunningBalance = Math.Abs(running),
                BalanceType = running >= 0 ? "Dr" : "Cr"
            });

            foreach (var p in postings)
            {
                running += p.Debit - p.Credit;

                entries.Add(new LedgerEntryDto
                {
                    Date = p.Date,
                    Particulars = p.Remarks ?? string.Empty,
                    VoucherType = p.VoucherType ?? string.Empty,
                    VoucherNo = p.VoucherNo ?? string.Empty,
                    Debit = p.Debit,
                    Credit = p.Credit,
                    RunningBalance = Math.Abs(running),
                    BalanceType = running >= 0 ? "Dr" : "Cr"
                });
            }

            // ── Closing totals ────────────────────────────────────────────────
            decimal closingDebit = entries.Sum(e => e.Debit);
            decimal closingCredit = entries.Sum(e => e.Credit);

            return new AccountLedgerDto
            {
                AccountName = account.AccountName,
                Entries = entries,
                ClosingDebit = closingDebit,
                ClosingCredit = closingCredit
            };
        }


        // ════════════════════════════════════════════════
        // PRIVATE HELPER — net balance per account
        // grouped by AccountGroup, filtered by BalanceTo
        // ════════════════════════════════════════════════
        private async Task<List<BalanceGroupDto>> GetGroupedBalancesAsync(
            int companyId, int yearId,
            DateTime asOnDate,
            BalanceTo balanceTo)
        {
            // ── Opening Balances ──────────────────────────────────
            var openingMap = (await _context.OpeningBalances
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.FinancialYearId == yearId)
                .ToListAsync())
                .GroupBy(x => x.AccountId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.BalanceType == BalanceType.Dr ? x.Amount : -x.Amount)
                );

            // ── Ledger Postings up to asOnDate ────────────────────
            var postingMap = (await _context.AccountLedgerPostings
                .Where(p =>
                    p.CompanyId == companyId &&
                    p.FinancialYearId == yearId &&
                    p.Date <= asOnDate &&
                    !p.IsDeleted)
                .ToListAsync())
                .GroupBy(p => p.AccountId)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Debit: g.Sum(p => p.Debit),
                        Credit: g.Sum(p => p.Credit)
                    )
                );

            // ── Accounts filtered by BalanceTo ────────────────────
            var accounts = await _context.Accounts
                .Include(a => a.AccountGroup)
                .Where(a =>
                    a.CompanyId == companyId &&
                    !a.IsDeleted &&
                    a.AccountGroup != null &&
                    a.AccountGroup.BalanceTo == balanceTo)
                .ToListAsync();

            // ── Compute net per account ───────────────────────────
            var rows = accounts
                .Select(a =>
                {
                    openingMap.TryGetValue(a.AccountId, out var openingNet);
                    postingMap.TryGetValue(a.AccountId, out var txn);

                    decimal net = openingNet + txn.Debit - txn.Credit;

                    return new
                    {
                        Account = a,
                        GroupName = a.AccountGroup!.GroupName,
                        SortOrder = a.AccountGroup!.SortOrder,
                        GroupType = a.AccountGroup!.GroupType,
                        Debit = net > 0 ? net : 0,
                        Credit = net < 0 ? -net : 0
                    };
                })
                .Where(x => x.Debit != 0 || x.Credit != 0)
                .ToList();

            // ── Group by AccountGroup ─────────────────────────────
            return rows
                .GroupBy(x => new { x.GroupName, x.SortOrder })
                .OrderBy(g => g.Key.SortOrder)
                .Select(g => new BalanceGroupDto
                {
                    GroupName = g.Key.GroupName,
                    Items = g.Select(x => new BalanceDto
                    {
                        AccountId = x.Account.AccountId,              // ✅ ADDED
                        AccountName = x.Account.AccountName,
                        Debit = x.Debit,
                        Credit = x.Credit
                    }).ToList()
                })
                .ToList();
        }

    }
}