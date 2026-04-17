using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class AccountLedgerService : IAccountLedgerService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public AccountLedgerService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════
        // GET LEDGER FOR ONE ACCOUNT
        // ════════════════════════════════════════════════
        public async Task<AccountLedgerResponseDto?> GetLedgerByAccountAsync(
            int accountId, DateTime? from, DateTime? to)
        {
            var companyId = _common.GetCompanyId();
            var financialYearId = _common.GetFinancialYearId();

            var account = await _context.Accounts
                .Include(a => a.AccountGroup)
                .FirstOrDefaultAsync(a =>
                    a.AccountId == accountId &&
                    a.CompanyId == companyId &&
                    !a.IsDeleted);

            if (account == null) return null;

            // ── Base filter ───────────────────────────────────
            var baseFilter = _context.AccountLedgerPostings
                .Where(p =>
                    p.AccountId == accountId &&
                    p.CompanyId == companyId &&
                    p.FinancialYearId == financialYearId &&
                    !p.IsDeleted);

            // ── Opening balance = entries BEFORE from date ────
            decimal openingBalance = 0;
            if (from.HasValue)
            {
                var opening = await baseFilter
                    .Where(p => p.Date < from.Value)
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        TotalDebit = g.Sum(p => (decimal?)p.Debit) ?? 0,
                        TotalCredit = g.Sum(p => (decimal?)p.Credit) ?? 0
                    })
                    .FirstOrDefaultAsync();

                if (opening != null)
                    openingBalance = opening.TotalDebit - opening.TotalCredit;
            }

            // ── Entries within date range ─────────────────────
            var query = baseFilter
                .Include(p => p.BusinessPartner)
                .AsQueryable();

            if (from.HasValue) query = query.Where(p => p.Date >= from.Value);
            if (to.HasValue) query = query.Where(p => p.Date <= to.Value);

            var entries = await query
                .OrderBy(p => p.Date)
                .ThenBy(p => p.PostingId)
                .ToListAsync();

            // ── Build running balance ─────────────────────────
            decimal running = openingBalance;
            var entryDtos = new List<AccountLedgerEntryDto>();

            foreach (var e in entries)
            {
                running += e.Debit - e.Credit;

                entryDtos.Add(new AccountLedgerEntryDto
                {
                    PostingId = e.PostingId,
                    Date = e.Date,
                    VoucherType = e.VoucherType,
                    VoucherNo = e.VoucherNo,
                    PartyName = e.BusinessPartner?.BusinessPartnerName,
                    Debit = e.Debit,
                    Credit = e.Credit,
                    Balance = running,
                    Remarks = e.Remarks
                });
            }

            return new AccountLedgerResponseDto
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                AccountCode = account.AccountCode ?? string.Empty,
                AccountGroupName = account.AccountGroup?.GroupName,
                OpeningBalance = openingBalance,
                TotalDebit = entryDtos.Sum(e => e.Debit),
                TotalCredit = entryDtos.Sum(e => e.Credit),
                ClosingBalance = running,
                Entries = entryDtos
            };
        }

        // ════════════════════════════════════════════════
        // GET ALL ACCOUNTS LEDGER SUMMARY  (single query)
        // ════════════════════════════════════════════════
        public async Task<List<AccountLedgerResponseDto>> GetAllLedgersAsync(
            DateTime? from, DateTime? to, int? accountGroupId)
        {
            var companyId = _common.GetCompanyId();
            var financialYearId = _common.GetFinancialYearId();

            // 1. Load accounts once
            var accountsQuery = _context.Accounts
                .Where(a => a.CompanyId == companyId && !a.IsDeleted)
                .Include(a => a.AccountGroup)
                .AsQueryable();

            if (accountGroupId.HasValue)
                accountsQuery = accountsQuery
                    .Where(a => a.AccountGroupId == accountGroupId.Value);

            var accounts = await accountsQuery.ToListAsync();
            var accountIds = accounts.Select(a => a.AccountId).ToHashSet();

            // 2. Load ALL postings in ONE query
            var allPostings = await _context.AccountLedgerPostings
                .Where(p =>
                    p.CompanyId == companyId &&
                    p.FinancialYearId == financialYearId &&
                    accountIds.Contains(p.AccountId) &&
                    !p.IsDeleted)
                .Include(p => p.BusinessPartner)
                .OrderBy(p => p.AccountId)
                .ThenBy(p => p.Date)
                .ThenBy(p => p.PostingId)
                .ToListAsync();

            // 3. Group in memory
            var postingsByAccount = allPostings.ToLookup(p => p.AccountId);
            var result = new List<AccountLedgerResponseDto>();

            foreach (var account in accounts)
            {
                var rows = postingsByAccount[account.AccountId].ToList();

                var openingBalance = from.HasValue
                    ? rows
                        .Where(p => p.Date < from.Value)
                        .Sum(p => p.Debit - p.Credit)
                    : 0;

                var periodRows = rows
                    .Where(p =>
                        (!from.HasValue || p.Date >= from.Value) &&
                        (!to.HasValue || p.Date <= to.Value))
                    .ToList();

                decimal running = openingBalance;
                var entryDtos = periodRows.Select(e =>
                {
                    running += e.Debit - e.Credit;
                    return new AccountLedgerEntryDto
                    {
                        PostingId = e.PostingId,
                        Date = e.Date,
                        VoucherType = e.VoucherType,
                        VoucherNo = e.VoucherNo,
                        PartyName = e.BusinessPartner?.BusinessPartnerName,
                        Debit = e.Debit,
                        Credit = e.Credit,
                        Balance = running,
                        Remarks = e.Remarks
                    };
                }).ToList();

                result.Add(new AccountLedgerResponseDto
                {
                    AccountId = account.AccountId,
                    AccountName = account.AccountName,
                    AccountCode = account.AccountCode ?? string.Empty,
                    AccountGroupName = account.AccountGroup?.GroupName,
                    OpeningBalance = openingBalance,
                    TotalDebit = entryDtos.Sum(e => e.Debit),
                    TotalCredit = entryDtos.Sum(e => e.Credit),
                    ClosingBalance = running,
                    Entries = entryDtos
                });
            }

            return result;
        }
    }
}
