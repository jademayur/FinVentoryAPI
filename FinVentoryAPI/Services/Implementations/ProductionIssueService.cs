using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.ProductionIssueDTOs;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class ProductionIssueService : IProductionIssueService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IStockLedgerService _stockLedger;
        private readonly IAuditLogService _auditLog;

        public ProductionIssueService(AppDbContext context, Common common, IStockLedgerService stockLedger, IAuditLogService auditLog)
        {
            _context = context;
            _common = common;
            _stockLedger = stockLedger;
            _auditLog = auditLog;
        }

        public async Task<ProductionIssueResponseDto> CreateAsync(CreateProductionIssueMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var financialYearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();

            var issueNo = await _common.GenerateDocumentNumber(_context, "Production Issue");

            var main = new ProductionIssueMain
            {
                CompanyId = companyId,
                FinancialYearId = financialYearId,
                IssueNo = issueNo,
                IssueDate = dto.IssueDate,
                ProductionOrderId = dto.ProductionOrderId,
                WarehouseId = dto.WarehouseId,
                LocationId = dto.LocationId,
                Remarks = dto.Remarks,
                Status = "Draft",
                CreatedBy = userId
            };

            _context.ProductionIssueMains.Add(main);
            await _context.SaveChangesAsync();

            foreach (var d in dto.Details)
            {
                var detail = new ProductionIssueDetail
                {
                    ProductionIssueId = main.ProductionIssueId,
                    ItemId = d.ItemId,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    CreatedBy = userId
                };
                _context.ProductionIssueDetails.Add(detail);
                await _context.SaveChangesAsync();

                if (d.Batches?.Any() == true)
                {
                    foreach (var b in d.Batches)
                        _context.ProductionIssueDetailBatches.Add(new ProductionIssueDetailBatch
                        {
                            ProductionIssueDetailId = detail.ProductionIssueDetailId,
                            ItemBatchId = b.ItemBatchId,
                            Qty = b.Qty
                        });
                }

                if (d.Serials?.Any() == true)
                {
                    foreach (var s in d.Serials)
                        _context.ProductionIssueDetailSerials.Add(new ProductionIssueDetailSerial
                        {
                            ProductionIssueDetailId = detail.ProductionIssueDetailId,
                            ItemSerialId = s.ItemSerialId
                        });
                }
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "ProductionIssue",
                action: "Create",
                entityId: main.ProductionIssueId,
                entityNo: main.IssueNo,
                newValues: new { main.IssueNo, main.ProductionOrderId, main.WarehouseId, main.Status });
            return await GetByIdAsync(main.ProductionIssueId) ?? throw new Exception("Failed to retrieve created issue.");
        }

        public async Task<ProductionIssueResponseDto?> UpdateAsync(int id, UpdateProductionIssueMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.ProductionIssueMains
                .Include(x => x.Details).ThenInclude(d => d.Batches)
                .Include(x => x.Details).ThenInclude(d => d.Serials)
                .FirstOrDefaultAsync(x => x.ProductionIssueId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null) return null;

            var oldValues = new { main.IssueNo, main.ProductionOrderId, main.WarehouseId, main.Status };

            main.IssueDate = dto.IssueDate;
            main.ProductionOrderId = dto.ProductionOrderId;
            main.WarehouseId = dto.WarehouseId;
            main.LocationId = dto.LocationId;
            main.Remarks = dto.Remarks;
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            var existingDetails = main.Details.ToList();
            _context.ProductionIssueDetails.RemoveRange(existingDetails);

            foreach (var d in dto.Details)
            {
                var detail = new ProductionIssueDetail
                {
                    ProductionIssueId = main.ProductionIssueId,
                    ItemId = d.ItemId,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    CreatedBy = userId
                };
                _context.ProductionIssueDetails.Add(detail);
                await _context.SaveChangesAsync();

                if (d.Batches?.Any() == true)
                {
                    foreach (var b in d.Batches)
                        _context.ProductionIssueDetailBatches.Add(new ProductionIssueDetailBatch
                        {
                            ProductionIssueDetailId = detail.ProductionIssueDetailId,
                            ItemBatchId = b.ItemBatchId,
                            Qty = b.Qty
                        });
                }

                if (d.Serials?.Any() == true)
                {
                    foreach (var s in d.Serials)
                        _context.ProductionIssueDetailSerials.Add(new ProductionIssueDetailSerial
                        {
                            ProductionIssueDetailId = detail.ProductionIssueDetailId,
                            ItemSerialId = s.ItemSerialId
                        });
                }
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "ProductionIssue",
                action: "Update",
                entityId: main.ProductionIssueId,
                entityNo: main.IssueNo,
                oldValues: oldValues,
                newValues: new { main.IssueNo, main.ProductionOrderId, main.WarehouseId, main.Status });
            return await GetByIdAsync(id);
        }

        public async Task<ProductionIssueResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.ProductionIssueMains
                .Include(x => x.Warehouse)
                .Include(x => x.Location)
                .Include(x => x.ProductionOrder)
                .Include(x => x.Details).ThenInclude(d => d.Item)
                .Include(x => x.Details).ThenInclude(d => d.Batches).ThenInclude(b => b.Batch)
                .Include(x => x.Details).ThenInclude(d => d.Serials).ThenInclude(s => s.Serial)
                .FirstOrDefaultAsync(x => x.ProductionIssueId == id && x.CompanyId == companyId && !x.IsDeleted);

            if (main == null) return null;

            return new ProductionIssueResponseDto
            {
                ProductionIssueId = main.ProductionIssueId,
                CompanyId = main.CompanyId,
                FinancialYearId = main.FinancialYearId,
                IssueNo = main.IssueNo,
                IssueDate = main.IssueDate,
                ProductionOrderId = main.ProductionOrderId,
                ProductionOrderNo = main.ProductionOrder?.OrderNo,
                WarehouseId = main.WarehouseId,
                WarehouseName = main.Warehouse?.WarehouseName,
                Status = main.Status,
                LocationId = main.LocationId,
                LocationName = main.Location?.LocationName,
                Remarks = main.Remarks,
                Details = main.Details.Where(d => !d.IsDeleted).Select(d => new ProductionIssueDetailResponseDto
                {
                    ProductionIssueDetailId = d.ProductionIssueDetailId,
                    ItemId = d.ItemId,
                    ItemName = d.Item?.ItemName,
                    ItemCode = d.Item?.ItemCode,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    Batches = d.Batches.Select(b => new ProductionIssueDetailBatchResponseDto
                    {
                        Id = b.Id,
                        ItemBatchId = b.ItemBatchId,
                        BatchNo = b.Batch?.BatchNo,
                        Qty = b.Qty
                    }).ToList(),
                    Serials = d.Serials.Select(s => new ProductionIssueDetailSerialResponseDto
                    {
                        Id = s.Id,
                        ItemSerialId = s.ItemSerialId,
                        SerialNo = s.Serial?.SerialNo
                    }).ToList()
                }).ToList(),
                CreatedDate = main.CreatedDate,
                ModifiedDate = main.ModifiedDate
            };
        }

        public async Task<PagedResponseDto<ProductionIssueListDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.ProductionIssueMains
                .Include(x => x.Warehouse)
                .Include(x => x.ProductionOrder)
                .Include(x => x.Details)
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x => x.IssueNo.ToLower().Contains(s) || (x.ProductionOrder != null && x.ProductionOrder.OrderNo!.ToLower().Contains(s)));
            }

            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("status") && request.Filters["status"] != null)
                {
                    var status = request.Filters["status"]!.ToString();
                    query = query.Where(x => x.Status == status);
                }
            }

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.IssueDate)
                .ThenByDescending(x => x.ProductionIssueId)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ProductionIssueListDto
                {
                    ProductionIssueId = x.ProductionIssueId,
                    IssueNo = x.IssueNo,
                    FinancialYearId = x.FinancialYearId,
                    IssueDate = x.IssueDate,
                    WarehouseName = x.Warehouse != null ? x.Warehouse.WarehouseName : null,
                    ProductionOrderNo = x.ProductionOrder != null ? x.ProductionOrder.OrderNo : null,
                    Status = x.Status,
                    DetailCount = x.Details.Count(d => !d.IsDeleted),
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            return new PagedResponseDto<ProductionIssueListDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = items
            };
        }

        public async Task<ProductionIssueResponseDto> ConfirmAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.ProductionIssueMains
                .Include(x => x.Details).ThenInclude(d => d.Item)
                .FirstOrDefaultAsync(x => x.ProductionIssueId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null) throw new Exception("Production Issue not found or not in Draft status.");

            main.Status = "Confirmed";
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            foreach (var d in main.Details.Where(d => !d.IsDeleted))
            {
                if (d.Item == null) continue;

                var warehouseId = d.Item.ItemManageBy == Enums.ItemManageBy.Regular ? (int?)null : main.WarehouseId;

                await _stockLedger.AddEntriesAsync(
                    companyId,
                    warehouseId,
                    main.IssueDate,
                    "Production Issue",
                    main.IssueNo,
                    null,
                    new List<StockLedgerLineDto>
                    {
                        new StockLedgerLineDto
                        {
                            ItemId = d.ItemId,
                            Qty = -d.Qty,
                            Rate = 0,
                            Remarks = $"Production Issue - {main.IssueNo}"
                        }
                    },
                    userId
                );
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve confirmed issue.");
        }

        public async Task<ProductionIssueResponseDto> CancelAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.ProductionIssueMains
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.ProductionIssueId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Confirmed");

            if (main == null) throw new Exception("Production Issue not found or not in Confirmed status.");

            main.Status = "Cancelled";
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            await _stockLedger.ReverseEntriesAsync(
                companyId,
                main.IssueNo,
                main.IssueNo + "-Reversal",
                DateTime.UtcNow,
                userId
            );

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve cancelled issue.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.ProductionIssueMains
                .FirstOrDefaultAsync(x => x.ProductionIssueId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null) return false;

            var oldValues = new { main.IssueNo, main.ProductionOrderId, main.WarehouseId, main.Status };

            main.IsDeleted = true;
            main.IsActive = false;
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "ProductionIssue",
                action: "Delete",
                entityId: main.ProductionIssueId,
                entityNo: main.IssueNo,
                oldValues: oldValues,
                remarks: "Soft deleted");
            return true;
        }
    }
}
