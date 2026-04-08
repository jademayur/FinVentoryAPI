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

            // 1. Validate Business Partner
            var bp = await _context.BusinessPartners
                .FirstOrDefaultAsync(x =>
                    x.BusinessPartnerId == dto.BusinessPartnerId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Business Partner not found.");

            // 2. Validate Location
            var locationExists = await _context.Locations
                .AnyAsync(x => x.LocationId == dto.LocationId && x.CompanyId == companyId);
            if (!locationExists)
                throw new Exception("Location not found.");

            // 3. Validate Sales Account
            var salesAccountExists = await _context.Accounts
                .AnyAsync(x =>
                    x.AccountId == dto.SalesAccountId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!salesAccountExists)
                throw new Exception("Sales Account not found.");

            // 4. Validate new fields
            await ValidateNewFieldsAsync(dto.BusinessPartnerId, companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            // 5. Generate Invoice Number
            var invoiceNo = await GenerateInvoiceNoAsync(companyId, finYearId);

            // 6. Build Main
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

                // ── New Fields ──────────────────────────────
                SalesStateCode = dto.SalesStateCode,
                BillStateCode = dto.BillStateCode,
                ContactPersonId = dto.ContactPersonId,
                SalesPersonId = dto.SalesPersonId,
                BillAddressId = dto.BillAddressId,
                ShipAddressId = dto.ShipAddressId,
            };

            // 7. Process detail lines
            decimal totalSubTotal = 0;
            decimal totalTaxAmount = 0;
            decimal totalCessAmount = 0;

            main.Details = new List<SalesInvoiceDetail>();
            main.TaxDetails = new List<SalesInvoiceTaxDetail>();

            foreach (var lineDto in dto.Details)
            {
                var detail = await BuildDetailWithTaxAsync(lineDto, userId, dto.SalesStateCode, dto.BillStateCode);

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                if (detail.TaxDetails != null)
                {
                    foreach (var taxDetail in detail.TaxDetails)
                    {
                        taxDetail.Invoice = main;
                        taxDetail.Detail = detail;
                        main.TaxDetails.Add(taxDetail);
                    }
                }

                main.Details.Add(detail);
            }

            // 8. Set totals
            main.SubTotal = totalSubTotal;
            main.TaxAmount = totalTaxAmount;
            main.CessAmount = totalCessAmount;
            main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

            // 9. Save
            try
            {
                _context.SalesInvoiceMains.Add(main);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }

            // 10. Return saved invoice
            return await GetByIdAsync(main.InvoiceId)
                ?? throw new Exception("Failed to retrieve saved invoice.");
        }

        // ────────────────────────────────────────────────────
        // UPDATE
        // ────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateSalesInvoiceMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            // 1. Fetch existing
            var main = await _context.SalesInvoiceMains
                .Include(m => m.Details)
                .Include(m => m.TaxDetails)
                .FirstOrDefaultAsync(x =>
                    x.InvoiceId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null)
                return false;

            if (main.Status != "Draft")
                throw new Exception("Only Draft invoices can be updated.");

            // 2. Validate Business Partner
            var bp = await _context.BusinessPartners
                .FirstOrDefaultAsync(x =>
                    x.BusinessPartnerId == dto.BusinessPartnerId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Business Partner not found.");

            // 3. Validate Location
            var locationExists = await _context.Locations
                .AnyAsync(x => x.LocationId == dto.LocationId && x.CompanyId == companyId);
            if (!locationExists)
                throw new Exception("Location not found.");

            // 4. Validate Sales Account
            var salesAccountExists = await _context.Accounts
                .AnyAsync(x =>
                    x.AccountId == dto.SalesAccountId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!salesAccountExists)
                throw new Exception("Sales Account not found.");

            // 5. Validate new fields
            await ValidateNewFieldsAsync(dto.BusinessPartnerId, companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            // 6. Update header fields
            main.InvoiceDate = dto.InvoiceDate;
            main.DueDate = dto.DueDate;
            main.BusinessPartnerId = dto.BusinessPartnerId;
            main.LocationId = dto.LocationId;
            main.SalesAccountId = dto.SalesAccountId;
            main.RoundOff = dto.RoundOff;
            main.Remarks = dto.Remarks;
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            // ── New Fields ──────────────────────────────────
            main.SalesStateCode = dto.SalesStateCode;
            main.BillStateCode = dto.BillStateCode;
            main.ContactPersonId = dto.ContactPersonId;
            main.SalesPersonId = dto.SalesPersonId;
            main.BillAddressId = dto.BillAddressId;
            main.ShipAddressId = dto.ShipAddressId;

            // 7. Remove old lines — TaxDetails first, then Details
            if (main.TaxDetails != null && main.TaxDetails.Any())
            {
                _context.SalesInvoiceTaxDetails.RemoveRange(main.TaxDetails);
                main.TaxDetails.Clear();
            }

            if (main.Details != null && main.Details.Any())
            {
                _context.SalesInvoiceDetails.RemoveRange(main.Details);
                main.Details.Clear();
            }

            await _context.SaveChangesAsync();

            // 8. Rebuild lines
            decimal totalSubTotal = 0;
            decimal totalTaxAmount = 0;
            decimal totalCessAmount = 0;

            var newDetails = new List<SalesInvoiceDetail>();
            var newTaxDetails = new List<SalesInvoiceTaxDetail>();

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

                var detail = await BuildDetailWithTaxAsync(createDto, userId, dto.SalesStateCode, dto.BillStateCode);
                detail.InvoiceId = main.InvoiceId;

                if (detail.TaxDetails != null)
                {
                    foreach (var td in detail.TaxDetails)
                    {
                        newTaxDetails.Add(new SalesInvoiceTaxDetail
                        {
                            InvoiceId = main.InvoiceId,
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
                            IGSTPostingAccountId = td.IGSTPostingAccountId,
                            CGSTPostingAccountId = td.CGSTPostingAccountId,
                            SGSTPostingAccountId = td.SGSTPostingAccountId,
                            CessPostingAccountId = td.CessPostingAccountId
                        });
                    }
                }

                detail.TaxDetails = null;

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                newDetails.Add(detail);
            }

            // 9. Save Details first — DB assigns DetailId
            await _context.SalesInvoiceDetails.AddRangeAsync(newDetails);
            await _context.SaveChangesAsync();

            // 10. Set DetailId on each new TaxDetail
            int taxIndex = 0;
            foreach (var detail in newDetails)
            {
                newTaxDetails[taxIndex].DetailId = detail.DetailId;
                taxIndex++;
            }

            // 11. Save fresh TaxDetails
            await _context.SalesInvoiceTaxDetails.AddRangeAsync(newTaxDetails);

            // 12. Recalculate totals
            main.SubTotal = totalSubTotal;
            main.TaxAmount = totalTaxAmount;
            main.CessAmount = totalCessAmount;
            main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

            // 13. Final save
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

            if (main == null)
                return null;

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

            if (main == null)
                return false;

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

            if (main == null)
                return false;

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

            if (main == null)
                return false;

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
                switch (sort.Column.ToLower())
                {
                    case "invoiceno":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.InvoiceNo)
                            : query.OrderBy(x => x.InvoiceNo);
                        break;
                    case "invoicedate":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.InvoiceDate)
                            : query.OrderBy(x => x.InvoiceDate);
                        break;
                    case "businesspartnername":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.BusinessPartner!.BusinessPartnerName)
                            : query.OrderBy(x => x.BusinessPartner!.BusinessPartnerName);
                        break;
                    case "nettotal":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.NetTotal)
                            : query.OrderBy(x => x.NetTotal);
                        break;
                    case "status":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.Status)
                            : query.OrderBy(x => x.Status);
                        break;
                    case "salesperson":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.SalesPerson!.SalesPersonName)
                            : query.OrderBy(x => x.SalesPerson!.SalesPersonName);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.InvoiceDate);
                        break;
                }
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

        // ────────────────────────────────────────────────────
        // PRIVATE — Validate New Fields
        // ────────────────────────────────────────────────────
        private async Task ValidateNewFieldsAsync(
            int businessPartnerId, int companyId,
            int? salesStateCode, int? billStateCode,
            int? contactPersonId, int? salesPersonId,
            int? billAddressId, int? shipAddressId)
        {
            // Validate SalesStateCode
            if (salesStateCode.HasValue && !Enum.IsDefined(typeof(GstState), salesStateCode.Value))
                throw new Exception("Invalid Sales State Code.");

            // Validate BillStateCode
            if (billStateCode.HasValue && !Enum.IsDefined(typeof(GstState), billStateCode.Value))
                throw new Exception("Invalid Bill State Code.");

            // Validate ContactPerson — must belong to this BP
            if (contactPersonId.HasValue)
            {
                var cpExists = await _context.BusinessPartnerContacts
                    .AnyAsync(x =>
                        x.BPContactId == contactPersonId &&
                        x.BusinessPartnerId == businessPartnerId);
                if (!cpExists)
                    throw new Exception("Contact Person not found for this Business Partner.");
            }

            // Validate SalesPerson — must belong to this company
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

            // Validate BillAddress — must belong to this BP
            if (billAddressId.HasValue)
            {
                var billExists = await _context.BusinessPartnerAddresses
                    .AnyAsync(x =>
                        x.BPAddressId == billAddressId &&
                        x.BusinessPartnerId == businessPartnerId);
                if (!billExists)
                    throw new Exception("Bill Address not found for this Business Partner.");
            }

            // Validate ShipAddress — must belong to this BP
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

        // ────────────────────────────────────────────────────
        // PRIVATE — Build Detail + TaxDetail
        // ────────────────────────────────────────────────────
        private async Task<SalesInvoiceDetail> BuildDetailWithTaxAsync(
            CreateSalesInvoiceDetailDto lineDto, int userId, int? salesStateCode, int? billStateCode)
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
            decimal addisDiscountAmount = Math.Round(afterFirstDiscount * lineDto.AddisDiscountRate / 100, 2);
            decimal taxableAmount = afterFirstDiscount - addisDiscountAmount;

            bool isIntraState = (salesStateCode.HasValue && billStateCode.HasValue)
                                ? salesStateCode.Value == billStateCode.Value
                                : true;
            if (lineDto.IsTaxIncluded)
            {
                decimal totalTaxRate = isIntraState
                    ? (tax.CGST + tax.SGST)   // intra → CGST + SGST
                    : tax.IGST;                // inter → IGST only

                if (totalTaxRate > 0)
                    taxableAmount = Math.Round(taxableAmount / (1 + totalTaxRate / 100), 2);
            }
       

            decimal igstAmount = (!isIntraState) ? Math.Round(taxableAmount * tax.IGST / 100, 2) : 0;
            decimal cgstAmount = (isIntraState) ? Math.Round(taxableAmount * tax.CGST / 100, 2) : 0;
            decimal sgstAmount = (isIntraState) ? Math.Round(taxableAmount * tax.SGST / 100, 2) : 0;
            decimal cessRate = hsn.Cess ?? 0;
            decimal cessAmount = Math.Round(taxableAmount * cessRate / 100, 2);
            decimal lineTaxAmount = igstAmount + cgstAmount + sgstAmount + cessAmount;

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
                AddisDiscountAmount = addisDiscountAmount,
                IsTaxIncluded = lineDto.IsTaxIncluded,
                TaxableAmount = taxableAmount,
                CessRate = cessRate,
                CessAmount = cessAmount,
                LineTaxAmount = lineTaxAmount,
                LineTotal = taxableAmount + lineTaxAmount,

                TaxDetails = new List<SalesInvoiceTaxDetail>
                {
                    new SalesInvoiceTaxDetail
                    {
                        TaxId                = tax.TaxId,

                        // ✅ Zero out unused side rates
                        IGSTRate             = isIntraState ? 0 : tax.IGST,
                        CGSTRate             = isIntraState ? tax.CGST : 0,
                        SGSTRate             = isIntraState ? tax.SGST : 0,

                        TaxableAmount        = taxableAmount,

                        // ✅ Zero out unused side amounts
                        IGSTAmount           = igstAmount,
                        CGSTAmount           = cgstAmount,
                        SGSTAmount           = sgstAmount,

                        CessRate             = cessRate,
                        CessAmount           = cessAmount,
                        TotalTaxAmount       = lineTaxAmount,

                        // ✅ Zero out unused side posting accounts
                        IGSTPostingAccountId = isIntraState ? null : tax.IGSTPostingAccountId,
                        CGSTPostingAccountId = isIntraState ? tax.CGSTPostingAccountId : null,
                        SGSTPostingAccountId = isIntraState ? tax.SGSTPostingAccountId : null,
                        CessPostingAccountId = hsn.CessPostingAc
                    }
                }
            };
        }

        // ────────────────────────────────────────────────────
        // PRIVATE — Generate Invoice Number
        // ────────────────────────────────────────────────────
        private async Task<string> GenerateInvoiceNoAsync(int companyId, int finYearId)
        {
            var count = await _context.SalesInvoiceMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"INV-FY{finYearId:D3}-{(count + 1):D4}";
        }

        // ────────────────────────────────────────────────────
        // PRIVATE — Map Entity → ResponseDto
        // ────────────────────────────────────────────────────
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

                // ── New Fields ──────────────────────────────
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
                BillAddressLine = main.BillAddress == null ? null :
                    string.Join(", ", new[]
                    {
                        main.BillAddress.AddressLine1,
                        main.BillAddress.AddressLine2,
                        main.BillAddress.City,
                        main.BillAddress.State.HasValue
                            ? ((GstState)main.BillAddress.State.Value).ToString() : null,
                        main.BillAddress.Pincode
                    }.Where(x => !string.IsNullOrWhiteSpace(x))),

                ShipAddressId = main.ShipAddressId,
                ShipAddressLine = main.ShipAddress == null ? null :
                    string.Join(", ", new[]
                    {
                        main.ShipAddress.AddressLine1,
                        main.ShipAddress.AddressLine2,
                        main.ShipAddress.City,
                        main.ShipAddress.State.HasValue
                            ? ((GstState)main.ShipAddress.State.Value).ToString() : null,
                        main.ShipAddress.Pincode
                    }.Where(x => !string.IsNullOrWhiteSpace(x))),

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

                    TaxDetails = d.TaxDetails?.Select(td => new SalesInvoiceTaxDetailResponseDto
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
                    }).ToList() ?? new List<SalesInvoiceTaxDetailResponseDto>()

                }).ToList() ?? new List<SalesInvoiceDetailResponseDto>(),

                TaxDetails = main.Details?
                    .Where(d => d.TaxDetails != null)
                    .SelectMany(d => d.TaxDetails!.Select(td => new SalesInvoiceTaxDetailResponseDto
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
                    }))
                    .ToList() ?? new List<SalesInvoiceTaxDetailResponseDto>()
            };
        }
    }
}