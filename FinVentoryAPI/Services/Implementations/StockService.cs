using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
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

            // ── Step 1: Read showZeroStock flag ───────────────────────────────────
            var showZeroStock = false;
            if (request.Filters != null && request.Filters.ContainsKey("zeroStock"))
                showZeroStock = ((System.Text.Json.JsonElement)request.Filters["zeroStock"]).GetBoolean();

            // ── Step 2: Fetch ALL stock totals for this company in one query ──────
            // We do this up-front so we can filter/sort by stock before pagination.
            var stockMap = await _context.StockLedgers
                .Where(sl => sl.CompanyId == companyId && !sl.IsDeleted)
                .GroupBy(sl => sl.ItemId)
                .Select(g => new { ItemId = g.Key, Total = g.Sum(sl => sl.Qty) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Total);

            // ── Step 3: Fetch all matching items (lightweight — no stock yet) ─────
            var itemsQuery = _context.Items
                .Where(i => i.CompanyId == companyId && !i.IsDeleted)
                .Select(i => new StockDto
                {
                    ItemId        = i.ItemId,
                    ItemCode      = i.ItemCode,
                    ItemName      = i.ItemName,
                    ItemGroupId   = i.ItemGroupId,
                    ItemGroupName = i.ItemGroup != null ? i.ItemGroup.ItemGroupName : null,
                    Unit          = i.BaseUnitId.ToString(),
                    Stock         = 0  // populated in memory below
                });

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                itemsQuery = itemsQuery.Where(x =>
                    x.ItemName.ToLower().Contains(s) ||
                    x.ItemCode.ToLower().Contains(s) ||
                    (x.ItemGroupName != null && x.ItemGroupName.ToLower().Contains(s)));
            }

            // Group filter
            if (request.Filters != null && request.Filters.ContainsKey("itemGroupId"))
            {
                var gid = ((System.Text.Json.JsonElement)request.Filters["itemGroupId"]).GetInt32();
                itemsQuery = itemsQuery.Where(x => x.ItemGroupId == gid);
            }

            // Materialise items (EF query ends here — everything below is in-memory)
            var allItems = await itemsQuery.ToListAsync();

            // ── Step 4: Populate stock from the map ───────────────────────────────
            foreach (var item in allItems)
                item.Stock = stockMap.TryGetValue(item.ItemId, out var qty) ? qty : 0;

            // ── Step 5: Zero-stock filter BEFORE pagination ───────────────────────
            // Toggle OFF (default) → hide items with stock <= 0
            // Toggle ON            → show everything including zero / negative
            if (!showZeroStock)
                allItems = allItems.Where(x => x.Stock > 0).ToList();

            // ── Step 6: Resolve unit enum ─────────────────────────────────────────
            foreach (var item in allItems)
            {
                if (int.TryParse(item.Unit, out var unitInt) && Enum.IsDefined(typeof(BaseUnit), unitInt))
                    item.Unit = ((BaseUnit)unitInt).ToString();
            }

            // ── Step 7: Sorting ───────────────────────────────────────────────────
            var sortCol = request.Sorts?.FirstOrDefault()?.Column?.ToLower();
            var sortDir = request.Sorts?.FirstOrDefault()?.Direction ?? "asc";

            allItems = sortCol switch
            {
                "itemname"  => sortDir == "desc" ? allItems.OrderByDescending(x => x.ItemName).ToList()      : allItems.OrderBy(x => x.ItemName).ToList(),
                "itemcode"  => sortDir == "desc" ? allItems.OrderByDescending(x => x.ItemCode).ToList()      : allItems.OrderBy(x => x.ItemCode).ToList(),
                "groupname" => sortDir == "desc" ? allItems.OrderByDescending(x => x.ItemGroupName).ToList() : allItems.OrderBy(x => x.ItemGroupName).ToList(),
                "stock"     => sortDir == "desc" ? allItems.OrderByDescending(x => x.Stock).ToList()         : allItems.OrderBy(x => x.Stock).ToList(),
                _           => allItems.OrderBy(x => x.ItemGroupName).ThenBy(x => x.ItemName).ToList()
            };

            // ── Step 8: Total count AFTER all filters ─────────────────────────────
            var totalRecords = allItems.Count;

            // ── Step 9: Paginate ──────────────────────────────────────────────────
            var pagedItems = allItems
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResponseDto<StockDto>
            {
                TotalRecords = totalRecords,
                PageNumber   = request.PageNumber,
                PageSize     = request.PageSize,
                Data         = pagedItems
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