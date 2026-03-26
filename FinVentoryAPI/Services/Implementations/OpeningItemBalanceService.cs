using FinVentoryAPI.Data;

using FinVentoryAPI.DTOs.OpeningItemBalanceDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class OpeningItemBalanceService : IOpeningItemBalanceService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public OpeningItemBalanceService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<OpeningItemBalanceResponseDto> SaveAsync(OpeningBalanceItemDto dto)
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            // 🔴 Validation
            if (dto.Items == null || !dto.Items.Any())
                throw new Exception("No data found.");

            if (dto.Items.Any(x => x.Amount <= 0))
                throw new Exception("Amount must be greater than zero.");

            if (dto.Items.GroupBy(x => x.ItemId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate Items not allowed.");

            // 🔁 Remove old opening (overwrite scenario)
            var existing = _context.OpeningItemBalances
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId);

            _context.OpeningItemBalances.RemoveRange(existing);

            // ➕ Insert new records
            var entities = dto.Items.Select(x => new OpeningItemBalance
            {
                CompanyId = companyId,
                FinancialYearId = yearId,
                ItemId = x.ItemId,
                Quantity = x.Quantity,
                Rate = x.Rate,
                Amount = x.Amount,                
            }).ToList();

            await _context.OpeningItemBalances.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // 📊 Summary
            var totalAmount = dto.Items
               .Sum(x => x.Amount);

            var totalQuantity = dto.Items
               .Sum(x => x.Quantity);

            return new OpeningItemBalanceResponseDto
            {
                TotalItem = dto.Items.Count,
                TotalAmount = totalAmount,
                TotalQty = totalQuantity
            };
        }

        public async Task<List<OpeningBalanceMatItemDto>> GetAsync()
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            return await _context.OpeningItemBalances
                .Include(x => x.Item)
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId)
                .Select(x => new OpeningBalanceMatItemDto
                {
                   ItemId = x.ItemId,
                   ItemName = x.Item.ItemName,
                   Quantity = (int)x.Quantity,
                   Rate = x.Rate,
                   Amount = x.Amount,
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync()
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            var data = await _context.OpeningItemBalances
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId)
                .ToListAsync();

            if (!data.Any())
                return false;

            _context.OpeningItemBalances.RemoveRange(data);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
