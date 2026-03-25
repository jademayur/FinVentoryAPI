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
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId) // ✅ IMPORTANT FILTER
                .ToListAsync();

            var result = data
                .GroupBy(x => x.Account.AccountGroup.GroupName)
                .Select(g => new BalanceGroupDto
                {
                    GroupName = g.Key,
                    Items = g.Select(x => new BalanceDto
                    {
                        AccountName = x.Account.AccountName,
                        Debit = x.BalanceType == BalanceType.Dr ? x.Amount : 0,
                        Credit = x.BalanceType == BalanceType.Cr ? x.Amount : 0
                    }).ToList()
                })
                .OrderBy(x => x.GroupName) // ✅ optional but good
                .ToList();

            return result;
        }


    }
}
