using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesQuotationDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class SalesQuotationService : ISalesQuotationService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IAuditLogService _auditLog;

        public SalesQuotationService(AppDbContext context, Common common, IAuditLogService auditLog)
        {
            _context = context;
            _common = common;
            _auditLog = auditLog;
        }

        // ════════════════════════════════════════════════════
        // CREATE
        // ════════════════════════════════════════════════════
        public async Task<SalesQuotationResponseDto> CreateAsync(CreateSalesQuotationMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            var quotationNo = await GenerateQuotationNoAsync(companyId, finYearId);

            var main = new SalesQuotationMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                QuotationNo = quotationNo,
                QuotationDate = dto.QuotationDate,
                ValidUntilDate = dto.ValidUntilDate,
                BusinessPartnerId = dto.BusinessPartnerId,
                LocationId = dto.LocationId,
                RoundOff = dto.RoundOff,
                Remarks = dto.Remarks,
                Status = "Draft",
                SalesStateCode = dto.SalesStateCode,
                BillStateCode = dto.BillStateCode,
                ContactPersonId = dto.ContactPersonId,
                SalesPersonId = dto.SalesPersonId,
                BillAddressId = dto.BillAddressId,
                ShipAddressId = dto.ShipAddressId,
                ParentQuotationId = null,
                RevisionNo = 0,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                Details = new List<SalesQuotationDetail>(),
                TaxDetails = new List<SalesQuotationTaxDetail>()
            };

            decimal totalSubTotal = 0;
            decimal totalTaxAmount = 0;
            decimal totalCessAmount = 0;

            foreach (var lineDto in dto.Details)
            {
                var detail = await BuildDetailWithTaxAsync(
                    lineDto, dto.SalesStateCode, dto.BillStateCode);

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                foreach (var td in detail.TaxDetails ?? Enumerable.Empty<SalesQuotationTaxDetail>())
                {
                    td.Quotation = main;
                    td.Detail = detail;
                    main.TaxDetails!.Add(td);
                }

                main.Details!.Add(detail);
            }

            main.SubTotal = totalSubTotal;
            main.TaxAmount = totalTaxAmount;
            main.CessAmount = totalCessAmount;
            main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.SalesQuotationMains.Add(main);
                await SaveChangesAsync();
                await transaction.CommitAsync();
                await _auditLog.LogAsync(
                      module: "SalesQuotation",
                      action: "Create",
                      entityId: main.QuotationId,
                      entityNo: main.QuotationNo,
                      newValues: new { main.QuotationNo, main.BusinessPartnerId, main.NetTotal, main.Status });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(main.QuotationId)
                ?? throw new Exception("Failed to retrieve saved quotation.");
        }

        // ════════════════════════════════════════════════════
        // UPDATE
        // ════════════════════════════════════════════════════
        public async Task<bool> UpdateAsync(int id, UpdateSalesQuotationMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.SalesQuotationMains
                .Include(m => m.Details!).ThenInclude(d => d.TaxDetails)
                .Include(m => m.TaxDetails)
                .Include(m => m.BusinessPartner)
                .FirstOrDefaultAsync(x =>
                    x.QuotationId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;
            if (main.Status != "Draft")
                throw new Exception("Only Draft quotations can be updated.");

            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            var incomingDetails = new List<SalesQuotationDetail>();
            foreach (var lineDto in dto.Details)
            {
                var createDto = new CreateSalesQuotationDetailDto
                {
                    ItemId = lineDto.ItemId,
                    PriceType = lineDto.PriceType,
                    Qty = lineDto.Qty,
                    Rate = lineDto.Rate,
                    DiscountRate = lineDto.DiscountRate,
                    AddisDiscountRate = lineDto.AddisDiscountRate,
                    IsTaxIncluded = lineDto.IsTaxIncluded,
                    ManualTaxId = lineDto.ManualTaxId,
                    ManualIgstRate = lineDto.ManualIgstRate,
                    ManualCgstRate = lineDto.ManualCgstRate,
                    ManualSgstRate = lineDto.ManualSgstRate,
                    ManualCessRate = lineDto.ManualCessRate
                };
                incomingDetails.Add(await BuildDetailWithTaxAsync(
                    createDto, dto.SalesStateCode, dto.BillStateCode));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                main.QuotationDate = dto.QuotationDate;
                main.ValidUntilDate = dto.ValidUntilDate;
                main.BusinessPartnerId = dto.BusinessPartnerId;
                main.LocationId = dto.LocationId;
                main.RoundOff = dto.RoundOff;
                main.Remarks = dto.Remarks;
                main.SalesStateCode = dto.SalesStateCode;
                main.BillStateCode = dto.BillStateCode;
                main.ContactPersonId = dto.ContactPersonId;
                main.SalesPersonId = dto.SalesPersonId;
                main.BillAddressId = dto.BillAddressId;
                main.ShipAddressId = dto.ShipAddressId;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

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

                        // Wipe existing tax rows and re-insert fresh
                        if (existing.TaxDetails != null && existing.TaxDetails.Any())
                        {
                            _context.SalesQuotationTaxDetails.RemoveRange(existing.TaxDetails);
                            await SaveChangesAsync(); // flush deletes before re-insert
                        }

                        foreach (var inTax in incoming.TaxDetails ?? new())
                        {
                            _context.SalesQuotationTaxDetails.Add(new SalesQuotationTaxDetail
                            {
                                QuotationId = main.QuotationId,
                                DetailId = existing.DetailId,
                                TaxId = inTax.TaxId,
                                IGSTRate = inTax.IGSTRate,
                                CGSTRate = inTax.CGSTRate,
                                SGSTRate = inTax.SGSTRate,
                                TaxableAmount = inTax.TaxableAmount,
                                IGSTAmount = inTax.IGSTAmount,
                                CGSTAmount = inTax.CGSTAmount,
                                SGSTAmount = inTax.SGSTAmount,
                                CessRate = inTax.CessRate,
                                CessAmount = inTax.CessAmount,
                                TotalTaxAmount = inTax.TotalTaxAmount
                            });
                        }


                        totalSubTotal += existing.TaxableAmount;
                        totalCessAmount += existing.CessAmount;
                        totalTaxAmount += existing.LineTaxAmount - existing.CessAmount;
                    }
                    else
                    {
                        incoming.QuotationId = main.QuotationId;
                        var newTaxList = incoming.TaxDetails ?? new();
                        incoming.TaxDetails = null;

                        _context.SalesQuotationDetails.Add(incoming);
                        await SaveChangesAsync();

                        foreach (var inTax in newTaxList)
                        {
                            inTax.QuotationId = main.QuotationId;
                            inTax.DetailId = incoming.DetailId;
                            _context.SalesQuotationTaxDetails.Add(inTax);
                        }

                        totalSubTotal += incoming.TaxableAmount;
                        totalCessAmount += incoming.CessAmount;
                        totalTaxAmount += incoming.LineTaxAmount - incoming.CessAmount;
                    }
                }

                if (existingDetails.Count > incomingDetails.Count)
                {
                    var surplus = existingDetails.Skip(incomingDetails.Count).ToList();
                    foreach (var sd in surplus)
                        if (sd.TaxDetails != null && sd.TaxDetails.Any())
                            _context.SalesQuotationTaxDetails.RemoveRange(sd.TaxDetails);

                    _context.SalesQuotationDetails.RemoveRange(surplus);
                }

                main.SubTotal = totalSubTotal;
                main.TaxAmount = totalTaxAmount;
                main.CessAmount = totalCessAmount;
                main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

                await SaveChangesAsync();
                await transaction.CommitAsync();
                await _auditLog.LogAsync(
                     module: "SalesQuotation",
                     action: "Update",
                     entityId: main.QuotationId,
                     entityNo: main.QuotationNo,
                     newValues: new { main.QuotationNo, main.BusinessPartnerId, main.NetTotal, main.Status });
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

            var main = await _context.SalesQuotationMains
                .Include(m => m.Details!).ThenInclude(d => d.TaxDetails)
                .Include(m => m.TaxDetails)
                .FirstOrDefaultAsync(x =>
                    x.QuotationId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;
            if (main.Status != "Draft")
                throw new Exception("Only Draft quotations can be deleted.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                main.IsDeleted = true;
                main.IsActive = false;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

                await SaveChangesAsync();
                await transaction.CommitAsync();
                await _auditLog.LogAsync(
                     module: "SalesQuotation",
                     action: "Delete",
                     entityId: main.QuotationId,
                     entityNo: main.QuotationNo,
                     remarks: "Soft deleted");
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ════════════════════════════════════════════════════
        // COPY
        // ════════════════════════════════════════════════════
        public async Task<SalesQuotationResponseDto> CopyAsync(int id, CopySalesQuotationDto? dto = null)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            // Load original with full details
            var original = await _context.SalesQuotationMains
                .Include(m => m.Details!).ThenInclude(d => d.TaxDetails)
                .FirstOrDefaultAsync(x =>
                    x.QuotationId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Quotation not found.");

            // Resolve overrides
            var targetBusinessPartnerId = dto?.BusinessPartnerId ?? original.BusinessPartnerId;
            var targetLocationId = dto?.LocationId ?? original.LocationId;

            // Detect if customer changed — reset partner-specific fields if so
            bool customerChanged = dto?.BusinessPartnerId.HasValue == true
                                && dto.BusinessPartnerId.Value != original.BusinessPartnerId;

            // Validate only if overrides provided
            if (dto != null && (dto.BusinessPartnerId.HasValue || dto.LocationId.HasValue))
            {
                await ValidateHeaderAsync(
                    targetBusinessPartnerId, targetLocationId, companyId,
                    original.SalesStateCode, original.BillStateCode,
                    customerChanged ? null : original.ContactPersonId,
                    original.SalesPersonId,
                    customerChanged ? null : original.BillAddressId,
                    customerChanged ? null : original.ShipAddressId);
            }

            // Fresh quotation number — no revision suffix
            var newQuotationNo = await GenerateQuotationNoAsync(companyId, finYearId);

            var copied = new SalesQuotationMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                QuotationNo = newQuotationNo,
                QuotationDate = dto?.QuotationDate ?? DateTime.UtcNow,
                ValidUntilDate = dto?.ValidUntilDate ?? original.ValidUntilDate,
                BusinessPartnerId = targetBusinessPartnerId,
                LocationId = targetLocationId,
                RoundOff = original.RoundOff,
                Remarks = dto?.Remarks ?? original.Remarks,
                Status = "Draft",
                SalesStateCode = original.SalesStateCode,
                BillStateCode = original.BillStateCode,
                SalesPersonId = original.SalesPersonId,

                // Reset partner-specific fields if customer changed
                ContactPersonId = customerChanged ? null : original.ContactPersonId,
                BillAddressId = customerChanged ? null : original.BillAddressId,
                ShipAddressId = customerChanged ? null : original.ShipAddressId,

                SubTotal = original.SubTotal,
                TaxAmount = original.TaxAmount,
                CessAmount = original.CessAmount,
                NetTotal = original.NetTotal,

                // Key difference from Revise: no parent link
                ParentQuotationId = null,
                RevisionNo = 0,

                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                Details = new List<SalesQuotationDetail>(),
                TaxDetails = new List<SalesQuotationTaxDetail>()
            };

            // Clone detail lines
            foreach (var d in original.Details ?? Enumerable.Empty<SalesQuotationDetail>())
            {
                var newDetail = new SalesQuotationDetail
                {
                    ItemId = d.ItemId,
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
                    TaxDetails = new List<SalesQuotationTaxDetail>()
                };

                foreach (var td in d.TaxDetails ?? Enumerable.Empty<SalesQuotationTaxDetail>())
                {
                    newDetail.TaxDetails!.Add(new SalesQuotationTaxDetail
                    {
                        TaxId = td.TaxId,
                        IGSTRate = td.IGSTRate,
                        CGSTRate = td.CGSTRate,
                        SGSTRate = td.SGSTRate,
                        TaxableAmount = td.TaxableAmount,
                        IGSTAmount = td.IGSTAmount,
                        CGSTAmount = td.CGSTAmount,
                        SGSTAmount = td.SGSTAmount,
                        CessRate = td.CessRate,
                        CessAmount = td.CessAmount,
                        TotalTaxAmount = td.TotalTaxAmount,
                        Quotation = copied,
                        Detail = newDetail
                    });
                }

                copied.Details!.Add(newDetail);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Original status is UNTOUCHED — key difference from Revise
                _context.SalesQuotationMains.Add(copied);
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(copied.QuotationId)
                ?? throw new Exception("Failed to retrieve copied quotation.");
        }

        // ════════════════════════════════════════════════════
        // REVISE
        // ════════════════════════════════════════════════════
        public async Task<SalesQuotationResponseDto> ReviseAsync(int id, ReviseSalesQuotationDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            // Load original with full details
            var original = await _context.SalesQuotationMains
                .Include(m => m.Details!).ThenInclude(d => d.TaxDetails)
                .FirstOrDefaultAsync(x =>
                    x.QuotationId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Quotation not found.");

            if (original.Status != "Draft" && original.Status != "Sent")
                throw new Exception("Only Draft or Sent quotations can be revised.");

            // Validate full header
            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            // ── Resolve revision number ────────────────────
            string baseNo;
            int nextRevision;

            if (original.ParentQuotationId == null)
            {
                // This IS the root quotation
                baseNo = original.QuotationNo;
                var maxRevision = await _context.SalesQuotationMains
                    .Where(x => x.ParentQuotationId == original.QuotationId && !x.IsDeleted)
                    .MaxAsync(x => (int?)x.RevisionNo) ?? 0;
                nextRevision = maxRevision + 1;
            }
            else
            {
                // This is already a revision — resolve root
                var root = await _context.SalesQuotationMains
                    .FirstOrDefaultAsync(x => x.QuotationId == original.ParentQuotationId)
                    ?? throw new Exception("Root quotation not found.");

                baseNo = root.QuotationNo;
                var maxRevision = await _context.SalesQuotationMains
                    .Where(x => x.ParentQuotationId == original.ParentQuotationId && !x.IsDeleted)
                    .MaxAsync(x => (int?)x.RevisionNo) ?? 0;
                nextRevision = maxRevision + 1;
            }

            var newQuotationNo = $"{baseNo}-R{nextRevision}";

            // ── Build fresh details from dto (recalculates tax/amounts) ──
            var builtDetails = new List<SalesQuotationDetail>();
            decimal totalSubTotal = 0;
            decimal totalTaxAmount = 0;
            decimal totalCessAmount = 0;

            foreach (var lineDto in dto.Details)
            {
                var detail = await BuildDetailWithTaxAsync(
                    lineDto, dto.SalesStateCode, dto.BillStateCode);

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                builtDetails.Add(detail);
            }

            // ── Build revised quotation ────────────────────
            var revised = new SalesQuotationMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                QuotationNo = newQuotationNo,
                QuotationDate = dto.QuotationDate,
                ValidUntilDate = dto.ValidUntilDate,
                BusinessPartnerId = dto.BusinessPartnerId,
                LocationId = dto.LocationId,
                RoundOff = dto.RoundOff,
                Remarks = dto.Remarks,
                Status = "Draft",
                SalesStateCode = dto.SalesStateCode,
                BillStateCode = dto.BillStateCode,
                ContactPersonId = dto.ContactPersonId,
                SalesPersonId = dto.SalesPersonId,
                BillAddressId = dto.BillAddressId,
                ShipAddressId = dto.ShipAddressId,
                SubTotal = totalSubTotal,
                TaxAmount = totalTaxAmount,
                CessAmount = totalCessAmount,
                NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff,

                // Key difference from Copy: link to root parent
                ParentQuotationId = original.ParentQuotationId ?? original.QuotationId,
                RevisionNo = nextRevision,

                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                Details = new List<SalesQuotationDetail>(),
                TaxDetails = new List<SalesQuotationTaxDetail>()
            };

            // Attach built details with tax
            foreach (var detail in builtDetails)
            {
                foreach (var td in detail.TaxDetails ?? Enumerable.Empty<SalesQuotationTaxDetail>())
                {
                    td.Quotation = revised;
                    td.Detail = detail;
                    revised.TaxDetails!.Add(td);
                }
                revised.Details!.Add(detail);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Mark original as Revised — key difference from Copy
                original.Status = "Revised";
                original.ModifiedBy = userId;
                original.ModifiedDate = DateTime.UtcNow;

                _context.SalesQuotationMains.Add(revised);
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(revised.QuotationId)
                ?? throw new Exception("Failed to retrieve revised quotation.");
        }

        // ════════════════════════════════════════════════════
        // GET ALL
        // ════════════════════════════════════════════════════
        public async Task<List<SalesQuotationResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var list = await _context.SalesQuotationMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.ContactPerson)
                .Include(x => x.SalesPerson)
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .OrderByDescending(x => x.QuotationDate)
                .ToListAsync();

            return list.Select(MapToResponseDto).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET BY ID
        // ════════════════════════════════════════════════════
        public async Task<SalesQuotationResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.SalesQuotationMains
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.ContactPerson)
                .Include(x => x.SalesPerson)
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .FirstOrDefaultAsync(x =>
                    x.QuotationId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            return main == null ? null : MapToResponseDto(main);
        }

        // ════════════════════════════════════════════════════
        // GET PAGED
        // ════════════════════════════════════════════════════
        public async Task<PagedResponseDto<SalesQuotationResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.SalesQuotationMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
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
                    x.QuotationNo.ToLower().Contains(search) ||
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
                    query = query.Where(x => x.QuotationDate >= fromDate);
                }
                if (request.Filters.ContainsKey("toDate"))
                {
                    var toDate = ((JsonElement)request.Filters["toDate"]).GetDateTime();
                    query = query.Where(x => x.QuotationDate <= toDate);
                }
                if (request.Filters.ContainsKey("salesPersonId"))
                {
                    var spId = ((JsonElement)request.Filters["salesPersonId"]).GetInt32();
                    query = query.Where(x => x.SalesPersonId == spId);
                }
                if (request.Filters.ContainsKey("validUntil"))
                {
                    var validUntil = ((JsonElement)request.Filters["validUntil"]).GetDateTime();
                    query = query.Where(x => x.ValidUntilDate <= validUntil);
                }
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "quotationno" => sort.Direction == "desc" ? query.OrderByDescending(x => x.QuotationNo) : query.OrderBy(x => x.QuotationNo),
                    "quotationdate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.QuotationDate) : query.OrderBy(x => x.QuotationDate),
                    "validuntildate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.ValidUntilDate) : query.OrderBy(x => x.ValidUntilDate),
                    "businesspartnername" => sort.Direction == "desc" ? query.OrderByDescending(x => x.BusinessPartner!.BusinessPartnerName) : query.OrderBy(x => x.BusinessPartner!.BusinessPartnerName),
                    "salesperson" => sort.Direction == "desc" ? query.OrderByDescending(x => x.SalesPerson!.SalesPersonName) : query.OrderBy(x => x.SalesPerson!.SalesPersonName),
                    "status" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "amount" => sort.Direction == "desc" ? query.OrderByDescending(x => x.SubTotal) : query.OrderBy(x => x.SubTotal),
                    "taxamount" => sort.Direction == "desc" ? query.OrderByDescending(x => x.TaxAmount) : query.OrderBy(x => x.TaxAmount),
                    "nettotal" or "netamount" => sort.Direction == "desc" ? query.OrderByDescending(x => x.NetTotal) : query.OrderBy(x => x.NetTotal),
                    _ => query.OrderByDescending(x => x.QuotationDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.QuotationDate);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .ToListAsync();

            return new PagedResponseDto<SalesQuotationResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapToResponseDto).ToList()
            };
        }

        // ════════════════════════════════════════════════════
        // GET BY CUSTOMER
        // ════════════════════════════════════════════════════
        public async Task<List<SalesQuotationListDto>> GetByCustomerAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();

            var list = await _context.SalesQuotationMains
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.BusinessPartnerId == businessPartnerId &&
                    !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .OrderByDescending(x => x.QuotationDate)
                .ToListAsync();

            return list.Select(x => new SalesQuotationListDto
            {
                QuotationId = x.QuotationId,
                QuotationNo = x.QuotationNo,
                QuotationDate = x.QuotationDate,
                ValidUntilDate = x.ValidUntilDate,
                Status = x.Status,
                BusinessPartnerName = x.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                NetTotal = x.NetTotal,
                ParentQuotationId = x.ParentQuotationId,
                RevisionNo = x.RevisionNo
            }).ToList();
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Build detail + tax
        // ════════════════════════════════════════════════════
        // ════════════════════════════════════════════════════════════════════════════
        // PRIVATE — Build detail + tax
        // Drop this entire method into SalesQuotationService to replace the existing one.
        // ════════════════════════════════════════════════════════════════════════════

        private async Task<SalesQuotationDetail> BuildDetailWithTaxAsync(
            CreateSalesQuotationDetailDto lineDto,
            int? salesStateCode, int? billStateCode)
        {
            var item = await _context.Items
                .Include(i => i.Hsn).ThenInclude(h => h!.tax)
                .FirstOrDefaultAsync(i => i.ItemId == lineDto.ItemId && !i.IsDeleted)
                ?? throw new Exception($"Item {lineDto.ItemId} not found.");

            bool hasHsn = item.HSNCodeId != 0 && item.Hsn != null;
            bool useManualTax = lineDto.ManualTaxId.HasValue;

            // ── Resolve tax rates ─────────────────────────────────────────────────
            int resolvedTaxId;
            decimal igstRate, cgstRate, sgstRate, cessRate;
            int hsnId;
            string hsnCode;

            if (useManualTax)
            {
                
                    var manualTax = await _context.Taxes
                        .FirstOrDefaultAsync(t => t.TaxId == lineDto.ManualTaxId && !t.IsDeleted)
                        ?? throw new Exception($"Tax {lineDto.ManualTaxId} not found.");

                    resolvedTaxId = manualTax.TaxId;

                    // Accept frontend-sent rates when available (they already reflect
                    // intra/inter-state switching); fall back to tax-master values.
                    igstRate = lineDto.ManualIgstRate ?? manualTax.IGST;
                    cgstRate = lineDto.ManualCgstRate ?? manualTax.CGST;
                    sgstRate = lineDto.ManualSgstRate ?? manualTax.SGST;
                    cessRate = lineDto.ManualCessRate ?? 0;
                

                // Use item's HSN info when available, else leave blank.
                hsnId = item.Hsn?.HsnId ?? 0;
                hsnCode = item.Hsn?.HsnName ?? string.Empty;
            }
            else
            {
                // Auto-load from HSN — item MUST have an HSN with a linked tax.
                if (!hasHsn)
                    throw new Exception(
                        $"Item '{item.ItemName}' has no HSN Code assigned. " +
                        "Please select a tax manually.");

                if (item.Hsn!.tax == null)
                    throw new Exception(
                        $"HSN '{item.Hsn.HsnName}' has no Tax assigned.");

                var hsn = item.Hsn;
                var tax = hsn.tax!;

                resolvedTaxId = tax.TaxId;
                igstRate = tax.IGST;
                cgstRate = tax.CGST;
                sgstRate = tax.SGST;
                cessRate = hsn.Cess ?? 0;
                hsnId = hsn.HsnId;
                hsnCode = hsn.HsnName;
            }

            // ── Intra / inter-state rate switching ────────────────────────────────
            bool isIntraState = (salesStateCode.HasValue && billStateCode.HasValue)
                ? salesStateCode.Value == billStateCode.Value
                : true;

            decimal effectiveIgst = isIntraState ? 0 : igstRate;
            decimal effectiveCgst = isIntraState ? cgstRate : 0;
            decimal effectiveSgst = isIntraState ? sgstRate : 0;

            // ── Amount calculation ────────────────────────────────────────────────
            decimal grossAmount = lineDto.Rate * lineDto.Qty;
            decimal discountAmt = Math.Round(grossAmount * lineDto.DiscountRate / 100, 2);
            decimal afterFirst = grossAmount - discountAmt;
            decimal addisDiscAmt = Math.Round(afterFirst * lineDto.AddisDiscountRate / 100, 2);
            decimal taxableAmount = afterFirst - addisDiscAmt;

            if (lineDto.IsTaxIncluded)
            {
                decimal totalTaxRate = isIntraState
                    ? (effectiveCgst + effectiveSgst)
                    : effectiveIgst;

                if (totalTaxRate > 0)
                    taxableAmount = Math.Round(taxableAmount / (1 + totalTaxRate / 100), 2);
            }

            decimal igstAmount = effectiveIgst > 0 ? Math.Round(taxableAmount * effectiveIgst / 100, 2) : 0;
            decimal cgstAmount = effectiveCgst > 0 ? Math.Round(taxableAmount * effectiveCgst / 100, 2) : 0;
            decimal sgstAmount = effectiveSgst > 0 ? Math.Round(taxableAmount * effectiveSgst / 100, 2) : 0;
            decimal cessAmt = cessRate > 0 ? Math.Round(taxableAmount * cessRate / 100, 2) : 0;
            decimal lineTaxAmt = igstAmount + cgstAmount + sgstAmount + cessAmt;

            // ── Build tax details row (empty for zero-tax) ────────────────────────
            var taxDetails = new List<SalesQuotationTaxDetail>
{
    new()
    {
        TaxId          = resolvedTaxId,   // 0 for exempt/export
        IGSTRate       = effectiveIgst,   // all zeros for exempt
        CGSTRate       = effectiveCgst,
        SGSTRate       = effectiveSgst,
        TaxableAmount  = taxableAmount,   // still needed for HSN summary
        IGSTAmount     = igstAmount,
        CGSTAmount     = cgstAmount,
        SGSTAmount     = sgstAmount,
        CessRate       = cessRate,
        CessAmount     = cessAmt,
        TotalTaxAmount = lineTaxAmt       // 0
    }
};
            return new SalesQuotationDetail
            {
                ItemId = lineDto.ItemId,
                HsnId = hsnId,
                HsnCode = hsnCode,
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
                CessAmount = cessAmt,
                LineTaxAmount = lineTaxAmt,
                LineTotal = taxableAmount + lineTaxAmt,
                TaxDetails = taxDetails
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Validate header
        // ════════════════════════════════════════════════════
        private async Task ValidateHeaderAsync(
            int businessPartnerId, int locationId, int companyId,
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

        // ════════════════════════════════════════════════════
        // PRIVATE — Generate quotation number
        // ════════════════════════════════════════════════════
        private async Task<string> GenerateQuotationNoAsync(int companyId, int finYearId)
        {
            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(x => x.FinancialYearId == finYearId);

            var yearLabel = financialYear != null
                ? $"{financialYear.StartDate.Year % 100}{financialYear.EndDate.Year % 100}"
                : finYearId.ToString();

            // Count only root quotations (no parent) for sequential numbering
            var count = await _context.SalesQuotationMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    x.ParentQuotationId == null);

            return $"QT-{yearLabel}-{(count + 1):D4}";
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Map to response DTO
        // ════════════════════════════════════════════════════
        private SalesQuotationResponseDto MapToResponseDto(SalesQuotationMain main)
        {
            return new SalesQuotationResponseDto
            {
                QuotationId = main.QuotationId,
                FinYearId = main.FinYearId,
                QuotationNo = main.QuotationNo,
                QuotationDate = main.QuotationDate,
                ValidUntilDate = main.ValidUntilDate,
                Status = main.Status,

                // ── Revision tracking ──────────────────────
                ParentQuotationId = main.ParentQuotationId,
                RevisionNo = main.RevisionNo,

                BusinessPartnerId = main.BusinessPartnerId,
                BusinessPartnerName = main.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                BusinessPartnerCode = main.BusinessPartner?.BusinessPartnerCode ?? string.Empty,

                LocationId = main.LocationId,
                LocationName = main.Location?.LocationName ?? string.Empty,

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

                Details = main.Details?.Select(d => new SalesQuotationDetailResponseDto
                {
                    DetailId = d.DetailId,
                    QuotationId = d.QuotationId,
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
                    TaxDetails = MapTaxDetails(d.TaxDetails)
                }).ToList() ?? new(),

                TaxDetails = main.Details?
                    .Where(d => d.TaxDetails != null)
                    .SelectMany(d => d.TaxDetails!.Select(MapTaxDetailDto))
                    .ToList() ?? new()
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

        private static List<SalesQuotationTaxDetailResponseDto> MapTaxDetails(
            IEnumerable<SalesQuotationTaxDetail>? rows) =>
            rows?.Select(MapTaxDetailDto).ToList() ?? new();

        private static SalesQuotationTaxDetailResponseDto MapTaxDetailDto(SalesQuotationTaxDetail td) =>
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