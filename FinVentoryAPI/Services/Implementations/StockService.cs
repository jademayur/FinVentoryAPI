using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
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

            // Base query — join Items with their current stock ledger balance
            var query = _context.Items
                .Where(i => i.CompanyId == companyId && !i.IsDeleted)
                .Select(i => new StockDto
                {
                    ItemId = i.ItemId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    ItemGroupId = i.ItemGroupId,
                    ItemGroupName = i.ItemGroup != null ? i.ItemGroup.ItemGroupName : null,
                    // Sum from stock ledger (purchases - sales)
                    Stock = _context.StockLedgers
                                        .Where(sl => sl.ItemId == i.ItemId && sl.CompanyId == companyId)
                                        .Sum(sl => (decimal?)sl.Qty) ?? 0,
                    Unit = i.BaseUnitId.ToString(),
                });

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x =>
                    x.ItemName.ToLower().Contains(s) ||
                    x.ItemCode.ToLower().Contains(s) ||
                    (x.ItemGroupName != null && x.ItemGroupName.ToLower().Contains(s)));
            }

            // FILTERS
            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("itemGroupId"))
                {
                    var gid = ((System.Text.Json.JsonElement)request.Filters["itemGroupId"]).GetInt32();
                    query = query.Where(x => x.ItemGroupId == gid);
                }

                if (request.Filters.ContainsKey("WarehouseId"))
                {
                    var lid = ((System.Text.Json.JsonElement)request.Filters["WarehouseId"]).GetInt32();
                    query = query.Where(x => x.WarehouseId == lid);
                }

                if (request.Filters.ContainsKey("zeroStock"))
                {
                    var showZero = ((System.Text.Json.JsonElement)request.Filters["zeroStock"]).GetBoolean();
                    if (!showZero) query = query.Where(x => x.Stock > 0);
                }
            }

            // SORTING
            query = (request.Sorts?.FirstOrDefault()?.Column.ToLower()) switch
            {
                "itemname" => request.Sorts![0].Direction == "desc"
                    ? query.OrderByDescending(x => x.ItemName) : query.OrderBy(x => x.ItemName),
                "itemcode" => request.Sorts![0].Direction == "desc"
                    ? query.OrderByDescending(x => x.ItemCode) : query.OrderBy(x => x.ItemCode),
                "stock" => request.Sorts![0].Direction == "desc"
                    ? query.OrderByDescending(x => x.Stock) : query.OrderBy(x => x.Stock),
                "groupname" => request.Sorts![0].Direction == "desc"
                    ? query.OrderByDescending(x => x.ItemGroupName) : query.OrderBy(x => x.ItemGroupName),
                _ => query.OrderBy(x => x.ItemGroupName).ThenBy(x => x.ItemName)
            };

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponseDto<StockDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }

        // Groups for filter dropdown
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
