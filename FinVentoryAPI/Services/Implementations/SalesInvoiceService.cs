using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesInvoiceDTOs;
using FinVentoryAPI.Entities;
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
                .AnyAsync(x =>
                    x.LocationId == dto.LocationId &&
                    x.CompanyId == companyId);

            if (!locationExists)
                throw new Exception("Location not found.");



            // 4. Generate Invoice Number
            var invoiceNo = await GenerateInvoiceNoAsync(companyId, finYearId);

            // 5. Build Main
            var main = new SalesInvoiceMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                InvoiceNo = invoiceNo,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate,
                BusinessPartnerId = dto.BusinessPartnerId,
                LocationId = dto.LocationId,
                AccountId = bp.AccountId,
                RoundOff = dto.RoundOff,
                Remarks = dto.Remarks,
                Status = "Draft",
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            // 6. Process detail lines
            decimal totalSubTotal = 0;
            decimal totalTaxAmount = 0;
            decimal totalCessAmount = 0;

            main.Details = new List<SalesInvoiceDetail>();
            main.TaxDetails = new List<SalesInvoiceTaxDetail>();

            foreach (var lineDto in dto.Details)
            {
                var (detail, taxDetail) = await BuildDetailWithTaxAsync(lineDto, userId);

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                main.Details.Add(detail);
                main.TaxDetails.Add(taxDetail);
            }

            // 7. Set totals
            main.SubTotal = totalSubTotal;
            main.TaxAmount = totalTaxAmount;
            main.CessAmount = totalCessAmount;
            main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

            // 8. Save
            _context.SalesInvoiceMains.Add(main);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(main.InvoiceId)
                ?? throw new Exception("Failed to retrieve saved invoice.");
        }

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

        public async Task<List<SalesInvoiceResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var invoices = await _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.Account)
                .Include(x => x.Details!)
                    .ThenInclude(d => d.Item)
                .Include(x => x.Details!)
                    .ThenInclude(d => d.Hsn)
                .Include(x => x.TaxDetails!)
                    .ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!)
                    .ThenInclude(td => td.CessPostingAccount)
                .OrderByDescending(x => x.InvoiceDate)
                .ToListAsync();

            return invoices.Select(MapToResponseDto).ToList();
        }

        public async Task<SalesInvoiceResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.SalesInvoiceMains
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.Account)
                .Include(x => x.Details!)
                    .ThenInclude(d => d.Item)
                .Include(x => x.Details!)
                    .ThenInclude(d => d.Hsn)
                .Include(x => x.TaxDetails!)
                    .ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!)
                    .ThenInclude(td => td.CessPostingAccount)
                .FirstOrDefaultAsync(x =>
                    x.InvoiceId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (main == null)
                return null;

            return MapToResponseDto(main);
        }

        public async Task<PagedResponseDto<SalesInvoiceResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.Account)
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

                    default:
                        query = query.OrderByDescending(x => x.InvoiceDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.InvoiceDate);
            }

            // TOTAL COUNT
            var totalRecords = await query.CountAsync();

            // PAGINATION
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(x => x.Details!)
                    .ThenInclude(d => d.Item)
                .Include(x => x.Details!)
                    .ThenInclude(d => d.Hsn)
                .Include(x => x.TaxDetails!)
                    .ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!)
                    .ThenInclude(td => td.CessPostingAccount)
                .ToListAsync();

            return new PagedResponseDto<SalesInvoiceResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapToResponseDto).ToList()
            };
        }

        public async Task<bool> PostAsync(int id)
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
                throw new Exception("Only Draft invoices can be posted.");

            main.Status = "Posted";
            main.ModifiedBy = _common.GetUserId();
            main.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

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
                .AnyAsync(x =>
                    x.LocationId == dto.LocationId &&
                    x.CompanyId == companyId);

            if (!locationExists)
                throw new Exception("Location not found.");

            // 4. Update header fields
            main.InvoiceDate = dto.InvoiceDate;
            main.DueDate = dto.DueDate;
            main.BusinessPartnerId = dto.BusinessPartnerId;
            main.LocationId = dto.LocationId;
            main.AccountId = bp.AccountId;
            main.RoundOff = dto.RoundOff;
            main.Remarks = dto.Remarks;
            main.ModifiedBy = userId;
            main.ModifiedDate = DateTime.UtcNow;

            // 5. Full replace — remove old lines
            _context.SalesInvoiceDetails.RemoveRange(main.Details!);
            _context.SalesInvoiceTaxDetails.RemoveRange(main.TaxDetails!);
            main.Details!.Clear();
            main.TaxDetails!.Clear();

            // 6. Rebuild lines
            decimal totalSubTotal = 0;
            decimal totalTaxAmount = 0;
            decimal totalCessAmount = 0;

            foreach (var lineDto in dto.Details)
            {
                // Map UpdateDto → CreateDto for reuse
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

                var (detail, taxDetail) = await BuildDetailWithTaxAsync(createDto, userId);

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                main.Details.Add(detail);
                main.TaxDetails.Add(taxDetail);
            }

            // 7. Recalculate totals
            main.SubTotal = totalSubTotal;
            main.TaxAmount = totalTaxAmount;
            main.CessAmount = totalCessAmount;
            main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

            await _context.SaveChangesAsync();

            return true;
        }


        private async Task<(SalesInvoiceDetail, SalesInvoiceTaxDetail)> BuildDetailWithTaxAsync(
           CreateSalesInvoiceDetailDto lineDto, int userId)
        {
            // 1. Fetch Item → Hsn → Tax
            var item = await _context.Items
                .Include(i => i.Hsn)
                    .ThenInclude(h => h!.tax)
                .FirstOrDefaultAsync(i => i.ItemId == lineDto.ItemId)
                ?? throw new Exception($"Item {lineDto.ItemId} not found.");

            var hsn = item.Hsn
                ?? throw new Exception($"HSN not found for Item {lineDto.ItemId}.");

            var tax = hsn.tax
                ?? throw new Exception($"Tax not found for HSN {hsn.HsnId}.");

            // 2. Calculate gross amount
            decimal grossAmount = lineDto.Rate * lineDto.Qty;

            // 3. Apply first discount
            decimal discountAmount = Math.Round(
                grossAmount * lineDto.DiscountRate / 100, 2);

            decimal afterFirstDiscount = grossAmount - discountAmount;

            // 4. Apply additional discount
            decimal addisDiscountAmount = Math.Round(
                afterFirstDiscount * lineDto.AddisDiscountRate / 100, 2);

            decimal taxableAmount = afterFirstDiscount - addisDiscountAmount;

            // 5. Handle tax-included price
            if (lineDto.IsTaxIncluded)
            {
                decimal totalTaxRate = tax.IGST > 0
                    ? tax.IGST
                    : tax.CGST + tax.SGST;

                taxableAmount = Math.Round(
                    taxableAmount / (1 + totalTaxRate / 100), 2);
            }

            // 6. GST amounts
            decimal igstAmount = Math.Round(taxableAmount * tax.IGST / 100, 2);
            decimal cgstAmount = Math.Round(taxableAmount * tax.CGST / 100, 2);
            decimal sgstAmount = Math.Round(taxableAmount * tax.SGST / 100, 2);

            // 7. Cess amounts
            decimal cessRate = hsn.Cess ?? 0;
            decimal cessAmount = Math.Round(taxableAmount * cessRate / 100, 2);

            decimal lineTaxAmount = igstAmount + cgstAmount + sgstAmount + cessAmount;

            // 8. Build Detail
            var detail = new SalesInvoiceDetail
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
                LineTotal = taxableAmount + lineTaxAmount
            };

            // 9. Build TaxDetail
            var taxDetail = new SalesInvoiceTaxDetail
            {
                TaxId = tax.TaxId,
                IGSTRate = tax.IGST,
                CGSTRate = tax.CGST,
                SGSTRate = tax.SGST,
                TaxableAmount = taxableAmount,
                IGSTAmount = igstAmount,
                CGSTAmount = cgstAmount,
                SGSTAmount = sgstAmount,
                CessRate = cessRate,
                CessAmount = cessAmount,
                TotalTaxAmount = lineTaxAmount,
                IGSTPostingAccountId = tax.IGSTPostingAccountId,
                CGSTPostingAccountId = tax.CGSTPostingAccountId,
                SGSTPostingAccountId = tax.SGSTPostingAccountId,
                CessPostingAccountId = hsn.CessPostingAc
            };

            return (detail, taxDetail);
        }


        private async Task<string> GenerateInvoiceNoAsync(int companyId, int finYearId)
        {
            var count = await _context.SalesInvoiceMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            // Format: INV-FY001-0001
            return $"INV-FY{finYearId:D3}-{(count + 1):D4}";
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

                AccountId = main.AccountId,
                AccountName = main.Account?.AccountName ?? string.Empty,

                SubTotal = main.SubTotal,
                TaxAmount = main.TaxAmount,
                CessAmount = main.CessAmount,
                RoundOff = main.RoundOff,
                NetTotal = main.NetTotal,
                Remarks = main.Remarks,

                CreatedBy = (int)main.CreatedBy,
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

                    TaxDetails = main.TaxDetails?
                        .Where(td => td.DetailId == d.DetailId)
                        .Select(td => new SalesInvoiceTaxDetailResponseDto
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

                TaxDetails = main.TaxDetails?.Select(td => new SalesInvoiceTaxDetailResponseDto
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
            };

        }
    }
}
