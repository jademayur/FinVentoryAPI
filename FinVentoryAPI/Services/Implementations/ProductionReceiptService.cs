using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.ProductionReceiptDTOs;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class ProductionReceiptService : IProductionReceiptService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IStockLedgerService _stockLedger;
        private readonly IAuditLogService _auditLog;

        public ProductionReceiptService(AppDbContext context, Common common, IStockLedgerService stockLedger, IAuditLogService auditLog)
        {
            _context = context;
            _common = common;
            _stockLedger = stockLedger;
            _auditLog = auditLog;
        }

        public async Task<ProductionReceiptResponseDto> CreateAsync(CreateProductionReceiptMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var financialYearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();

            var receiptNo = await _common.GenerateDocumentNumber(_context, "Production Receipt");

            var main = new ProductionReceiptMain
            {
                CompanyId = companyId,
                FinancialYearId = financialYearId,
                ReceiptNo = receiptNo,
                ReceiptDate = dto.ReceiptDate,
                ProductionOrderId = dto.ProductionOrderId,
                WarehouseId = dto.WarehouseId,
                LocationId = dto.LocationId,
                Remarks = dto.Remarks,
                Status = "Draft",
                CreatedBy = userId
            };

            _context.ProductionReceiptMains.Add(main);
            await _context.SaveChangesAsync();

            foreach (var d in dto.Details)
            {
                var detail = new ProductionReceiptDetail
                {
                    ProductionReceiptId = main.ProductionReceiptId,
                    ItemId = d.ItemId,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    CreatedBy = userId
                };
                _context.ProductionReceiptDetails.Add(detail);
                await _context.SaveChangesAsync();

                if (d.Batches?.Any() == true)
                {
                    foreach (var b in d.Batches)
                        _context.ProductionReceiptDetailBatches.Add(new ProductionReceiptDetailBatch
                        {
                            ProductionReceiptDetailId = detail.ProductionReceiptDetailId,
                            ItemBatchId = b.ItemBatchId,
                            Qty = b.Qty
                        });
                }

                if (d.Serials?.Any() == true)
                {
                    foreach (var s in d.Serials)
                        _context.ProductionReceiptDetailSerials.Add(new ProductionReceiptDetailSerial
                        {
                            ProductionReceiptDetailId = detail.ProductionReceiptDetailId,
                            ItemSerialId = s.ItemSerialId
                        });
                }
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "ProductionReceipt",
                action: "Create",
                entityId: main.ProductionReceiptId,
                entityNo: main.ReceiptNo,
                newValues: new { main.ReceiptNo, main.ProductionOrderId, main.WarehouseId, main.Status });
            return await GetByIdAsync(main.ProductionReceiptId) ?? throw new Exception("Failed to retrieve created receipt.");
        }

        public async Task<ProductionReceiptResponseDto?> UpdateAsync(int id, UpdateProductionReceiptMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.ProductionReceiptMains
                .Include(x => x.Details).ThenInclude(d => d.Batches)
                .Include(x => x.Details).ThenInclude(d => d.Serials)
                .FirstOrDefaultAsync(x => x.ProductionReceiptId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null) return null;

            var oldValues = new { main.ReceiptNo, main.ProductionOrderId, main.WarehouseId, main.Status };

            main.ReceiptDate = dto.ReceiptDate;
            main.ProductionOrderId = dto.ProductionOrderId;
            main.WarehouseId = dto.WarehouseId;
            main.LocationId = dto.LocationId;
            main.Remarks = dto.Remarks;
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            var existingDetails = main.Details.ToList();
            _context.ProductionReceiptDetails.RemoveRange(existingDetails);

            foreach (var d in dto.Details)
            {
                var detail = new ProductionReceiptDetail
                {
                    ProductionReceiptId = main.ProductionReceiptId,
                    ItemId = d.ItemId,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    CreatedBy = userId
                };
                _context.ProductionReceiptDetails.Add(detail);
                await _context.SaveChangesAsync();

                if (d.Batches?.Any() == true)
                {
                    foreach (var b in d.Batches)
                        _context.ProductionReceiptDetailBatches.Add(new ProductionReceiptDetailBatch
                        {
                            ProductionReceiptDetailId = detail.ProductionReceiptDetailId,
                            ItemBatchId = b.ItemBatchId,
                            Qty = b.Qty
                        });
                }

                if (d.Serials?.Any() == true)
                {
                    foreach (var s in d.Serials)
                        _context.ProductionReceiptDetailSerials.Add(new ProductionReceiptDetailSerial
                        {
                            ProductionReceiptDetailId = detail.ProductionReceiptDetailId,
                            ItemSerialId = s.ItemSerialId
                        });
                }
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "ProductionReceipt",
                action: "Update",
                entityId: main.ProductionReceiptId,
                entityNo: main.ReceiptNo,
                oldValues: oldValues,
                newValues: new { main.ReceiptNo, main.ProductionOrderId, main.WarehouseId, main.Status });
            return await GetByIdAsync(id);
        }

        public async Task<ProductionReceiptResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.ProductionReceiptMains
                .Include(x => x.Warehouse)
                .Include(x => x.Location)
                .Include(x => x.ProductionOrder)
                .Include(x => x.Details).ThenInclude(d => d.Item)
                .Include(x => x.Details).ThenInclude(d => d.Batches).ThenInclude(b => b.Batch)
                .Include(x => x.Details).ThenInclude(d => d.Serials).ThenInclude(s => s.Serial)
                .FirstOrDefaultAsync(x => x.ProductionReceiptId == id && x.CompanyId == companyId && !x.IsDeleted);

            if (main == null) return null;

            return new ProductionReceiptResponseDto
            {
                ProductionReceiptId = main.ProductionReceiptId,
                CompanyId = main.CompanyId,
                FinancialYearId = main.FinancialYearId,
                ReceiptNo = main.ReceiptNo,
                ReceiptDate = main.ReceiptDate,
                ProductionOrderId = main.ProductionOrderId,
                ProductionOrderNo = main.ProductionOrder?.OrderNo,
                WarehouseId = main.WarehouseId,
                WarehouseName = main.Warehouse?.WarehouseName,
                Status = main.Status,
                LocationId = main.LocationId,
                LocationName = main.Location?.LocationName,
                Remarks = main.Remarks,
                Details = main.Details.Where(d => !d.IsDeleted).Select(d => new ProductionReceiptDetailResponseDto
                {
                    ProductionReceiptDetailId = d.ProductionReceiptDetailId,
                    ItemId = d.ItemId,
                    ItemName = d.Item?.ItemName,
                    ItemCode = d.Item?.ItemCode,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    Batches = d.Batches.Select(b => new ProductionReceiptDetailBatchResponseDto
                    {
                        Id = b.Id,
                        ItemBatchId = b.ItemBatchId,
                        BatchNo = b.Batch?.BatchNo,
                        Qty = b.Qty
                    }).ToList(),
                    Serials = d.Serials.Select(s => new ProductionReceiptDetailSerialResponseDto
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

        public async Task<PagedResponseDto<ProductionReceiptListDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.ProductionReceiptMains
                .Include(x => x.Warehouse)
                .Include(x => x.ProductionOrder)
                .Include(x => x.Details)
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x => x.ReceiptNo.ToLower().Contains(s) || (x.ProductionOrder != null && x.ProductionOrder.OrderNo!.ToLower().Contains(s)));
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
                .OrderByDescending(x => x.ReceiptDate)
                .ThenByDescending(x => x.ProductionReceiptId)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ProductionReceiptListDto
                {
                    ProductionReceiptId = x.ProductionReceiptId,
                    ReceiptNo = x.ReceiptNo,
                    FinancialYearId = x.FinancialYearId,
                    ReceiptDate = x.ReceiptDate,
                    WarehouseName = x.Warehouse != null ? x.Warehouse.WarehouseName : null,
                    ProductionOrderNo = x.ProductionOrder != null ? x.ProductionOrder.OrderNo : null,
                    Status = x.Status,
                    DetailCount = x.Details.Count(d => !d.IsDeleted),
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            return new PagedResponseDto<ProductionReceiptListDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = items
            };
        }

        public async Task<ProductionReceiptResponseDto> ConfirmAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.ProductionReceiptMains
                .Include(x => x.Details).ThenInclude(d => d.Item)
                .FirstOrDefaultAsync(x => x.ProductionReceiptId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null) throw new Exception("Production Receipt not found or not in Draft status.");

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
                    main.ReceiptDate,
                    "Production Receipt",
                    main.ReceiptNo,
                    null,
                    new List<StockLedgerLineDto>
                    {
                        new StockLedgerLineDto
                        {
                            ItemId = d.ItemId,
                            Qty = d.Qty,
                            Rate = 0,
                            Remarks = $"Production Receipt - {main.ReceiptNo}"
                        }
                    },
                    userId
                );
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve confirmed receipt.");
        }

        public async Task<ProductionReceiptResponseDto> CancelAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.ProductionReceiptMains
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.ProductionReceiptId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Confirmed");

            if (main == null) throw new Exception("Production Receipt not found or not in Confirmed status.");

            main.Status = "Cancelled";
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            await _stockLedger.ReverseEntriesAsync(
                companyId,
                main.ReceiptNo,
                main.ReceiptNo + "-Reversal",
                DateTime.UtcNow,
                userId
            );

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve cancelled receipt.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.ProductionReceiptMains
                .FirstOrDefaultAsync(x => x.ProductionReceiptId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null) return false;

            var oldValues = new { main.ReceiptNo, main.ProductionOrderId, main.WarehouseId, main.Status };

            main.IsDeleted = true;
            main.IsActive = false;
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "ProductionReceipt",
                action: "Delete",
                entityId: main.ProductionReceiptId,
                entityNo: main.ReceiptNo,
                oldValues: oldValues,
                remarks: "Soft deleted");
            return true;
        }
    }
}
