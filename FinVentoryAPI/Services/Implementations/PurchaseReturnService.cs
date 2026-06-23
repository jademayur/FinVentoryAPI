using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.PurchaseReturnDTOs;
using FinVentoryAPI.DTOs.SalesReturnDTOs;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IStockLedgerService _stockLedger;
        private readonly IAccountLedgerPostingService _accountLedger;

        public PurchaseReturnService(
            AppDbContext context,
            Common common,
            IStockLedgerService stockLedger,
            IAccountLedgerPostingService accountLedger)
        {
            _context = context;
            _common = common;
            _stockLedger = stockLedger;
            _accountLedger = accountLedger;
        }

        // ════════════════════════════════════════════════════
        // CREATE
        // ════════════════════════════════════════════════════
        public async Task<PurchaseReturnResponseDto> CreateAsync(CreatePurchaseReturnMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            // ✅ Validate BEFORE transaction (read-only)
            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId,
                dto.PurchaseAccountId, companyId,
                dto.PurchaseStateCode, dto.BillStateCode,
                dto.BillAddressId);

            var returnNo = await GenerateReturnNoAsync(companyId, finYearId);

            var main = new PurchaseReturnMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                ReturnNo = returnNo,
                ReturnDate = dto.ReturnDate,
                OriginalInvoiceId = dto.OriginalInvoiceId,
                OriginalInvoiceNo = dto.OriginalInvoiceNo,
                OriginalInvoiceDate = dto.OriginalInvoiceDate,
                NoteType = dto.NoteType,
                BusinessPartnerId = dto.BusinessPartnerId,
                LocationId = dto.LocationId,
                PurchaseAccountId = dto.PurchaseAccountId,
                RoundOff = dto.RoundOff,
                Remarks = dto.Remarks,
                Status = "Draft",
                PurchaseStateCode = dto.PurchaseStateCode,
                BillStateCode = dto.BillStateCode,
                BillAddressId = dto.BillAddressId,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                Details = new List<PurchaseReturnDetail>(),
                TaxDetails = new List<PurchaseReturnTaxDetail>()
            };

            decimal totalSubTotal = 0;
            decimal totalTaxAmount = 0;
            decimal totalCessAmount = 0;

            foreach (var lineDto in dto.Details)
            {
                var detail = await BuildDetailWithTaxAsync(
                    lineDto, userId, dto.PurchaseStateCode, dto.BillStateCode);

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                foreach (var td in detail.TaxDetails ?? Enumerable.Empty<PurchaseReturnTaxDetail>())
                {
                    td.Return = main;
                    td.Detail = detail;
                    main.TaxDetails!.Add(td);
                }

                main.Details!.Add(detail);
            }

            main.SubTotal = totalSubTotal;
            main.TaxAmount = totalTaxAmount;
            main.CessAmount = totalCessAmount;
            main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

            // ── Begin Transaction ─────────────────────────────────────────
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.PurchaseReturnMains.Add(main);
                await SaveChangesAsync();

                // ✅ Clear tracker so EF re-fetches ItemSerial/ItemBatch fresh
                _context.ChangeTracker.Clear();
                _context.Attach(main);

                for (int i = 0; i < dto.Details.Count; i++)
                {
                    var lineDto = dto.Details[i];

                    var detail = await _context.PurchaseReturnDetails
                        .FirstOrDefaultAsync(d =>
                            d.ReturnId == main.ReturnId &&
                            d.ItemId == lineDto.ItemId)
                        ?? throw new Exception($"Detail for item {lineDto.ItemId} not found after save.");

                    var updateDto = new UpdatePurchaseReturnDetailDto
                    {
                        ItemId = lineDto.ItemId,
                        PriceType = lineDto.PriceType,
                        Qty = lineDto.Qty,
                        Rate = lineDto.Rate,
                        DiscountRate = lineDto.DiscountRate,
                        AddisDiscountRate = lineDto.AddisDiscountRate,
                        IsTaxIncluded = lineDto.IsTaxIncluded,
                        Batches = lineDto.Batches,
                        Serials = lineDto.Serials
                    };

                    await SaveBatchAllocationsAsync(detail, updateDto, companyId, main.FinYearId, isNew: true);
                }

                await SaveChangesAsync();

                // Re-fetch with all includes for stock/account posting
                var mainForPosting = await _context.PurchaseReturnMains
                    .Include(m => m.Details!).ThenInclude(d => d.TaxDetails)
                    .Include(m => m.TaxDetails)
                    .Include(m => m.BusinessPartner)
                    .FirstOrDefaultAsync(m => m.ReturnId == main.ReturnId)
                    ?? throw new Exception("Purchase return not found after save.");

                await PostStockLedgerAsync(mainForPosting, isReversal: false);
                await PostAccountLedgerAsync(mainForPosting, isReversal: false);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(main.ReturnId)
                ?? throw new Exception("Failed to retrieve saved purchase return.");
        }

        // ════════════════════════════════════════════════════
        // UPDATE
        // ════════════════════════════════════════════════════
        public async Task<bool> UpdateAsync(int id, UpdatePurchaseReturnMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.PurchaseReturnMains
                .Include(m => m.Details!).ThenInclude(d => d.TaxDetails)
                .Include(m => m.Details!).ThenInclude(d => d.Batches!).ThenInclude(b => b.Batch)
                .Include(m => m.Details!).ThenInclude(d => d.Serials!).ThenInclude(s => s.Serial)
                .Include(m => m.TaxDetails)
                .Include(m => m.BusinessPartner)
                .FirstOrDefaultAsync(x =>
                    x.ReturnId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;
            if (main.Status != "Draft")
                throw new Exception("Only Draft returns can be updated.");

            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId,
                dto.PurchaseAccountId, companyId,
                dto.PurchaseStateCode, dto.BillStateCode,
                dto.BillAddressId);

            // Build fresh detail calculations before transaction (read-only)
            var incomingDetails = new List<PurchaseReturnDetail>();
            foreach (var lineDto in dto.Details)
            {
                var createDto = new CreatePurchaseReturnDetailDto
                {
                    ItemId = lineDto.ItemId,
                    PriceType = lineDto.PriceType,
                    Qty = lineDto.Qty,
                    Rate = lineDto.Rate,
                    DiscountRate = lineDto.DiscountRate,
                    AddisDiscountRate = lineDto.AddisDiscountRate,
                    IsTaxIncluded = lineDto.IsTaxIncluded
                };
                incomingDetails.Add(await BuildDetailWithTaxAsync(
                    createDto, userId, dto.PurchaseStateCode, dto.BillStateCode));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update header
                main.ReturnDate = dto.ReturnDate;
                main.OriginalInvoiceId = dto.OriginalInvoiceId;
                main.OriginalInvoiceNo = dto.OriginalInvoiceNo;
                main.OriginalInvoiceDate = dto.OriginalInvoiceDate;
                main.NoteType = dto.NoteType;
                main.BusinessPartnerId = dto.BusinessPartnerId;
                main.LocationId = dto.LocationId;
                main.PurchaseAccountId = dto.PurchaseAccountId;
                main.RoundOff = dto.RoundOff;
                main.Remarks = dto.Remarks;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;
                main.PurchaseStateCode = dto.PurchaseStateCode;
                main.BillStateCode = dto.BillStateCode;
                main.BillAddressId = dto.BillAddressId;

                var existingDetails = main.Details!.ToList();
                decimal totalSubTotal = 0;
                decimal totalTaxAmount = 0;
                decimal totalCessAmount = 0;

                for (int i = 0; i < incomingDetails.Count; i++)
                {
                    var incoming = incomingDetails[i];
                    var incomingDto = dto.Details[i];

                    if (i < existingDetails.Count)
                    {
                        var existing = existingDetails[i];
                        await ReverseBatchAllocationsAsync(existing);

                        existing.ItemId = incoming.ItemId;
                        existing.HsnId = incoming.HsnId;
                        existing.HsnCode = incoming.HsnCode;
                        existing.PriceType = incoming.PriceType;
                        existing.Qty = incoming.Qty;
                        existing.Rate = incoming.Rate;
                        existing.DiscountRate = incoming.DiscountRate;
                        existing.AddisDiscountRate = incoming.AddisDiscountRate;
                        existing.DiscountAmount = incoming.DiscountAmount;
                        existing.AddisDiscountAmount = incoming.AddisDiscountAmount;
                        existing.IsTaxIncluded = incoming.IsTaxIncluded;
                        existing.TaxableAmount = incoming.TaxableAmount;
                        existing.CessRate = incoming.CessRate;
                        existing.CessAmount = incoming.CessAmount;
                        existing.LineTaxAmount = incoming.LineTaxAmount;
                        existing.LineTotal = incoming.LineTotal;

                        // Sync tax details
                        var existingTaxList = existing.TaxDetails?.ToList() ?? new();
                        var incomingTaxList = incoming.TaxDetails ?? new();

                        for (int t = 0; t < incomingTaxList.Count; t++)
                        {
                            var inTax = incomingTaxList[t];
                            if (t < existingTaxList.Count)
                            {
                                var exTax = existingTaxList[t];
                                exTax.TaxId = inTax.TaxId;
                                exTax.IGSTRate = inTax.IGSTRate;
                                exTax.CGSTRate = inTax.CGSTRate;
                                exTax.SGSTRate = inTax.SGSTRate;
                                exTax.TaxableAmount = inTax.TaxableAmount;
                                exTax.IGSTAmount = inTax.IGSTAmount;
                                exTax.CGSTAmount = inTax.CGSTAmount;
                                exTax.SGSTAmount = inTax.SGSTAmount;
                                exTax.CessRate = inTax.CessRate;
                                exTax.CessAmount = inTax.CessAmount;
                                exTax.TotalTaxAmount = inTax.TotalTaxAmount;
                                exTax.IGSTPostingAccountId = inTax.IGSTPostingAccountId;
                                exTax.CGSTPostingAccountId = inTax.CGSTPostingAccountId;
                                exTax.SGSTPostingAccountId = inTax.SGSTPostingAccountId;
                                exTax.CessPostingAccountId = inTax.CessPostingAccountId;
                            }
                            else
                            {
                                inTax.ReturnId = main.ReturnId;
                                inTax.DetailId = existing.DetailId;
                                _context.PurchaseReturnTaxDetails.Add(inTax);
                            }
                        }

                        if (existingTaxList.Count > incomingTaxList.Count)
                            _context.PurchaseReturnTaxDetails.RemoveRange(
                                existingTaxList.Skip(incomingTaxList.Count));

                        await SaveBatchAllocationsAsync(existing, incomingDto, companyId, main.FinYearId, isNew: false);

                        totalSubTotal += existing.TaxableAmount;
                        totalCessAmount += existing.CessAmount;
                        totalTaxAmount += existing.LineTaxAmount - existing.CessAmount;
                    }
                    else
                    {
                        // Insert new detail row
                        incoming.ReturnId = main.ReturnId;
                        var newTaxList = incoming.TaxDetails ?? new();
                        incoming.TaxDetails = null!;
                        incoming.Batches = null;
                        incoming.Serials = null;

                        _context.PurchaseReturnDetails.Add(incoming);
                        await SaveChangesAsync();

                        foreach (var inTax in newTaxList)
                        {
                            inTax.ReturnId = main.ReturnId;
                            inTax.DetailId = incoming.DetailId;
                            _context.PurchaseReturnTaxDetails.Add(inTax);
                        }

                        await SaveBatchAllocationsAsync(incoming, incomingDto, companyId, main.FinYearId, isNew: true);

                        totalSubTotal += incoming.TaxableAmount;
                        totalCessAmount += incoming.CessAmount;
                        totalTaxAmount += incoming.LineTaxAmount - incoming.CessAmount;
                    }
                }

                // Remove surplus detail rows
                if (existingDetails.Count > incomingDetails.Count)
                {
                    var surplus = existingDetails.Skip(incomingDetails.Count).ToList();
                    foreach (var sd in surplus)
                    {
                        await ReverseBatchAllocationsAsync(sd);
                        if (sd.TaxDetails != null && sd.TaxDetails.Any())
                            _context.PurchaseReturnTaxDetails.RemoveRange(sd.TaxDetails);
                    }
                    _context.PurchaseReturnDetails.RemoveRange(surplus);
                }

                main.SubTotal = totalSubTotal;
                main.TaxAmount = totalTaxAmount;
                main.CessAmount = totalCessAmount;
                main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

                await SaveChangesAsync();

                await UpdateStockLedgerAsync(main);
                await UpdateAccountLedgerAsync(main);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            _context.ChangeTracker.Clear();
            return true;
        }

        // ════════════════════════════════════════════════════
        // DELETE
        // ════════════════════════════════════════════════════
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.PurchaseReturnMains
                .Include(m => m.TaxDetails)
                .Include(m => m.BusinessPartner)
                .Include(m => m.Details!)
                    .ThenInclude(d => d.Batches!).ThenInclude(b => b.Batch)
                .Include(m => m.Details!)
                    .ThenInclude(d => d.Serials!).ThenInclude(s => s.Serial)
                .FirstOrDefaultAsync(x =>
                    x.ReturnId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;
            if (main.Status != "Draft")
                throw new Exception("Only Draft returns can be deleted.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (main.Details != null)
                    foreach (var detail in main.Details)
                        await ReverseBatchAllocationsAsync(detail);

                main.IsDeleted = true;
                main.IsActive = false;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

                await _stockLedger.SoftDeleteByVoucherAsync(companyId, main.ReturnNo, userId);
                await _accountLedger.SoftDeleteByVoucherAsync(
                    companyId, main.FinYearId, main.ReturnNo, userId);

                await SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ════════════════════════════════════════════════════
        // GET ALL
        // ════════════════════════════════════════════════════
        public async Task<List<PurchaseReturnResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var returns = await _context.PurchaseReturnMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.PurchaseAccount)
                .Include(x => x.BillAddress)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.Batches!).ThenInclude(b => b.Batch)
                .Include(x => x.Details!).ThenInclude(d => d.Serials!).ThenInclude(s => s.Serial)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .OrderByDescending(x => x.ReturnDate)
                .ToListAsync();

            return returns.Select(MapToResponseDto).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET BY ID
        // ════════════════════════════════════════════════════
        public async Task<PurchaseReturnResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.PurchaseReturnMains
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.PurchaseAccount)
                .Include(x => x.BillAddress)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.Batches!).ThenInclude(b => b.Batch)
                .Include(x => x.Details!).ThenInclude(d => d.Serials!).ThenInclude(s => s.Serial)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.IGSTPostingAccount)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.CGSTPostingAccount)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.SGSTPostingAccount)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.CessPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .FirstOrDefaultAsync(x =>
                    x.ReturnId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return null;
            return MapToResponseDto(main);
        }

        // ════════════════════════════════════════════════════
        // GET PAGED
        // ════════════════════════════════════════════════════
        public async Task<PagedResponseDto<PurchaseReturnResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.PurchaseReturnMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.PurchaseAccount)
                .Include(x => x.Details!)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.ReturnNo.ToLower().Contains(search) ||
                    x.BusinessPartner!.BusinessPartnerName.ToLower().Contains(search));
            }

            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("status"))
                {
                    var status = ((JsonElement)request.Filters["status"]).GetString();
                    query = query.Where(x => x.Status == status);
                }
                if (request.Filters.ContainsKey("businessPartnerId"))
                {
                    var bpId = ((JsonElement)request.Filters["businessPartnerId"]).GetInt32();
                    query = query.Where(x => x.BusinessPartnerId == bpId);
                }
                if (request.Filters.ContainsKey("noteType"))
                {
                    var noteType = ((JsonElement)request.Filters["noteType"]).GetString();
                    query = query.Where(x => x.NoteType == noteType);
                }
                if (request.Filters.ContainsKey("finYearId"))
                {
                    var finYearId = ((JsonElement)request.Filters["finYearId"]).GetInt32();
                    query = query.Where(x => x.FinYearId == finYearId);
                }
                if (request.Filters.ContainsKey("fromDate"))
                {
                    var fromDate = ((JsonElement)request.Filters["fromDate"]).GetDateTime();
                    query = query.Where(x => x.ReturnDate >= fromDate);
                }
                if (request.Filters.ContainsKey("toDate"))
                {
                    var toDate = ((JsonElement)request.Filters["toDate"]).GetDateTime();
                    query = query.Where(x => x.ReturnDate <= toDate);
                }
                if (request.Filters.ContainsKey("originalInvoiceId"))
                {
                    var invId = ((JsonElement)request.Filters["originalInvoiceId"]).GetInt32();
                    query = query.Where(x => x.OriginalInvoiceId == invId);
                }
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "returnno" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.ReturnNo) : query.OrderBy(x => x.ReturnNo),
                    "returndate" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.ReturnDate) : query.OrderBy(x => x.ReturnDate),
                    "businesspartnername" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.BusinessPartner!.BusinessPartnerName)
                        : query.OrderBy(x => x.BusinessPartner!.BusinessPartnerName),
                    "status" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "nettotal" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.NetTotal) : query.OrderBy(x => x.NetTotal),
                    _ => query.OrderByDescending(x => x.ReturnDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.ReturnDate);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.Batches!).ThenInclude(b => b.Batch)
                .Include(x => x.Details!).ThenInclude(d => d.Serials!).ThenInclude(s => s.Serial)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .ToListAsync();

            return new PagedResponseDto<PurchaseReturnResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapToResponseDto).ToList()
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Batch / Serial
        // Purchase Return = stock going BACK OUT to supplier.
        // Batch  → reduce AvailableQty (undo the original receipt).
        // Serial → set Status = Returned (item sent back to supplier).
        // ════════════════════════════════════════════════════
        private async Task SaveBatchAllocationsAsync(
            PurchaseReturnDetail detail,
            UpdatePurchaseReturnDetailDto lineDto,
            int companyId,
            int finYearId,
            bool isNew)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemId == detail.ItemId)
                ?? throw new Exception($"Item {detail.ItemId} not found.");

            switch (item.ItemManageBy)
            {
                case ItemManageBy.Batch:
                    await SaveBatchLinesAsync(detail, lineDto, companyId, finYearId);
                    break;
                case ItemManageBy.Serial:
                    await SaveSerialLinesAsync(detail, lineDto, companyId);
                    break;
                    // Regular → nothing to do
            }
        }

        private async Task SaveBatchLinesAsync(
            PurchaseReturnDetail detail,
            UpdatePurchaseReturnDetailDto lineDto,
            int companyId,
            int finYearId)
        {
            if (lineDto.Batches == null || !lineDto.Batches.Any())
                throw new Exception(
                    $"Item {detail.ItemId} is Batch-managed. Please select at least one batch.");

            decimal totalAllocated = lineDto.Batches.Sum(b => b.Qty);
            if (totalAllocated != detail.Qty)
                throw new Exception(
                    $"Batch qty total ({totalAllocated}) must equal return qty ({detail.Qty}).");

            foreach (var batchDto in lineDto.Batches)
            {
                var batch = await _context.ItemBatches
                    .FirstOrDefaultAsync(b =>
                        b.BatchNo == batchDto.BatchNo &&
                        b.CompanyId == companyId &&
                        b.ItemId == detail.ItemId &&
                        !b.IsDeleted)
                    ?? throw new Exception($"Batch {batchDto.BatchNo} not found.");

                if (batch.AvailableQty < batchDto.Qty)
                    throw new Exception(
                        $"Batch '{batch.BatchNo}' has only {batch.AvailableQty} units available " +
                        $"but {batchDto.Qty} requested for return.");

                // Purchase Return → reduce available (stock going out)
                batch.AvailableQty -= batchDto.Qty;
                batch.ReceivedQty -= batchDto.Qty;

                _context.PurchaseReturnDetailBatches.Add(new PurchaseReturnDetailBatch
                {
                    DetailId = detail.DetailId,
                    ReturnId = detail.ReturnId,
                    BatchNo = batchDto.BatchNo,
                    Qty = batchDto.Qty
                });
            }
        }

        private async Task SaveSerialLinesAsync(
            PurchaseReturnDetail detail,
            UpdatePurchaseReturnDetailDto lineDto,
            int companyId)
        {
            if (lineDto.Serials == null || !lineDto.Serials.Any())
                throw new Exception(
                    $"Item {detail.ItemId} is Serial-managed. Please select at least one serial number.");

            if (lineDto.Serials.Count != (int)detail.Qty)
                throw new Exception(
                    $"Serial count ({lineDto.Serials.Count}) must equal return qty ({detail.Qty}).");

            foreach (var serialDto in lineDto.Serials)
            {
                // Always read fresh from DB
                var serial = await _context.ItemSerials
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.SerialNo == serialDto.SerialNo &&
                        s.CompanyId == companyId &&
                        s.ItemId == detail.ItemId &&
                        !s.IsDeleted)
                    ?? throw new Exception($"Serial {serialDto.SerialNo} not found.");

                // Must be InStock to be returnable to supplier
                if (serial.Status != SerialStatus.InStock)
                    throw new Exception(
                        $"Serial '{serial.SerialNo}' is not InStock (current: {serial.Status}). Cannot return to supplier.");

                // Set to Returned
                var trackedSerial = await _context.ItemSerials
                    .FirstOrDefaultAsync(s => s.SerialNo == serialDto.SerialNo);

                if (trackedSerial != null)
                    trackedSerial.Status = SerialStatus.Returned;

                _context.PurchaseReturnDetailSerials.Add(new PurchaseReturnDetailSerial
                {
                    DetailId = detail.DetailId,
                    ReturnId = detail.ReturnId,
                    SerialNo = serialDto.SerialNo
                });
            }
        }

        /// <summary>
        /// When a purchase return detail is updated/deleted:
        /// Undo stock reduction — restore batch qty, reset serial to InStock.
        /// </summary>
        private async Task ReverseBatchAllocationsAsync(PurchaseReturnDetail detail)
        {
            var userId = _common.GetUserId();

            // Reverse Batches — restore qty
            if (detail.Batches != null && detail.Batches.Any())
            {
                foreach (var alloc in detail.Batches)
                {
                    var batch = alloc.Batch
                        ?? await _context.ItemBatches.FindAsync(alloc.BatchId);

                    if (batch != null)
                    {
                        batch.AvailableQty += alloc.Qty;
                        batch.ReceivedQty += alloc.Qty;
                        batch.ModifiedBy = userId;
                        batch.ModifiedDate = DateTime.UtcNow;
                    }
                }
                _context.PurchaseReturnDetailBatches.RemoveRange(detail.Batches);
            }

            // Reverse Serials — set back to InStock
            if (detail.Serials != null && detail.Serials.Any())
            {
                foreach (var alloc in detail.Serials)
                {
                    var serial = alloc.Serial
                        ?? await _context.ItemSerials.FindAsync(alloc.SerialId);

                    if (serial != null)
                    {
                        serial.Status = SerialStatus.InStock;
                        serial.ModifiedBy = userId;
                        serial.ModifiedDate = DateTime.UtcNow;
                    }
                }
                _context.PurchaseReturnDetailSerials.RemoveRange(detail.Serials);
            }
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Validation
        // ════════════════════════════════════════════════════
        private async Task ValidateHeaderAsync(
            int businessPartnerId, int locationId, int purchaseAccountId,
            int companyId,
            int? purchaseStateCode, int? billStateCode,
            int? billAddressId)
        {
            var bpExists = await _context.BusinessPartners
                .AnyAsync(x =>
                    x.BusinessPartnerId == businessPartnerId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!bpExists) throw new Exception("Business Partner (Supplier) not found.");

            var locationExists = await _context.Locations
                .AnyAsync(x => x.LocationId == locationId && x.CompanyId == companyId);
            if (!locationExists) throw new Exception("Location not found.");

            var purchaseAccountExists = await _context.Accounts
                .AnyAsync(x =>
                    x.AccountId == purchaseAccountId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!purchaseAccountExists) throw new Exception("Purchase Account not found.");

            if (purchaseStateCode.HasValue && !Enum.IsDefined(typeof(GstState), purchaseStateCode.Value))
                throw new Exception("Invalid Purchase State Code.");

            if (billStateCode.HasValue && !Enum.IsDefined(typeof(GstState), billStateCode.Value))
                throw new Exception("Invalid Bill State Code.");

            if (billAddressId.HasValue)
            {
                var billExists = await _context.BusinessPartnerAddresses
                    .AnyAsync(x =>
                        x.BPAddressId == billAddressId &&
                        x.BusinessPartnerId == businessPartnerId);
                if (!billExists)
                    throw new Exception("Bill Address not found for this Business Partner.");
            }
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Tax Calculation
        // Same logic as PurchaseInvoiceService.BuildDetailWithTaxAsync
        // ════════════════════════════════════════════════════
        private async Task<PurchaseReturnDetail> BuildDetailWithTaxAsync(
            CreatePurchaseReturnDetailDto lineDto, int userId,
            int? purchaseStateCode, int? billStateCode)
        {
            var item = await _context.Items
                .Include(i => i.Hsn).ThenInclude(h => h!.tax)
                .FirstOrDefaultAsync(i => i.ItemId == lineDto.ItemId && !i.IsDeleted)
                ?? throw new Exception($"Item {lineDto.ItemId} not found.");

            if (item.HSNCodeId == 0)
                throw new Exception($"Item '{item.ItemName}' has no HSN Code assigned.");
            if (item.Hsn == null)
                throw new Exception($"Item '{item.ItemName}' — HSN (Id: {item.HSNCodeId}) not found.");
            if (item.Hsn.tax == null)
                throw new Exception($"HSN '{item.Hsn.HsnName}' has no Tax assigned.");

            var hsn = item.Hsn;
            var tax = hsn.tax;

            decimal grossAmount = lineDto.Rate * lineDto.Qty;
            decimal discountAmt = Math.Round(grossAmount * lineDto.DiscountRate / 100, 2);
            decimal afterFirst = grossAmount - discountAmt;
            decimal addisDiscAmt = Math.Round(afterFirst * lineDto.AddisDiscountRate / 100, 2);
            decimal taxableAmount = afterFirst - addisDiscAmt;

            bool isIntraState = (purchaseStateCode.HasValue && billStateCode.HasValue)
                ? purchaseStateCode.Value == billStateCode.Value : true;

            if (lineDto.IsTaxIncluded)
            {
                decimal totalTaxRate = isIntraState
                    ? (tax.CGST + tax.SGST) : tax.IGST;
                if (totalTaxRate > 0)
                    taxableAmount = Math.Round(taxableAmount / (1 + totalTaxRate / 100), 2);
            }

            decimal igstAmount = !isIntraState ? Math.Round(taxableAmount * tax.IGST / 100, 2) : 0;
            decimal cgstAmount = isIntraState ? Math.Round(taxableAmount * tax.CGST / 100, 2) : 0;
            decimal sgstAmount = isIntraState ? Math.Round(taxableAmount * tax.SGST / 100, 2) : 0;
            decimal cessRate = hsn.Cess ?? 0;
            decimal cessAmount = Math.Round(taxableAmount * cessRate / 100, 2);
            decimal lineTaxAmt = igstAmount + cgstAmount + sgstAmount + cessAmount;

            return new PurchaseReturnDetail
            {
                ItemId = lineDto.ItemId,
                HsnId = hsn.HsnId,
                HsnCode = hsn.HsnName,
                PriceType = lineDto.PriceType,
                Qty = lineDto.Qty,
                Rate = lineDto.Rate,
                DiscountRate = lineDto.DiscountRate,
                AddisDiscountRate = lineDto.AddisDiscountRate,
                DiscountAmount = discountAmt,
                AddisDiscountAmount = addisDiscAmt,
                IsTaxIncluded = lineDto.IsTaxIncluded,
                TaxableAmount = taxableAmount,
                CessRate = cessRate,
                CessAmount = cessAmount,
                LineTaxAmount = lineTaxAmt,
                LineTotal = taxableAmount + lineTaxAmt,
                TaxDetails = new List<PurchaseReturnTaxDetail>
                {
                    new()
                    {
                        TaxId                = tax.TaxId,
                        IGSTRate             = isIntraState ? 0        : tax.IGST,
                        CGSTRate             = isIntraState ? tax.CGST : 0,
                        SGSTRate             = isIntraState ? tax.SGST : 0,
                        TaxableAmount        = taxableAmount,
                        IGSTAmount           = igstAmount,
                        CGSTAmount           = cgstAmount,
                        SGSTAmount           = sgstAmount,
                        CessRate             = cessRate,
                        CessAmount           = cessAmount,
                        TotalTaxAmount       = lineTaxAmt,
                        IGSTPostingAccountId = isIntraState ? null : tax.IGSTPostingAccountId,
                        CGSTPostingAccountId = isIntraState ? tax.CGSTPostingAccountId : null,
                        SGSTPostingAccountId = isIntraState ? tax.SGSTPostingAccountId : null,
                        CessPostingAccountId = hsn.CessPostingAc
                    }
                }
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Return Number
        // ════════════════════════════════════════════════════
        private async Task<string> GenerateReturnNoAsync(int companyId, int finYearId)
        {
            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(x => x.FinancialYearId == finYearId);

            var yearLabel = financialYear != null
                ? $"{financialYear.StartDate.Year % 100}{financialYear.EndDate.Year % 100}"
                : finYearId.ToString();

            var count = await _context.PurchaseReturnMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"PR-{yearLabel}-{(count + 1):D4}";
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Stock Ledger
        // Purchase Return = stock OUT (negative qty), opposite of Purchase Invoice
        // ════════════════════════════════════════════════════
        private async Task PostStockLedgerAsync(PurchaseReturnMain main, bool isReversal)
        {
            if (main.Details == null || !main.Details.Any()) return;

            var lines = main.Details.Select(d => new StockLedgerLineDto
            {
                ItemId = d.ItemId,
                Qty = isReversal ? d.Qty : -d.Qty,  // negative = stock OUT on return
                Rate = d.Rate,
                Remarks = $"Purchase Return: {main.ReturnNo}"
            }).ToList();

            await _stockLedger.AddEntriesAsync(
                companyId: main.CompanyId,
                warehouseId: null,
                date: main.ReturnDate,
                voucherType: "Purchase Return",
                voucherNo: main.ReturnNo,
                businessPartnerId: main.BusinessPartnerId,
                lines: lines,
                createdBy: (int?)main.CreatedBy);
        }

        private async Task UpdateStockLedgerAsync(PurchaseReturnMain main)
        {
            if (main.Details == null || !main.Details.Any()) return;

            var lines = main.Details.Select(d => new StockLedgerLineDto
            {
                ItemId = d.ItemId,
                Qty = -d.Qty,
                Rate = d.Rate,
                Remarks = $"Purchase Return: {main.ReturnNo}"
            }).ToList();

            await _stockLedger.UpdateEntriesAsync(
                companyId: main.CompanyId,
                warehouseId: null,
                date: main.ReturnDate,
                voucherType: "Purchase Return",
                voucherNo: main.ReturnNo,
                businessPartnerId: main.BusinessPartnerId,
                lines: lines,
                modifiedBy: (int?)main.ModifiedBy);
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Account Ledger
        // Purchase Return journal (opposite of Purchase Invoice):
        //   Cr  Purchase Account       (reverse the expense)
        //   Cr  Tax Input Accounts     (reverse IGST/CGST/SGST input credit)
        //   Dr  Supplier (AP) Account  (reduce payable)
        // ════════════════════════════════════════════════════
        private async Task PostAccountLedgerAsync(PurchaseReturnMain main, bool isReversal)
        {
            var bp = main.BusinessPartner
                ?? await _context.BusinessPartners
                    .FirstOrDefaultAsync(x => x.BusinessPartnerId == main.BusinessPartnerId);
            if (bp == null) return;

            var lines = BuildAccountLedgerLines(main, bp, isReversal);
            await _accountLedger.AddEntriesAsync(
                companyId: main.CompanyId,
                financialYearId: main.FinYearId,
                date: main.ReturnDate,
                voucherType: "Purchase Return",
                voucherNo: main.ReturnNo,
                lines: lines,
                createdBy: (int?)main.CreatedBy);
        }

        private async Task UpdateAccountLedgerAsync(PurchaseReturnMain main)
        {
            var bp = main.BusinessPartner
                ?? await _context.BusinessPartners
                    .FirstOrDefaultAsync(x => x.BusinessPartnerId == main.BusinessPartnerId);
            if (bp == null) return;

            var lines = BuildAccountLedgerLines(main, bp, isReversal: false);
            await _accountLedger.UpdateEntriesAsync(
                companyId: main.CompanyId,
                financialYearId: main.FinYearId,
                date: main.ReturnDate,
                voucherType: "Purchase Return",
                voucherNo: main.ReturnNo,
                lines: lines,
                modifiedBy: (int?)main.ModifiedBy);
        }

        private List<AccountLedgerLineDto> BuildAccountLedgerLines(
            PurchaseReturnMain main, BusinessPartner bp, bool isReversal)
        {
            var lines = new List<AccountLedgerLineDto>
            {
                // Supplier (AP) — Debit on return (reduce payable)
                new() {
                    AccountId         = bp.AccountId,
                    BusinessPartnerId = main.BusinessPartnerId,
                    Debit             = isReversal ? 0 : main.NetTotal,
                    Credit            = isReversal ? main.NetTotal : 0,
                    Remarks           = $"Purchase Return: {main.ReturnNo}"
                },
                // Purchase Account — Credit on return (reverse expense)
                new() {
                    AccountId         = main.PurchaseAccountId,
                    BusinessPartnerId = main.BusinessPartnerId,
                    Debit             = isReversal ? main.SubTotal : 0,
                    Credit            = isReversal ? 0 : main.SubTotal,
                    Remarks           = $"Purchase Return: {main.ReturnNo}"
                }
            };

            if (main.TaxDetails != null && main.TaxDetails.Any())
            {
                void AddTaxLines(
                    Func<PurchaseReturnTaxDetail, int?> getAccountId,
                    Func<PurchaseReturnTaxDetail, decimal> getAmount,
                    string label)
                {
                    var groups = main.TaxDetails
                        .Where(t => getAccountId(t).HasValue
                                 && getAccountId(t)!.Value > 0
                                 && getAmount(t) > 0)
                        .GroupBy(t => getAccountId(t)!.Value);

                    foreach (var g in groups)
                        lines.Add(new AccountLedgerLineDto
                        {
                            AccountId = g.Key,
                            BusinessPartnerId = main.BusinessPartnerId,
                            // Input tax reversed → Credit on return (reduce ITC)
                            Debit = isReversal ? g.Sum(getAmount) : 0,
                            Credit = isReversal ? 0 : g.Sum(getAmount),
                            Remarks = $"{label} Input Return - Purchase Return: {main.ReturnNo}"
                        });
                }

                AddTaxLines(t => t.IGSTPostingAccountId, t => t.IGSTAmount, "IGST");
                AddTaxLines(t => t.CGSTPostingAccountId, t => t.CGSTAmount, "CGST");
                AddTaxLines(t => t.SGSTPostingAccountId, t => t.SGSTAmount, "SGST");
                AddTaxLines(t => t.CessPostingAccountId, t => t.CessAmount, "Cess");
            }

            return lines;
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Mapping
        // ════════════════════════════════════════════════════
        private PurchaseReturnResponseDto MapToResponseDto(PurchaseReturnMain main)
        {
            return new PurchaseReturnResponseDto
            {
                ReturnId = main.ReturnId,
                FinYearId = main.FinYearId,
                ReturnNo = main.ReturnNo,
                ReturnDate = main.ReturnDate,
                OriginalInvoiceId = main.OriginalInvoiceId,
                OriginalInvoiceNo = main.OriginalInvoiceNo,
                OriginalInvoiceDate = main.OriginalInvoiceDate,
                NoteType = main.NoteType,
                BusinessPartnerId = main.BusinessPartnerId,
                BusinessPartnerName = main.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                LocationId = main.LocationId,
                LocationName = main.Location?.LocationName ?? string.Empty,
                PurchaseAccountId = main.PurchaseAccountId,
                PurchaseAccountName = main.PurchaseAccount?.AccountName ?? string.Empty,
                PurchaseStateCode = main.PurchaseStateCode,
                BillStateCode = main.BillStateCode,
                BillAddressId = main.BillAddressId,
                BillAddressLine = FormatAddress(main.BillAddress),
                SubTotal = main.SubTotal,
                TaxAmount = main.TaxAmount,
                CessAmount = main.CessAmount,
                RoundOff = main.RoundOff,
                NetTotal = main.NetTotal,
                Status = main.Status,
                Remarks = main.Remarks,
                CreatedBy = (int)(main.CreatedBy ?? 0),
                CreatedDate = main.CreatedDate,
                ModifiedBy = main.ModifiedBy,
                ModifiedDate = main.ModifiedDate,

                Details = main.Details?.Select(d => new PurchaseReturnDetailResponseDto
                {
                    DetailId = d.DetailId,
                    ReturnId = d.ReturnId,
                    ItemId = d.ItemId,
                    ItemName = d.Item?.ItemName ?? string.Empty,
                    ItemCode = d.Item?.ItemCode,
                    HsnId = d.HsnId,
                    HsnCode = d.HsnCode,
                    PriceType = d.PriceType,
                    Qty = d.Qty,
                    Rate = d.Rate,
                    DiscountRate = d.DiscountRate,
                    AddisDiscountRate = d.AddisDiscountRate,
                    DiscountAmount = d.DiscountAmount,
                    AddisDiscountAmount = d.AddisDiscountAmount,
                    IsTaxIncluded = d.IsTaxIncluded,
                    TaxableAmount = d.TaxableAmount,
                    CessRate = d.CessRate,
                    CessAmount = d.CessAmount,
                    LineTaxAmount = d.LineTaxAmount,
                    LineTotal = d.LineTotal,
                    ItemManageBy = d.Item?.ItemManageBy.ToString(),

                    Batches = d.Batches?.Select(b => new ReturnBatchResponseDto
                    {
                        Id = b.Id,
                        DetailId = b.DetailId,
                        BatchId = b.BatchId,
                        BatchNo = b.Batch?.BatchNo,
                        ExpiryDate = b.Batch?.ExpiryDate,
                        Qty = b.Qty
                    }).ToList(),

                    Serials = d.Serials?.Select(s => new ReturnSerialResponseDto
                    {
                        Id = s.Id,
                        DetailId = s.DetailId,
                        SerialId = s.SerialId,
                        SerialNo = s.Serial?.SerialNo,
                        Status = s.Serial?.Status.ToString()
                    }).ToList(),

                    TaxDetails = d.TaxDetails?.Select(MapTaxDetailDto).ToList()
                }).ToList() ?? new List<PurchaseReturnDetailResponseDto>(),

                TaxDetails = main.Details?
                    .Where(d => d.TaxDetails != null)
                    .SelectMany(d => d.TaxDetails!.Select(MapTaxDetailDto))
                    .ToList() ?? new List<PurchaseReturnTaxDetailResponseDto>()
            };
        }

        private static string? FormatAddress(BusinessPartnerAddress? addr) =>
            addr == null ? null :
            string.Join(", ", new[]
            {
                addr.AddressLine1, addr.AddressLine2, addr.City,
                addr.State.HasValue ? ((GstState)addr.State.Value).ToString() : null,
                addr.Pincode
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

        private static PurchaseReturnTaxDetailResponseDto MapTaxDetailDto(PurchaseReturnTaxDetail td) =>
            new()
            {
                TaxDetailId = td.TaxDetailId,
                DetailId = td.DetailId,
                TaxId = td.TaxId,
                TaxName = td.Tax?.TaxName ?? string.Empty,
                IGSTRate = td.IGSTRate,
                CGSTRate = td.CGSTRate,
                SGSTRate = td.SGSTRate,
                CessRate = td.CessRate,
                TaxableAmount = td.TaxableAmount,
                IGSTAmount = td.IGSTAmount,
                CGSTAmount = td.CGSTAmount,
                SGSTAmount = td.SGSTAmount,
                CessAmount = td.CessAmount,
                TotalTaxAmount = td.TotalTaxAmount
            };

        private async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Database error: {inner}");
            }
        }
    }
}
