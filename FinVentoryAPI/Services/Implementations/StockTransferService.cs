using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.DTOs.StockTransferDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class StockTransferService : IStockTransferService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IStockLedgerService _stockLedger;
        private readonly IAuditLogService _auditLog;

        public StockTransferService(AppDbContext context, Common common, IStockLedgerService stockLedger, IAuditLogService auditLog)
        {
            _context = context;
            _common = common;
            _stockLedger = stockLedger;
            _auditLog = auditLog;
        }

        public async Task<StockTransferResponseDto> CreateAsync(CreateStockTransferMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var financialYearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();

            var transferNo = await _common.GenerateDocumentNumber(_context, "Stock Transfer");

            var main = new StockTransferMain
            {
                CompanyId = companyId,
                FinancialYearId = financialYearId,
                TransferNo = transferNo,
                TransferDate = dto.TransferDate,
                FromWarehouseId = dto.FromWarehouseId,
                ToWarehouseId = dto.ToWarehouseId,
                LocationId = dto.LocationId,
                Remarks = dto.Remarks,
                Status = "Draft",
                CreatedBy = userId
            };

            _context.StockTransferMains.Add(main);
            await _context.SaveChangesAsync();

            foreach (var d in dto.Details)
            {
                var detail = new StockTransferDetail
                {
                    TransferId = main.TransferId,
                    ItemId = d.ItemId,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    CreatedBy = userId
                };
                _context.StockTransferDetails.Add(detail);
                await _context.SaveChangesAsync();

                if (d.Batches?.Any() == true)
                {
                    foreach (var b in d.Batches)
                        _context.StockTransferDetailBatches.Add(new StockTransferDetailBatch
                        {
                            TransferDetailId = detail.TransferDetailId,
                            ItemBatchId = b.ItemBatchId,
                            Qty = b.Qty
                        });
                }

                if (d.Serials?.Any() == true)
                {
                    foreach (var s in d.Serials)
                        _context.StockTransferDetailSerials.Add(new StockTransferDetailSerial
                        {
                            TransferDetailId = detail.TransferDetailId,
                            ItemSerialId = s.ItemSerialId
                        });
                }
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "StockTransfer",
                action: "Create",
                entityId: main.TransferId,
                entityNo: main.TransferNo,
                newValues: new { main.TransferNo, main.FromWarehouseId, main.ToWarehouseId, main.Status });
            return await GetByIdAsync(main.TransferId) ?? throw new Exception("Failed to retrieve created transfer.");
        }

        public async Task<bool> UpdateAsync(int id, UpdateStockTransferMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.StockTransferMains
                .Include(x => x.Details).ThenInclude(x => x.Batches)
                .Include(x => x.Details).ThenInclude(x => x.Serials)
                .FirstOrDefaultAsync(x => x.TransferId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null) return false;

            var oldValues = new { main.TransferNo, main.FromWarehouseId, main.ToWarehouseId, main.Status };

            main.TransferDate = dto.TransferDate;
            main.FromWarehouseId = dto.FromWarehouseId;
            main.ToWarehouseId = dto.ToWarehouseId;
            main.LocationId = dto.LocationId;
            main.Remarks = dto.Remarks;
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            // Remove old details
            foreach (var d in main.Details)
            {
                _context.StockTransferDetailSerials.RemoveRange(d.Serials);
                _context.StockTransferDetailBatches.RemoveRange(d.Batches);
            }
            _context.StockTransferDetails.RemoveRange(main.Details);

            // Add new details
            foreach (var d in dto.Details)
            {
                var detail = new StockTransferDetail
                {
                    TransferId = main.TransferId,
                    ItemId = d.ItemId,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    CreatedBy = userId
                };
                _context.StockTransferDetails.Add(detail);
                await _context.SaveChangesAsync();

                if (d.Batches?.Any() == true)
                {
                    foreach (var b in d.Batches)
                        _context.StockTransferDetailBatches.Add(new StockTransferDetailBatch
                        {
                            TransferDetailId = detail.TransferDetailId,
                            ItemBatchId = b.ItemBatchId,
                            Qty = b.Qty
                        });
                }

                if (d.Serials?.Any() == true)
                {
                    foreach (var s in d.Serials)
                        _context.StockTransferDetailSerials.Add(new StockTransferDetailSerial
                        {
                            TransferDetailId = detail.TransferDetailId,
                            ItemSerialId = s.ItemSerialId
                        });
                }
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "StockTransfer",
                action: "Update",
                entityId: main.TransferId,
                entityNo: main.TransferNo,
                oldValues: oldValues,
                newValues: new { main.TransferNo, main.FromWarehouseId, main.ToWarehouseId, main.Status });
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.StockTransferMains
                .FirstOrDefaultAsync(x => x.TransferId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null) return false;

            var oldValues = new { main.TransferNo, main.FromWarehouseId, main.ToWarehouseId, main.Status };

            main.IsDeleted = true;
            main.IsActive = false;
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "StockTransfer",
                action: "Delete",
                entityId: main.TransferId,
                entityNo: main.TransferNo,
                oldValues: oldValues,
                remarks: "Soft deleted");
            return true;
        }

        public async Task<StockTransferResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.StockTransferMains
                .Include(x => x.Details).ThenInclude(x => x.Item)
                .Include(x => x.Details).ThenInclude(x => x.Batches).ThenInclude(x => x.Batch)
                .Include(x => x.Details).ThenInclude(x => x.Serials).ThenInclude(x => x.Serial)
                .Include(x => x.FromWarehouse)
                .Include(x => x.ToWarehouse)
                .Include(x => x.Location)
                .FirstOrDefaultAsync(x => x.TransferId == id && x.CompanyId == companyId && !x.IsDeleted);

            if (main == null) return null;

            return new StockTransferResponseDto
            {
                TransferId = main.TransferId,
                CompanyId = main.CompanyId,
                FinancialYearId = main.FinancialYearId,
                TransferNo = main.TransferNo,
                TransferDate = main.TransferDate,
                FromWarehouseId = main.FromWarehouseId,
                FromWarehouseName = main.FromWarehouse?.WarehouseName,
                ToWarehouseId = main.ToWarehouseId,
                ToWarehouseName = main.ToWarehouse?.WarehouseName,
                Status = main.Status,
                LocationId = main.LocationId,
                LocationName = main.Location?.LocationName,
                Remarks = main.Remarks,
                Details = main.Details.Where(d => !d.IsDeleted).Select(d => new StockTransferDetailResponseDto
                {
                    TransferDetailId = d.TransferDetailId,
                    ItemId = d.ItemId,
                    ItemName = d.Item?.ItemName,
                    ItemCode = d.Item?.ItemCode,
                    Qty = d.Qty,
                    Remarks = d.Remarks,
                    Batches = d.Batches.Select(b => new StockTransferDetailBatchResponseDto
                    {
                        Id = b.Id,
                        ItemBatchId = b.ItemBatchId,
                        BatchNo = b.Batch?.BatchNo,
                        Qty = b.Qty
                    }).ToList(),
                    Serials = d.Serials.Select(s => new StockTransferDetailSerialResponseDto
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

        public async Task<PagedResponseDto<StockTransferListDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.StockTransferMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x => x.TransferNo.ToLower().Contains(s));
            }

            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("status"))
                {
                    var status = ((JsonElement)request.Filters["status"]).GetString();
                    query = query.Where(x => x.Status == status);
                }
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.TransferDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new StockTransferListDto
                {
                    TransferId = x.TransferId,
                    TransferNo = x.TransferNo,
                    FinancialYearId = x.FinancialYearId,
                    TransferDate = x.TransferDate,
                    FromWarehouseName = x.FromWarehouse != null ? x.FromWarehouse.WarehouseName : null,
                    ToWarehouseName = x.ToWarehouse != null ? x.ToWarehouse.WarehouseName : null,
                    Status = x.Status,
                    DetailCount = x.Details.Count(d => !d.IsDeleted),
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            return new PagedResponseDto<StockTransferListDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }

        public async Task<StockTransferResponseDto> ConfirmAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.StockTransferMains
                .Include(x => x.Details).ThenInclude(x => x.Batches)
                .Include(x => x.Details).ThenInclude(x => x.Serials)
                .FirstOrDefaultAsync(x => x.TransferId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");

            if (main == null)
                throw new Exception("Transfer not found or not in Draft status.");

            if (!main.Details.Any())
                throw new Exception("Transfer must have at least one detail line.");

            // Post stock ledger entries
            var lines = main.Details.Select(d => new StockLedgerLineDto
            {
                ItemId = d.ItemId,
                Qty = d.Qty,
                Remarks = $"Transfer: {main.TransferNo}"
            }).ToList();

            // OUT from source warehouse
            await _stockLedger.AddEntriesAsync(
                companyId, main.FromWarehouseId, main.TransferDate,
                "Stock Transfer", main.TransferNo, null,
                lines.Select(l => new StockLedgerLineDto
                {
                    ItemId = l.ItemId, Qty = -l.Qty, Remarks = l.Remarks
                }).ToList(), userId);

            // IN to destination warehouse
            await _stockLedger.AddEntriesAsync(
                companyId, main.ToWarehouseId, main.TransferDate,
                "Stock Transfer", main.TransferNo, null,
                lines, userId);

            main.Status = "Confirmed";
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve confirmed transfer.");
        }

        public async Task<StockTransferResponseDto> CancelAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.StockTransferMains
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.TransferId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Confirmed");

            if (main == null)
                throw new Exception("Transfer not found or not in Confirmed status.");

            // Reverse stock ledger entries
            await _stockLedger.ReverseEntriesAsync(
                companyId, main.TransferNo, $"{main.TransferNo}-Reversal",
                main.TransferDate, userId);

            main.Status = "Cancelled";
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve cancelled transfer.");
        }
    }
}
