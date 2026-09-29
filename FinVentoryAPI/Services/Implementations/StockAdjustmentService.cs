using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.StockAdjustmentDTOs;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class StockAdjustmentService : IStockAdjustmentService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IStockLedgerService _stockLedger;
        private readonly IAuditLogService _auditLog;
        private readonly IApprovalService _approvalService;
        private readonly ICompanyConfigService _companyConfigService;

        public StockAdjustmentService(AppDbContext context, Common common, IStockLedgerService stockLedger, IAuditLogService auditLog, IApprovalService approvalService, ICompanyConfigService companyConfigService)
        {
            _context = context;
            _common = common;
            _stockLedger = stockLedger;
            _auditLog = auditLog;
            _approvalService = approvalService;
            _companyConfigService = companyConfigService;
        }

        public async Task<StockAdjustmentResponseDto> CreateAsync(CreateStockAdjustmentMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var financialYearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();

            var adjustmentNo = await _common.GenerateDocumentNumber(_context, "Stock Adjustment");

            var main = new StockAdjustmentMain
            {
                CompanyId = companyId,
                FinancialYearId = financialYearId,
                AdjustmentNo = adjustmentNo,
                AdjustmentDate = dto.AdjustmentDate,
                WarehouseId = dto.WarehouseId,
                AdjustmentType = dto.AdjustmentType,
                Reason = dto.Reason,
                LocationId = dto.LocationId,
                Remarks = dto.Remarks,
                Status = "Draft",
                CreatedBy = userId
            };

            _context.StockAdjustmentMains.Add(main);
            await _context.SaveChangesAsync();

            foreach (var d in dto.Details)
            {
                var detail = new StockAdjustmentDetail
                {
                    AdjustmentId = main.AdjustmentId,
                    ItemId = d.ItemId,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    CreatedBy = userId
                };
                _context.StockAdjustmentDetails.Add(detail);
                await _context.SaveChangesAsync();

                if (d.Batches?.Any() == true)
                {
                    foreach (var b in d.Batches)
                        _context.StockAdjustmentDetailBatches.Add(new StockAdjustmentDetailBatch
                        {
                            AdjustmentDetailId = detail.AdjustmentDetailId,
                            ItemBatchId = b.ItemBatchId,
                            Qty = b.Qty
                        });
                }

                if (d.Serials?.Any() == true)
                {
                    foreach (var s in d.Serials)
                        _context.StockAdjustmentDetailSerials.Add(new StockAdjustmentDetailSerial
                        {
                            AdjustmentDetailId = detail.AdjustmentDetailId,
                            ItemSerialId = s.ItemSerialId
                        });
                }
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "StockAdjustment",
                action: "Create",
                entityId: main.AdjustmentId,
                entityNo: main.AdjustmentNo,
                newValues: new { main.AdjustmentNo, main.WarehouseId, main.AdjustmentType, main.Status });
            return await GetByIdAsync(main.AdjustmentId) ?? throw new Exception("Failed to retrieve created adjustment.");
        }

        public async Task<bool> UpdateAsync(int id, UpdateStockAdjustmentMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.StockAdjustmentMains
                .Include(x => x.Details).ThenInclude(x => x.Batches)
                .Include(x => x.Details).ThenInclude(x => x.Serials)
                .FirstOrDefaultAsync(x => x.AdjustmentId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null) return false;

            var oldValues = new { main.AdjustmentNo, main.WarehouseId, main.AdjustmentType, main.Status };

            main.AdjustmentDate = dto.AdjustmentDate;
            main.WarehouseId = dto.WarehouseId;
            main.AdjustmentType = dto.AdjustmentType;
            main.Reason = dto.Reason;
            main.LocationId = dto.LocationId;
            main.Remarks = dto.Remarks;
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            // Remove old details
            foreach (var d in main.Details)
            {
                _context.StockAdjustmentDetailSerials.RemoveRange(d.Serials);
                _context.StockAdjustmentDetailBatches.RemoveRange(d.Batches);
            }
            _context.StockAdjustmentDetails.RemoveRange(main.Details);

            // Add new details
            foreach (var d in dto.Details)
            {
                var detail = new StockAdjustmentDetail
                {
                    AdjustmentId = main.AdjustmentId,
                    ItemId = d.ItemId,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    CreatedBy = userId
                };
                _context.StockAdjustmentDetails.Add(detail);
                await _context.SaveChangesAsync();

                if (d.Batches?.Any() == true)
                {
                    foreach (var b in d.Batches)
                        _context.StockAdjustmentDetailBatches.Add(new StockAdjustmentDetailBatch
                        {
                            AdjustmentDetailId = detail.AdjustmentDetailId,
                            ItemBatchId = b.ItemBatchId,
                            Qty = b.Qty
                        });
                }

                if (d.Serials?.Any() == true)
                {
                    foreach (var s in d.Serials)
                        _context.StockAdjustmentDetailSerials.Add(new StockAdjustmentDetailSerial
                        {
                            AdjustmentDetailId = detail.AdjustmentDetailId,
                            ItemSerialId = s.ItemSerialId
                        });
                }
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "StockAdjustment",
                action: "Update",
                entityId: main.AdjustmentId,
                entityNo: main.AdjustmentNo,
                oldValues: oldValues,
                newValues: new { main.AdjustmentNo, main.WarehouseId, main.AdjustmentType, main.Status });
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.StockAdjustmentMains
                .FirstOrDefaultAsync(x => x.AdjustmentId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null) return false;

            var oldValues = new { main.AdjustmentNo, main.WarehouseId, main.AdjustmentType, main.Status };

            main.IsDeleted = true;
            main.IsActive = false;
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "StockAdjustment",
                action: "Delete",
                entityId: main.AdjustmentId,
                entityNo: main.AdjustmentNo,
                oldValues: oldValues,
                remarks: "Soft deleted");
            return true;
        }

        public async Task<StockAdjustmentResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.StockAdjustmentMains
                .Include(x => x.Details).ThenInclude(x => x.Item)
                .Include(x => x.Details).ThenInclude(x => x.Batches).ThenInclude(x => x.Batch)
                .Include(x => x.Details).ThenInclude(x => x.Serials).ThenInclude(x => x.Serial)
                .Include(x => x.Warehouse)
                .Include(x => x.Location)
                .FirstOrDefaultAsync(x => x.AdjustmentId == id && x.CompanyId == companyId && !x.IsDeleted);

            if (main == null) return null;

            return new StockAdjustmentResponseDto
            {
                AdjustmentId = main.AdjustmentId,
                CompanyId = main.CompanyId,
                FinancialYearId = main.FinancialYearId,
                AdjustmentNo = main.AdjustmentNo,
                AdjustmentDate = main.AdjustmentDate,
                WarehouseId = main.WarehouseId,
                WarehouseName = main.Warehouse?.WarehouseName,
                AdjustmentType = main.AdjustmentType,
                Reason = main.Reason,
                Status = main.Status,
                LocationId = main.LocationId,
                LocationName = main.Location?.LocationName,
                Remarks = main.Remarks,
                Details = main.Details.Where(d => !d.IsDeleted).Select(d => new StockAdjustmentDetailResponseDto
                {
                    AdjustmentDetailId = d.AdjustmentDetailId,
                    ItemId = d.ItemId,
                    ItemName = d.Item?.ItemName,
                    ItemCode = d.Item?.ItemCode,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    Batches = d.Batches.Select(b => new StockAdjustmentDetailBatchResponseDto
                    {
                        Id = b.Id,
                        ItemBatchId = b.ItemBatchId,
                        BatchNo = b.Batch?.BatchNo,
                        Qty = b.Qty
                    }).ToList(),
                    Serials = d.Serials.Select(s => new StockAdjustmentDetailSerialResponseDto
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

        public async Task<PagedResponseDto<StockAdjustmentListDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.StockAdjustmentMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x => x.AdjustmentNo.ToLower().Contains(s));
            }

            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("status"))
                {
                    var status = ((JsonElement)request.Filters["status"]).GetString();
                    query = query.Where(x => x.Status == status);
                }
                if (request.Filters.ContainsKey("adjustmentType"))
                {
                    var type = ((JsonElement)request.Filters["adjustmentType"]).GetString();
                    query = query.Where(x => x.AdjustmentType == type);
                }
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.AdjustmentDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new StockAdjustmentListDto
                {
                    AdjustmentId = x.AdjustmentId,
                    AdjustmentNo = x.AdjustmentNo,
                    FinancialYearId = x.FinancialYearId,
                    AdjustmentDate = x.AdjustmentDate,
                    WarehouseName = x.Warehouse != null ? x.Warehouse.WarehouseName : null,
                    AdjustmentType = x.AdjustmentType,
                    Reason = x.Reason,
                    Status = x.Status,
                    DetailCount = x.Details.Count(d => !d.IsDeleted),
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            return new PagedResponseDto<StockAdjustmentListDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }

        public async Task<StockAdjustmentResponseDto> ConfirmAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.StockAdjustmentMains
                .Include(x => x.Details).ThenInclude(x => x.Batches)
                .Include(x => x.Details).ThenInclude(x => x.Serials)
                .FirstOrDefaultAsync(x => x.AdjustmentId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null)
                throw new Exception("Adjustment not found or not in Draft status.");

            if (!main.Details.Any())
                throw new Exception("Adjustment must have at least one detail line.");

            // Check if approval is required
            var approvalRequired = await _companyConfigService.GetValueAsync(companyId, "ApprovalRequired_StockAdjustment");
            if (approvalRequired?.ToLower() == "true")
            {
                await _approvalService.SubmitForApprovalAsync("StockAdjustment", id);
                return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve adjustment after submitting for approval.");
            }

            var sign = main.AdjustmentType == "Increase" ? 1m : -1m;

            // Post stock ledger entries
            var lines = main.Details.Select(d => new StockLedgerLineDto
            {
                ItemId = d.ItemId,
                Qty = sign * d.Qty,
                Remarks = $"Adjustment ({main.Reason}): {main.AdjustmentNo}"
            }).ToList();

            await _stockLedger.AddEntriesAsync(
                companyId, main.WarehouseId, main.AdjustmentDate,
                "Stock Adjustment", main.AdjustmentNo, null,
                lines, userId);

            // Update batch quantities for batch-managed items
            foreach (var d in main.Details)
            {
                if (d.Batches?.Any() == true)
                {
                    foreach (var b in d.Batches)
                    {
                        var batch = await _context.ItemBatches.FindAsync(b.ItemBatchId);
                        if (batch != null)
                        {
                            if (main.AdjustmentType == "Increase")
                            {
                                batch.ReceivedQty += b.Qty;
                                batch.AvailableQty += b.Qty;
                            }
                            else
                            {
                                batch.UsedQty += b.Qty;
                                batch.AvailableQty -= b.Qty;
                            }
                        }
                    }
                }

                // Update serial status for serial-managed items
                if (d.Serials?.Any() == true)
                {
                    foreach (var s in d.Serials)
                    {
                        var serial = await _context.ItemSerials.FindAsync(s.ItemSerialId);
                        if (serial != null)
                        {
                            serial.Status = main.AdjustmentType == "Increase"
                                ? Enums.SerialStatus.InStock
                                : Enums.SerialStatus.Scrapped;
                        }
                    }
                }
            }

            main.Status = "Confirmed";
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve confirmed adjustment.");
        }

        public async Task<StockAdjustmentResponseDto> CancelAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.StockAdjustmentMains
                .Include(x => x.Details).ThenInclude(x => x.Batches)
                .Include(x => x.Details).ThenInclude(x => x.Serials)
                .FirstOrDefaultAsync(x => x.AdjustmentId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Confirmed");

            if (main == null)
                throw new Exception("Adjustment not found or not in Confirmed status.");

            // Reverse stock ledger entries
            await _stockLedger.ReverseEntriesAsync(
                companyId, main.AdjustmentNo, $"{main.AdjustmentNo}-Reversal",
                main.AdjustmentDate, userId);

            // Reverse batch quantities
            var sign = main.AdjustmentType == "Increase" ? -1m : 1m;
            foreach (var d in main.Details)
            {
                if (d.Batches?.Any() == true)
                {
                    foreach (var b in d.Batches)
                    {
                        var batch = await _context.ItemBatches.FindAsync(b.ItemBatchId);
                        if (batch != null)
                        {
                            if (main.AdjustmentType == "Increase")
                            {
                                batch.ReceivedQty += sign * b.Qty;
                                batch.AvailableQty += sign * b.Qty;
                            }
                            else
                            {
                                batch.UsedQty += sign * b.Qty;
                                batch.AvailableQty += sign * b.Qty;
                            }
                        }
                    }
                }

                if (d.Serials?.Any() == true)
                {
                    foreach (var s in d.Serials)
                    {
                        var serial = await _context.ItemSerials.FindAsync(s.ItemSerialId);
                        if (serial != null)
                        {
                            serial.Status = main.AdjustmentType == "Increase"
                                ? Enums.SerialStatus.InStock
                                : Enums.SerialStatus.InStock;
                        }
                    }
                }
            }

            main.Status = "Cancelled";
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve cancelled adjustment.");
        }
    }
}
