using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.StockReportDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class StockReportService : IStockReportService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public StockReportService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ══════════════════════════════════════════════
        // GENERATE REPORT
        // ══════════════════════════════════════════════
        public async Task<StockReportResponseDto> GenerateAsync(StockReportRequestDto req)
        {
            var companyId = _common.GetCompanyId();
            var finYearId = _common.GetFinancialYearId();

            // ── Base query for stock ledger entries ──────────────────
            var ledgerQuery = _context.StockLedgers
                .Where(x => x.CompanyId == companyId && !x.IsDeleted);

            // ── Base query for items ─────────────────────────────────
            var itemQuery = _context.Items
                .Where(x => x.CompanyId == companyId && !x.IsDeleted);

            // ── Apply shared filters ────────────────────────────────
            if (req.ItemGroupIds?.Count > 0)
                itemQuery = itemQuery.Where(x => req.ItemGroupIds.Contains(x.ItemGroupId));

            if (req.ItemIds?.Count > 0)
                itemQuery = itemQuery.Where(x => req.ItemIds.Contains(x.ItemId));

            if (req.WarehouseIds?.Count > 0)
                ledgerQuery = ledgerQuery.Where(x => x.WarehouseId.HasValue
                                                  && req.WarehouseIds.Contains(x.WarehouseId.Value));

            if (req.FromDate.HasValue)
                ledgerQuery = ledgerQuery.Where(x => x.Date >= req.FromDate.Value);

            if (req.ToDate.HasValue)
                ledgerQuery = ledgerQuery.Where(x => x.Date <= req.ToDate.Value);

            // ── Dispatch to report builder ──────────────────────────
            object data;
            StockReportMetaDto meta;

            switch (req.ReportType)
            {
                case "StockRegister":
                    data = await BuildStockRegisterAsync(itemQuery, ledgerQuery, req);
                    meta = await BuildStockMetaAsync(itemQuery, ledgerQuery);
                    break;

                case "StockValuation":
                    data = await BuildStockValuationAsync(itemQuery, ledgerQuery, req);
                    meta = await BuildStockMetaAsync(itemQuery, ledgerQuery);
                    break;

                case "ItemLedger":
                    data = await BuildItemLedgerAsync(ledgerQuery, itemQuery, req);
                    meta = await BuildItemLedgerMetaAsync(ledgerQuery);
                    break;

                case "StockAge":
                    data = await BuildStockAgeAsync(itemQuery, ledgerQuery, req);
                    meta = await BuildStockMetaAsync(itemQuery, ledgerQuery);
                    break;

                case "DeadStock":
                    data = await BuildDeadStockAsync(itemQuery, ledgerQuery, req);
                    meta = await BuildDeadStockMetaAsync(itemQuery, ledgerQuery, req);
                    break;

                case "StockGroupSummary":
                    data = await BuildStockGroupSummaryAsync(itemQuery, ledgerQuery, req);
                    meta = await BuildStockMetaAsync(itemQuery, ledgerQuery);
                    break;

                case "WarehouseStock":
                    data = await BuildWarehouseStockAsync(itemQuery, ledgerQuery, req);
                    meta = await BuildStockMetaAsync(itemQuery, ledgerQuery);
                    break;

                default:
                    throw new ArgumentException($"Unknown ReportType: {req.ReportType}");
            }

            return new StockReportResponseDto
            {
                ReportType = req.ReportType,
                FromDate = req.FromDate?.ToString("dd MMM yyyy") ?? "",
                ToDate = req.ToDate?.ToString("dd MMM yyyy") ?? "",
                Meta = meta,
                Data = data
            };
        }

        // ══════════════════════════════════════════════
        // FILTER OPTIONS
        // ══════════════════════════════════════════════
        public async Task<StockReportFilterOptionsDto> GetFilterOptionsAsync()
        {
            var companyId = _common.GetCompanyId();

            var itemGroups = await _context.ItemGroups
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Select(x => new StockReportIdNameDto { Id = x.ItemGroupId, Name = x.ItemGroupName })
                .OrderBy(x => x.Name)
                .ToListAsync();

            var warehouses = await _context.Warehouses
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Select(x => new StockReportIdNameDto { Id = x.WarehouseId, Name = x.WarehouseName })
                .OrderBy(x => x.Name)
                .ToListAsync();

            var items = await _context.Items
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Select(x => new StockReportItemOptionDto
                {
                    ItemId = x.ItemId,
                    ItemName = x.ItemName,
                    ItemCode = x.ItemCode ?? ""
                })
                .OrderBy(x => x.ItemName)
                .ToListAsync();

            return new StockReportFilterOptionsDto
            {
                ItemGroups = itemGroups,
                Warehouses = warehouses,
                Items = items
            };
        }

        // ══════════════════════════════════════════════
        // REPORT BUILDERS
        // ══════════════════════════════════════════════

        // ── 1. Stock Register ─────────────────────────────────
        private async Task<List<StockRegisterRowDto>> BuildStockRegisterAsync(
            IQueryable<Item> itemQuery, IQueryable<StockLedger> ledgerQuery,
            StockReportRequestDto req)
        {
            // Compute stock per item from ledger
            var stockByItem = await ledgerQuery
                .GroupBy(x => new { x.ItemId, x.WarehouseId })
                .Select(g => new
                {
                    g.Key.ItemId,
                    WarehouseId = g.Key.WarehouseId,
                    Qty = g.Sum(x => x.Qty)
                })
                .ToListAsync();

            var stockDict = stockByItem
                .GroupBy(x => x.ItemId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        TotalQty = g.Sum(x => x.Qty),
                        Warehouses = g.Where(x => x.WarehouseId.HasValue)
                            .ToDictionary(x => x.WarehouseId!.Value, x => x.Qty)
                    });

            var items = await itemQuery
                .Include(x => x.ItemGroup)
                .OrderBy(x => x.ItemName)
                .ToListAsync();

            var warehouseDict = await _context.Warehouses
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.WarehouseId, x => x.WarehouseName);

            var rows = new List<StockRegisterRowDto>();
            foreach (var item in items)
            {
                if (stockDict.TryGetValue(item.ItemId, out var stock))
                {
                    if (stock.Warehouses.Count > 0)
                    {
                        foreach (var wh in stock.Warehouses)
                        {
                            rows.Add(new StockRegisterRowDto
                            {
                                ItemId = item.ItemId,
                                ItemCode = item.ItemCode ?? "",
                                ItemName = item.ItemName,
                                GroupName = item.ItemGroup?.ItemGroupName ?? "",
                                WarehouseName = warehouseDict.TryGetValue(wh.Key, out var wn) ? wn : "Default",
                                Qty = wh.Value,
                                Unit = item.BaseUnitId.ToString(),
                                Tracking = item.ItemManageBy.ToString()
                            });
                        }
                    }
                    else
                    {
                        rows.Add(new StockRegisterRowDto
                        {
                            ItemId = item.ItemId,
                            ItemCode = item.ItemCode ?? "",
                            ItemName = item.ItemName,
                            GroupName = item.ItemGroup?.ItemGroupName ?? "",
                            WarehouseName = "Default",
                            Qty = stock.TotalQty,
                            Unit = item.BaseUnitId.ToString(),
                            Tracking = item.ItemManageBy.ToString()
                        });
                    }
                }
            }

            return rows
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();
        }

        // ── 2. Stock Valuation ────────────────────────────────
        private async Task<List<StockValuationRowDto>> BuildStockValuationAsync(
            IQueryable<Item> itemQuery, IQueryable<StockLedger> ledgerQuery,
            StockReportRequestDto req)
        {
            var ledgerData = await ledgerQuery
                .GroupBy(x => new { x.ItemId, x.WarehouseId })
                .Select(g => new
                {
                    g.Key.ItemId,
                    WarehouseId = g.Key.WarehouseId,
                    Qty = g.Sum(x => x.Qty),
                    TotalRate = g.Sum(x => (x.Qty > 0 ? x.Qty * (x.Rate ?? 0) : 0)),
                    TotalInQty = g.Where(x => x.Qty > 0).Sum(x => x.Qty)
                })
                .ToListAsync();

            var items = await itemQuery
                .Include(x => x.ItemGroup)
                .ToDictionaryAsync(x => x.ItemId);

            var warehouseDict = await _context.Warehouses
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.WarehouseId, x => x.WarehouseName);

            var rows = ledgerData
                .Where(x => items.ContainsKey(x.ItemId) && x.Qty != 0)
                .Select(x =>
                {
                    var item = items[x.ItemId];
                    var avgRate = x.TotalInQty > 0 ? x.TotalRate / x.TotalInQty : 0;
                    return new StockValuationRowDto
                    {
                        ItemId = x.ItemId,
                        ItemCode = item.ItemCode ?? "",
                        ItemName = item.ItemName,
                        GroupName = item.ItemGroup?.ItemGroupName ?? "",
                        WarehouseName = x.WarehouseId.HasValue
                            && warehouseDict.TryGetValue(x.WarehouseId.Value, out var wn) ? wn : "Default",
                        Qty = x.Qty,
                        AvgRate = Math.Round(avgRate, 2),
                        TotalValue = Math.Round(x.Qty * avgRate, 2),
                        Unit = item.BaseUnitId.ToString()
                    };
                })
                .OrderByDescending(x => x.TotalValue)
                .ToList();

            return rows
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();
        }

        // ── 3. Item Ledger ────────────────────────────────────
        private async Task<List<ItemLedgerRowDto>> BuildItemLedgerAsync(
            IQueryable<StockLedger> ledgerQuery, IQueryable<Item> itemQuery,
            StockReportRequestDto req)
        {
            var itemDict = await itemQuery.ToDictionaryAsync(x => x.ItemId);
            var bpDict = await _context.BusinessPartners
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.BusinessPartnerId, x => x.BusinessPartnerName);
            var warehouseDict = await _context.Warehouses
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.WarehouseId, x => x.WarehouseName);

            // Get entries ordered by date, compute running balance per item
            var entries = await ledgerQuery
                .OrderBy(x => x.Date)
                .ThenBy(x => x.LedgerId)
                .ToListAsync();

            var balances = new Dictionary<int, decimal>();
            var rows = new List<ItemLedgerRowDto>();

            foreach (var e in entries)
            {
                if (!itemDict.ContainsKey(e.ItemId)) continue;

                if (!balances.ContainsKey(e.ItemId))
                    balances[e.ItemId] = 0;

                balances[e.ItemId] += e.Qty;

                rows.Add(new ItemLedgerRowDto
                {
                    ItemId = e.ItemId,
                    ItemCode = itemDict[e.ItemId].ItemCode ?? "",
                    ItemName = itemDict[e.ItemId].ItemName,
                    Date = e.Date.ToString("dd MMM yy"),
                    VoucherType = e.VoucherType ?? "",
                    VoucherNo = e.VoucherNo ?? "",
                    PartyName = e.BusinessPartnerId.HasValue
                        && bpDict.TryGetValue(e.BusinessPartnerId.Value, out var bp) ? bp : "",
                    InQty = e.Qty > 0 ? e.Qty : 0,
                    OutQty = e.Qty < 0 ? Math.Abs(e.Qty) : 0,
                    Balance = balances[e.ItemId],
                    WarehouseName = e.WarehouseId.HasValue
                        && warehouseDict.TryGetValue(e.WarehouseId.Value, out var wn) ? wn : "",
                    Remarks = e.Remarks ?? ""
                });
            }

            return rows
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();
        }

        // ── 4. Stock Age Analysis ─────────────────────────────
        private async Task<List<StockAgeRowDto>> BuildStockAgeAsync(
            IQueryable<Item> itemQuery, IQueryable<StockLedger> ledgerQuery,
            StockReportRequestDto req)
        {
            // Get last purchase date per item
            var lastPurchase = await ledgerQuery
                .Where(x => x.Qty > 0)
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    LastDate = g.Max(x => x.Date)
                })
                .ToDictionaryAsync(x => x.ItemId, x => x.LastDate);

            // Get current stock per item
            var stockDict = await ledgerQuery
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    Qty = g.Sum(x => x.Qty)
                })
                .Where(x => x.Qty > 0)
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            var items = await itemQuery
                .Include(x => x.ItemGroup)
                .ToDictionaryAsync(x => x.ItemId);

            var today = DateTime.Today;
            var rows = new List<StockAgeRowDto>();

            foreach (var itemId in stockDict.Keys.Where(k => items.ContainsKey(k)))
            {
                var item = items[itemId];
                var qty = stockDict[itemId];
                var lastDate = lastPurchase.TryGetValue(itemId, out var ld) ? ld : (DateTime?)null;
                var ageDays = lastDate.HasValue ? (today - lastDate.Value).Days : 999;

                rows.Add(new StockAgeRowDto
                {
                    ItemId = itemId,
                    ItemCode = item.ItemCode ?? "",
                    ItemName = item.ItemName,
                    GroupName = item.ItemGroup?.ItemGroupName ?? "",
                    Qty = qty,
                    Unit = item.BaseUnitId.ToString(),
                    LastPurchaseDate = lastDate?.ToString("dd MMM yyyy") ?? "N/A",
                    AgeDays = ageDays,
                    AgeBucket = GetAgeBucket(ageDays)
                });
            }

            return rows
                .OrderByDescending(x => x.AgeDays)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();
        }

        // ── 5. Dead Stock ─────────────────────────────────────
        private async Task<List<DeadStockRowDto>> BuildDeadStockAsync(
            IQueryable<Item> itemQuery, IQueryable<StockLedger> ledgerQuery,
            StockReportRequestDto req)
        {
            var cutoffDate = DateTime.Today.AddDays(-req.DeadStockDays);

            // Get last transaction date per item
            var lastTxn = await ledgerQuery
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    LastDate = g.Max(x => x.Date)
                })
                .ToDictionaryAsync(x => x.ItemId, x => x.LastDate);

            // Get current stock
            var stockDict = await ledgerQuery
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    Qty = g.Sum(x => x.Qty)
                })
                .Where(x => x.Qty > 0)
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            // Get average rate for valuation
            var avgRates = await ledgerQuery
                .Where(x => x.Qty > 0 && x.Rate.HasValue)
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    AvgRate = g.Sum(x => x.Qty * x.Rate!.Value) / g.Sum(x => x.Qty)
                })
                .ToDictionaryAsync(x => x.ItemId, x => x.AvgRate);

            var items = await itemQuery
                .Include(x => x.ItemGroup)
                .ToDictionaryAsync(x => x.ItemId);

            var today = DateTime.Today;
            var rows = new List<DeadStockRowDto>();

            foreach (var itemId in stockDict.Keys.Where(k => items.ContainsKey(k)))
            {
                var item = items[itemId];
                var qty = stockDict[itemId];
                var lastDate = lastTxn.TryGetValue(itemId, out var ld) ? ld : (DateTime?)null;
                var daysInactive = lastDate.HasValue ? (today - lastDate.Value).Days : 999;

                if (daysInactive < req.DeadStockDays) continue;

                var rate = avgRates.TryGetValue(itemId, out var ar) ? ar : 0;

                rows.Add(new DeadStockRowDto
                {
                    ItemId = itemId,
                    ItemCode = item.ItemCode ?? "",
                    ItemName = item.ItemName,
                    GroupName = item.ItemGroup?.ItemGroupName ?? "",
                    Qty = qty,
                    Unit = item.BaseUnitId.ToString(),
                    LastTransactionDate = lastDate?.ToString("dd MMM yyyy") ?? "N/A",
                    DaysInactive = daysInactive,
                    EstimatedValue = Math.Round(qty * rate, 2)
                });
            }

            return rows
                .OrderByDescending(x => x.DaysInactive)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();
        }

        // ── 6. Stock Summary by Group ─────────────────────────
        private async Task<List<StockGroupSummaryRowDto>> BuildStockGroupSummaryAsync(
            IQueryable<Item> itemQuery, IQueryable<StockLedger> ledgerQuery,
            StockReportRequestDto req)
        {
            var stockDict = await ledgerQuery
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    Qty = g.Sum(x => x.Qty)
                })
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            var avgRates = await ledgerQuery
                .Where(x => x.Qty > 0 && x.Rate.HasValue)
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    AvgRate = g.Sum(x => x.Qty * x.Rate!.Value) / g.Sum(x => x.Qty)
                })
                .ToDictionaryAsync(x => x.ItemId, x => x.AvgRate);

            var items = await itemQuery
                .Include(x => x.ItemGroup)
                .ToListAsync();

            var grouped = items
                .Where(i => stockDict.ContainsKey(i.ItemId))
                .GroupBy(i => new { i.ItemGroupId, GroupName = i.ItemGroup?.ItemGroupName ?? "Ungrouped" })
                .Select(g => new StockGroupSummaryRowDto
                {
                    ItemGroupId = g.Key.ItemGroupId,
                    GroupName = g.Key.GroupName,
                    TotalItems = g.Count(),
                    TotalQty = g.Sum(i => stockDict[i.ItemId]),
                    TotalValue = g.Sum(i =>
                    {
                        var qty = stockDict[i.ItemId];
                        var rate = avgRates.TryGetValue(i.ItemId, out var ar) ? ar : 0;
                        return qty * rate;
                    })
                })
                .OrderByDescending(x => x.TotalValue)
                .ToList();

            return grouped
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();
        }

        // ── 7. Warehouse-wise Stock ───────────────────────────
        private async Task<List<WarehouseStockRowDto>> BuildWarehouseStockAsync(
            IQueryable<Item> itemQuery, IQueryable<StockLedger> ledgerQuery,
            StockReportRequestDto req)
        {
            var stockByWh = await ledgerQuery
                .GroupBy(x => new { x.ItemId, x.WarehouseId })
                .Select(g => new
                {
                    g.Key.ItemId,
                    WarehouseId = g.Key.WarehouseId,
                    Qty = g.Sum(x => x.Qty)
                })
                .ToListAsync();

            var avgRates = await ledgerQuery
                .Where(x => x.Qty > 0 && x.Rate.HasValue)
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    AvgRate = g.Sum(x => x.Qty * x.Rate!.Value) / g.Sum(x => x.Qty)
                })
                .ToDictionaryAsync(x => x.ItemId, x => x.AvgRate);

            var warehouseDict = await _context.Warehouses
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.WarehouseId, x => x.WarehouseName);

            var grouped = stockByWh
                .GroupBy(x => x.WarehouseId)
                .Select(g => new WarehouseStockRowDto
                {
                    WarehouseId = g.Key,
                    WarehouseName = g.Key.HasValue
                        && warehouseDict.TryGetValue(g.Key.Value, out var wn) ? wn : "Default",
                    TotalItems = g.Select(x => x.ItemId).Distinct().Count(),
                    TotalQty = g.Sum(x => x.Qty),
                    TotalValue = g.Sum(x =>
                    {
                        var rate = avgRates.TryGetValue(x.ItemId, out var ar) ? ar : 0;
                        return x.Qty * rate;
                    })
                })
                .OrderByDescending(x => x.TotalValue)
                .ToList();

            return grouped
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();
        }

        // ══════════════════════════════════════════════
        // META BUILDERS
        // ══════════════════════════════════════════════

        private async Task<StockReportMetaDto> BuildStockMetaAsync(
            IQueryable<Item> itemQuery, IQueryable<StockLedger> ledgerQuery)
        {
            var stockDict = await ledgerQuery
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    Qty = g.Sum(x => x.Qty)
                })
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            var totalItems = stockDict.Count(x => x.Value != 0);
            var totalQty = stockDict.Values.Sum();

            return new StockReportMetaDto
            {
                TotalRecords = totalItems,
                TotalQty = totalQty,
                TotalValue = 0
            };
        }

        private async Task<StockReportMetaDto> BuildItemLedgerMetaAsync(
            IQueryable<StockLedger> ledgerQuery)
        {
            var count = await ledgerQuery.CountAsync();
            return new StockReportMetaDto
            {
                TotalRecords = count
            };
        }

        private async Task<StockReportMetaDto> BuildDeadStockMetaAsync(
            IQueryable<Item> itemQuery, IQueryable<StockLedger> ledgerQuery,
            StockReportRequestDto req)
        {
            var cutoffDate = DateTime.Today.AddDays(-req.DeadStockDays);

            var lastTxn = await ledgerQuery
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    LastDate = g.Max(x => x.Date)
                })
                .ToListAsync();

            var stockDict = await ledgerQuery
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    Qty = g.Sum(x => x.Qty)
                })
                .Where(x => x.Qty > 0)
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            var today = DateTime.Today;
            var deadCount = lastTxn.Count(x =>
                stockDict.ContainsKey(x.ItemId) &&
                (today - x.LastDate).Days >= req.DeadStockDays);

            return new StockReportMetaDto
            {
                TotalRecords = deadCount
            };
        }

        // ══════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════

        private static string GetAgeBucket(int days)
        {
            if (days <= 30) return "0-30 Days";
            if (days <= 60) return "31-60 Days";
            if (days <= 90) return "61-90 Days";
            if (days <= 180) return "91-180 Days";
            if (days <= 365) return "181-365 Days";
            return "1 Year+";
        }
    }
}
