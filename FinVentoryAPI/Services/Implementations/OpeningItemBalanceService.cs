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
        private readonly IStockLedgerService _stockLedgerService;

        public OpeningItemBalanceService(
            AppDbContext context,
            Common common,
            IStockLedgerService stockLedgerService)
        {
            _context = context;
            _common = common;
            _stockLedgerService = stockLedgerService;
        }

        // ────────────────────────────────────────────────────
        // SAVE  (full overwrite — delete old, insert new)
        // ────────────────────────────────────────────────────
        public async Task<OpeningItemBalanceResponseDto> SaveAsync(OpeningBalanceItemDto dto)
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();

            var financialYear = await _context.FinancialYears
                .Where(x => x.FinancialYearId == yearId)
                .Select(x => new { x.StartDate })
                .FirstOrDefaultAsync();

            if (financialYear == null)
                throw new Exception("Financial year not found.");

            // ── Validation ────────────────────────────────────
            if (dto.Items == null || !dto.Items.Any())
                throw new Exception("No data found.");

            if (dto.Items.Any(x => x.Amount <= 0))
                throw new Exception("Amount must be greater than zero.");

            if (dto.Items.GroupBy(x => x.ItemId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate Items not allowed.");

            // Validate batch/serial rules before touching the DB
            await ValidateBatchSerialAsync(dto.Items, companyId);

            // ── Remove old opening balance rows ───────────────
            var existing = _context.OpeningItemBalances
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId);
            _context.OpeningItemBalances.RemoveRange(existing);

            // ── Soft-delete old stock ledger entries ──────────
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

            // ── Soft-delete old opening-tagged batch/serial rows
            await SoftDeleteOpeningBatchSerialAsync(companyId, yearId, userId);

            // ── Insert new opening balance rows ───────────────
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

            // ── Create ItemBatch / ItemSerial records ─────────
            foreach (var itemDto in dto.Items)
                await ApplyBatchSerialAsync(itemDto, companyId, yearId, userId);

            await _context.SaveChangesAsync();

            // ── Post to Stock Ledger ──────────────────────────
            var stockLines = dto.Items.Select(x => new StockLedgerLineDto
            {
                ItemId = x.ItemId,
                Qty = x.Quantity,
                Rate = x.Rate,
                Remarks = "Opening Balance"
            }).ToList();

            await _stockLedgerService.AddEntriesAsync(
                companyId: companyId,
                warehouseId: null,
                date: financialYear.StartDate,
                voucherType: "Opening-Balance",
                voucherNo: $"OPB-{yearId}",
                businessPartnerId: null,
                lines: stockLines,
                createdBy: userId
            );

            return new OpeningItemBalanceResponseDto
            {
                TotalItem = dto.Items.Count,
                TotalAmount = dto.Items.Sum(x => x.Amount),
                TotalQty = dto.Items.Sum(x => x.Quantity)
            };
        }

        // ────────────────────────────────────────────────────
        // GET
        // ────────────────────────────────────────────────────
        public async Task<List<OpeningBalanceMatItemResponseDto>> GetAsync()
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();

            var rows = await _context.OpeningItemBalances
                .Include(x => x.Item)
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId)
                .ToListAsync();

            var result = new List<OpeningBalanceMatItemResponseDto>();

            foreach (var row in rows)
            {
                var responseDto = new OpeningBalanceMatItemResponseDto
                {
                    ItemId = row.ItemId,
                    ItemName = row.Item?.ItemName ?? string.Empty,
                    ItemManageBy = row.Item?.ItemManageBy.ToString(),
                    Quantity = row.Quantity,
                    Rate = row.Rate,
                    Amount = row.Amount,
                };

                if (row.Item?.ItemManageBy == ItemManageBy.Batch)
                {
                    responseDto.Batches = await _context.ItemBatches
                        .Where(b =>
                            b.CompanyId == companyId &&
                            b.ItemId == row.ItemId &&                          
                            !b.IsDeleted)
                        .Select(b => new OpeningBatchResponseDto
                        {
                            BatchId = b.BatchId,
                            BatchNo = b.BatchNo,
                            ManufactureDate = b.ManufactureDate,
                            ExpiryDate = b.ExpiryDate,
                            Qty = b.ReceivedQty,
                            AvailableQty = b.AvailableQty
                        })
                        .ToListAsync();
                }

                if (row.Item?.ItemManageBy == ItemManageBy.Serial)
                {
                    responseDto.Serials = await _context.ItemSerials
                        .Where(s =>
                            s.CompanyId == companyId &&
                            s.ItemId == row.ItemId &&                           
                            !s.IsDeleted)
                        .Select(s => new OpeningSerialResponseDto
                        {
                            SerialId = s.SerialId,
                            SerialNo = s.SerialNo,
                            Status = s.Status.ToString(),
                            WarrantyExpiry = s.WarrantyExpiry
                        })
                        .ToListAsync();
                }

                result.Add(responseDto);
            }

            return result;
        }

        // ────────────────────────────────────────────────────
        // DELETE
        // ────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync()
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();

            var data = await _context.OpeningItemBalances
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId)
                .ToListAsync();

            if (!data.Any()) return false;

            // Soft-delete opening-tagged batch/serial records
            await SoftDeleteOpeningBatchSerialAsync(companyId, yearId, userId);

            _context.OpeningItemBalances.RemoveRange(data);
            await _context.SaveChangesAsync();
            return true;
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Validates batch/serial rules for all items before any DB write.
        /// </summary>
        private async Task ValidateBatchSerialAsync(
            List<OpeningBalanceMatItemDto> items, int companyId)
        {
            foreach (var item in items)
            {
                var dbItem = await _context.Items
                    .FirstOrDefaultAsync(i =>
                        i.ItemId == item.ItemId &&
                        i.CompanyId == companyId &&
                        !i.IsDeleted)
                    ?? throw new Exception($"Item {item.ItemId} not found.");

                switch (dbItem.ItemManageBy)
                {
                    case ItemManageBy.Batch:
                        if (item.Batches == null || !item.Batches.Any())
                            throw new Exception(
                                $"Item '{dbItem.ItemName}' is Batch-managed. " +
                                "Please provide batch details.");

                        if (item.Batches.Sum(b => b.Qty) != item.Quantity)
                            throw new Exception(
                                $"Item '{dbItem.ItemName}': batch qty total must equal item quantity ({item.Quantity}).");

                        foreach (var b in item.Batches)
                            if (!b.BatchId.HasValue && string.IsNullOrWhiteSpace(b.BatchNo))
                                throw new Exception(
                                    $"Item '{dbItem.ItemName}': each batch must have a BatchId or BatchNo.");
                        break;

                    case ItemManageBy.Serial:
                        if (item.Serials == null || !item.Serials.Any())
                            throw new Exception(
                                $"Item '{dbItem.ItemName}' is Serial-managed. " +
                                "Please provide serial numbers.");

                        if (item.Serials.Count != (int)item.Quantity)
                            throw new Exception(
                                $"Item '{dbItem.ItemName}': serial count ({item.Serials.Count}) " +
                                $"must equal item quantity ({item.Quantity}).");

                        foreach (var s in item.Serials)
                            if (!s.SerialId.HasValue && string.IsNullOrWhiteSpace(s.SerialNo))
                                throw new Exception(
                                    $"Item '{dbItem.ItemName}': each serial must have a SerialId or SerialNo.");
                        break;

                        // Regular → nothing to validate
                }
            }
        }

        /// <summary>
        /// Creates ItemBatch / ItemSerial records for one item's opening entry.
        /// Tags each record with OpeningFinYearId so they can be reversed cleanly.
        /// </summary>
        private async Task ApplyBatchSerialAsync(
            OpeningBalanceMatItemDto itemDto, int companyId, int yearId, int userId)
        {
            var dbItem = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemId == itemDto.ItemId);

            if (dbItem == null) return;

            switch (dbItem.ItemManageBy)
            {
                case ItemManageBy.Batch:
                    await ApplyBatchesAsync(itemDto, companyId, yearId, userId);
                    break;

                case ItemManageBy.Serial:
                    await ApplySerialsAsync(itemDto, companyId, yearId, userId);
                    break;

                    // Regular → nothing to do
            }
        }

        private async Task ApplyBatchesAsync(
            OpeningBalanceMatItemDto itemDto, int companyId, int yearId, int userId)
        {
            foreach (var b in itemDto.Batches!)
            {
                if (b.BatchId.HasValue)
                {
                    // Existing batch — top up qty
                    var existing = await _context.ItemBatches
                        .FirstOrDefaultAsync(x =>
                            x.BatchId == b.BatchId &&
                            x.CompanyId == companyId &&
                            !x.IsDeleted)
                        ?? throw new Exception($"Batch {b.BatchId} not found.");

                    existing.ReceivedQty += b.Qty;
                    existing.AvailableQty += b.Qty;
                    existing.ModifiedBy = userId;
                    existing.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    // New batch — create it
                    var duplicate = await _context.ItemBatches
                        .AnyAsync(x =>
                            x.CompanyId == companyId &&
                            x.ItemId == itemDto.ItemId &&
                            x.BatchNo == b.BatchNo &&
                            !x.IsDeleted);

                    if (duplicate)
                        throw new Exception(
                            $"Batch '{b.BatchNo}' already exists for item {itemDto.ItemId}.");

                    _context.ItemBatches.Add(new ItemBatch
                    {
                        CompanyId = companyId,
                        ItemId = itemDto.ItemId,
                        BatchNo = b.BatchNo!,
                        ManufactureDate = b.ManufactureDate,
                        ExpiryDate = b.ExpiryDate,
                        ReceivedQty = b.Qty,
                        UsedQty = 0,
                        AvailableQty = b.Qty,                        
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }
        }

        private async Task ApplySerialsAsync(
            OpeningBalanceMatItemDto itemDto, int companyId, int yearId, int userId)
        {
            foreach (var s in itemDto.Serials!)
            {
                if (s.SerialId.HasValue)
                {
                    // Existing serial — just verify it is InStock
                    var existing = await _context.ItemSerials
                        .FirstOrDefaultAsync(x =>
                            x.SerialId == s.SerialId &&
                            x.CompanyId == companyId &&
                            !x.IsDeleted)
                        ?? throw new Exception($"Serial {s.SerialId} not found.");

                    if (existing.Status != SerialStatus.InStock)
                        throw new Exception(
                            $"Serial '{existing.SerialNo}' is not InStock (status: {existing.Status}).");
                    // Already InStock — no change needed
                }
                else
                {
                    // New serial — create it
                    var duplicate = await _context.ItemSerials
                        .AnyAsync(x =>
                            x.CompanyId == companyId &&
                            x.ItemId == itemDto.ItemId &&
                            x.SerialNo == s.SerialNo &&
                            !x.IsDeleted);

                    if (duplicate)
                        throw new Exception(
                            $"Serial '{s.SerialNo}' already exists for item {itemDto.ItemId}.");

                    _context.ItemSerials.Add(new ItemSerial
                    {
                        CompanyId = companyId,
                        ItemId = itemDto.ItemId,
                        SerialNo = s.SerialNo!,
                        Status = SerialStatus.InStock,
                        PurchaseDate = s.PurchaseDate,
                        WarrantyExpiry = s.WarrantyExpiry,                        
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }
        }

        /// <summary>
        /// Soft-deletes all ItemBatch and ItemSerial rows tagged with this
        /// financial year's opening balance. Called on overwrite (SaveAsync)
        /// and on DeleteAsync. Never touches GRN-created stock.
        /// </summary>
        private async Task SoftDeleteOpeningBatchSerialAsync(
            int companyId, int yearId, int userId)
        {
            var batches = await _context.ItemBatches
                .Where(b =>
                    b.CompanyId == companyId &&
                    b.FinYearId == yearId &&
                    !b.IsDeleted)
                .ToListAsync();

            foreach (var b in batches)
            {
                b.IsDeleted = true;
                b.IsActive = false;
                b.ModifiedBy = userId;
                b.ModifiedDate = DateTime.UtcNow;
            }

            var serials = await _context.ItemSerials
                .Where(s =>
                    s.CompanyId == companyId &&
                    s.FinYearId == yearId &&
                    !s.IsDeleted)
                .ToListAsync();

            foreach (var s in serials)
            {
                s.IsDeleted = true;
                s.IsActive = false;
                s.ModifiedBy = userId;
                s.ModifiedDate = DateTime.UtcNow;
            }

            // No SaveChangesAsync here — caller handles the flush
        }
    }
}