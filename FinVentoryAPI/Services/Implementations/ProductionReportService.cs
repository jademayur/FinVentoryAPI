using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.ProductionReportDTOs;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class ProductionReportService : IProductionReportService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public ProductionReportService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<ProductionReportResponseDto> GenerateAsync(ProductionReportRequestDto req)
        {
            var companyId = _common.GetCompanyId();

            var baseQuery = _context.ProductionOrders
                .Where(x => x.CompanyId == companyId && !x.IsDeleted);

            if (req.DateFrom.HasValue)
            {
                var dateFrom = DateOnly.FromDateTime(req.DateFrom.Value);
                baseQuery = baseQuery.Where(x => x.OrderDate >= dateFrom);
            }

            if (req.DateTo.HasValue)
            {
                var dateTo = DateOnly.FromDateTime(req.DateTo.Value);
                baseQuery = baseQuery.Where(x => x.OrderDate <= dateTo);
            }

            if (req.ItemIds?.Count > 0)
                baseQuery = baseQuery.Where(x => req.ItemIds.Contains(x.ItemId));

            if (req.Statuses?.Count > 0)
            {
                var statusValues = req.Statuses
                    .Select(s => (ProductionOrderStatus)s)
                    .ToList();
                baseQuery = baseQuery.Where(x => statusValues.Contains(x.Status));
            }

            var meta = await baseQuery
                .GroupBy(_ => 1)
                .Select(g => new ProductionReportMetaDto
                {
                    TotalOrders = g.Count(),
                    TotalPlannedQty = g.Sum(x => x.PlannedQuantity),
                    TotalActualQty = g.Sum(x => x.ActualQuantity ?? 0),
                    TotalCompleted = g.Count(x => x.Status == ProductionOrderStatus.Completed),
                    TotalInProgress = g.Count(x => x.Status == ProductionOrderStatus.InProgress),
                    TotalDraft = g.Count(x => x.Status == ProductionOrderStatus.Draft),
                    TotalCancelled = g.Count(x => x.Status == ProductionOrderStatus.Cancelled)
                })
                .FirstOrDefaultAsync() ?? new ProductionReportMetaDto();

            meta.PageNumber = req.PageNumber;
            meta.PageSize = req.PageSize;
            meta.TotalPages = (int)Math.Ceiling((double)meta.TotalOrders / req.PageSize);
            meta.TotalRecords = meta.TotalOrders;

            object data = req.ReportType switch
            {
                "ProductionRegister" => await BuildProductionRegisterAsync(baseQuery, req),
                "ProductionSummary" => await BuildProductionSummaryAsync(baseQuery, req),
                "MaterialConsumption" => await BuildMaterialConsumptionAsync(baseQuery, companyId, req),
                "StatusSummary" => await BuildStatusSummaryAsync(baseQuery),
                "MonthlyProduction" => await BuildMonthlyProductionAsync(baseQuery, req),
                "DailyProduction" => await BuildDailyProductionAsync(baseQuery, req),
                _ => throw new ArgumentException($"Unknown ReportType: {req.ReportType}")
            };

            return new ProductionReportResponseDto
            {
                ReportType = req.ReportType,
                DateFrom = req.DateFrom?.ToString("dd MMM yyyy") ?? "",
                DateTo = req.DateTo?.ToString("dd MMM yyyy") ?? "",
                Meta = meta,
                Data = data
            };
        }

        public async Task<ProductionReportFilterOptionsDto> GetFilterOptionsAsync()
        {
            var companyId = _common.GetCompanyId();

            var itemIds = await _context.ProductionOrders
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Select(x => x.ItemId)
                .Distinct()
                .ToListAsync();

            var items = await _context.Items
                .Where(x => itemIds.Contains(x.ItemId) && !x.IsDeleted)
                .Select(x => new ProductionItemOptionDto
                {
                    ItemId = x.ItemId,
                    ItemName = x.ItemName,
                    ItemCode = x.ItemCode
                })
                .OrderBy(x => x.ItemName)
                .ToListAsync();

            var statuses = Enum.GetValues(typeof(ProductionOrderStatus))
                .Cast<ProductionOrderStatus>()
                .Select(s => new ProductionStatusOptionDto
                {
                    StatusId = (int)s,
                    StatusName = s.ToString()
                })
                .ToList();

            return new ProductionReportFilterOptionsDto
            {
                Items = items,
                Statuses = statuses
            };
        }

        // ════════════════════════════════════════════════════════
        // 1. Production Register
        // ════════════════════════════════════════════════════════
        private async Task<List<ProductionRegisterRowDto>> BuildProductionRegisterAsync(
            IQueryable<Entities.ProductionOrder> q, ProductionReportRequestDto req)
        {
            var orders = await q
                .Include(x => x.FinishedGood)
                .Include(x => x.Bom)
                .Include(x => x.Lines)
                .OrderByDescending(x => x.OrderDate)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            return orders.Select(x => new ProductionRegisterRowDto
            {
                ProductionOrderId = x.ProductionOrderId,
                OrderNo = x.OrderNo,
                OrderDate = x.OrderDate?.ToString("dd MMM yy"),
                ItemName = x.FinishedGood?.ItemName ?? "",
                ItemCode = x.FinishedGood?.ItemCode,
                BomName = x.Bom?.BomName,
                PlannedQuantity = x.PlannedQuantity,
                ActualQuantity = x.ActualQuantity,
                Status = x.Status.ToString(),
                StatusId = (int)x.Status,
                PlannedStartDate = x.PlannedStartDate?.ToString("dd MMM yy"),
                PlannedEndDate = x.PlannedEndDate?.ToString("dd MMM yy"),
                ActualCompletionDate = x.ActualCompletionDate?.ToString("dd MMM yy"),
                DaysTaken = x.PlannedStartDate != null && x.ActualCompletionDate != null
                    ? (int)(x.ActualCompletionDate.Value.DayNumber - x.PlannedStartDate.Value.DayNumber)
                    : (int?)null,
                Notes = x.Notes
            }).ToList();
        }

        // ════════════════════════════════════════════════════════
        // 2. Production Summary
        // ════════════════════════════════════════════════════════
        private async Task<List<ProductionSummaryRowDto>> BuildProductionSummaryAsync(
            IQueryable<Entities.ProductionOrder> q, ProductionReportRequestDto req)
        {
            var query = q.Include(x => x.FinishedGood);

            var rows = await query
                .GroupBy(x => new { x.ItemId, x.FinishedGood.ItemName, x.FinishedGood.ItemCode })
                .Select(g => new ProductionSummaryRowDto
                {
                    ItemId = g.Key.ItemId,
                    ItemName = g.Key.ItemName,
                    ItemCode = g.Key.ItemCode,
                    OrderCount = g.Count(),
                    TotalPlanned = g.Sum(x => x.PlannedQuantity),
                    TotalActual = g.Sum(x => x.ActualQuantity ?? 0),
                    AvgYield = g.Average(x => x.ActualQuantity != null && x.PlannedQuantity > 0
                        ? x.ActualQuantity.Value / x.PlannedQuantity : 0),
                    CompletionRate = (decimal)g.Count(x => x.Status == ProductionOrderStatus.Completed)
                                     / g.Count() * 100
                })
                .OrderByDescending(x => x.TotalPlanned)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            return rows;
        }

        // ════════════════════════════════════════════════════════
        // 3. Material Consumption
        // ════════════════════════════════════════════════════════
        private async Task<List<MaterialConsumptionRowDto>> BuildMaterialConsumptionAsync(
            IQueryable<Entities.ProductionOrder> q, int companyId, ProductionReportRequestDto req)
        {
            var orderIds = await q.Select(x => x.ProductionOrderId).ToListAsync();

            IQueryable<Entities.ProductionOrderLine> lines = _context.ProductionOrderLines
                .Where(l => orderIds.Contains(l.ProductionOrderId))
                .Include(l => l.Component);

            if (req.ItemIds?.Count > 0)
                lines = lines.Where(l => req.ItemIds.Contains(l.ItemId));

            var rows = await lines
                .GroupBy(l => new { l.ItemId, l.Component.ItemName, l.Component.ItemCode })
                .Select(g => new MaterialConsumptionRowDto
                {
                    ItemId = g.Key.ItemId,
                    ItemName = g.Key.ItemName,
                    ItemCode = g.Key.ItemCode,
                    TotalPlannedConsumption = g.Sum(l => l.PlannedQuantity),
                    TotalActualConsumption = g.Sum(l => l.ActualQuantity ?? 0),
                    Variance = g.Sum(l => l.PlannedQuantity) - g.Sum(l => l.ActualQuantity ?? 0),
                    WastagePercent = g.Average(l => l.WastagePercent)
                })
                .OrderByDescending(x => x.TotalPlannedConsumption)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            return rows;
        }

        // ════════════════════════════════════════════════════════
        // 4. Status Summary
        // ════════════════════════════════════════════════════════
        private async Task<List<StatusSummaryRowDto>> BuildStatusSummaryAsync(
            IQueryable<Entities.ProductionOrder> q)
        {
            var totalOrders = await q.CountAsync();

            var grouped = await q
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var rows = grouped.Select(g => new StatusSummaryRowDto
            {
                StatusName = g.Status.ToString(),
                StatusId = (int)g.Status,
                Count = g.Count,
                Percentage = totalOrders > 0
                    ? (decimal)g.Count / totalOrders * 100
                    : 0
            }).OrderBy(x => x.StatusId).ToList();

            foreach (ProductionOrderStatus s in Enum.GetValues(typeof(ProductionOrderStatus)))
            {
                if (!rows.Any(r => r.StatusId == (int)s))
                {
                    rows.Add(new StatusSummaryRowDto
                    {
                        StatusName = s.ToString(),
                        StatusId = (int)s,
                        Count = 0,
                        Percentage = 0
                    });
                }
            }

            return rows.OrderBy(x => x.StatusId).ToList();
        }

        // ════════════════════════════════════════════════════════
        // 5. Monthly Production (with detail)
        // ════════════════════════════════════════════════════════
        private async Task<List<MonthlyProductionDetailsRowDto>> BuildMonthlyProductionAsync(
            IQueryable<Entities.ProductionOrder> q, ProductionReportRequestDto req)
        {
            var rows = await q
                .GroupBy(x => new { x.OrderDate.Value.Year, x.OrderDate.Value.Month })
                .Select(g => new MonthlyProductionRowDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    OrderCount = g.Count(),
                    CompletedCount = g.Count(x => x.Status == ProductionOrderStatus.Completed),
                    TotalPlanned = g.Sum(x => x.PlannedQuantity),
                    TotalActual = g.Sum(x => x.ActualQuantity ?? 0)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            foreach (var r in rows)
                r.MonthLabel = new DateTime(r.Year, r.Month, 1).ToString("MMM yyyy");

            var materialized = await q
                .Include(x => x.FinishedGood)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            return rows.Select(r => new MonthlyProductionDetailsRowDto
            {
                Year = r.Year,
                Month = r.Month,
                MonthLabel = r.MonthLabel,
                OrderCount = r.OrderCount,
                CompletedCount = r.CompletedCount,
                TotalPlanned = r.TotalPlanned,
                TotalActual = r.TotalActual,
                Orders = materialized
                    .Where(o => o.OrderDate.HasValue
                        && o.OrderDate.Value.Year == r.Year
                        && o.OrderDate.Value.Month == r.Month)
                    .Select(o => MapToOrderDetail(o))
                    .ToList()
            }).ToList();
        }

        // ════════════════════════════════════════════════════════
        // 6. Daily Production (with detail)
        // ════════════════════════════════════════════════════════
        private async Task<List<DailyProductionDetailsRowDto>> BuildDailyProductionAsync(
            IQueryable<Entities.ProductionOrder> q, ProductionReportRequestDto req)
        {
            var grouped = await q
                .GroupBy(x => x.OrderDate)
                .Select(g => new
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    CompletedCount = g.Count(x => x.Status == ProductionOrderStatus.Completed),
                    TotalPlanned = g.Sum(x => x.PlannedQuantity),
                    TotalActual = g.Sum(x => x.ActualQuantity ?? 0)
                })
                .OrderByDescending(x => x.Date)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            var materialized = await q
                .Include(x => x.FinishedGood)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            return grouped.Select(g => new DailyProductionDetailsRowDto
            {
                Date = g.Date?.ToString("dd MMM yy") ?? "",
                OrderCount = g.OrderCount,
                CompletedCount = g.CompletedCount,
                TotalPlanned = g.TotalPlanned,
                TotalActual = g.TotalActual,
                CompletionRate = g.OrderCount > 0
                    ? (decimal)g.CompletedCount / g.OrderCount * 100
                    : 0,
                Orders = materialized
                    .Where(o => o.OrderDate == g.Date)
                    .Select(o => MapToOrderDetail(o))
                    .ToList()
            }).ToList();
        }

        private static ProductionOrderDetailLineDto MapToOrderDetail(Entities.ProductionOrder o)
        {
            return new ProductionOrderDetailLineDto
            {
                ProductionOrderId = o.ProductionOrderId,
                OrderNo = o.OrderNo,
                OrderDate = o.OrderDate?.ToString("dd MMM yy"),
                ItemName = o.FinishedGood?.ItemName ?? "",
                ItemCode = o.FinishedGood?.ItemCode,
                Status = o.Status.ToString(),
                StatusId = (int)o.Status,
                PlannedQuantity = o.PlannedQuantity,
                ActualQuantity = o.ActualQuantity,
                PlannedStartDate = o.PlannedStartDate?.ToString("dd MMM yy"),
                PlannedEndDate = o.PlannedEndDate?.ToString("dd MMM yy"),
                ActualCompletionDate = o.ActualCompletionDate?.ToString("dd MMM yy"),
                DaysTaken = o.PlannedStartDate != null && o.ActualCompletionDate != null
                    ? (int)(o.ActualCompletionDate.Value.DayNumber - o.PlannedStartDate.Value.DayNumber)
                    : (int?)null
            };
        }
    }
}
