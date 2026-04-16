using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class AccountLedgerPostingService : IAccountLedgerPostingService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public AccountLedgerPostingService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════
        // ADD SINGLE ENTRY
        // ════════════════════════════════════════════════
        public async Task AddEntryAsync(
            int companyId, int financialYearId,
            int accountId, int businessPartnerId,
            DateTime date, string voucherType, string voucherNo,
            decimal debit, decimal credit,
            string? remarks = null, int? createdBy = null)
        {
            _context.AccountLedgerPostings.Add(new AccountLedgerPosting
            {
                CompanyId = companyId,
                FinancialYearId = financialYearId,   // ✅
                AccountId = accountId,
                BusinessPartnerId = businessPartnerId,
                Date = date,
                VoucherType = voucherType,
                VoucherNo = voucherNo,
                Debit = debit,
                Credit = credit,
                Remarks = remarks,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════
        // ADD MULTIPLE ENTRIES
        // ════════════════════════════════════════════════
        public async Task AddEntriesAsync(
            int companyId, int financialYearId,
            DateTime date, string voucherType, string voucherNo,
            List<AccountLedgerLineDto> lines,
            int? createdBy = null)
        {
            foreach (var line in lines)
            {
                _context.AccountLedgerPostings.Add(new AccountLedgerPosting
                {
                    CompanyId = companyId,
                    FinancialYearId = financialYearId,   // ✅
                    AccountId = line.AccountId,
                    BusinessPartnerId = line.BusinessPartnerId,
                    Date = date,
                    VoucherType = voucherType,
                    VoucherNo = voucherNo,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    Remarks = line.Remarks,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════
        // REVERSE ENTRIES
        // ════════════════════════════════════════════════
        public async Task ReverseEntriesAsync(
            int companyId, int financialYearId,
            string originalVoucherNo,
            string reversalVoucherNo, DateTime reversalDate,
            int? modifiedBy = null)
        {
            var originals = await _context.AccountLedgerPostings
                .Where(p =>
                    p.CompanyId == companyId &&
                    p.FinancialYearId == financialYearId &&   // ✅
                    p.VoucherNo == originalVoucherNo &&
                    !p.IsDeleted)
                .ToListAsync();

            foreach (var orig in originals)
            {
                _context.AccountLedgerPostings.Add(new AccountLedgerPosting
                {
                    CompanyId = orig.CompanyId,
                    FinancialYearId = orig.FinancialYearId,   // ✅
                    AccountId = orig.AccountId,
                    BusinessPartnerId = orig.BusinessPartnerId,
                    Date = reversalDate,
                    VoucherType = orig.VoucherType + "-Reversal",
                    VoucherNo = reversalVoucherNo,
                    Debit = orig.Credit,   // ← flip
                    Credit = orig.Debit,    // ← flip
                    Remarks = $"Reversal of {originalVoucherNo}",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = modifiedBy,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════
        // GET LEDGER FOR ONE ACCOUNT
        // ════════════════════════════════════════════════
        public async Task<AccountLedgerResponseDto?> GetLedgerByAccountAsync(
            int accountId, DateTime? from, DateTime? to)
        {
            var companyId = _common.GetCompanyId();
            var financialYearId = _common.GetFinancialYearId();   // ✅ from token/session

            var account = await _context.Accounts
                .Include(a => a.AccountGroup)
                .FirstOrDefaultAsync(a =>
                    a.AccountId == accountId &&
                    a.CompanyId == companyId &&
                    !a.IsDeleted);

            if (account == null) return null;

            // ── Base filter — always scoped to company + financial year ──
            var baseFilter = _context.AccountLedgerPostings
                .Where(p =>
                    p.AccountId == accountId &&
                    p.CompanyId == companyId &&
                    p.FinancialYearId == financialYearId &&   // ✅
                    !p.IsDeleted);

            // ── Opening balance = entries BEFORE from date ────────────
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

            // ── Entries within date range ──────────────────────────────
            var query = baseFilter
                .Include(p => p.BusinessPartner)
                .AsQueryable();

            if (from.HasValue) query = query.Where(p => p.Date >= from.Value);
            if (to.HasValue) query = query.Where(p => p.Date <= to.Value);

            var entries = await query
                .OrderBy(p => p.Date)
                .ThenBy(p => p.PostingId)
                .ToListAsync();

            // ── Build running balance ──────────────────────────────────
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
        // GET ALL ACCOUNTS LEDGER SUMMARY
        // ════════════════════════════════════════════════
        public async Task<List<AccountLedgerResponseDto>> GetAllLedgersAsync(
            DateTime? from, DateTime? to, int? accountGroupId)
        {
            var companyId = _common.GetCompanyId();
            // FinancialYearId is applied inside GetLedgerByAccountAsync via _common ✅

            var accountsQuery = _context.Accounts
                .Where(a => a.CompanyId == companyId && !a.IsDeleted)
                .Include(a => a.AccountGroup)
                .AsQueryable();

            if (accountGroupId.HasValue)
                accountsQuery = accountsQuery
                    .Where(a => a.AccountGroupId == accountGroupId.Value);

            var accounts = await accountsQuery.ToListAsync();

            var result = new List<AccountLedgerResponseDto>();

            foreach (var account in accounts)
            {
                var ledger = await GetLedgerByAccountAsync(account.AccountId, from, to);
                if (ledger != null)
                    result.Add(ledger);
            }

            return result;
        }

        // ════════════════════════════════════════════════
        // DELETE SINGLE ENTRY  (soft delete)
        // ════════════════════════════════════════════════
        public async Task<bool> DeleteEntryAsync(int postingId)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var entry = await _context.AccountLedgerPostings
                .FirstOrDefaultAsync(p =>
                    p.PostingId == postingId &&
                    p.CompanyId == companyId &&
                    !p.IsDeleted);

            if (entry == null) return false;

            entry.IsDeleted = true;
            entry.IsActive = false;
            entry.ModifiedBy = userId;
            entry.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
