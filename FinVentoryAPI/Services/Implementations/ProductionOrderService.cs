using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.ProductionDTOs;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class ProductionOrderService : IProductionOrderService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IStockLedgerService _stockLedger;

        public ProductionOrderService(
            AppDbContext context,
            Common common,
            IStockLedgerService stockLedger)
        {
            _context = context;
            _common = common;
            _stockLedger = stockLedger;
        }

        // ─────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────
        public async Task<ProductionOrderResponseDto> CreateAsync(CreateProductionOrderDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var fyId = _common.GetFinancialYearId();

            var orderNo = await GenerateOrderNoAsync(companyId, fyId);

            var order = new ProductionOrder
            {
                CompanyId = companyId,
                FinancialYearId = fyId,
                OrderNo = orderNo,
                OrderDate = dto.OrderDate,
                ItemId = dto.ItemId,
                BomId = dto.BomId,
                PlannedQuantity = dto.PlannedQuantity,
                UnitId = dto.UnitId,
                Status = ProductionOrderStatus.Draft,
                Notes = dto.Notes,
                RefNo = dto.RefNo,
                RefDate = dto.RefDate,
                PlannedStartDate = dto.PlannedStartDate,
                PlannedEndDate = dto.PlannedEndDate,
                CreatedBy = userId
            };

            _context.ProductionOrders.Add(order);
            await _context.SaveChangesAsync();

            if (dto.Lines?.Any() == true)
            {
                var lines = MapLineDtos(order.ProductionOrderId, dto.Lines);
                await _context.ProductionOrderLines.AddRangeAsync(lines);
                await _context.SaveChangesAsync();
            }

            return (await GetByIdAsync(order.ProductionOrderId))!;
        }

        // ─────────────────────────────────────────────────────
        // UPDATE  (only Draft or InProgress)
        // ─────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateProductionOrderDto dto)
        {
            var companyId = _common.GetCompanyId();

            var order = await _context.ProductionOrders
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (order == null) return false;

            if (order.Status == ProductionOrderStatus.Completed ||
                order.Status == ProductionOrderStatus.Cancelled)
                throw new Exception("Completed or Cancelled orders cannot be edited.");

            //order.OrderDate = dto.OrderDate;
            order.ItemId = dto.ItemId;
            order.BomId = dto.BomId;
            order.PlannedQuantity = dto.PlannedQuantity;
            order.UnitId = dto.UnitId;
            order.Notes = dto.Notes;
            order.RefNo = dto.RefNo;
            order.RefDate = dto.RefDate;
            order.PlannedStartDate = dto.PlannedStartDate;
            order.PlannedEndDate = dto.PlannedEndDate;
            order.ModifiedBy = _common.GetUserId();
            order.ModifiedDate = DateTime.UtcNow;

            // Replace lines
            _context.ProductionOrderLines.RemoveRange(order.Lines);

            if (dto.Lines?.Any() == true)
            {
                var newLines = MapLineDtos(order.ProductionOrderId, dto.Lines);
                await _context.ProductionOrderLines.AddRangeAsync(newLines);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────
        // DELETE  (soft — only Draft)
        // ─────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (order == null) return false;

            if (order.Status != ProductionOrderStatus.Draft)
                throw new Exception("Only Draft orders can be deleted.");

            order.IsDeleted = true;
            order.IsActive = false;
            order.ModifiedBy = _common.GetUserId();
            order.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────
        // SET IN PROGRESS
        // ─────────────────────────────────────────────────────
        public async Task<bool> SetInProgressAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (order == null) return false;

            if (order.Status != ProductionOrderStatus.Draft)
                throw new Exception("Only Draft orders can be moved to InProgress.");

            order.Status = ProductionOrderStatus.InProgress;
            order.ModifiedBy = _common.GetUserId();
            order.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────
        // COMPLETE  → posts stock ledger entries
        // ─────────────────────────────────────────────────────
        public async Task<bool> CompleteAsync(int id, CompleteProductionOrderDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var order = await _context.ProductionOrders
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (order == null) return false;

            if (order.Status == ProductionOrderStatus.Completed)
                throw new Exception("Order is already completed.");

            if (order.Status == ProductionOrderStatus.Cancelled)
                throw new Exception("Cancelled orders cannot be completed.");

            // Save actual quantities on lines
            foreach (var lineDto in dto.Lines)
            {
                var line = order.Lines
                    .FirstOrDefault(l => l.ProductionOrderLineId == lineDto.ProductionOrderLineId);

                if (line != null)
                    line.ActualQuantity = lineDto.ActualQuantity;
            }

            // Update header
            order.ActualQuantity = dto.ActualQuantity;
            order.ActualCompletionDate = dto.ActualCompletionDate;
            order.Status = ProductionOrderStatus.Completed;
            order.ModifiedBy = userId;
            order.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // ── Post to Stock Ledger ──────────────────────────
            var stockLines = new List<StockLedgerLineDto>();

            // 1. Finished good → StockIn (+)
            stockLines.Add(new StockLedgerLineDto
            {
                ItemId = order.ItemId,
                Qty = dto.ActualQuantity,           // positive = IN
                Rate = null,
                Remarks = $"Production completed: {order.OrderNo}"
            });

            // 2. Raw materials → StockOut (-)
            foreach (var line in order.Lines)
            {
                var actualQty = line.ActualQuantity ?? line.PlannedQuantity;
                stockLines.Add(new StockLedgerLineDto
                {
                    ItemId = line.ItemId,
                    Qty = -actualQty,               // negative = OUT
                    Rate = null,
                    Remarks = $"Raw material consumed: {order.OrderNo}"
                });
            }

            await _stockLedger.AddEntriesAsync(
                companyId: companyId,
                warehouseId: dto.WarehouseId,
                date: DateTime.UtcNow,
                voucherType: "Production",
                voucherNo: order.OrderNo,
                businessPartnerId: null,
                lines: stockLines,
                createdBy: userId
            );

            return true;
        }

        // ─────────────────────────────────────────────────────
        // CANCEL  → reverses stock if was Completed
        // ─────────────────────────────────────────────────────
        public async Task<bool> CancelAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (order == null) return false;

            if (order.Status == ProductionOrderStatus.Cancelled)
                throw new Exception("Order is already cancelled.");

            // If completed → reverse stock ledger entries
            if (order.Status == ProductionOrderStatus.Completed)
            {
                await _stockLedger.ReverseEntriesAsync(
                    companyId: companyId,
                    originalVoucherNo: order.OrderNo,
                    reversalVoucherNo: order.OrderNo + "-REV",
                    reversalDate: DateTime.UtcNow,
                    modifiedBy: userId
                );
            }

            order.Status = ProductionOrderStatus.Cancelled;
            order.ModifiedBy = userId;
            order.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────
        // GET BY ID
        // ─────────────────────────────────────────────────────
        public async Task<ProductionOrderResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var order = await _context.ProductionOrders
                .Include(x => x.FinishedGood)
                .Include(x => x.Bom)
                .Include(x => x.Lines)
                    .ThenInclude(l => l.Component)
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (order == null) return null;

            return MapToResponseDto(order);
        }

        // ─────────────────────────────────────────────────────
        // PAGED LIST
        // ─────────────────────────────────────────────────────
        public async Task<PagedResponseDto<ProductionOrderListItemDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.ProductionOrders
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.FinishedGood)
                .Include(x => x.Bom)
                .Include(x => x.Lines)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x =>
                    x.OrderNo.ToLower().Contains(s) ||
                    x.FinishedGood.ItemName.ToLower().Contains(s) ||
                    (x.RefNo ?? "").ToLower().Contains(s));
            }

            // Filters
            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("status"))
                {
                    var status = ((JsonElement)request.Filters["status"]).GetInt32();
                    query = query.Where(x => (int)x.Status == status);
                }

                if (request.Filters.ContainsKey("itemId"))
                {
                    var itemId = ((JsonElement)request.Filters["itemId"]).GetInt32();
                    query = query.Where(x => x.ItemId == itemId);
                }
            }

            // Sorting
            if (request.Sorts?.Any() == true)
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "orderno" => sort.Direction == "desc" ? query.OrderByDescending(x => x.OrderNo) : query.OrderBy(x => x.OrderNo),
                    "orderdate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.OrderDate) : query.OrderBy(x => x.OrderDate),
                    "itemname" => sort.Direction == "desc" ? query.OrderByDescending(x => x.FinishedGood.ItemName) : query.OrderBy(x => x.FinishedGood.ItemName),
                    "status" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    _ => query.OrderByDescending(x => x.CreatedDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedDate);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ProductionOrderListItemDto
                {
                    ProductionOrderId = x.ProductionOrderId,
                    OrderNo = x.OrderNo,
                   // OrderDate = x.OrderDate,
                    ItemName = x.FinishedGood.ItemName,
                    ItemCode = x.FinishedGood.ItemCode,
                    BomName = x.Bom != null ? x.Bom.BomName : null,
                    PlannedQuantity = x.PlannedQuantity,
                    ActualQuantity = x.ActualQuantity,
                    Status = x.Status.ToString(),
                    StatusId = (int)x.Status,
                    LineCount = x.Lines.Count,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            return new PagedResponseDto<ProductionOrderListItemDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }

        // ─────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────
        private async Task<string> GenerateOrderNoAsync(int companyId, int fyId)
        {
            // Get financial year short code e.g. 2425
            var fy = await _context.FinancialYears
                .FirstOrDefaultAsync(f => f.FinancialYearId == fyId);

            var fyCode = fy != null
                ? $"{fy.StartDate.Year % 100}{fy.EndDate.Year % 100}"
                : DateTime.UtcNow.Year.ToString();

            var last = await _context.ProductionOrders
                .Where(x => x.CompanyId == companyId && x.FinancialYearId == fyId)
                .OrderByDescending(x => x.ProductionOrderId)
                .Select(x => x.OrderNo)
                .FirstOrDefaultAsync();

            int next = 1;
            if (last != null)
            {
                var parts = last.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastNum))
                    next = lastNum + 1;
            }

            return $"PRD-{fyCode}-{next:D4}";
        }

        private static List<ProductionOrderLine> MapLineDtos(
            int orderId, List<CreateProductionOrderLineDto> dtos) =>
            dtos.Select(l => new ProductionOrderLine
            {
                ProductionOrderId = orderId,
                ItemId = l.ItemId,
                PlannedQuantity = l.PlannedQuantity,
                UnitId = l.UnitId,
                WastagePercent = l.WastagePercent,
                SortOrder = l.SortOrder,
                Notes = l.Notes
            }).ToList();

        private static ProductionOrderResponseDto MapToResponseDto(ProductionOrder o) =>
            new()
            {
                ProductionOrderId = o.ProductionOrderId,
                OrderNo = o.OrderNo,
               // OrderDate = o.OrderDate,
                ItemId = o.ItemId,
                ItemName = o.FinishedGood?.ItemName,
                ItemCode = o.FinishedGood?.ItemCode,
                BomId = o.BomId,
                BomName = o.Bom?.BomName,
                PlannedQuantity = o.PlannedQuantity,
                ActualQuantity = o.ActualQuantity,
                UnitId = o.UnitId,
                Status = o.Status.ToString(),
                StatusId = (int)o.Status,
                Notes = o.Notes,
                RefNo = o.RefNo,
                RefDate = o.RefDate,
                PlannedStartDate = o.PlannedStartDate,
                PlannedEndDate = o.PlannedEndDate,
                ActualCompletionDate = o.ActualCompletionDate,
                IsActive = o.IsActive,
                CreatedDate = o.CreatedDate,
                UpdatedDate = o.ModifiedDate,
                Lines = o.Lines
                    .OrderBy(l => l.SortOrder)
                    .Select(l => new ProductionOrderLineResponseDto
                    {
                        ProductionOrderLineId = l.ProductionOrderLineId,
                        ItemId = l.ItemId,
                        ItemName = l.Component?.ItemName,
                        ItemCode = l.Component?.ItemCode,
                        PlannedQuantity = l.PlannedQuantity,
                        ActualQuantity = l.ActualQuantity,
                        UnitId = l.UnitId,
                        WastagePercent = l.WastagePercent,
                        SortOrder = l.SortOrder,
                        Notes = l.Notes
                    }).ToList()
            };
    }
}
