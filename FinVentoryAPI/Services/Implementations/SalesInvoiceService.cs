using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesInvoiceDTOs;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class SalesInvoiceService : ISalesInvoiceService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IStockLedgerService _stockLedger;
        private readonly IAccountLedgerPostingService _accountLedger;

        public SalesInvoiceService(
            AppDbContext context, Common common,
            IStockLedgerService stockLedger,
            IAccountLedgerPostingService accountLedger)
        {
            _context = context;
            _common = common;
            _stockLedger = stockLedger;
            _accountLedger = accountLedger;
        }
        // ════════════════════════════════════════════════════
        // CREATE  — replace your existing CreateAsync with this
        // ════════════════════════════════════════════════════
        public async Task<SalesInvoiceResponseDto> CreateAsync(CreateSalesInvoiceMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            // ✅ Validate BEFORE transaction (read-only)
            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, dto.SalesAccountId,
                companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            var invoiceNo = await GenerateInvoiceNoAsync(companyId, finYearId);

            var main = new SalesInvoiceMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                InvoiceNo = invoiceNo,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate,
                BusinessPartnerId = dto.BusinessPartnerId,
                LocationId = dto.LocationId,
                SalesAccountId = dto.SalesAccountId,
                RoundOff = dto.RoundOff,
                Remarks = dto.Remarks,
                Status = "Draft",
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                SalesStateCode = dto.SalesStateCode,
                BillStateCode = dto.BillStateCode,
                ContactPersonId = dto.ContactPersonId,
                SalesPersonId = dto.SalesPersonId,
                BillAddressId = dto.BillAddressId,
                ShipAddressId = dto.ShipAddressId,
                TransportName = dto.TransportName,
                VehicleNo = dto.VehicleNo,
                LrNo = dto.LrNo,
                LrDate = dto.LrDate,
                Details = new List<SalesInvoiceDetail>(),
                TaxDetails = new List<SalesInvoiceTaxDetail>()
            };

            decimal totalSubTotal = 0;
            decimal totalTaxAmount = 0;
            decimal totalCessAmount = 0;

            foreach (var lineDto in dto.Details)
            {
                var detail = await BuildDetailWithTaxAsync(
                    lineDto, userId, dto.SalesStateCode, dto.BillStateCode);

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                foreach (var td in detail.TaxDetails ?? Enumerable.Empty<SalesInvoiceTaxDetail>())
                {
                    td.Invoice = main;
                    td.Detail = detail;
                    main.TaxDetails!.Add(td);
                }

                main.Details!.Add(detail);
            }

            main.SubTotal = totalSubTotal;
            main.TaxAmount = totalTaxAmount;
            main.CessAmount = totalCessAmount;
            main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

            // ── Begin Transaction ─────────────────────────────────────────────────
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.SalesInvoiceMains.Add(main);
                await SaveChangesAsync(); // ← DetailIds populated after this

                // ── Post batch / serial allocations ──────────────────────────────
                // ✅ FIX: Clear the change tracker BEFORE processing serials
                //    so EF re-fetches ItemSerial fresh from DB instead of
                //    returning the already-tracked (Status=Sold) entity.
                _context.ChangeTracker.Clear();

                // Re-attach main so we can still navigate to details
                _context.Attach(main);

                for (int i = 0; i < dto.Details.Count; i++)
                {
                    var lineDto = dto.Details[i];

                    // Re-fetch the saved detail by position so DetailId/InvoiceId are correct
                    var detail = await _context.SalesInvoiceDetails
                        .FirstOrDefaultAsync(d =>
                            d.InvoiceId == main.InvoiceId &&
                            d.ItemId == lineDto.ItemId)
                        ?? throw new Exception($"Detail for item {lineDto.ItemId} not found after save.");

                    var updateDto = new UpdateSalesInvoiceDetailDto
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

                    await SaveBatchAllocationsAsync(detail, updateDto, companyId, isNew: true);
                }

                await SaveChangesAsync(); // batch/serial rows

                // Re-fetch main with all includes for stock/account posting
                var mainForPosting = await _context.SalesInvoiceMains
                    .Include(m => m.Details!)
                        .ThenInclude(d => d.TaxDetails)
                    .Include(m => m.TaxDetails)
                    .Include(m => m.BusinessPartner)
                    .FirstOrDefaultAsync(m => m.InvoiceId == main.InvoiceId)
                    ?? throw new Exception("Invoice not found after save.");

                await PostStockLedgerAsync(mainForPosting, isReversal: false);
                await PostAccountLedgerAsync(mainForPosting, isReversal: false);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(main.InvoiceId)
                ?? throw new Exception("Failed to retrieve saved invoice.");
        }


        // ════════════════════════════════════════════════════
        // FIXED SaveSerialLinesAsync
        // ════════════════════════════════════════════════════
        //private async Task SaveSerialLinesAsync(
        //    SalesInvoiceDetail detail,
        //    UpdateSalesInvoiceDetailDto lineDto,
        //    int companyId)
        //{
        //    if (lineDto.Serials == null || !lineDto.Serials.Any())
        //        throw new Exception(
        //            $"Item {detail.ItemId} is Serial-managed. " +
        //            "Please select at least one serial number.");

        //    if (lineDto.Serials.Count != (int)detail.Qty)
        //        throw new Exception(
        //            $"Serial count ({lineDto.Serials.Count}) must equal line qty ({detail.Qty}).");

        //    // ── Check for duplicate serial IDs in the request ────────────────────
        //    var duplicateSerials = lineDto.Serials
        //        .GroupBy(s => s.SerialId)
        //        .Where(g => g.Count() > 1)
        //        .Select(g => g.Key)
        //        .ToList();

        //    if (duplicateSerials.Any())
        //        throw new Exception(
        //            $"Duplicate serial numbers in request: {string.Join(", ", duplicateSerials)}.");

        //    foreach (var serialDto in lineDto.Serials)
        //    {
        //        // ✅ FIX: Use AsNoTracking so EF always fetches fresh from DB
        //        //    instead of returning the change-tracker cached entity
        //        var serial = await _context.ItemSerials
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(s =>
        //                s.SerialId == serialDto.SerialId &&
        //                s.CompanyId == companyId &&
        //                !s.IsDeleted)
        //            ?? throw new Exception(
        //                $"Serial {serialDto.SerialId} not found in company {companyId}.");

        //        // Verify it belongs to the correct item
        //        if (serial.ItemId != detail.ItemId)
        //            throw new Exception(
        //                $"Serial '{serial.SerialNo}' (ItemId={serial.ItemId}) " +
        //                $"does not belong to item {detail.ItemId}.");

        //        // ✅ FIX: Check DB status directly — not the tracked entity status
        //        if (serial.Status != SerialStatus.InStock)
        //            throw new Exception(
        //                $"Serial '{serial.SerialNo}' is not available (status: {serial.Status}).");

        //        // ✅ FIX: Update status via a direct tracked entity fetch
        //        var trackedSerial = await _context.ItemSerials
        //            .FirstOrDefaultAsync(s => s.SerialId == serialDto.SerialId);

        //        if (trackedSerial != null)
        //            trackedSerial.Status = SerialStatus.Sold;

        //        _context.SalesInvoiceDetailSerials.Add(new SalesInvoiceDetailSerial
        //        {
        //            DetailId = detail.DetailId,
        //            InvoiceId = detail.InvoiceId,
        //            SerialId = serialDto.SerialId
        //        });
        //    }
        //}
        // ════════════════════════════════════════════════════
        // UPDATE
        // ════════════════════════════════════════════════════
        public async Task<bool> UpdateAsync(int id, UpdateSalesInvoiceMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.SalesInvoiceMains
                .Include(m => m.Details!)
                    .ThenInclude(d => d.TaxDetails)
                .Include(m => m.Details!)
                    .ThenInclude(d => d.Batches!)
                        .ThenInclude(b => b.Batch)
                .Include(m => m.Details!)
                    .ThenInclude(d => d.Serials!)
                        .ThenInclude(s => s.Serial)
                .Include(m => m.TaxDetails)
                .Include(m => m.BusinessPartner)
                .FirstOrDefaultAsync(x =>
                    x.InvoiceId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;
            if (main.Status != "Draft")
                throw new Exception("Only Draft invoices can be updated.");

            // ✅ Validate BEFORE transaction (read-only)
            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, dto.SalesAccountId,
                companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            // ── Build fresh detail calculations (read-only, before transaction) ──
            var incomingDetails = new List<SalesInvoiceDetail>();
            foreach (var lineDto in dto.Details)
            {
                var createDto = new CreateSalesInvoiceDetailDto
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
                    createDto, userId, dto.SalesStateCode, dto.BillStateCode));
            }

            // ── Begin Transaction ─────────────────────────────────────────────────
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ── Update header ─────────────────────────────────
                main.InvoiceDate = dto.InvoiceDate;
                main.DueDate = dto.DueDate;
                main.BusinessPartnerId = dto.BusinessPartnerId;
                main.LocationId = dto.LocationId;
                main.SalesAccountId = dto.SalesAccountId;
                main.RoundOff = dto.RoundOff;
                main.Remarks = dto.Remarks;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;
                main.SalesStateCode = dto.SalesStateCode;
                main.BillStateCode = dto.BillStateCode;
                main.ContactPersonId = dto.ContactPersonId;
                main.SalesPersonId = dto.SalesPersonId;
                main.BillAddressId = dto.BillAddressId;
                main.ShipAddressId = dto.ShipAddressId;
                main.TransportName = dto.TransportName;
                main.VehicleNo = dto.VehicleNo;
                main.LrNo = dto.LrNo;
                main.LrDate = dto.LrDate;

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
                                inTax.InvoiceId = main.InvoiceId;
                                inTax.DetailId = existing.DetailId;
                                _context.SalesInvoiceTaxDetails.Add(inTax);
                            }
                        }

                        if (existingTaxList.Count > incomingTaxList.Count)
                            _context.SalesInvoiceTaxDetails.RemoveRange(
                                existingTaxList.Skip(incomingTaxList.Count));

                        await SaveBatchAllocationsAsync(existing, incomingDto, main.CompanyId, isNew: false);

                        totalSubTotal += existing.TaxableAmount;
                        totalCessAmount += existing.CessAmount;
                        totalTaxAmount += existing.LineTaxAmount - existing.CessAmount;
                    }
                    else
                    {
                        // ── Insert new detail row ────────────────────────
                        incoming.InvoiceId = main.InvoiceId;
                        var newTaxList = incoming.TaxDetails ?? new();
                        incoming.TaxDetails = null;
                        incoming.Batches = null;
                        incoming.Serials = null;

                        _context.SalesInvoiceDetails.Add(incoming);
                        await SaveChangesAsync(); // ← flush to get DetailId

                        foreach (var inTax in newTaxList)
                        {
                            inTax.InvoiceId = main.InvoiceId;
                            inTax.DetailId = incoming.DetailId;
                            _context.SalesInvoiceTaxDetails.Add(inTax);
                        }

                        await SaveBatchAllocationsAsync(incoming, incomingDto, main.CompanyId, isNew: true);

                        totalSubTotal += incoming.TaxableAmount;
                        totalCessAmount += incoming.CessAmount;
                        totalTaxAmount += incoming.LineTaxAmount - incoming.CessAmount;
                    }
                }

                // ── Remove surplus detail rows ─────────────────────
                if (existingDetails.Count > incomingDetails.Count)
                {
                    var surplus = existingDetails.Skip(incomingDetails.Count).ToList();
                    foreach (var sd in surplus)
                    {
                        await ReverseBatchAllocationsAsync(sd);
                        if (sd.TaxDetails != null && sd.TaxDetails.Any())
                            _context.SalesInvoiceTaxDetails.RemoveRange(sd.TaxDetails);
                    }
                    _context.SalesInvoiceDetails.RemoveRange(surplus);
                }

                main.SubTotal = totalSubTotal;
                main.TaxAmount = totalTaxAmount;
                main.CessAmount = totalCessAmount;
                main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

                await SaveChangesAsync(); // final flush — header + all detail changes

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

            var main = await _context.SalesInvoiceMains
                .Include(m => m.TaxDetails)
                .Include(m => m.BusinessPartner)
                .Include(m => m.Details!)
                    .ThenInclude(d => d.Batches!)
                        .ThenInclude(b => b.Batch)
                .Include(m => m.Details!)
                    .ThenInclude(d => d.Serials!)
                        .ThenInclude(s => s.Serial)
                .FirstOrDefaultAsync(x =>
                    x.InvoiceId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;
            if (main.Status != "Draft")
                throw new Exception("Only Draft invoices can be deleted.");

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

                await _stockLedger.SoftDeleteByVoucherAsync(
                    companyId, main.InvoiceNo, userId);
                await _accountLedger.SoftDeleteByVoucherAsync(
                    companyId, main.FinYearId, main.InvoiceNo, userId);

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
        public async Task<List<SalesInvoiceResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var invoices = await _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.SalesAccount)
                .Include(x => x.ContactPerson)
                .Include(x => x.SalesPerson)
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.Batches!).ThenInclude(b => b.Batch)
                .Include(x => x.Details!).ThenInclude(d => d.Serials!).ThenInclude(s => s.Serial)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.IGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.CGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.SGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.CessPostingAccount)
                .OrderByDescending(x => x.InvoiceDate)
                .ToListAsync();

            return invoices.Select(MapToResponseDto).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET BY ID
        // ════════════════════════════════════════════════════
        public async Task<SalesInvoiceResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.SalesInvoiceMains
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.SalesAccount)
                .Include(x => x.ContactPerson)
                .Include(x => x.SalesPerson)
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
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
                .Include(x => x.TaxDetails!).ThenInclude(td => td.IGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.CGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.SGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.CessPostingAccount)
                .FirstOrDefaultAsync(x =>
                    x.InvoiceId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return null;
            return MapToResponseDto(main);
        }

        // ════════════════════════════════════════════════════
        // GET PAGED
        // ════════════════════════════════════════════════════
        public async Task<PagedResponseDto<SalesInvoiceResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.SalesAccount)
                .Include(x => x.ContactPerson)
                .Include(x => x.SalesPerson)
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                .Include(x => x.Details!)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.InvoiceNo.ToLower().Contains(search) ||
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
                if (request.Filters.ContainsKey("locationId"))
                {
                    var locationId = ((JsonElement)request.Filters["locationId"]).GetInt32();
                    query = query.Where(x => x.LocationId == locationId);
                }
                if (request.Filters.ContainsKey("finYearId"))
                {
                    var finYearId = ((JsonElement)request.Filters["finYearId"]).GetInt32();
                    query = query.Where(x => x.FinYearId == finYearId);
                }
                if (request.Filters.ContainsKey("fromDate"))
                {
                    var fromDate = ((JsonElement)request.Filters["fromDate"]).GetDateTime();
                    query = query.Where(x => x.InvoiceDate >= fromDate);
                }
                if (request.Filters.ContainsKey("toDate"))
                {
                    var toDate = ((JsonElement)request.Filters["toDate"]).GetDateTime();
                    query = query.Where(x => x.InvoiceDate <= toDate);
                }
                if (request.Filters.ContainsKey("salesPersonId"))
                {
                    var spId = ((JsonElement)request.Filters["salesPersonId"]).GetInt32();
                    query = query.Where(x => x.SalesPersonId == spId);
                }
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "invoiceno" => sort.Direction == "desc" ? query.OrderByDescending(x => x.InvoiceNo) : query.OrderBy(x => x.InvoiceNo),
                    "invoicedate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.InvoiceDate) : query.OrderBy(x => x.InvoiceDate),
                    "businesspartnername" => sort.Direction == "desc" ? query.OrderByDescending(x => x.BusinessPartner!.BusinessPartnerName) : query.OrderBy(x => x.BusinessPartner!.BusinessPartnerName),
                    "salesperson" => sort.Direction == "desc" ? query.OrderByDescending(x => x.SalesPerson!.SalesPersonName) : query.OrderBy(x => x.SalesPerson!.SalesPersonName),
                    "status" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "amount" => sort.Direction == "desc" ? query.OrderByDescending(x => x.SubTotal) : query.OrderBy(x => x.SubTotal),
                    "taxamount" => sort.Direction == "desc" ? query.OrderByDescending(x => x.TaxAmount) : query.OrderBy(x => x.TaxAmount),
                    "discount" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Details!.Sum(d => d.DiscountAmount + d.AddisDiscountAmount)) : query.OrderBy(x => x.Details!.Sum(d => d.DiscountAmount + d.AddisDiscountAmount)),
                    "nettotal" or "netamount" => sort.Direction == "desc" ? query.OrderByDescending(x => x.NetTotal) : query.OrderBy(x => x.NetTotal),
                    _ => query.OrderByDescending(x => x.InvoiceDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.InvoiceDate);
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
                .Include(x => x.TaxDetails!).ThenInclude(td => td.IGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.CGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.SGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.CessPostingAccount)
                .ToListAsync();

            return new PagedResponseDto<SalesInvoiceResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapToResponseDto).ToList()
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Batch / Serial
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Validates and persists batch or serial allocations for one detail line.
        /// Also updates ItemBatch.AvailableQty / ItemBatch.UsedQty
        ///  and ItemSerial.Status to Sold.
        /// </summary>
        private async Task SaveBatchAllocationsAsync(
     SalesInvoiceDetail detail,
     UpdateSalesInvoiceDetailDto lineDto,       // ← fixed
     int companyId,
     bool isNew)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemId == detail.ItemId)
                ?? throw new Exception($"Item {detail.ItemId} not found.");

            switch (item.ItemManageBy)
            {
                case ItemManageBy.Batch:
                    await SaveBatchLinesAsync(detail, lineDto, companyId);
                    break;
                case ItemManageBy.Serial:
                    await SaveSerialLinesAsync(detail, lineDto, companyId);
                    break;
                    // ItemManageBy.Regular → nothing to do
            }
        }

        private async Task SaveBatchLinesAsync(
    SalesInvoiceDetail detail,
    UpdateSalesInvoiceDetailDto lineDto,       // ← fixed
    int companyId)
        {
            if (lineDto.Batches == null || !lineDto.Batches.Any())
                throw new Exception(
                    $"Item {detail.ItemId} is Batch-managed. " +
                    "Please select at least one batch.");

            decimal totalAllocated = lineDto.Batches.Sum(b => b.Qty);
            if (totalAllocated != detail.Qty)
                throw new Exception(
                    $"Batch qty total ({totalAllocated}) must equal line qty ({detail.Qty}).");

            foreach (var batchDto in lineDto.Batches)
            {
                var batch = await _context.ItemBatches
                    .FirstOrDefaultAsync(b =>
                        b.BatchId == batchDto.BatchId &&
                        b.CompanyId == companyId &&
                        b.ItemId == detail.ItemId &&
                        !b.IsDeleted)
                    ?? throw new Exception($"Batch {batchDto.BatchId} not found.");

                if (batch.AvailableQty < batchDto.Qty)
                    throw new Exception(
                        $"Batch '{batch.BatchNo}' has only {batch.AvailableQty} units available " +
                        $"but {batchDto.Qty} requested.");

                batch.UsedQty += batchDto.Qty;
                batch.AvailableQty -= batchDto.Qty;

                _context.SalesInvoiceDetailBatches.Add(new SalesInvoiceDetailBatch
                {
                    DetailId = detail.DetailId,
                    InvoiceId = detail.InvoiceId,
                    BatchId = batchDto.BatchId,
                    Qty = batchDto.Qty
                });
            }
        }

        private async Task SaveSerialLinesAsync(
    SalesInvoiceDetail detail,
    UpdateSalesInvoiceDetailDto lineDto,
    int companyId)
        {
            if (lineDto.Serials == null || !lineDto.Serials.Any())
                throw new Exception(
                    $"Item {detail.ItemId} is Serial-managed. " +
                    "Please select at least one serial number.");

            if (lineDto.Serials.Count != (int)detail.Qty)
                throw new Exception(
                    $"Serial count ({lineDto.Serials.Count}) must equal line qty ({detail.Qty}).");

            foreach (var serialDto in lineDto.Serials)
            {
                // Always read fresh from DB — never use tracked cache
                var serial = await _context.ItemSerials
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.SerialId == serialDto.SerialId &&
                        s.CompanyId == companyId &&
                        s.ItemId == detail.ItemId &&
                        !s.IsDeleted)
                    ?? throw new Exception($"Serial {serialDto.SerialId} not found.");

                if (serial.Status != SerialStatus.InStock)
                    throw new Exception(
                        $"Serial '{serial.SerialNo}' is not available (status: {serial.Status}).");

                // Fetch tracked entity separately just for status update
                var trackedSerial = await _context.ItemSerials
                    .FirstOrDefaultAsync(s => s.SerialId == serialDto.SerialId);

                if (trackedSerial != null)
                    trackedSerial.Status = SerialStatus.Sold;

                _context.SalesInvoiceDetailSerials.Add(new SalesInvoiceDetailSerial
                {
                    DetailId = detail.DetailId,
                    InvoiceId = detail.InvoiceId,
                    SerialId = serialDto.SerialId
                });
            }
        }
        //    private async Task SaveSerialLinesAsync(
        //SalesInvoiceDetail detail,
        //UpdateSalesInvoiceDetailDto lineDto,       // ← fixed
        //int companyId)
        //    {          


        //        if (lineDto.Serials == null || !lineDto.Serials.Any())
        //            throw new Exception(
        //                $"Item {detail.ItemId} is Serial-managed. " +
        //                "Please select at least one serial number.");

        //        if (lineDto.Serials.Count != (int)detail.Qty)
        //            throw new Exception(
        //                $"Serial count ({lineDto.Serials.Count}) must equal line qty ({detail.Qty}).");

        //        foreach (var serialDto in lineDto.Serials)
        //        {
        //            var serial = await _context.ItemSerials
        //                .FirstOrDefaultAsync(s =>
        //                    s.SerialId == serialDto.SerialId &&
        //                    s.CompanyId == companyId &&
        //                    s.ItemId == detail.ItemId &&
        //                    !s.IsDeleted)
        //                ?? throw new Exception($"Serial {serialDto.SerialId} not found.");

        //            if (serial.Status != SerialStatus.InStock)
        //                throw new Exception(
        //                    $"Serial '{serial.SerialNo}' is not available (status: {serial.Status}).");

        //            serial.Status = SerialStatus.Sold;

        //            _context.SalesInvoiceDetailSerials.Add(new SalesInvoiceDetailSerial
        //            {
        //                DetailId = detail.DetailId,
        //                InvoiceId = detail.InvoiceId,
        //                SerialId = serialDto.SerialId
        //            });
        //        }
        //    }

        /// <summary>
        /// When a detail line is updated or deleted, restore the previously
        /// allocated batch quantities and reset serial statuses.
        /// </summary>
        private async Task ReverseBatchAllocationsAsync(SalesInvoiceDetail detail)
        {
            // ── Reverse Batches ───────────────────────────────
            if (detail.Batches != null && detail.Batches.Any())
            {
                foreach (var alloc in detail.Batches)
                {
                    var batch = alloc.Batch
                        ?? await _context.ItemBatches.FindAsync(alloc.BatchId);

                    if (batch != null)
                    {
                        batch.UsedQty -= alloc.Qty;
                        batch.AvailableQty += alloc.Qty;
                    }
                }
                _context.SalesInvoiceDetailBatches.RemoveRange(detail.Batches);
            }

            // ── Reverse Serials ───────────────────────────────
            if (detail.Serials != null && detail.Serials.Any())
            {
                foreach (var alloc in detail.Serials)
                {
                    var serial = alloc.Serial
                        ?? await _context.ItemSerials.FindAsync(alloc.SerialId);

                    if (serial != null)
                        serial.Status = SerialStatus.InStock;
                }
                _context.SalesInvoiceDetailSerials.RemoveRange(detail.Serials);
            }
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — existing (unchanged)
        // ════════════════════════════════════════════════════

        private async Task ValidateHeaderAsync(
            int businessPartnerId, int locationId, int salesAccountId,
            int companyId,
            int? salesStateCode, int? billStateCode,
            int? contactPersonId, int? salesPersonId,
            int? billAddressId, int? shipAddressId)
        {
            var bpExists = await _context.BusinessPartners
                .AnyAsync(x =>
                    x.BusinessPartnerId == businessPartnerId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!bpExists) throw new Exception("Business Partner not found.");

            var locationExists = await _context.Locations
                .AnyAsync(x => x.LocationId == locationId && x.CompanyId == companyId);
            if (!locationExists) throw new Exception("Location not found.");

            var salesAccountExists = await _context.Accounts
                .AnyAsync(x =>
                    x.AccountId == salesAccountId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!salesAccountExists) throw new Exception("Sales Account not found.");

            await ValidateNewFieldsAsync(
                businessPartnerId, companyId,
                salesStateCode, billStateCode,
                contactPersonId, salesPersonId,
                billAddressId, shipAddressId);
        }

        private async Task ValidateNewFieldsAsync(
            int businessPartnerId, int companyId,
            int? salesStateCode, int? billStateCode,
            int? contactPersonId, int? salesPersonId,
            int? billAddressId, int? shipAddressId)
        {
            if (salesStateCode.HasValue && !Enum.IsDefined(typeof(GstState), salesStateCode.Value))
                throw new Exception("Invalid Sales State Code.");

            if (billStateCode.HasValue && !Enum.IsDefined(typeof(GstState), billStateCode.Value))
                throw new Exception("Invalid Bill State Code.");

            if (contactPersonId.HasValue)
            {
                var cpExists = await _context.BusinessPartnerContacts
                    .AnyAsync(x =>
                        x.BPContactId == contactPersonId &&
                        x.BusinessPartnerId == businessPartnerId);
                if (!cpExists)
                    throw new Exception("Contact Person not found for this Business Partner.");
            }

            if (salesPersonId.HasValue)
            {
                var spExists = await _context.SalesPersons
                    .AnyAsync(x =>
                        x.SalesPersonId == salesPersonId &&
                        x.CompanyId == companyId &&
                        !x.IsDeleted);
                if (!spExists) throw new Exception("Sales Person not found.");
            }

            if (billAddressId.HasValue)
            {
                var billExists = await _context.BusinessPartnerAddresses
                    .AnyAsync(x =>
                        x.BPAddressId == billAddressId &&
                        x.BusinessPartnerId == businessPartnerId);
                if (!billExists)
                    throw new Exception("Bill Address not found for this Business Partner.");
            }

            if (shipAddressId.HasValue)
            {
                var shipExists = await _context.BusinessPartnerAddresses
                    .AnyAsync(x =>
                        x.BPAddressId == shipAddressId &&
                        x.BusinessPartnerId == businessPartnerId);
                if (!shipExists)
                    throw new Exception("Ship Address not found for this Business Partner.");
            }
        }

        private async Task<SalesInvoiceDetail> BuildDetailWithTaxAsync(
            CreateSalesInvoiceDetailDto lineDto, int userId,
            int? salesStateCode, int? billStateCode)
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
            decimal discountAmount = Math.Round(grossAmount * lineDto.DiscountRate / 100, 2);
            decimal afterFirst = grossAmount - discountAmount;
            decimal addisDiscAmt = Math.Round(afterFirst * lineDto.AddisDiscountRate / 100, 2);
            decimal taxableAmount = afterFirst - addisDiscAmt;

            bool isIntraState = (salesStateCode.HasValue && billStateCode.HasValue)
                ? salesStateCode.Value == billStateCode.Value : true;

            if (lineDto.IsTaxIncluded)
            {
                decimal totalTaxRate = isIntraState
                    ? (tax.CGST + tax.SGST) : tax.IGST;
                if (totalTaxRate > 0)
                    taxableAmount = Math.Round(taxableAmount / (1 + totalTaxRate / 100), 2);
            }

            decimal igstAmount = (!isIntraState) ? Math.Round(taxableAmount * tax.IGST / 100, 2) : 0;
            decimal cgstAmount = (isIntraState) ? Math.Round(taxableAmount * tax.CGST / 100, 2) : 0;
            decimal sgstAmount = (isIntraState) ? Math.Round(taxableAmount * tax.SGST / 100, 2) : 0;
            decimal cessRate = hsn.Cess ?? 0;
            decimal cessAmount = Math.Round(taxableAmount * cessRate / 100, 2);
            decimal lineTaxAmt = igstAmount + cgstAmount + sgstAmount + cessAmount;

            return new SalesInvoiceDetail
            {
                ItemId = lineDto.ItemId,
                HsnId = hsn.HsnId,
                HsnCode = hsn.HsnName,
                PriceType = lineDto.PriceType,
                Qty = lineDto.Qty,
                Rate = lineDto.Rate,
                DiscountRate = lineDto.DiscountRate,
                AddisDiscountRate = lineDto.AddisDiscountRate,
                DiscountAmount = discountAmount,
                AddisDiscountAmount = addisDiscAmt,
                IsTaxIncluded = lineDto.IsTaxIncluded,
                TaxableAmount = taxableAmount,
                CessRate = cessRate,
                CessAmount = cessAmount,
                LineTaxAmount = lineTaxAmt,
                LineTotal = taxableAmount + lineTaxAmt,
                TaxDetails = new List<SalesInvoiceTaxDetail>
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

        private async Task<string> GenerateInvoiceNoAsync(int companyId, int finYearId)
        {
            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(x => x.FinancialYearId == finYearId);

            var yearLabel = financialYear != null
                ? $"{financialYear.StartDate.Year % 100}{financialYear.EndDate.Year % 100}"
                : finYearId.ToString();

            var count = await _context.SalesInvoiceMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"INV-{yearLabel}-{(count + 1):D4}";
        }

        private SalesInvoiceResponseDto MapToResponseDto(SalesInvoiceMain main)
        {
            return new SalesInvoiceResponseDto
            {
                InvoiceId = main.InvoiceId,
                FinYearId = main.FinYearId,
                InvoiceNo = main.InvoiceNo,
                InvoiceDate = main.InvoiceDate,
                DueDate = main.DueDate,
                Status = main.Status,

                BusinessPartnerId = main.BusinessPartnerId,
                BusinessPartnerName = main.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                BusinessPartnerCode = main.BusinessPartner?.BusinessPartnerCode ?? string.Empty,

                LocationId = main.LocationId,
                LocationName = main.Location?.LocationName ?? string.Empty,

                SalesAccountId = main.SalesAccountId,
                SalesAccountName = main.SalesAccount?.AccountName ?? string.Empty,
                ReceivableAccountId = main.BusinessPartner?.AccountId ?? 0,

                SalesStateCode = main.SalesStateCode,
                SalesStateName = main.SalesStateCode.HasValue
                    ? ((GstState)main.SalesStateCode.Value).ToString() : null,

                BillStateCode = main.BillStateCode,
                BillStateName = main.BillStateCode.HasValue
                    ? ((GstState)main.BillStateCode.Value).ToString() : null,

                ContactPersonId = main.ContactPersonId,
                ContactPersonName = main.ContactPerson?.Name,
                ContactPersonMobile = main.ContactPerson?.Mobile,

                SalesPersonId = main.SalesPersonId,
                SalesPersonName = main.SalesPerson?.SalesPersonName,

                BillAddressId = main.BillAddressId,
                BillAddressLine = FormatAddress(main.BillAddress),

                ShipAddressId = main.ShipAddressId,
                ShipAddressLine = FormatAddress(main.ShipAddress),

                SubTotal = main.SubTotal,
                TaxAmount = main.TaxAmount,
                CessAmount = main.CessAmount,
                RoundOff = main.RoundOff,
                NetTotal = main.NetTotal,

                Amount = main.SubTotal,
                Discount = main.Details?.Sum(d => d.DiscountAmount + d.AddisDiscountAmount) ?? 0,
                NetAmount = main.NetTotal,
                Remarks = main.Remarks,

                CreatedBy = (int)(main.CreatedBy ?? 0),
                CreatedDate = main.CreatedDate,
                ModifiedBy = main.ModifiedBy,
                ModifiedDate = main.ModifiedDate,

                TransportName = main.TransportName,
                VehicleNo = main.VehicleNo,
                LrNo = main.LrNo,
                LrDate = main.LrDate,

                Details = main.Details?.Select(d => new SalesInvoiceDetailResponseDto
                {
                    DetailId = d.DetailId,
                    InvoiceId = d.InvoiceId,
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

                    // ── Batch / Serial allocations ───────────────
                    Batches = d.Batches?.Select(b => new SalesInvoiceDetailBatchResponseDto
                    {
                        Id = b.Id,
                        DetailId = b.DetailId,
                        BatchId = b.BatchId,
                        BatchNo = b.Batch?.BatchNo,
                        ExpiryDate = b.Batch?.ExpiryDate,
                        Qty = b.Qty
                    }).ToList(),

                    Serials = d.Serials?.Select(s => new SalesInvoiceDetailSerialResponseDto
                    {
                        Id = s.Id,
                        DetailId = s.DetailId,
                        SerialId = s.SerialId,
                        SerialNo = s.Serial?.SerialNo,
                        Status = s.Serial?.Status.ToString()
                    }).ToList(),

                    TaxDetails = MapTaxDetails(d.TaxDetails)
                }).ToList() ?? new List<SalesInvoiceDetailResponseDto>(),

                TaxDetails = main.Details?
                    .Where(d => d.TaxDetails != null)
                    .SelectMany(d => d.TaxDetails!.Select(MapTaxDetailDto))
                    .ToList() ?? new List<SalesInvoiceTaxDetailResponseDto>()
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

        private static List<SalesInvoiceTaxDetailResponseDto> MapTaxDetails(
            IEnumerable<SalesInvoiceTaxDetail>? rows) =>
            rows?.Select(MapTaxDetailDto).ToList()
            ?? new List<SalesInvoiceTaxDetailResponseDto>();

        private static SalesInvoiceTaxDetailResponseDto MapTaxDetailDto(SalesInvoiceTaxDetail td) =>
            new()
            {
                TaxDetailId = td.TaxDetailId,
                DetailId = td.DetailId,
                TaxId = td.TaxId,
                TaxName = td.Tax?.TaxName ?? string.Empty,
                TaxType = td.Tax?.TaxType,
                IGSTRate = td.IGSTRate,
                CGSTRate = td.CGSTRate,
                SGSTRate = td.SGSTRate,
                CessRate = td.CessRate,
                TaxableAmount = td.TaxableAmount,
                IGSTAmount = td.IGSTAmount,
                CGSTAmount = td.CGSTAmount,
                SGSTAmount = td.SGSTAmount,
                CessAmount = td.CessAmount,
                TotalTaxAmount = td.TotalTaxAmount,
                IGSTPostingAccount = td.IGSTPostingAccount?.AccountName,
                CGSTPostingAccount = td.CGSTPostingAccount?.AccountName,
                SGSTPostingAccount = td.SGSTPostingAccount?.AccountName,
                CessPostingAccount = td.CessPostingAccount?.AccountName
            };

        private async Task PostStockLedgerAsync(SalesInvoiceMain main, bool isReversal)
        {
            if (main.Details == null || !main.Details.Any()) return;
            var lines = main.Details.Select(d => new StockLedgerLineDto
            {
                ItemId = d.ItemId,
                Qty = isReversal ? d.Qty : -d.Qty,
                Rate = d.Rate,
                Remarks = $"Sales Invoice: {main.InvoiceNo}"
            }).ToList();
            await _stockLedger.AddEntriesAsync(
                companyId: main.CompanyId, warehouseId: null,
                date: main.InvoiceDate, voucherType: "Sales Invoice",
                voucherNo: main.InvoiceNo, businessPartnerId: main.BusinessPartnerId,
                lines: lines, createdBy: (int?)main.CreatedBy);
        }

        private async Task PostAccountLedgerAsync(SalesInvoiceMain main, bool isReversal)
        {
            var bp = main.BusinessPartner
                ?? await _context.BusinessPartners
                    .FirstOrDefaultAsync(x => x.BusinessPartnerId == main.BusinessPartnerId);
            if (bp == null) return;
            var lines = BuildAccountLedgerLines(main, bp, isReversal);
            await _accountLedger.AddEntriesAsync(
                companyId: main.CompanyId, financialYearId: main.FinYearId,
                date: main.InvoiceDate, voucherType: "Sales Invoice",
                voucherNo: main.InvoiceNo, lines: lines,
                createdBy: (int?)main.CreatedBy);
        }

        private async Task UpdateStockLedgerAsync(SalesInvoiceMain main)
        {
            if (main.Details == null || !main.Details.Any()) return;
            var lines = main.Details.Select(d => new StockLedgerLineDto
            {
                ItemId = d.ItemId,
                Qty = -d.Qty,
                Rate = d.Rate,
                Remarks = $"Sales Invoice: {main.InvoiceNo}"
            }).ToList();
            await _stockLedger.UpdateEntriesAsync(
                companyId: main.CompanyId, warehouseId: null,
                date: main.InvoiceDate, voucherType: "Sales Invoice",
                voucherNo: main.InvoiceNo, businessPartnerId: main.BusinessPartnerId,
                lines: lines, modifiedBy: (int?)main.ModifiedBy);
        }

        private async Task UpdateAccountLedgerAsync(SalesInvoiceMain main)
        {
            var bp = main.BusinessPartner
                ?? await _context.BusinessPartners
                    .FirstOrDefaultAsync(x => x.BusinessPartnerId == main.BusinessPartnerId);
            if (bp == null) return;
            var lines = BuildAccountLedgerLines(main, bp, isReversal: false);
            await _accountLedger.UpdateEntriesAsync(
                companyId: main.CompanyId, financialYearId: main.FinYearId,
                date: main.InvoiceDate, voucherType: "Sales Invoice",
                voucherNo: main.InvoiceNo, lines: lines,
                modifiedBy: (int?)main.ModifiedBy);
        }

        private List<AccountLedgerLineDto> BuildAccountLedgerLines(
            SalesInvoiceMain main, BusinessPartner bp, bool isReversal)
        {
            var lines = new List<AccountLedgerLineDto>
            {
                new() {
                    AccountId         = bp.AccountId,
                    BusinessPartnerId = main.BusinessPartnerId,
                    Debit             = isReversal ? 0 : main.NetTotal,
                    Credit            = isReversal ? main.NetTotal : 0,
                    Remarks           = $"Sales Invoice: {main.InvoiceNo}"
                },
                new() {
                    AccountId         = main.SalesAccountId,
                    BusinessPartnerId = main.BusinessPartnerId,
                    Debit             = isReversal ? main.SubTotal : 0,
                    Credit            = isReversal ? 0 : main.SubTotal,
                    Remarks           = $"Sales Invoice: {main.InvoiceNo}"
                }
            };

            if (main.TaxDetails != null && main.TaxDetails.Any())
            {
                void AddTaxLines(
                    Func<SalesInvoiceTaxDetail, int?> getAccountId,
                    Func<SalesInvoiceTaxDetail, decimal> getAmount,
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
                            Debit = isReversal ? g.Sum(getAmount) : 0,
                            Credit = isReversal ? 0 : g.Sum(getAmount),
                            Remarks = $"{label} - Sales Invoice: {main.InvoiceNo}"
                        });
                }

                AddTaxLines(t => t.IGSTPostingAccountId, t => t.IGSTAmount, "IGST");
                AddTaxLines(t => t.CGSTPostingAccountId, t => t.CGSTAmount, "CGST");
                AddTaxLines(t => t.SGSTPostingAccountId, t => t.SGSTAmount, "SGST");
                AddTaxLines(t => t.CessPostingAccountId, t => t.CessAmount, "Cess");
            }

            return lines;
        }

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