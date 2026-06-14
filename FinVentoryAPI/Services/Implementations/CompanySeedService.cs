using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class CompanySeedService : ICompanySeedService
    {
        private readonly AppDbContext context;
        public CompanySeedService(AppDbContext context  )
        {
            this.context = context;
        }
        public async Task<SeedResultDto> SeedAllAsync(int companyId, int userId)
        {
            var result = new SeedResultDto();

            await using var tx = await context.Database.BeginTransactionAsync();
            try
            {
                result.AccountGroups = await SeedAccountGroupsAsync(companyId, userId);
                result.Accounts = await SeedAccountsAsync(companyId, userId);
                result.Taxes = await SeedTaxesAsync(companyId, userId);
                result.FinancialYear = await SeedFinancialYearAsync(companyId, userId);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            return result;
        }

        private async Task<int> SeedAccountGroupsAsync(int companyId, int userId)
        {
            if (await context.AccountGroups.AnyAsync(x => x.CompanyId == companyId && !x.IsDeleted))
                return 0;

            var groups = DefaultSeedData.AccountGroups.Select(g => new AccountGroup
            {
                CompanyId = companyId,
                GroupName = g.Name,
                GroupType = g.GroupType,
                BalanceTo = g.BalanceTo,
                SortOrder = g.Sort,
                ParentGroupId = null,
                IsActive = true,
                CreatedBy = userId,
            }).ToList();

            context.AccountGroups.AddRange(groups);
            await context.SaveChangesAsync();
            return groups.Count;
        }

        private async Task<int> SeedAccountsAsync(int companyId, int userId)
        {
            if (await context.Accounts.AnyAsync(x => x.CompanyId == companyId && !x.IsDeleted))
                return 0;

            // Build GroupName → AccountGroupId map from groups just inserted
            var groupMap = await context.AccountGroups
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .ToDictionaryAsync(x => x.GroupName, x => x.AccountGroupId);

            var accounts = DefaultSeedData.Accounts.Select(a => new Account
            {
                CompanyId = companyId,
                AccountName = a.Name,
                AccountCode = a.Code,
                AccountGroupId = groupMap[a.GroupName],
                AccountType = a.Type,
                BookType = a.Book,
                BookSubType = a.SubBook,
                IsActive = true,
                CreatedBy = userId,
            }).ToList();

            context.Accounts.AddRange(accounts);
            await context.SaveChangesAsync();
            return accounts.Count;
        }

        private async Task<int> SeedTaxesAsync(int companyId, int userId)
        {
            if (await context.Taxes.AnyAsync(x => x.CompanyId == companyId && !x.IsDeleted))
                return 0;

            var taxes = DefaultSeedData.Taxes.Select(t => new Tax
            {
                CompanyId = companyId,
                TaxName = t.Name,
                TaxType = t.Type,
                TaxRate = t.Rate,
                IGST = t.IGST,
                CGST = t.CGST,
                SGST = t.SGST,
                // Posting accounts left null — user can wire them up later
                IGSTPostingAccountId = null,
                CGSTPostingAccountId = null,
                SGSTPostingAccountId = null,
                IsActive = true,
                CreatedBy = userId,
            }).ToList();

            context.Taxes.AddRange(taxes);
            await context.SaveChangesAsync();
            return taxes.Count;
        }

        private async Task<bool> SeedFinancialYearAsync(int companyId, int userId)
        {
            if (await context.FinancialYears.AnyAsync(x => x.CompanyId == companyId))
                return false;

            var (start, end, name) = GetIndianFY(DateTime.Today);

            context.FinancialYears.Add(new FinancialYear
            {
                CompanyId = companyId,
                YearName = name,
                StartDate = start,
                EndDate = end,
                IsActive = true,
                IsClosed = false,
                CreatedBy = userId,
            });

            await context.SaveChangesAsync();
            return true;
        }

        private static (DateTime start, DateTime end, string name) GetIndianFY(DateTime today)
        {
            int startYear = today.Month >= 4 ? today.Year : today.Year - 1;
            return (
                new DateTime(startYear, 4, 1),
                new DateTime(startYear + 1, 3, 31),
                $"FY {startYear}-{(startYear + 1) % 100:D2}"   // "FY 2025-26"
            );
        }
    }
}
