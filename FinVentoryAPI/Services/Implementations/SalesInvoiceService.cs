using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesInvoiceDTOs;
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Migrations;
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

        public SalesInvoiceService(AppDbContext context, Common common, IStockLedgerService stockLedger, IAccountLedgerPostingService accountLedger)
        {
            _context = context;
            _common = common;
            _stockLedger = stockLedger;
            _accountLedger = accountLedger;
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

            await PostStockLedgerAsync(main, isReversal: false);
            await PostAccountLedgerAsync(main, isReversal: false);

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
                .Include(m => m.TaxDetails)       
                .Include(m => m.BusinessPartner)  
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

            await UpdateStockLedgerAsync(main);
            await UpdateAccountLedgerAsync(main);

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
            var userId = _common.GetUserId();

            var main = await _context.SalesInvoiceMains
                .Include(m => m.TaxDetails)
                .Include(m => m.BusinessPartner)
                .FirstOrDefaultAsync(x =>
                    x.InvoiceId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null) return false;

            if (main.Status != "Draft")
                throw new Exception("Only Draft invoices can be deleted.");

            main.IsDeleted = true;

            // 4. In DeleteAsync — after setting main.IsDeleted = true, before SaveChangesAsync
            await _stockLedger.SoftDeleteByVoucherAsync(
     companyId, main.InvoiceNo, _common.GetUserId());
            await _accountLedger.SoftDeleteByVoucherAsync(
         companyId, main.FinYearId, main.InvoiceNo, userId);

            main.IsActive = false;
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
                .Include(x => x.Details!)
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

                    "salesperson" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.SalesPerson!.SalesPersonName)
                        : query.OrderBy(x => x.SalesPerson!.SalesPersonName),

                    "status" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status),

                    "amount" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.SubTotal)
                        : query.OrderBy(x => x.SubTotal),

                    "taxamount" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.TaxAmount)
                        : query.OrderBy(x => x.TaxAmount),

                    "discount" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.Details!.Sum(d => d.DiscountAmount + d.AddisDiscountAmount))
                        : query.OrderBy(x => x.Details!.Sum(d => d.DiscountAmount + d.AddisDiscountAmount)),

                    "nettotal" or "netamount" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.NetTotal)
                        : query.OrderBy(x => x.NetTotal),

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
            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(x => x.FinancialYearId == finYearId);


            //var yearLabel = financialYear?.YearName ?? finYearId.ToString();
            // yearLabel = "2025-2026"

            // Optionally format it as "2526" (short form)
            var yearLabel = financialYear != null
                ? $"{financialYear.StartDate.Year % 100}{financialYear.EndDate.Year % 100}"
                : finYearId.ToString();
           // yearLabel = "2526"
            var count = await _context.SalesInvoiceMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"INV-{yearLabel}-{(count + 1):D4}";
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

                // AFTER — add the 4 aliased fields below the existing ones:
                SubTotal = main.SubTotal,
                TaxAmount = main.TaxAmount,
                CessAmount = main.CessAmount,
                RoundOff = main.RoundOff,
                NetTotal = main.NetTotal,

                // Fields consumed by the list page
                Amount = main.SubTotal,                                          // gross taxable
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

        // 5. Add this private helper method inside SalesInvoiceService
        private async Task PostStockLedgerAsync(SalesInvoiceMain main, bool isReversal)
        {
            if (main.Details == null || !main.Details.Any()) return;

            var lines = main.Details.Select(d => new StockLedgerLineDto
            {
                ItemId = d.ItemId,
                Qty = isReversal ? d.Qty : -d.Qty,   // negative = stock going OUT on sale
                Rate = d.Rate,
                Remarks = $"Sales Invoice: {main.InvoiceNo}"
            }).ToList();

            await _stockLedger.AddEntriesAsync(
                companyId: main.CompanyId,
                warehouseId: null,              // set from main.LocationId if you map Location → Warehouse
                date: main.InvoiceDate,
                voucherType: "Sales Invoice",
                voucherNo: main.InvoiceNo,
                businessPartnerId: main.BusinessPartnerId,
                lines: lines,
                createdBy: (int?)main.CreatedBy
            );
        }

        // ── Post to Account Ledger ────────────────────────────
        private async Task PostAccountLedgerAsync(SalesInvoiceMain main, bool isReversal)
        {
            // Load BusinessPartner with AccountId if not already loaded
            var bp = main.BusinessPartner
                ?? await _context.BusinessPartners
                    .FirstOrDefaultAsync(x => x.BusinessPartnerId == main.BusinessPartnerId);

            if (bp == null) return;

            // ── Double entry for Sales Invoice ────────────────
            //
            //  Normal:   DR Receivable (customer)   → NetTotal
            //            CR Sales Account           → SubTotal
            //            CR Tax Accounts            → TaxAmount + CessAmount
            //
            //  Reversal: flip Debit ↔ Credit

            var lines = new List<AccountLedgerLineDto>
    {
        // 1. Receivable account (Business Partner)
        new()
        {
            AccountId         = bp.AccountId,
            BusinessPartnerId = main.BusinessPartnerId,
            Debit             = isReversal ? 0 : main.NetTotal,
            Credit            = isReversal ? main.NetTotal : 0,
            Remarks           = $"Sales Invoice: {main.InvoiceNo}"
        },

        // 2. Sales account
        new()
        {
            AccountId         = main.SalesAccountId,
            BusinessPartnerId = main.BusinessPartnerId,
            Debit             = isReversal ? main.SubTotal : 0,
            Credit            = isReversal ? 0 : main.SubTotal,
            Remarks           = $"Sales Invoice: {main.InvoiceNo}"
        }
    };

            // 3. Tax accounts — one line per unique tax posting account
            if (main.TaxDetails != null && main.TaxDetails.Any())
            {
                // ── IGST (inter-state) ────────────────────────────
                var igstGroups = main.TaxDetails
                    .Where(t => t.IGSTPostingAccountId.HasValue
                             && t.IGSTPostingAccountId.Value > 0    // ✅ guard against 0
                             && t.IGSTAmount > 0)
                    .GroupBy(t => t.IGSTPostingAccountId!.Value);

                foreach (var g in igstGroups)
                {
                    lines.Add(new AccountLedgerLineDto
                    {
                        AccountId = g.Key,
                        BusinessPartnerId = main.BusinessPartnerId,
                        Debit = isReversal ? g.Sum(t => t.IGSTAmount) : 0,
                        Credit = isReversal ? 0 : g.Sum(t => t.IGSTAmount),
                        Remarks = $"IGST - Sales Invoice: {main.InvoiceNo}"
                    });
                }

                // ── CGST (intra-state) ────────────────────────────
                var cgstGroups = main.TaxDetails
                    .Where(t => t.CGSTPostingAccountId.HasValue
                             && t.CGSTPostingAccountId.Value > 0    // ✅ guard against 0
                             && t.CGSTAmount > 0)
                    .GroupBy(t => t.CGSTPostingAccountId!.Value);

                foreach (var g in cgstGroups)
                {
                    lines.Add(new AccountLedgerLineDto
                    {
                        AccountId = g.Key,
                        BusinessPartnerId = main.BusinessPartnerId,
                        Debit = isReversal ? g.Sum(t => t.CGSTAmount) : 0,
                        Credit = isReversal ? 0 : g.Sum(t => t.CGSTAmount),
                        Remarks = $"CGST - Sales Invoice: {main.InvoiceNo}"
                    });
                }

                // ── SGST (intra-state) ────────────────────────────
                var sgstGroups = main.TaxDetails
                    .Where(t => t.SGSTPostingAccountId.HasValue
                             && t.SGSTPostingAccountId.Value > 0    // ✅ guard against 0
                             && t.SGSTAmount > 0)
                    .GroupBy(t => t.SGSTPostingAccountId!.Value);

                foreach (var g in sgstGroups)
                {
                    lines.Add(new AccountLedgerLineDto
                    {
                        AccountId = g.Key,
                        BusinessPartnerId = main.BusinessPartnerId,
                        Debit = isReversal ? g.Sum(t => t.SGSTAmount) : 0,
                        Credit = isReversal ? 0 : g.Sum(t => t.SGSTAmount),
                        Remarks = $"SGST - Sales Invoice: {main.InvoiceNo}"
                    });
                }

                // ── Cess ──────────────────────────────────────────
                var cessGroups = main.TaxDetails
                    .Where(t => t.CessPostingAccountId.HasValue
                             && t.CessPostingAccountId.Value > 0    // ✅ guard against 0
                             && t.CessAmount > 0)
                    .GroupBy(t => t.CessPostingAccountId!.Value);

                foreach (var g in cessGroups)
                {
                    lines.Add(new AccountLedgerLineDto
                    {
                        AccountId = g.Key,
                        BusinessPartnerId = main.BusinessPartnerId,
                        Debit = isReversal ? g.Sum(t => t.CessAmount) : 0,
                        Credit = isReversal ? 0 : g.Sum(t => t.CessAmount),
                        Remarks = $"Cess - Sales Invoice: {main.InvoiceNo}"
                    });
                }
            }

            await _accountLedger.AddEntriesAsync(
                companyId: main.CompanyId,
                financialYearId: main.FinYearId,
                date: main.InvoiceDate,
                voucherType: "Sales Invoice",
                voucherNo: main.InvoiceNo,
                lines: lines,
                createdBy: (int?)main.CreatedBy
            );
        }

        private async Task UpdateStockLedgerAsync(SalesInvoiceMain main)
        {
            if (main.Details == null || !main.Details.Any()) return;

            var lines = main.Details.Select(d => new StockLedgerLineDto
            {
                ItemId = d.ItemId,
                Qty = -d.Qty,   // negative = stock OUT on sale
                Rate = d.Rate,
                Remarks = $"Sales Invoice: {main.InvoiceNo}"
            }).ToList();

            await _stockLedger.UpdateEntriesAsync(
                companyId: main.CompanyId,
                warehouseId: null,
                date: main.InvoiceDate,
                voucherType: "Sales Invoice",
                voucherNo: main.InvoiceNo,
                businessPartnerId: main.BusinessPartnerId,
                lines: lines,
                modifiedBy: (int?)main.ModifiedBy
            );
        }

        private async Task UpdateAccountLedgerAsync(SalesInvoiceMain main)
        {
            var bp = main.BusinessPartner
                ?? await _context.BusinessPartners
                    .FirstOrDefaultAsync(x => x.BusinessPartnerId == main.BusinessPartnerId);

            if (bp == null) return;

            var lines = BuildAccountLedgerLines(main, bp, isReversal: false);

            await _accountLedger.UpdateEntriesAsync(
                companyId: main.CompanyId,
                financialYearId: main.FinYearId,
                date: main.InvoiceDate,
                voucherType: "Sales Invoice",
                voucherNo: main.InvoiceNo,
                lines: lines,
                modifiedBy: (int?)main.ModifiedBy
            );
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
    }
}