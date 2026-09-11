using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.JobWorkReceiptDTOs;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class JobWorkReceiptService : IJobWorkReceiptService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IStockLedgerService _stockLedger;
        private readonly IAuditLogService _auditLog;

        public JobWorkReceiptService(AppDbContext context, Common common, IStockLedgerService stockLedger, IAuditLogService auditLog)
        {
            _context = context;
            _common = common;
            _stockLedger = stockLedger;
            _auditLog = auditLog;
        }

        public async Task<JobWorkReceiptResponseDto> CreateAsync(CreateJobWorkReceiptMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var financialYearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();
            var receiptNo = await _common.GenerateDocumentNumber(_context, "Job Work Receipt");

            var main = new JobWorkReceiptMain
            {
                CompanyId = companyId, FinancialYearId = financialYearId, ReceiptNo = receiptNo,
                ReceiptDate = dto.ReceiptDate, BusinessPartnerId = dto.BusinessPartnerId,
                WarehouseId = dto.WarehouseId, LocationId = dto.LocationId,
                Remarks = dto.Remarks, Status = "Draft", CreatedBy = userId
            };
            _context.JobWorkReceiptMains.Add(main);
            await _context.SaveChangesAsync();

            foreach (var d in dto.Details)
            {
                var detail = new JobWorkReceiptDetail
                {
                    JobWorkReceiptId = main.JobWorkReceiptId, ItemId = d.ItemId, Qty = d.Qty,
                    Remarks = d.Remarks, CreatedBy = userId
                };
                _context.JobWorkReceiptDetails.Add(detail);
                await _context.SaveChangesAsync();

                if (d.Batches?.Any() == true)
                    foreach (var b in d.Batches)
                        _context.JobWorkReceiptDetailBatches.Add(new JobWorkReceiptDetailBatch
                        { JobWorkReceiptDetailId = detail.JobWorkReceiptDetailId, ItemBatchId = b.ItemBatchId, Qty = b.Qty });

                if (d.Serials?.Any() == true)
                    foreach (var s in d.Serials)
                        _context.JobWorkReceiptDetailSerials.Add(new JobWorkReceiptDetailSerial
                        { JobWorkReceiptDetailId = detail.JobWorkReceiptDetailId, ItemSerialId = s.ItemSerialId });
            }
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "JobWorkReceipt",
                action: "Create",
                entityId: main.JobWorkReceiptId,
                entityNo: main.ReceiptNo,
                newValues: new { main.ReceiptNo, main.BusinessPartnerId, main.WarehouseId, main.Status });
            return await GetByIdAsync(main.JobWorkReceiptId) ?? throw new Exception("Failed to retrieve created receipt.");
        }

        public async Task<JobWorkReceiptResponseDto?> UpdateAsync(int id, UpdateJobWorkReceiptMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var main = await _context.JobWorkReceiptMains
                .Include(x => x.Details).ThenInclude(d => d.Batches)
                .Include(x => x.Details).ThenInclude(d => d.Serials)
                .FirstOrDefaultAsync(x => x.JobWorkReceiptId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");
            if (main == null) return null;

            var oldValues = new { main.ReceiptNo, main.BusinessPartnerId, main.WarehouseId, main.Status };

            main.ReceiptDate = dto.ReceiptDate; main.BusinessPartnerId = dto.BusinessPartnerId;
            main.WarehouseId = dto.WarehouseId; main.LocationId = dto.LocationId;
            main.Remarks = dto.Remarks; main.ModifiedBy = userId; main.ModifiedDate = DateTime.UtcNow;

            _context.JobWorkReceiptDetails.RemoveRange(main.Details.ToList());
            foreach (var d in dto.Details)
            {
                var detail = new JobWorkReceiptDetail
                {
                    JobWorkReceiptId = main.JobWorkReceiptId, ItemId = d.ItemId, Qty = d.Qty,
                    Remarks = d.Remarks, CreatedBy = userId
                };
                _context.JobWorkReceiptDetails.Add(detail);
                await _context.SaveChangesAsync();
                if (d.Batches?.Any() == true)
                    foreach (var b in d.Batches)
                        _context.JobWorkReceiptDetailBatches.Add(new JobWorkReceiptDetailBatch
                        { JobWorkReceiptDetailId = detail.JobWorkReceiptDetailId, ItemBatchId = b.ItemBatchId, Qty = b.Qty });
                if (d.Serials?.Any() == true)
                    foreach (var s in d.Serials)
                        _context.JobWorkReceiptDetailSerials.Add(new JobWorkReceiptDetailSerial
                        { JobWorkReceiptDetailId = detail.JobWorkReceiptDetailId, ItemSerialId = s.ItemSerialId });
            }
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "JobWorkReceipt",
                action: "Update",
                entityId: main.JobWorkReceiptId,
                entityNo: main.ReceiptNo,
                oldValues: oldValues,
                newValues: new { main.ReceiptNo, main.BusinessPartnerId, main.WarehouseId, main.Status });
            return await GetByIdAsync(id);
        }

        public async Task<JobWorkReceiptResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var main = await _context.JobWorkReceiptMains
                .Include(x => x.Warehouse).Include(x => x.Location).Include(x => x.BusinessPartner)
                .Include(x => x.Details).ThenInclude(d => d.Item)
                .Include(x => x.Details).ThenInclude(d => d.Batches).ThenInclude(b => b.Batch)
                .Include(x => x.Details).ThenInclude(d => d.Serials).ThenInclude(s => s.Serial)
                .FirstOrDefaultAsync(x => x.JobWorkReceiptId == id && x.CompanyId == companyId && !x.IsDeleted);
            if (main == null) return null;

            return new JobWorkReceiptResponseDto
            {
                JobWorkReceiptId = main.JobWorkReceiptId, CompanyId = main.CompanyId, FinancialYearId = main.FinancialYearId,
                ReceiptNo = main.ReceiptNo, ReceiptDate = main.ReceiptDate,
                BusinessPartnerId = main.BusinessPartnerId, BusinessPartnerName = main.BusinessPartner?.BusinessPartnerName,
                WarehouseId = main.WarehouseId, WarehouseName = main.Warehouse?.WarehouseName,
                Status = main.Status, LocationId = main.LocationId, LocationName = main.Location?.LocationName,
                Remarks = main.Remarks,
                Details = main.Details.Where(d => !d.IsDeleted).Select(d => new JobWorkReceiptDetailResponseDto
                {
                    JobWorkReceiptDetailId = d.JobWorkReceiptDetailId, ItemId = d.ItemId,
                    ItemName = d.Item?.ItemName, ItemCode = d.Item?.ItemCode, Qty = d.Qty, Remarks = d.Remarks,
                    Batches = d.Batches.Select(b => new JobWorkReceiptDetailBatchResponseDto
                    { Id = b.Id, ItemBatchId = b.ItemBatchId, BatchNo = b.Batch?.BatchNo, Qty = b.Qty }).ToList(),
                    Serials = d.Serials.Select(s => new JobWorkReceiptDetailSerialResponseDto
                    { Id = s.Id, ItemSerialId = s.ItemSerialId, SerialNo = s.Serial?.SerialNo }).ToList()
                }).ToList(),
                CreatedDate = main.CreatedDate, ModifiedDate = main.ModifiedDate
            };
        }

        public async Task<PagedResponseDto<JobWorkReceiptListDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();
            var query = _context.JobWorkReceiptMains
                .Include(x => x.Warehouse).Include(x => x.BusinessPartner).Include(x => x.Details)
                .Where(x => x.CompanyId == companyId && !x.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x => x.ReceiptNo.ToLower().Contains(s) || (x.BusinessPartner != null && x.BusinessPartner.BusinessPartnerName!.ToLower().Contains(s)));
            }
            if (request.Filters != null && request.Filters.ContainsKey("status") && request.Filters["status"] != null)
                query = query.Where(x => x.Status == request.Filters["status"]!.ToString());

            var totalRecords = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.ReceiptDate).ThenByDescending(x => x.JobWorkReceiptId)
                .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
                .Select(x => new JobWorkReceiptListDto
                {
                    JobWorkReceiptId = x.JobWorkReceiptId, ReceiptNo = x.ReceiptNo, FinancialYearId = x.FinancialYearId,
                    ReceiptDate = x.ReceiptDate, BusinessPartnerName = x.BusinessPartner != null ? x.BusinessPartner.BusinessPartnerName : null,
                    WarehouseName = x.Warehouse != null ? x.Warehouse.WarehouseName : null,
                    Status = x.Status, DetailCount = x.Details.Count(d => !d.IsDeleted), CreatedDate = x.CreatedDate
                }).ToListAsync();

            return new PagedResponseDto<JobWorkReceiptListDto> { TotalRecords = totalRecords, PageNumber = request.PageNumber, PageSize = request.PageSize, Data = items };
        }

        public async Task<JobWorkReceiptResponseDto> ConfirmAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var main = await _context.JobWorkReceiptMains
                .Include(x => x.Details).ThenInclude(d => d.Item)
                .FirstOrDefaultAsync(x => x.JobWorkReceiptId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");
            if (main == null) throw new Exception("Job Work Receipt not found or not in Draft status.");

            main.Status = "Confirmed"; main.ModifiedBy = userId; main.ModifiedDate = DateTime.UtcNow;

            foreach (var d in main.Details.Where(d => !d.IsDeleted))
            {
                if (d.Item == null) continue;
                var warehouseId = d.Item.ItemManageBy == Enums.ItemManageBy.Regular ? (int?)null : main.WarehouseId;
                await _stockLedger.AddEntriesAsync(companyId, warehouseId, main.ReceiptDate,
                    "Job Work Receipt", main.ReceiptNo, main.BusinessPartnerId,
                    new List<StockLedgerLineDto> { new StockLedgerLineDto { ItemId = d.ItemId, Qty = d.Qty, Rate = 0, Remarks = $"Job Work Receipt - {main.ReceiptNo}" } }, userId);
            }
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve confirmed receipt.");
        }

        public async Task<JobWorkReceiptResponseDto> CancelAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var main = await _context.JobWorkReceiptMains.Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.JobWorkReceiptId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Confirmed");
            if (main == null) throw new Exception("Job Work Receipt not found or not in Confirmed status.");

            main.Status = "Cancelled"; main.ModifiedBy = userId; main.ModifiedDate = DateTime.UtcNow;
            await _stockLedger.ReverseEntriesAsync(companyId, main.ReceiptNo, main.ReceiptNo + "-Reversal", DateTime.UtcNow, userId);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve cancelled receipt.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var main = await _context.JobWorkReceiptMains
                .FirstOrDefaultAsync(x => x.JobWorkReceiptId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");
            if (main == null) return false;

            var oldValues = new { main.ReceiptNo, main.BusinessPartnerId, main.WarehouseId, main.Status };

            main.IsDeleted = true; main.IsActive = false; main.ModifiedBy = userId; main.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "JobWorkReceipt",
                action: "Delete",
                entityId: main.JobWorkReceiptId,
                entityNo: main.ReceiptNo,
                oldValues: oldValues,
                remarks: "Soft deleted");
            return true;
        }
    }
}
