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
        // Exposes the real DB error instead of the generic EF wrapper
        // ────────────────────────────────────────────────────
        private async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Database error: {inner}");
            }
        }

        // ────────────────────────────────────────────────────
        // SAVE  (upsert — insert new, update existing)
        // ────────────────────────────────────────────────────
        public async Task<OpeningItemBalanceResponseDto> SaveAsync(OpeningBalanceItemDto dto)
        {
            var companyId = _common.GetCompanyId();
            var yearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();

            var financialYear = await _context.FinancialYears
                .Where(x => x.FinancialYearId == yearId)
                .Select(x => new { x.StartDate })
                .FirstOrDefaultAsync()
                ?? throw new Exception($"Financial year not found (yearId={yearId}). Check your auth token.");

            if (dto.Items == null || !dto.Items.Any())
                throw new Exception("No data found.");

            if (dto.Items.Any(x => x.Amount <= 0))
                throw new Exception("Amount must be greater than zero.");

            if (dto.Items.GroupBy(x => x.ItemId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate items are not allowed.");

            // ✅ Validate BEFORE opening transaction (read-only, no need to be inside)
            await ValidateBatchSerialAsync(dto.Items, companyId);

            // ── Begin Transaction ─────────────────────────────────────────────────
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ── Upsert opening balance rows ───────────────────────────────────
                var incomingItemIds = dto.Items.Select(x => x.ItemId).ToHashSet();

                var existingBalances = await _context.OpeningItemBalances
                    .Where(x => x.CompanyId == companyId && x.FinancialYearId == yearId)
                    .ToListAsync();

                var toRemove = existingBalances
                    .Where(x => !incomingItemIds.Contains(x.ItemId))
                    .ToList();
                _context.OpeningItemBalances.RemoveRange(toRemove);

                foreach (var itemDto in dto.Items)
                {
                    var existing = existingBalances.FirstOrDefault(x => x.ItemId == itemDto.ItemId);

                    if (existing != null)
                    {
                        existing.Quantity = itemDto.Quantity;
                        existing.Rate = itemDto.Rate;
                        existing.Amount = itemDto.Amount;
                    }
                    else
                    {
                        await _context.OpeningItemBalances.AddAsync(new OpeningItemBalance
                        {
                            CompanyId = companyId,
                            FinancialYearId = yearId,
                            ItemId = itemDto.ItemId,
                            Quantity = itemDto.Quantity,
                            Rate = itemDto.Rate,
                            Amount = itemDto.Amount,
                        });
                    }
                }

                await SaveChangesAsync(); // SaveChanges #1 — opening balance rows

                // ── Upsert batch / serial records ─────────────────────────────────
                foreach (var itemDto in dto.Items)
                    await UpsertBatchSerialAsync(itemDto, companyId, yearId, userId);

                await SaveChangesAsync(); // SaveChanges #2 — batch / serial rows

                // ── Upsert stock ledger entry ──────────────────────────────────────
                await UpsertStockLedgerAsync(
                    dto.Items, companyId, yearId,
                    financialYear.StartDate, userId);

                // ── Commit ────────────────────────────────────────────────────────
                await transaction.CommitAsync();

                return new OpeningItemBalanceResponseDto
                {
                    TotalItem = dto.Items.Count,
                    TotalAmount = dto.Items.Sum(x => x.Amount),
                    TotalQty = dto.Items.Sum(x => x.Quantity)
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // re-throw so the controller returns the real error message
            }
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
                            s.FinYearId == yearId &&
                            !s.IsDeleted)
                        .Select(s => new OpeningSerialResponseDto
                        {
                            SerialId = s.SerialId,
                            SerialNo = s.SerialNo,
                            Status = (int)s.Status,
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

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await SoftDeleteOpeningBatchSerialAsync(companyId, yearId, userId);

                _context.OpeningItemBalances.RemoveRange(data);
                await SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════

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
                                $"Item '{dbItem.ItemName}' is Batch-managed. Please provide batch details.");

                        if (item.Batches.Sum(b => b.Qty) != item.Quantity)
                            throw new Exception(
                                $"Item '{dbItem.ItemName}': batch qty total ({item.Batches.Sum(b => b.Qty)}) " +
                                $"must equal item quantity ({item.Quantity}).");

                        foreach (var b in item.Batches)
                            if (!b.BatchId.HasValue && string.IsNullOrWhiteSpace(b.BatchNo))
                                throw new Exception(
                                    $"Item '{dbItem.ItemName}': each batch must have a BatchId or BatchNo.");
                        break;

                    case ItemManageBy.Serial:
                        if (item.Serials == null || !item.Serials.Any())
                            throw new Exception(
                                $"Item '{dbItem.ItemName}' is Serial-managed. Please provide serial numbers.");

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

        private async Task UpsertBatchSerialAsync(
            OpeningBalanceMatItemDto itemDto, int companyId, int yearId, int userId)
        {
            var dbItem = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemId == itemDto.ItemId);

            if (dbItem == null) return;

            switch (dbItem.ItemManageBy)
            {
                case ItemManageBy.Batch:
                    await UpsertBatchesAsync(itemDto, companyId, yearId, userId);
                    break;

                case ItemManageBy.Serial:
                    await UpsertSerialsAsync(itemDto, companyId, yearId, userId);
                    break;
                    // Regular → nothing to do
            }
        }

        private async Task UpsertBatchesAsync(
            OpeningBalanceMatItemDto itemDto, int companyId, int yearId, int userId)
        {
            var existingBatches = await _context.ItemBatches
                .Where(b =>
                    b.CompanyId == companyId &&
                    b.ItemId == itemDto.ItemId &&
                    !b.IsDeleted)
                .ToListAsync();

            var incomingBatchIds = itemDto.Batches!
                .Where(b => b.BatchId.HasValue)
                .Select(b => b.BatchId!.Value)
                .ToHashSet();

            // Soft-delete removed batches
            foreach (var old in existingBatches.Where(b => !incomingBatchIds.Contains(b.BatchId)))
            {
                old.IsDeleted = true;
                old.IsActive = false;
                old.ModifiedBy = userId;
                old.ModifiedDate = DateTime.UtcNow;
            }

            foreach (var b in itemDto.Batches!)
            {
                if (b.BatchId.HasValue)
                {
                    // UPDATE
                    var existing = existingBatches
                        .FirstOrDefault(x => x.BatchId == b.BatchId)
                        ?? throw new Exception($"Batch {b.BatchId} not found.");

                    existing.BatchNo = b.BatchNo ?? existing.BatchNo;
                    existing.ManufactureDate = b.ManufactureDate;
                    existing.ExpiryDate = b.ExpiryDate;
                    existing.ReceivedQty = b.Qty;
                    existing.AvailableQty = b.Qty;
                    existing.ModifiedBy = userId;
                    existing.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    // INSERT — check duplicate BatchNo
                    var duplicate = await _context.ItemBatches
                        .AnyAsync(x =>
                            x.CompanyId == companyId &&
                            x.ItemId == itemDto.ItemId &&
                            x.BatchNo == b.BatchNo &&
                            !x.IsDeleted);

                    if (duplicate)
                        throw new Exception($"Batch '{b.BatchNo}' already exists for this item.");

                    _context.ItemBatches.Add(new ItemBatch
                    {
                        CompanyId = companyId,
                        ItemId = itemDto.ItemId,
                        FinYearId = yearId,
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

        private async Task UpsertSerialsAsync(
    OpeningBalanceMatItemDto itemDto, int companyId, int yearId, int userId)
        {
            // ← was: s.FinYearId == yearId  (wrong field — does not exist on ItemSerial)
            var existingSerials = await _context.ItemSerials
                .Where(s =>
                    s.CompanyId == companyId &&
                    s.ItemId == itemDto.ItemId &&
                    s.FinYearId == yearId &&   // ← FIXED
                    !s.IsDeleted)
                .ToListAsync();

            var incomingSerialIds = itemDto.Serials!
                .Where(s => s.SerialId.HasValue)
                .Select(s => s.SerialId!.Value)
                .ToHashSet();

            // Soft-delete serials that were removed from the incoming list
            foreach (var old in existingSerials.Where(s => !incomingSerialIds.Contains(s.SerialId)))
            {
                old.IsDeleted = true;
                old.IsActive = false;
                old.ModifiedBy = userId;
                old.ModifiedDate = DateTime.UtcNow;
            }

            foreach (var s in itemDto.Serials!)
            {
                if (s.SerialId.HasValue)
                {
                    // UPDATE existing serial
                    var existing = existingSerials
                        .FirstOrDefault(x => x.SerialId == s.SerialId)
                        ?? throw new Exception($"Serial {s.SerialId} not found.");

                    if (existing.Status != SerialStatus.InStock)
                        throw new Exception(
                            $"Serial '{existing.SerialNo}' is not InStock (status: {existing.Status}).");

                    existing.WarrantyExpiry = s.WarrantyExpiry;
                    existing.PurchaseDate = s.PurchaseDate;
                    existing.ModifiedBy = userId;
                    existing.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    // INSERT new serial — guard against duplicate SerialNo
                    var duplicate = await _context.ItemSerials
                        .AnyAsync(x =>
                            x.CompanyId == companyId &&
                            x.ItemId == itemDto.ItemId &&
                            x.SerialNo == s.SerialNo &&
                            !x.IsDeleted);

                    if (duplicate)
                        throw new Exception(
                            $"Serial '{s.SerialNo}' already exists for this item.");

                    _context.ItemSerials.Add(new ItemSerial
                    {
                        CompanyId = companyId,
                        ItemId = itemDto.ItemId,
                        FinYearId = yearId,           // ← FIXED (was FinYearId)
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

        private async Task UpsertStockLedgerAsync(
            List<OpeningBalanceMatItemDto> items,
            int companyId, int yearId,
            DateTime startDate, int userId)
        {
            var voucherNo = $"OPB-{yearId}";

            var oldLines = await _context.StockLedgers
                .Where(sl =>
                    sl.CompanyId == companyId &&
                    sl.VoucherNo == voucherNo &&
                    sl.VoucherType == "Opening-Balance" &&
                    !sl.IsDeleted)
                .ToListAsync();

            foreach (var line in oldLines)
            {
                line.IsDeleted = true;
                line.IsActive = false;
                line.ModifiedBy = userId;
                line.ModifiedDate = DateTime.UtcNow;
            }

            var stockLines = items.Select(x => new StockLedgerLineDto
            {
                ItemId = x.ItemId,
                Qty = x.Quantity,
                Rate = x.Rate,
                Remarks = "Opening Balance"
            }).ToList();

            await _stockLedgerService.AddEntriesAsync(
                companyId: companyId,
                warehouseId: null,
                date: startDate,
                voucherType: "Opening-Balance",
                voucherNo: voucherNo,
                businessPartnerId: null,
                lines: stockLines,
                createdBy: userId
            );
        }

        private async Task SoftDeleteOpeningBatchSerialAsync(
     int companyId, int yearId, int userId)
        {
            var batches = await _context.ItemBatches
                .Where(b =>
                    b.CompanyId == companyId &&
                    b.FinYearId == yearId &&   // ← FIXED
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
                    s.FinYearId == yearId &&   // ← FIXED
                    !s.IsDeleted)
                .ToListAsync();

            foreach (var s in serials)
            {
                s.IsDeleted = true;
                s.IsActive = false;
                s.ModifiedBy = userId;
                s.ModifiedDate = DateTime.UtcNow;
            }
        }
    }
}