using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.JobWorkIssueDTOs;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class JobWorkIssueService : IJobWorkIssueService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IStockLedgerService _stockLedger;
        private readonly IAuditLogService _auditLog;

        public JobWorkIssueService(AppDbContext context, Common common, IStockLedgerService stockLedger, IAuditLogService auditLog)
        {
            _context = context;
            _common = common;
            _stockLedger = stockLedger;
            _auditLog = auditLog;
        }

        public async Task<JobWorkIssueResponseDto> CreateAsync(CreateJobWorkIssueMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var financialYearId = _common.GetFinancialYearId();
            var userId = _common.GetUserId();
            var issueNo = await _common.GenerateDocumentNumber(_context, "Job Work Issue");

            var main = new JobWorkIssueMain
            {
                CompanyId = companyId, FinancialYearId = financialYearId, IssueNo = issueNo,
                IssueDate = dto.IssueDate, BusinessPartnerId = dto.BusinessPartnerId,
                WarehouseId = dto.WarehouseId, LocationId = dto.LocationId,
                Remarks = dto.Remarks, Status = "Draft", CreatedBy = userId
            };
            _context.JobWorkIssueMains.Add(main);
            await _context.SaveChangesAsync();

            foreach (var d in dto.Details)
            {
                var detail = new JobWorkIssueDetail
                {
                    JobWorkIssueId = main.JobWorkIssueId, ItemId = d.ItemId, Qty = d.Qty,
                    Remarks = d.Remarks, CreatedBy = userId
                };
                _context.JobWorkIssueDetails.Add(detail);
                await _context.SaveChangesAsync();

                if (d.Batches?.Any() == true)
                    foreach (var b in d.Batches)
                        _context.JobWorkIssueDetailBatches.Add(new JobWorkIssueDetailBatch
                        { JobWorkIssueDetailId = detail.JobWorkIssueDetailId, ItemBatchId = b.ItemBatchId, Qty = b.Qty });

                if (d.Serials?.Any() == true)
                    foreach (var s in d.Serials)
                        _context.JobWorkIssueDetailSerials.Add(new JobWorkIssueDetailSerial
                        { JobWorkIssueDetailId = detail.JobWorkIssueDetailId, ItemSerialId = s.ItemSerialId });
            }
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "JobWorkIssue",
                action: "Create",
                entityId: main.JobWorkIssueId,
                entityNo: main.IssueNo,
                newValues: new { main.IssueNo, main.BusinessPartnerId, main.WarehouseId, main.Status });
            return await GetByIdAsync(main.JobWorkIssueId) ?? throw new Exception("Failed to retrieve created issue.");
        }

        public async Task<JobWorkIssueResponseDto?> UpdateAsync(int id, UpdateJobWorkIssueMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var main = await _context.JobWorkIssueMains
                .Include(x => x.Details).ThenInclude(d => d.Batches)
                .Include(x => x.Details).ThenInclude(d => d.Serials)
                .FirstOrDefaultAsync(x => x.JobWorkIssueId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");
            if (main == null) return null;

            var oldValues = new { main.IssueNo, main.BusinessPartnerId, main.WarehouseId, main.Status };

            main.IssueDate = dto.IssueDate; main.BusinessPartnerId = dto.BusinessPartnerId;
            main.WarehouseId = dto.WarehouseId; main.LocationId = dto.LocationId;
            main.Remarks = dto.Remarks; main.ModifiedBy = userId; main.ModifiedDate = DateTime.UtcNow;

            _context.JobWorkIssueDetails.RemoveRange(main.Details.ToList());
            foreach (var d in dto.Details)
            {
                var detail = new JobWorkIssueDetail
                {
                    JobWorkIssueId = main.JobWorkIssueId, ItemId = d.ItemId, Qty = d.Qty,
                    Remarks = d.Remarks, CreatedBy = userId
                };
                _context.JobWorkIssueDetails.Add(detail);
                await _context.SaveChangesAsync();
                if (d.Batches?.Any() == true)
                    foreach (var b in d.Batches)
                        _context.JobWorkIssueDetailBatches.Add(new JobWorkIssueDetailBatch
                        { JobWorkIssueDetailId = detail.JobWorkIssueDetailId, ItemBatchId = b.ItemBatchId, Qty = b.Qty });
                if (d.Serials?.Any() == true)
                    foreach (var s in d.Serials)
                        _context.JobWorkIssueDetailSerials.Add(new JobWorkIssueDetailSerial
                        { JobWorkIssueDetailId = detail.JobWorkIssueDetailId, ItemSerialId = s.ItemSerialId });
            }
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "JobWorkIssue",
                action: "Update",
                entityId: main.JobWorkIssueId,
                entityNo: main.IssueNo,
                oldValues: oldValues,
                newValues: new { main.IssueNo, main.BusinessPartnerId, main.WarehouseId, main.Status });
            return await GetByIdAsync(id);
        }

        public async Task<JobWorkIssueResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var main = await _context.JobWorkIssueMains
                .Include(x => x.Warehouse).Include(x => x.Location).Include(x => x.BusinessPartner)
                .Include(x => x.Details).ThenInclude(d => d.Item)
                .Include(x => x.Details).ThenInclude(d => d.Batches).ThenInclude(b => b.Batch)
                .Include(x => x.Details).ThenInclude(d => d.Serials).ThenInclude(s => s.Serial)
                .FirstOrDefaultAsync(x => x.JobWorkIssueId == id && x.CompanyId == companyId && !x.IsDeleted);
            if (main == null) return null;

            return new JobWorkIssueResponseDto
            {
                JobWorkIssueId = main.JobWorkIssueId, CompanyId = main.CompanyId, FinancialYearId = main.FinancialYearId,
                IssueNo = main.IssueNo, IssueDate = main.IssueDate,
                BusinessPartnerId = main.BusinessPartnerId, BusinessPartnerName = main.BusinessPartner?.BusinessPartnerName,
                WarehouseId = main.WarehouseId, WarehouseName = main.Warehouse?.WarehouseName,
                Status = main.Status, LocationId = main.LocationId, LocationName = main.Location?.LocationName,
                Remarks = main.Remarks,
                Details = main.Details.Where(d => !d.IsDeleted).Select(d => new JobWorkIssueDetailResponseDto
                {
                    JobWorkIssueDetailId = d.JobWorkIssueDetailId, ItemId = d.ItemId,
                    ItemName = d.Item?.ItemName, ItemCode = d.Item?.ItemCode, Qty = d.Qty, Remarks = d.Remarks,
                    Batches = d.Batches.Select(b => new JobWorkIssueDetailBatchResponseDto
                    { Id = b.Id, ItemBatchId = b.ItemBatchId, BatchNo = b.Batch?.BatchNo, Qty = b.Qty }).ToList(),
                    Serials = d.Serials.Select(s => new JobWorkIssueDetailSerialResponseDto
                    { Id = s.Id, ItemSerialId = s.ItemSerialId, SerialNo = s.Serial?.SerialNo }).ToList()
                }).ToList(),
                CreatedDate = main.CreatedDate, ModifiedDate = main.ModifiedDate
            };
        }

        public async Task<PagedResponseDto<JobWorkIssueListDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();
            var query = _context.JobWorkIssueMains
                .Include(x => x.Warehouse).Include(x => x.BusinessPartner).Include(x => x.Details)
                .Where(x => x.CompanyId == companyId && !x.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x => x.IssueNo.ToLower().Contains(s) || (x.BusinessPartner != null && x.BusinessPartner.BusinessPartnerName!.ToLower().Contains(s)));
            }
            if (request.Filters != null && request.Filters.ContainsKey("status") && request.Filters["status"] != null)
                query = query.Where(x => x.Status == request.Filters["status"]!.ToString());

            var totalRecords = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.IssueDate).ThenByDescending(x => x.JobWorkIssueId)
                .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
                .Select(x => new JobWorkIssueListDto
                {
                    JobWorkIssueId = x.JobWorkIssueId, IssueNo = x.IssueNo, FinancialYearId = x.FinancialYearId,
                    IssueDate = x.IssueDate, BusinessPartnerName = x.BusinessPartner != null ? x.BusinessPartner.BusinessPartnerName : null,
                    WarehouseName = x.Warehouse != null ? x.Warehouse.WarehouseName : null,
                    Status = x.Status, DetailCount = x.Details.Count(d => !d.IsDeleted), CreatedDate = x.CreatedDate
                }).ToListAsync();

            return new PagedResponseDto<JobWorkIssueListDto> { TotalRecords = totalRecords, PageNumber = request.PageNumber, PageSize = request.PageSize, Data = items };
        }

        public async Task<JobWorkIssueResponseDto> ConfirmAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var main = await _context.JobWorkIssueMains
                .Include(x => x.Details).ThenInclude(d => d.Item)
                .FirstOrDefaultAsync(x => x.JobWorkIssueId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");
            if (main == null) throw new Exception("Job Work Issue not found or not in Draft status.");

            main.Status = "Confirmed"; main.ModifiedBy = userId; main.ModifiedDate = DateTime.UtcNow;

            foreach (var d in main.Details.Where(d => !d.IsDeleted))
            {
                if (d.Item == null) continue;
                var warehouseId = d.Item.ItemManageBy == Enums.ItemManageBy.Regular ? (int?)null : main.WarehouseId;
                await _stockLedger.AddEntriesAsync(companyId, warehouseId, main.IssueDate,
                    "Job Work Issue", main.IssueNo, main.BusinessPartnerId,
                    new List<StockLedgerLineDto> { new StockLedgerLineDto { ItemId = d.ItemId, Qty = -d.Qty, Rate = 0, Remarks = $"Job Work Issue - {main.IssueNo}" } }, userId);
            }
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve confirmed issue.");
        }

        public async Task<JobWorkIssueResponseDto> CancelAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var main = await _context.JobWorkIssueMains.Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.JobWorkIssueId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Confirmed");
            if (main == null) throw new Exception("Job Work Issue not found or not in Confirmed status.");

            main.Status = "Cancelled"; main.ModifiedBy = userId; main.ModifiedDate = DateTime.UtcNow;
            await _stockLedger.ReverseEntriesAsync(companyId, main.IssueNo, main.IssueNo + "-Reversal", DateTime.UtcNow, userId);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve cancelled issue.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var main = await _context.JobWorkIssueMains
                .FirstOrDefaultAsync(x => x.JobWorkIssueId == id && x.CompanyId == companyId && !x.IsDeleted && x.Status == "Draft");
            if (main == null) return false;

            var oldValues = new { main.IssueNo, main.BusinessPartnerId, main.WarehouseId, main.Status };

            main.IsDeleted = true; main.IsActive = false; main.ModifiedBy = userId; main.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(
                module: "JobWorkIssue",
                action: "Delete",
                entityId: main.JobWorkIssueId,
                entityNo: main.IssueNo,
                oldValues: oldValues,
                remarks: "Soft deleted");
            return true;
        }
    }
}
