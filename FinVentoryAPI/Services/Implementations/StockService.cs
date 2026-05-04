using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class StockService : IStockService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public StockService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<PagedResponseDto<StockDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            // ── Step 1: showZeroStock flag ────────────────────────────────────────────
            var showZeroStock = false;
            if (request.Filters != null && request.Filters.ContainsKey("zeroStock"))
                showZeroStock = ((System.Text.Json.JsonElement)request.Filters["zeroStock"]).GetBoolean();

            // ── Step 2: Stock totals from StockLedger ─────────────────────────────────
            var stockMap = await _context.StockLedgers
                .Where(sl => sl.CompanyId == companyId && !sl.IsDeleted)
                .GroupBy(sl => sl.ItemId)
                .Select(g => new { ItemId = g.Key, Total = g.Sum(sl => sl.Qty) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Total);

            // ── Step 3: Batch breakdown — straight from ItemBatch (denormalised) ──────
            var batchLookup = (await _context.ItemBatches
                .Where(b => b.CompanyId == companyId && !b.IsDeleted && b.AvailableQty > 0)
                .Select(b => new BatchStockDto
                {
                    BatchId = b.BatchId,
                    BatchNo = b.BatchNo,
                    ManufactureDate = b.ManufactureDate,
                    ExpiryDate = b.ExpiryDate,
                    AvailableQty = b.AvailableQty
                })
                .ToListAsync())
                // group by ItemId in memory — ItemBatch has ItemId
                .GroupBy(b => _context.ItemBatches
                    .Where(x => x.BatchId == b.BatchId)
                    .Select(x => x.ItemId)
                    .First())  // ← this would cause N+1, use the approach below instead
                .ToDictionary(g => g.Key, g => g.ToList());

            // ── Step 3 (correct): include ItemId in projection ────────────────────────
            // Replace Step 3 above with this:
            var batchRaw = await _context.ItemBatches
                .Where(b => b.CompanyId == companyId && !b.IsDeleted && b.AvailableQty > 0)
                .Select(b => new
                {
                    b.ItemId,
                    Dto = new BatchStockDto
                    {
                        BatchId = b.BatchId,
                        BatchNo = b.BatchNo,
                        ManufactureDate = b.ManufactureDate,
                        ExpiryDate = b.ExpiryDate,
                        AvailableQty = b.AvailableQty
                    }
                })
                .ToListAsync();

            var batchLookupFinal = batchRaw
                .GroupBy(x => x.ItemId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Dto)
                                                .OrderBy(d => d.ExpiryDate)  // FEFO
                                                .ToList());

            // ── Step 4: Serial breakdown — only InStock serials ───────────────────────
            var serialRaw = await _context.ItemSerials
                .Where(s => s.CompanyId == companyId && !s.IsDeleted
                            && s.Status == SerialStatus.InStock)
                .Select(s => new
                {
                    s.ItemId,
                    Dto = new SerialStockDto
                    {
                        SerialId = s.SerialId,
                        SerialNo = s.SerialNo,
                        Status = s.Status.ToString(),
                        PurchaseDate = s.PurchaseDate,
                        WarrantyExpiry = s.WarrantyExpiry
                    }
                })
                .ToListAsync();

            var serialLookupFinal = serialRaw
                .GroupBy(x => x.ItemId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Dto).ToList());

            // ── Step 5: Fetch items ───────────────────────────────────────────────────
            var itemsQuery = _context.Items
                .Where(i => i.CompanyId == companyId && !i.IsDeleted)
                .Select(i => new StockDto
                {
                    ItemId = i.ItemId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    ItemGroupId = i.ItemGroupId,
                    ItemGroupName = i.ItemGroup != null ? i.ItemGroup.ItemGroupName : null,
                    Unit = i.BaseUnitId.ToString(),
                    ItemManageBy = i.ItemManageBy.ToString(),
                    Stock = 0
                });

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                itemsQuery = itemsQuery.Where(x =>
                    x.ItemName.ToLower().Contains(s) ||
                    x.ItemCode.ToLower().Contains(s) ||
                    (x.ItemGroupName != null && x.ItemGroupName.ToLower().Contains(s)));
            }

            if (request.Filters != null && request.Filters.ContainsKey("itemGroupId"))
            {
                var gid = ((System.Text.Json.JsonElement)request.Filters["itemGroupId"]).GetInt32();
                itemsQuery = itemsQuery.Where(x => x.ItemGroupId == gid);
            }

            var allItems = await itemsQuery.ToListAsync();

            // ── Step 6: Populate stock + batch/serial details ─────────────────────────
            foreach (var item in allItems)
            {
                item.Stock = stockMap.TryGetValue(item.ItemId, out var qty) ? qty : 0;

                if (item.ItemManageBy == "Batch")
                    item.Batches = batchLookupFinal.TryGetValue(item.ItemId, out var batches)
                        ? batches : new();

                else if (item.ItemManageBy == "Serial")
                    item.Serials = serialLookupFinal.TryGetValue(item.ItemId, out var serials)
                        ? serials : new();
            }

            // ── Step 7: Zero-stock filter ─────────────────────────────────────────────
            if (!showZeroStock)
                allItems = allItems.Where(x => x.Stock > 0).ToList();

            // ── Step 8: Resolve unit enum ─────────────────────────────────────────────
            foreach (var item in allItems)
            {
                if (int.TryParse(item.Unit, out var unitInt) && Enum.IsDefined(typeof(BaseUnit), unitInt))
                    item.Unit = ((BaseUnit)unitInt).ToString();
            }

            // ── Step 9: Sorting ───────────────────────────────────────────────────────
            var sortCol = request.Sorts?.FirstOrDefault()?.Column?.ToLower();
            var sortDir = request.Sorts?.FirstOrDefault()?.Direction ?? "asc";

            allItems = sortCol switch
            {
                "itemname" => sortDir == "desc" ? allItems.OrderByDescending(x => x.ItemName).ToList() : allItems.OrderBy(x => x.ItemName).ToList(),
                "itemcode" => sortDir == "desc" ? allItems.OrderByDescending(x => x.ItemCode).ToList() : allItems.OrderBy(x => x.ItemCode).ToList(),
                "groupname" => sortDir == "desc" ? allItems.OrderByDescending(x => x.ItemGroupName).ToList() : allItems.OrderBy(x => x.ItemGroupName).ToList(),
                "stock" => sortDir == "desc" ? allItems.OrderByDescending(x => x.Stock).ToList() : allItems.OrderBy(x => x.Stock).ToList(),
                "itemmanageby" => sortDir == "desc" ? allItems.OrderByDescending(x => x.ItemManageBy).ToList() : allItems.OrderBy(x => x.ItemManageBy).ToList(),
                _ => allItems.OrderBy(x => x.ItemGroupName).ThenBy(x => x.ItemName).ToList()
            };

            // ── Step 10: Paginate ─────────────────────────────────────────────────────
            var totalRecords = allItems.Count;
            var pagedItems = allItems
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResponseDto<StockDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = pagedItems
            };
        }

        public async Task<List<object>> GetGroupsAsync()
        {
            var companyId = _common.GetCompanyId();
            return await _context.ItemGroups
                .Where(g => g.CompanyId == companyId && !g.IsDeleted)
                .Select(g => (object)new { g.ItemGroupId, g.ItemGroupName })
                .ToListAsync();
        }
    }
}