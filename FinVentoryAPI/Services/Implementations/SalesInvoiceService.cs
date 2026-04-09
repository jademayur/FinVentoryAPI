using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesInvoiceDTOs;
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

        public SalesInvoiceService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ────────────────────────────────────────────────────
        // CREATE
        // ────────────────────────────────────────────────────
        public async Task<SalesInvoiceResponseDto> CreateAsync(CreateSalesInvoiceMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            // 1. Validate header
            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, dto.SalesAccountId,
                companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            // 2. Generate invoice number
            var invoiceNo = await GenerateInvoiceNoAsync(companyId, finYearId);

            // 3. Build main entity
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

            // 4. Process detail lines
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

            // 5. Set totals
            main.SubTotal = totalSubTotal;
            main.TaxAmount = totalTaxAmount;
            main.CessAmount = totalCessAmount;
            main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

            // 6. Save
            try
            {
                _context.SalesInvoiceMains.Add(main);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }

            // 7. Return saved invoice
            return await GetByIdAsync(main.InvoiceId)
                ?? throw new Exception("Failed to retrieve saved invoice.");
        }

        // ────────────────────────────────────────────────────
        // UPDATE  (smart diff — no delete / reinsert)
        // ────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateSalesInvoiceMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            // 1. Load with existing lines
            var main = await _context.SalesInvoiceMains
                .Include(m => m.Details!)
                    .ThenInclude(d => d.TaxDetails)
                .FirstOrDefaultAsync(x =>
                    x.InvoiceId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;

            if (main.Status != "Draft")
                throw new Exception("Only Draft invoices can be updated.");

            // 2. Validate header
            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, dto.SalesAccountId,
                companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            // 3. Update header fields
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

            // 4. Build freshly-calculated details in memory
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

            // 5. Smart diff on Details — matched by index
            //    i < existing  → update in-place   (DetailId preserved)
            //    i >= existing → insert new row
            //    surplus existing rows → remove
            var existingDetails = main.Details!.ToList();

            decimal totalSubTotal = 0;
            decimal totalTaxAmount = 0;
            decimal totalCessAmount = 0;

            for (int i = 0; i < incomingDetails.Count; i++)
            {
                var incoming = incomingDetails[i];

                if (i < existingDetails.Count)
                {
                    // ── UPDATE existing detail in-place ──────────────────
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

                    // ── Smart diff TaxDetails for this detail ─────────
                    var existingTaxList = existing.TaxDetails?.ToList()
                        ?? new List<SalesInvoiceTaxDetail>();
                    var incomingTaxList = incoming.TaxDetails
                        ?? new List<SalesInvoiceTaxDetail>();

                    for (int t = 0; t < incomingTaxList.Count; t++)
                    {
                        var inTax = incomingTaxList[t];

                        if (t < existingTaxList.Count)
                        {
                            // update tax row in-place
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
                            // insert new tax row for this detail
                            inTax.InvoiceId = main.InvoiceId;
                            inTax.DetailId = existing.DetailId;
                            _context.SalesInvoiceTaxDetails.Add(inTax);
                        }
                    }

                    // remove surplus tax rows
                    if (existingTaxList.Count > incomingTaxList.Count)
                    {
                        var surplusTax = existingTaxList
                            .Skip(incomingTaxList.Count).ToList();
                        _context.SalesInvoiceTaxDetails.RemoveRange(surplusTax);
                    }

                    totalSubTotal += existing.TaxableAmount;
                    totalCessAmount += existing.CessAmount;
                    totalTaxAmount += existing.LineTaxAmount - existing.CessAmount;
                }
                else
                {
                    // ── INSERT new detail row ─────────────────────────
                    incoming.InvoiceId = main.InvoiceId;

                    var newTaxList = incoming.TaxDetails
                        ?? new List<SalesInvoiceTaxDetail>();

                    // detach tax nav so EF doesn't try to save them before DetailId exists
                    incoming.TaxDetails = null;

                    _context.SalesInvoiceDetails.Add(incoming);
                    await _context.SaveChangesAsync(); // flush → DetailId is now populated

                    foreach (var inTax in newTaxList)
                    {
                        inTax.InvoiceId = main.InvoiceId;
                        inTax.DetailId = incoming.DetailId;
                        _context.SalesInvoiceTaxDetails.Add(inTax);
                    }

                    totalSubTotal += incoming.TaxableAmount;
                    totalCessAmount += incoming.CessAmount;
                    totalTaxAmount += incoming.LineTaxAmount - incoming.CessAmount;
                }
            }

            // 6. Remove surplus detail rows (and their child tax rows)
            if (existingDetails.Count > incomingDetails.Count)
            {
                var surplusDetails = existingDetails
                    .Skip(incomingDetails.Count).ToList();

                foreach (var sd in surplusDetails)
                {
                    if (sd.TaxDetails != null && sd.TaxDetails.Any())
                        _context.SalesInvoiceTaxDetails.RemoveRange(sd.TaxDetails);
                }

                _context.SalesInvoiceDetails.RemoveRange(surplusDetails);
            }

            // 7. Recalculate header totals
            main.SubTotal = totalSubTotal;
            main.TaxAmount = totalTaxAmount;
            main.CessAmount = totalCessAmount;
            main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

            // 8. Final save
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }

            _context.ChangeTracker.Clear();
            return true;
        }

        // ────────────────────────────────────────────────────
        // GET ALL
        // ────────────────────────────────────────────────────
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
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.IGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.CGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.SGSTPostingAccount)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.CessPostingAccount)
                .OrderByDescending(x => x.InvoiceDate)
                .ToListAsync();

            return invoices.Select(MapToResponseDto).ToList();
        }

        // ────────────────────────────────────────────────────
        // GET BY ID
        // ────────────────────────────────────────────────────
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

        // ────────────────────────────────────────────────────
        // DELETE
        // ────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.SalesInvoiceMains
                .FirstOrDefaultAsync(x =>
                    x.InvoiceId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;

            if (main.Status != "Draft")
                throw new Exception("Only Draft invoices can be deleted.");

            main.IsDeleted = true;
            main.IsActive = false;
            main.ModifiedBy = _common.GetUserId();
            main.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────
        // POST
        // ────────────────────────────────────────────────────
        public async Task<bool> PostAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.SalesInvoiceMains
                .Include(x => x.BusinessPartner)
                .FirstOrDefaultAsync(x =>
                    x.InvoiceId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;

            if (main.Status != "Draft")
                throw new Exception("Only Draft invoices can be posted.");

            main.Status = "Posted";
            main.ModifiedBy = _common.GetUserId();
            main.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────
        // CANCEL
        // ────────────────────────────────────────────────────
        public async Task<bool> CancelAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.SalesInvoiceMains
                .FirstOrDefaultAsync(x =>
                    x.InvoiceId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;

            if (main.Status == "Cancelled")
                throw new Exception("Invoice is already cancelled.");

            main.Status = "Cancelled";
            main.ModifiedBy = _common.GetUserId();
            main.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────
        // GET PAGED
        // ────────────────────────────────────────────────────
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
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.InvoiceNo.ToLower().Contains(search) ||
                    x.BusinessPartner!.BusinessPartnerName.ToLower().Contains(search));
            }

            // FILTERS
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

            // SORTING
            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "invoiceno" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.InvoiceNo)
                        : query.OrderBy(x => x.InvoiceNo),
                    "invoicedate" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.InvoiceDate)
                        : query.OrderBy(x => x.InvoiceDate),
                    "businesspartnername" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.BusinessPartner!.BusinessPartnerName)
                        : query.OrderBy(x => x.BusinessPartner!.BusinessPartnerName),
                    "nettotal" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.NetTotal)
                        : query.OrderBy(x => x.NetTotal),
                    "status" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status),
                    "salesperson" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.SalesPerson!.SalesPersonName)
                        : query.OrderBy(x => x.SalesPerson!.SalesPersonName),
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
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════

        // ── Validate header (shared by Create + Update) ──────
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
            if (!bpExists)
                throw new Exception("Business Partner not found.");

            var locationExists = await _context.Locations
                .AnyAsync(x => x.LocationId == locationId && x.CompanyId == companyId);
            if (!locationExists)
                throw new Exception("Location not found.");

            var salesAccountExists = await _context.Accounts
                .AnyAsync(x =>
                    x.AccountId == salesAccountId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!salesAccountExists)
                throw new Exception("Sales Account not found.");

            await ValidateNewFieldsAsync(
                businessPartnerId, companyId,
                salesStateCode, billStateCode,
                contactPersonId, salesPersonId,
                billAddressId, shipAddressId);
        }

        // ── Validate optional new fields ─────────────────────
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
                if (!spExists)
                    throw new Exception("Sales Person not found.");
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

        // ── Build detail + tax lines ──────────────────────────
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
            decimal afterFirstDiscount = grossAmount - discountAmount;
            decimal addisDiscountAmt = Math.Round(afterFirstDiscount * lineDto.AddisDiscountRate / 100, 2);
            decimal taxableAmount = afterFirstDiscount - addisDiscountAmt;

            bool isIntraState = (salesStateCode.HasValue && billStateCode.HasValue)
                ? salesStateCode.Value == billStateCode.Value
                : true;

            if (lineDto.IsTaxIncluded)
            {
                decimal totalTaxRate = isIntraState
                    ? (tax.CGST + tax.SGST)
                    : tax.IGST;

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
                AddisDiscountAmount = addisDiscountAmt,
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

        // ── Generate invoice number ───────────────────────────
        private async Task<string> GenerateInvoiceNoAsync(int companyId, int finYearId)
        {
            var count = await _context.SalesInvoiceMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"INV-FY{finYearId:D3}-{(count + 1):D4}";
        }

        // ── Map entity → response DTO ─────────────────────────
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
                addr.AddressLine1,
                addr.AddressLine2,
                addr.City,
                addr.State.HasValue
                    ? ((GstState)addr.State.Value).ToString() : null,
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
    }
}