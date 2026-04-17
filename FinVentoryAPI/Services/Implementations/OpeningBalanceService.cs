using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.DTOs.OpeningBalanceDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class OpeningBalanceService : IOpeningBalanceService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IAccountLedgerPostingService _accountLedgerService;

        public OpeningBalanceService(AppDbContext context, Common common, IAccountLedgerPostingService accountLedgerService)
        {
            _context = context;
            _common = common;
            _accountLedgerService = accountLedgerService;
        }

        public async Task<OpeningBalanceResponseDto> SaveAsync(OpeningBalanceDto dto)
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();

            // 🔴 Validation
            if (dto.Items == null || !dto.Items.Any())
                throw new Exception("No data found.");

            if (dto.Items.Any(x => x.Amount <= 0))
                throw new Exception("Amount must be greater than zero.");

            if (dto.Items.GroupBy(x => x.AccountId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate accounts not allowed.");

            // 🔁 Remove old opening balance records (overwrite scenario)
            var existing = _context.OpeningBalances
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId);

            _context.OpeningBalances.RemoveRange(existing);

            // ✅ Soft-delete old Account Ledger entries for opening balance
            var oldLedgerEntries = await _context.AccountLedgerPostings
                .Where(p =>
                    p.CompanyId == companyId &&
                    p.FinancialYearId == yearId &&
                    p.VoucherType == "Opening-Balance" &&
                    !p.IsDeleted)
                .ToListAsync();

            foreach (var entry in oldLedgerEntries)
            {
                entry.IsDeleted = true;
                entry.IsActive = false;
                entry.ModifiedBy = userId;
                entry.ModifiedDate = DateTime.UtcNow;
            }

            // ➕ Insert new opening balance records
            var entities = dto.Items.Select(x => new OpeningBalance
            {
                CompanyId = companyId,
                FinancialYearId = yearId,
                AccountId = x.AccountId,
                Amount = x.Amount,
                BalanceType = x.BalanceType
            }).ToList();

            await _context.OpeningBalances.AddRangeAsync(entities);
            await _context.SaveChangesAsync(); // ✅ Saves both soft-deletes + new records together

            // ✅ Push new entries into Account Ledger
            var ledgerLines = dto.Items.Select(x => new AccountLedgerLineDto
            {
                AccountId = x.AccountId,
                BusinessPartnerId = null,
                Debit = x.BalanceType == BalanceType.Dr ? x.Amount : 0,
                Credit = x.BalanceType == BalanceType.Cr ? x.Amount : 0,
                Remarks = "Opening Balance"
            }).ToList();

            await _accountLedgerService.AddEntriesAsync(
                companyId: companyId,
                financialYearId: yearId,
                date: DateTime.Today,
                voucherType: "Opening-Balance",
                voucherNo: $"OPB-{yearId}",
                lines: ledgerLines,
                createdBy: userId
            );

            // 📊 Summary
            var totalDebit = dto.Items
                .Where(x => x.BalanceType == BalanceType.Dr)
                .Sum(x => x.Amount);

            var totalCredit = dto.Items
                .Where(x => x.BalanceType == BalanceType.Cr)
                .Sum(x => x.Amount);

            return new OpeningBalanceResponseDto
            {
                TotalAccounts = dto.Items.Count,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit
            };
        }

        public async Task<List<OpeningBalanceItemDto>> GetAsync()
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            return await _context.OpeningBalances
                .Include(x => x.Account)
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId)
                .Select(x => new OpeningBalanceItemDto
                {
                    AccountId = x.AccountId,
                    AccountName = x.Account.AccountName,
                    Amount = x.Amount,
                    BalanceType = x.BalanceType
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync()
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            var data = await _context.OpeningBalances
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId)
                .ToListAsync();

            if (!data.Any())
                return false;

            _context.OpeningBalances.RemoveRange(data);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}