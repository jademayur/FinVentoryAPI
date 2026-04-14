using FinVentoryAPI.Data;

using FinVentoryAPI.DTOs.OpeningItemBalanceDTOs;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
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
        private readonly IStockLedgerService _stockLedgerService; // ✅ Inject

        public OpeningItemBalanceService(
            AppDbContext context,
            Common common,
            IStockLedgerService stockLedgerService) // ✅ Add to constructor
        {
            _context = context;
            _common = common;
            _stockLedgerService = stockLedgerService;
        }

        public async Task<OpeningItemBalanceResponseDto> SaveAsync(OpeningBalanceItemDto dto)
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();

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

            // ✅ Also reverse/delete old stock ledger entries for opening balance
            var oldLedgerEntries = await _context.StockLedgers
                .Where(sl =>
                    sl.CompanyId == companyId &&
                    sl.VoucherType == "Opening-Balance" &&
                    !sl.IsDeleted)
                .ToListAsync();

            foreach (var entry in oldLedgerEntries)
            {
                entry.IsDeleted = true;
                entry.IsActive = false;
                entry.ModifiedBy = userId;
                entry.ModifiedDate = DateTime.UtcNow;
            }

            // ➕ Insert new opening balance records
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
            await _context.SaveChangesAsync(); // Save both removals and new records

            // ✅ Push entries into Stock Ledger
            var stockLines = dto.Items.Select(x => new StockLedgerLineDto
            {
                ItemId = x.ItemId,
                Qty = x.Quantity,
                Rate = x.Rate,
                Remarks = "Opening Balance"
            }).ToList();

            await _stockLedgerService.AddEntriesAsync(
                companyId: companyId,
                warehouseId: null,               // or pass if you have a default warehouse
                date: DateTime.Today,            // or use financial year start date
                voucherType: "Opening-Balance",
                voucherNo: $"OPB-{yearId}",      // unique voucher number per year
                businessPartnerId: null,
                lines: stockLines,
                createdBy: userId
            );

            // 📊 Summary
            return new OpeningItemBalanceResponseDto
            {
                TotalItem = dto.Items.Count,
                TotalAmount = dto.Items.Sum(x => x.Amount),
                TotalQty = dto.Items.Sum(x => x.Quantity)
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
