// Services/Implementations/StockLedgerService.cs
using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class StockLedgerService : IStockLedgerService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public StockLedgerService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════
        // ADD SINGLE ENTRY
        // ════════════════════════════════════════════════
        public async Task AddEntryAsync(
            int companyId, int itemId, int? warehouseId,
            DateTime date, string voucherType, string voucherNo,
            int? businessPartnerId, decimal qty,
            decimal? rate = null, string? remarks = null,
            int? createdBy = null)
        {
            _context.StockLedgers.Add(new StockLedger
            {
                CompanyId = companyId,
                ItemId = itemId,
                WarehouseId = warehouseId,
                Date = date,
                VoucherType = voucherType,
                VoucherNo = voucherNo,
                BusinessPartnerId = businessPartnerId,
                Qty = qty,
                Rate = rate,
                Remarks = remarks,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════
        // ADD MULTIPLE ENTRIES  (multi-line vouchers)
        // ════════════════════════════════════════════════
        public async Task AddEntriesAsync(
            int companyId, int? warehouseId,
            DateTime date, string voucherType, string voucherNo,
            int? businessPartnerId,
            List<StockLedgerLineDto> lines,
            int? createdBy = null)
        {
            foreach (var line in lines)
            {
                _context.StockLedgers.Add(new StockLedger
                {
                    CompanyId = companyId,
                    ItemId = line.ItemId,
                    WarehouseId = warehouseId,
                    Date = date,
                    VoucherType = voucherType,
                    VoucherNo = voucherNo,
                    BusinessPartnerId = businessPartnerId,
                    Qty = line.Qty,
                    Rate = line.Rate,
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
        // REVERSE ENTRIES  (on voucher cancel)
        // ════════════════════════════════════════════════
        public async Task ReverseEntriesAsync(
            int companyId, string originalVoucherNo,
            string reversalVoucherNo, DateTime reversalDate,
            int? modifiedBy = null)
        {
            var originals = await _context.StockLedgers
                .Where(sl =>
                    sl.CompanyId == companyId &&
                    sl.VoucherNo == originalVoucherNo &&
                    !sl.IsDeleted)
                .ToListAsync();

            foreach (var orig in originals)
            {
                _context.StockLedgers.Add(new StockLedger
                {
                    CompanyId = orig.CompanyId,
                    ItemId = orig.ItemId,
                    WarehouseId = orig.WarehouseId,
                    Date = reversalDate,
                    VoucherType = orig.VoucherType + "-Reversal",
                    VoucherNo = reversalVoucherNo,
                    BusinessPartnerId = orig.BusinessPartnerId,
                    Qty = -orig.Qty,   // ← flip sign
                    Rate = orig.Rate,
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
        // GET LEDGER FOR ONE ITEM
        // ════════════════════════════════════════════════
        public async Task<StockLedgerResponseDto?> GetLedgerByItemAsync(
            int itemId, DateTime? from, DateTime? to)
        {
            var companyId = _common.GetCompanyId();

            var item = await _context.Items
                .Include(i => i.ItemGroup)
                .FirstOrDefaultAsync(i =>
                    i.ItemId == itemId &&
                    i.CompanyId == companyId &&
                    !i.IsDeleted);

            if (item == null) return null;

            // ── Opening stock = all entries BEFORE from date ──
            decimal openingStock = 0;
            if (from.HasValue)
            {
                openingStock = await _context.StockLedgers
                    .Where(sl =>
                        sl.ItemId == itemId &&
                        sl.CompanyId == companyId &&
                        sl.Date < from.Value &&
                        !sl.IsDeleted)
                    .SumAsync(sl => (decimal?)sl.Qty) ?? 0;
            }

            // ── Entries within date range ─────────────────────
            var query = _context.StockLedgers
                .Where(sl =>
                    sl.ItemId == itemId &&
                    sl.CompanyId == companyId &&
                    !sl.IsDeleted)
                .Include(sl => sl.Warehouse)
                .Include(sl => sl.BusinessPartner)
                .AsQueryable();

            if (from.HasValue) query = query.Where(sl => sl.Date >= from.Value);
            if (to.HasValue) query = query.Where(sl => sl.Date <= to.Value);

            var entries = await query
                .OrderBy(sl => sl.Date)
                .ThenBy(sl => sl.LedgerId)
                .ToListAsync();

            // ── Build running balance ─────────────────────────
            decimal running = openingStock;
            var entryDtos = new List<StockLedgerEntryDto>();

            foreach (var e in entries)
            {
                running += e.Qty;

                entryDtos.Add(new StockLedgerEntryDto
                {
                    LedgerId = e.LedgerId,
                    Date = e.Date,
                    VoucherType = e.VoucherType ?? string.Empty,
                    VoucherNo = e.VoucherNo ?? string.Empty,
                    PartyName = e.BusinessPartner?.BusinessPartnerName,
                    WarehouseName = e.Warehouse?.WarehouseName,
                    InQty = e.Qty > 0 ? e.Qty : 0,
                    OutQty = e.Qty < 0 ? -e.Qty : 0,
                    Balance = running,
                    Rate = e.Rate,
                    Remarks = e.Remarks
                });
            }

            // ── Unit name from BaseUnitId ─────────────────────
            var unitName = ((BaseUnit)item.BaseUnitId).ToString();

            return new StockLedgerResponseDto
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                ItemCode = item.ItemCode ?? string.Empty,
                Unit = unitName,
                ItemGroupName = item.ItemGroup?.ItemGroupName,
                OpeningStock = openingStock,
                TotalIn = entryDtos.Sum(e => e.InQty),
                TotalOut = entryDtos.Sum(e => e.OutQty),
                ClosingStock = running,
                Entries = entryDtos
            };
        }

        // ════════════════════════════════════════════════
        // GET ALL ITEMS LEDGER SUMMARY
        // ════════════════════════════════════════════════
  
        public async Task<List<StockLedgerResponseDto>> GetAllLedgersAsync(
            DateTime? from, DateTime? to, int? itemGroupId)
        {
            var companyId = _common.GetCompanyId();

            // 1. Load items once
            var itemsQuery = _context.Items
                .Where(i => i.CompanyId == companyId && !i.IsDeleted)
                .Include(i => i.ItemGroup)
                .AsQueryable();

            if (itemGroupId.HasValue)
                itemsQuery = itemsQuery.Where(i => i.ItemGroupId == itemGroupId.Value);

            var items = await itemsQuery.ToListAsync();
            var itemIds = items.Select(i => i.ItemId).ToHashSet();

            // 2. Load ALL ledger rows for these items in ONE query
            var allLedgers = await _context.StockLedgers
                .Where(sl =>
                    sl.CompanyId == companyId &&
                    itemIds.Contains(sl.ItemId) &&
                    !sl.IsDeleted)
                .Include(sl => sl.Warehouse)
                .Include(sl => sl.BusinessPartner)
                .OrderBy(sl => sl.ItemId)
                .ThenBy(sl => sl.Date)
                .ThenBy(sl => sl.LedgerId)
                .ToListAsync();

            // 3. Group in memory and build DTOs
            var ledgerByItem = allLedgers.ToLookup(sl => sl.ItemId);
            var result = new List<StockLedgerResponseDto>();

            foreach (var item in items)
            {
                var rows = ledgerByItem[item.ItemId].ToList();

                var openingStock = from.HasValue
                    ? rows.Where(sl => sl.Date < from.Value).Sum(sl => sl.Qty)
                    : 0;

                var periodRows = rows
                    .Where(sl =>
                        (!from.HasValue || sl.Date >= from.Value) &&
                        (!to.HasValue || sl.Date <= to.Value))
                    .ToList();

                decimal running = openingStock;
                var entryDtos = periodRows.Select(e =>
                {
                    running += e.Qty;
                    return new StockLedgerEntryDto
                    {
                        LedgerId = e.LedgerId,
                        Date = e.Date,
                        VoucherType = e.VoucherType ?? string.Empty,
                        VoucherNo = e.VoucherNo ?? string.Empty,
                        PartyName = e.BusinessPartner?.BusinessPartnerName,
                        WarehouseName = e.Warehouse?.WarehouseName,
                        InQty = e.Qty > 0 ? e.Qty : 0,
                        OutQty = e.Qty < 0 ? -e.Qty : 0,
                        Balance = running,
                        Rate = e.Rate,
                        Remarks = e.Remarks
                    };
                }).ToList();

                result.Add(new StockLedgerResponseDto
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    ItemCode = item.ItemCode ?? string.Empty,
                    Unit = GetUnitName(item.BaseUnitId),   // safe enum helper
                    ItemGroupName = item.ItemGroup?.ItemGroupName,
                    OpeningStock = openingStock,
                    TotalIn = entryDtos.Sum(e => e.InQty),
                    TotalOut = entryDtos.Sum(e => e.OutQty),
                    ClosingStock = running,
                    Entries = entryDtos
                });
            }

            return result;
        }

        // ════════════════════════════════════════════════
        // SAFE ENUM → STRING
        // ════════════════════════════════════════════════
        private static string GetUnitName(int baseUnitId) =>
            Enum.IsDefined(typeof(BaseUnit), baseUnitId)
                ? ((BaseUnit)baseUnitId).ToString()
                : $"Unit#{baseUnitId}";

        // ════════════════════════════════════════════════
        // DELETE SINGLE ENTRY  (soft delete)
        // ════════════════════════════════════════════════
        public async Task<bool> DeleteEntryAsync(int ledgerId)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var entry = await _context.StockLedgers
                .FirstOrDefaultAsync(sl =>
                    sl.LedgerId == ledgerId &&
                    sl.CompanyId == companyId &&
                    !sl.IsDeleted);

            if (entry == null) return false;

            entry.IsDeleted = true;
            entry.IsActive = false;
            entry.ModifiedBy = userId;
            entry.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ════════════════════════════════════════════════
        // UPDATE ENTRIES IN-PLACE  (on voucher edit)
        // ════════════════════════════════════════════════
        public async Task UpdateEntriesAsync(
            int companyId, int? warehouseId,
            DateTime date, string voucherType, string voucherNo,
            int? businessPartnerId,
            List<StockLedgerLineDto> lines,
            int? modifiedBy = null)
        {
            var existing = await _context.StockLedgers
                .Where(sl =>
                    sl.CompanyId == companyId &&
                    sl.VoucherNo == voucherNo &&
                    !sl.IsDeleted)
                .OrderBy(sl => sl.LedgerId)
                .ToListAsync();

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (i < existing.Count)
                {
                    // ── UPDATE in-place ──────────────────────────
                    var entry = existing[i];
                    entry.ItemId = line.ItemId;
                    entry.WarehouseId = warehouseId;
                    entry.Date = date;
                    entry.BusinessPartnerId = businessPartnerId;
                    entry.Qty = line.Qty;
                    entry.Rate = line.Rate;
                    entry.Remarks = line.Remarks;
                    entry.ModifiedBy = modifiedBy;
                    entry.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    // ── INSERT new line (invoice gained a row) ───
                    _context.StockLedgers.Add(new StockLedger
                    {
                        CompanyId = companyId,
                        ItemId = line.ItemId,
                        WarehouseId = warehouseId,
                        Date = date,
                        VoucherType = voucherType,
                        VoucherNo = voucherNo,
                        BusinessPartnerId = businessPartnerId,
                        Qty = line.Qty,
                        Rate = line.Rate,
                        Remarks = line.Remarks,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = modifiedBy,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            // ── REMOVE surplus rows (invoice lost a line) ────
            if (existing.Count > lines.Count)
            {
                var surplus = existing.Skip(lines.Count).ToList();
                _context.StockLedgers.RemoveRange(surplus); // hard delete surplus ledger rows
            }

            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════
        // SOFT DELETE ALL ENTRIES FOR A VOUCHER
        // ════════════════════════════════════════════════
        public async Task SoftDeleteByVoucherAsync(
            int companyId, string voucherNo, int? modifiedBy = null)
        {
            var entries = await _context.StockLedgers
                .Where(sl =>
                    sl.CompanyId == companyId &&
                    sl.VoucherNo == voucherNo &&
                    !sl.IsDeleted)
                .ToListAsync();

            foreach (var e in entries)
            {
                e.IsDeleted = true;
                e.IsActive = false;
                e.ModifiedBy = modifiedBy;
                e.ModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}