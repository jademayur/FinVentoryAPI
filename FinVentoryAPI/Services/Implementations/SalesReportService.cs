using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.SalesReportDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class SalesReportService: ISalesReportService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public SalesReportService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════
        // GENERATE REPORT
        // ════════════════════════════════════════════════
        public async Task<SalesReportResponseDto> GenerateAsync(SalesReportRequestDto req)
        {
            var companyId = _common.GetCompanyId();
            var finYearId = _common.GetFinancialYearId();

            // ── Base query — always scoped to company + financial year ──
            var baseQuery = _context.SalesInvoiceMains
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            // ── Apply shared filters ───────────────────────────────────
            if (req.FromDate.HasValue)
                baseQuery = baseQuery.Where(x => x.InvoiceDate >= req.FromDate.Value);

            if (req.ToDate.HasValue)
                baseQuery = baseQuery.Where(x => x.InvoiceDate <= req.ToDate.Value);

            if (req.BusinessPartnerIds?.Count > 0)
                baseQuery = baseQuery
                    .Where(x => req.BusinessPartnerIds.Contains(x.BusinessPartnerId));

            if (req.SalesPersonIds?.Count > 0)
                baseQuery = baseQuery
                    .Where(x => x.SalesPersonId.HasValue
                             && req.SalesPersonIds.Contains(x.SalesPersonId.Value));

            if (req.LocationIds?.Count > 0)
                baseQuery = baseQuery
                    .Where(x => req.LocationIds.Contains(x.LocationId));

            if (req.Statuses?.Count > 0)
                baseQuery = baseQuery
                    .Where(x => req.Statuses.Contains(x.Status));

            if (req.GstTypes?.Count > 0)
                baseQuery = baseQuery
                    .Where(x => x.BusinessPartner != null
                             && req.GstTypes.Contains(
                                 x.BillStateCode == x.SalesStateCode ? "B2B" : "IGST"));
            // NOTE: adjust GstType logic to match your actual B2B/B2C flag field

            // ── Meta totals (always from filtered base) ────────────────
            var meta = await baseQuery
                .GroupBy(_ => 1)
                .Select(g => new SalesReportMetaDto
                {
                    TotalSubTotal = g.Sum(x => x.SubTotal),
                    TotalDiscount = g.Sum(x =>
                        x.Details!.Sum(d => d.DiscountAmount + d.AddisDiscountAmount)),
                    TotalTax = g.Sum(x => x.TaxAmount),
                    TotalCess = g.Sum(x => x.CessAmount),
                    TotalNet = g.Sum(x => x.NetTotal),
                    TotalRecords = g.Count()
                })
                .FirstOrDefaultAsync() ?? new SalesReportMetaDto();

            // ── Dispatch to report builder ─────────────────────────────
            object data = req.ReportType switch
            {
                "SalesRegister" => await BuildSalesRegisterAsync(baseQuery),
                "SalesRegisterDetails" => await BuildSalesRegisterDetailsAsync(baseQuery, req),
                "ItemWise" => await BuildItemWiseAsync(baseQuery, req),
                "PartyWise" => await BuildPartyWiseAsync(baseQuery),
                "TaxWise" => await BuildTaxWiseAsync(baseQuery),
                "MonthlySummary" => await BuildMonthlySummaryAsync(baseQuery),
                "MonthlyGST" => await BuildMonthlyGSTAsync(baseQuery),
                _ => throw new ArgumentException(
                                              $"Unknown ReportType: {req.ReportType}")
            };

            return new SalesReportResponseDto
            {
                ReportType = req.ReportType,
                FromDate = req.FromDate?.ToString("dd MMM yyyy") ?? "",
                ToDate = req.ToDate?.ToString("dd MMM yyyy") ?? "",
                Meta = meta,
                Data = data
            };
        }

        // ════════════════════════════════════════════════
        // FILTER OPTIONS
        // ════════════════════════════════════════════════
        public async Task<SalesReportFilterOptionsDto> GetFilterOptionsAsync()
        {
            var companyId = _common.GetCompanyId();
            var finYearId = _common.GetFinancialYearId();

            // Only return parties/persons that have at least one invoice
            // in this company+year (keeps dropdowns clean)
            var invoiceQuery = _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && x.FinYearId == finYearId
                         && !x.IsDeleted);

            var businessPartnerIds = await invoiceQuery
                .Select(x => x.BusinessPartnerId)
                .Distinct()
                .ToListAsync();

            var salesPersonIds = await invoiceQuery
                .Where(x => x.SalesPersonId.HasValue)
                .Select(x => x.SalesPersonId!.Value)
                .Distinct()
                .ToListAsync();

            var locationIds = await invoiceQuery
                .Select(x => x.LocationId)
                .Distinct()
                .ToListAsync();

            var itemIds = await _context.SalesInvoiceDetails
                .Where(d => invoiceQuery
                    .Select(x => x.InvoiceId)
                    .Contains(d.InvoiceId))
                .Select(d => d.ItemId)
                .Distinct()
                .ToListAsync();

            var businessPartners = await _context.BusinessPartners
                .Where(x => businessPartnerIds.Contains(x.BusinessPartnerId)
                         && !x.IsDeleted)
                .Select(x => new IdNameDto
                {
                    Id = x.BusinessPartnerId,
                    Name = x.BusinessPartnerName
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            var items = await _context.Items
                .Where(x => itemIds.Contains(x.ItemId) && !x.IsDeleted)
                .Select(x => new ItemOptionDto
                {
                    ItemId = x.ItemId,
                    ItemName = x.ItemName,
                    ItemCode = x.ItemCode
                })
                .OrderBy(x => x.ItemName)
                .ToListAsync();

            var salesPersons = await _context.SalesPersons
                .Where(x => salesPersonIds.Contains(x.SalesPersonId)
                         && x.CompanyId == companyId
                         && !x.IsDeleted)
                .Select(x => new IdNameDto
                {
                    Id = x.SalesPersonId,
                    Name = x.SalesPersonName
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            var locations = await _context.Locations
                .Where(x => locationIds.Contains(x.LocationId)
                         && x.CompanyId == companyId)
                .Select(x => new IdNameDto
                {
                    Id = x.LocationId,
                    Name = x.LocationName
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return new SalesReportFilterOptionsDto
            {
                BusinessPartners = businessPartners,
                Items = items,
                SalesPersons = salesPersons,
                Locations = locations,
                GstTypes = new List<string> { "B2B", "B2C", "EXPORT", "EXEMPT" },
                Statuses = new List<string> { "Draft", "Confirmed", "Cancelled" }
            };
        }

        // ════════════════════════════════════════════════
        // REPORT BUILDERS
        // ════════════════════════════════════════════════

        // ── 1. Sales Register ─────────────────────────────────
        private async Task<List<SalesRegisterRowDto>> BuildSalesRegisterAsync(
            IQueryable<SalesInvoiceMain> q)
        {
            return await q
                .Include(x => x.BusinessPartner)
                .Include(x => x.SalesPerson)
                .Include(x => x.Location)
                .Include(x => x.Details)
                .OrderBy(x => x.InvoiceDate)
                .ThenBy(x => x.InvoiceNo)
                .Select(x => new SalesRegisterRowDto
                {
                    InvoiceNo = x.InvoiceNo,
                    InvoiceDate = x.InvoiceDate.ToString("dd MMM yy"),
                    PartyName = x.BusinessPartner!.BusinessPartnerName,
                    //GstNo = x.BusinessPartner.GSTNo,
                    GstType = x.BillStateCode == x.SalesStateCode ? "B2B" : "IGST",
                    SalesPerson = x.SalesPerson != null
                                      ? x.SalesPerson.SalesPersonName : null,
                    Location = x.Location != null
                                      ? x.Location.LocationName : null,
                    Status = x.Status,
                    SubTotal = x.SubTotal,
                    Discount = x.Details!
                                     .Sum(d => d.DiscountAmount + d.AddisDiscountAmount),
                    TaxableAmount = x.SubTotal,
                    TaxAmount = x.TaxAmount,
                    CessAmount = x.CessAmount,
                    NetTotal = x.NetTotal,
                    Remarks = x.Remarks
                })
                .ToListAsync();
        }

        // ── 2. Sales Register Details (with item lines) ───────
        private async Task<List<SalesRegisterDetailsRowDto>> BuildSalesRegisterDetailsAsync(
            IQueryable<SalesInvoiceMain> q, SalesReportRequestDto req)
        {
            var invoices = await q
                .Include(x => x.BusinessPartner)
                .Include(x => x.SalesPerson)
                .Include(x => x.Location)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails)
                .OrderBy(x => x.InvoiceDate)
                .ThenBy(x => x.InvoiceNo)
                .ToListAsync();

            return invoices.Select(x =>
            {
                // Apply item filter client-side (after main query)
                var details = req.ItemIds?.Count > 0
                    ? x.Details!.Where(d => req.ItemIds.Contains(d.ItemId)).ToList()
                    : x.Details!.ToList();

                return new SalesRegisterDetailsRowDto
                {
                    InvoiceNo = x.InvoiceNo,
                    InvoiceDate = x.InvoiceDate.ToString("dd MMM yy"),
                    PartyName = x.BusinessPartner!.BusinessPartnerName,
                    //GstNo = x.BusinessPartner.GSTNo,
                    GstType = x.BillStateCode == x.SalesStateCode ? "B2B" : "IGST",
                    SalesPerson = x.SalesPerson?.SalesPersonName,
                    Location = x.Location?.LocationName,
                    Status = x.Status,
                    SubTotal = x.SubTotal,
                    Discount = details.Sum(d => d.DiscountAmount + d.AddisDiscountAmount),
                    TaxableAmount = x.SubTotal,
                    TaxAmount = x.TaxAmount,
                    CessAmount = x.CessAmount,
                    NetTotal = x.NetTotal,
                    Remarks = x.Remarks,
                    Items = details.Select(d => new SalesRegisterItemLineDto
                    {
                        ItemName = d.Item?.ItemName ?? "",
                        ItemCode = d.Item?.ItemCode,
                        HsnCode = d.HsnCode,
                        Qty = d.Qty,
                        Rate = d.Rate,
                        DiscountAmt = d.DiscountAmount + d.AddisDiscountAmount,
                        TaxableAmount = d.TaxableAmount,
                        TaxAmount = d.LineTaxAmount - d.CessAmount,
                        CessAmount = d.CessAmount,
                        LineTotal = d.LineTotal
                    }).ToList()
                };
            }).ToList();
        }

        // ── 3. Item Wise Sales ────────────────────────────────
        private async Task<List<ItemWiseSalesRowDto>> BuildItemWiseAsync(
            IQueryable<SalesInvoiceMain> q, SalesReportRequestDto req)
        {
            var invoiceIds = await q.Select(x => x.InvoiceId).ToListAsync();

            IQueryable<SalesInvoiceDetail> detailQuery = _context.SalesInvoiceDetails
    .Where(d => invoiceIds.Contains(d.InvoiceId))
    .Include(d => d.Item);

            if (req.ItemIds?.Count > 0)
                detailQuery = detailQuery.Where(d => req.ItemIds.Contains(d.ItemId));


            return await detailQuery
                .GroupBy(d => new
                {
                    d.ItemId,
                    ItemName = d.Item!.ItemName,
                    ItemCode = d.Item.ItemCode,
                    d.HsnCode
                })
                .Select(g => new ItemWiseSalesRowDto
                {
                    ItemId = g.Key.ItemId,
                    ItemName = g.Key.ItemName,
                    ItemCode = g.Key.ItemCode,
                    HsnCode = g.Key.HsnCode,
                    InvoiceCount = g.Select(d => d.InvoiceId).Distinct().Count(),
                    TotalQty = g.Sum(d => d.Qty),
                    TotalDiscount = g.Sum(d => d.DiscountAmount + d.AddisDiscountAmount),
                    TotalTaxable = g.Sum(d => d.TaxableAmount),
                    TotalTax = g.Sum(d => d.LineTaxAmount - d.CessAmount),
                    TotalCess = g.Sum(d => d.CessAmount),
                    TotalNet = g.Sum(d => d.LineTotal)
                })
                .OrderByDescending(r => r.TotalNet)
                .ToListAsync();
        }

        // ── 4. Party Wise Sales ───────────────────────────────
        private async Task<List<PartyWiseSalesRowDto>> BuildPartyWiseAsync(
            IQueryable<SalesInvoiceMain> q)
        {
            return await q
                .Include(x => x.BusinessPartner)
                .GroupBy(x => new
                {
                    x.BusinessPartnerId,
                    PartyName = x.BusinessPartner!.BusinessPartnerName,
                    //GstNo = x.BusinessPartner.GSTNo
                })
                .Select(g => new PartyWiseSalesRowDto
                {
                    BusinessPartnerId = g.Key.BusinessPartnerId,
                    PartyName = g.Key.PartyName,
                    //GstNo = g.Key.GstNo,
                    GstType = g.Any(x => x.BillStateCode != x.SalesStateCode)
                                            ? "IGST" : "B2B",
                    InvoiceCount = g.Count(),
                    TotalSubTotal = g.Sum(x => x.SubTotal),
                    TotalDiscount = g.Sum(x =>
                                            x.Details!.Sum(d =>
                                                d.DiscountAmount + d.AddisDiscountAmount)),
                    TotalTax = g.Sum(x => x.TaxAmount),
                    TotalCess = g.Sum(x => x.CessAmount),
                    TotalNet = g.Sum(x => x.NetTotal)
                })
                .OrderByDescending(r => r.TotalNet)
                .ToListAsync();
        }

        // ── 5. Tax Wise Sales ─────────────────────────────────
        private async Task<List<TaxWiseSalesRowDto>> BuildTaxWiseAsync(
            IQueryable<SalesInvoiceMain> q)
        {
            var invoiceIds = await q.Select(x => x.InvoiceId).ToListAsync();

            return await _context.SalesInvoiceTaxDetails
                .Where(t => invoiceIds.Contains(t.InvoiceId))
                .Include(t => t.Tax)
                .GroupBy(t => new
                {
                    TaxName = t.Tax != null ? t.Tax.TaxName : "Unknown"
                })
                .Select(g => new TaxWiseSalesRowDto
                {
                    TaxName = g.Key.TaxName,
                    TaxableAmount = g.Sum(t => t.TaxableAmount),
                    IGSTAmount = g.Sum(t => t.IGSTAmount),
                    CGSTAmount = g.Sum(t => t.CGSTAmount),
                    SGSTAmount = g.Sum(t => t.SGSTAmount),
                    CessAmount = g.Sum(t => t.CessAmount),
                    TotalTax = g.Sum(t => t.TotalTaxAmount),
                    NetAmount = g.Sum(t => t.TaxableAmount + t.TotalTaxAmount)
                })
                .OrderBy(r => r.TaxName)
                .ToListAsync();
        }

        // ── 6. Monthly Summary ────────────────────────────────
        private async Task<List<MonthlySummaryRowDto>> BuildMonthlySummaryAsync(
            IQueryable<SalesInvoiceMain> q)
        {
            var rows = await q
                .Include(x => x.Details)
                .GroupBy(x => new { x.InvoiceDate.Year, x.InvoiceDate.Month })
                .Select(g => new MonthlySummaryRowDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    InvoiceCount = g.Count(),
                    SubTotal = g.Sum(x => x.SubTotal),
                    Discount = g.Sum(x =>
                                       x.Details!.Sum(d =>
                                           d.DiscountAmount + d.AddisDiscountAmount)),
                    TaxAmount = g.Sum(x => x.TaxAmount),
                    CessAmount = g.Sum(x => x.CessAmount),
                    NetTotal = g.Sum(x => x.NetTotal)
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToListAsync();

            // Set human-readable label after query (avoids EF translation issue)
            foreach (var r in rows)
                r.MonthLabel = new DateTime(r.Year, r.Month, 1)
                    .ToString("MMM yyyy");

            return rows;
        }

        // ── 7. Monthly GST Summary ────────────────────────────
        private async Task<List<MonthlyGSTRowDto>> BuildMonthlyGSTAsync(
            IQueryable<SalesInvoiceMain> q)
        {
            var invoiceIds = await q.Select(x => x.InvoiceId).ToListAsync();

            // Join TaxDetails back to invoice date
            var rows = await _context.SalesInvoiceTaxDetails
                .Where(t => invoiceIds.Contains(t.InvoiceId))
                .Join(_context.SalesInvoiceMains,
                      t => t.InvoiceId,
                      inv => inv.InvoiceId,
                      (t, inv) => new { t, inv })
                .GroupBy(x => new
                {
                    x.inv.InvoiceDate.Year,
                    x.inv.InvoiceDate.Month,
                    // B2B = intra-state, IGST = inter-state
                    GstType = x.inv.BillStateCode == x.inv.SalesStateCode ? "B2B" : "IGST"
                })
                .Select(g => new MonthlyGSTRowDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    GstType = g.Key.GstType,
                    TaxableAmount = g.Sum(x => x.t.TaxableAmount),
                    IGSTAmount = g.Sum(x => x.t.IGSTAmount),
                    CGSTAmount = g.Sum(x => x.t.CGSTAmount),
                    SGSTAmount = g.Sum(x => x.t.SGSTAmount),
                    CessAmount = g.Sum(x => x.t.CessAmount),
                    TotalTax = g.Sum(x => x.t.TotalTaxAmount),
                    NetAmount = g.Sum(x => x.t.TaxableAmount + x.t.TotalTaxAmount)
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month).ThenBy(r => r.GstType)
                .ToListAsync();

            foreach (var r in rows)
                r.MonthLabel = new DateTime(r.Year, r.Month, 1)
                    .ToString("MMM yyyy");

            return rows;
        }
    }
}
