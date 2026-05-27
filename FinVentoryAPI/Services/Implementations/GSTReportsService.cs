using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.Gstr1DTOs;
using FinVentoryAPI.DTOs.Gstr3bDTOs;
using FinVentoryAPI.DTOs.Gstr9DTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class GSTReportsService : IGSTReportsService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        // ₹2.5 lakh threshold for B2CL classification
        private const decimal B2CLThreshold = 250000m;

        public GSTReportsService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════════════════════════
        // PRIVATE HELPER — resolve billing address from BPAddresses collection
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns the billing BusinessPartnerAddress for a sales invoice.
        /// Falls back to the default address, then any first address.
        /// </summary>
        private static BusinessPartnerAddress? GetBillingAddress(Entities.SalesInvoiceMain inv)
        {
            var addresses = inv.BusinessPartner?.BPAddresses;
            if (addresses == null || !addresses.Any()) return null;

            // Prefer explicit Billing type
            return addresses.FirstOrDefault(a => a.Type == AddressType.Billing)
                ?? addresses.FirstOrDefault(a => a.IsDefault)
                ?? addresses.FirstOrDefault();
        }

        /// <summary>
        /// Returns the billing BusinessPartnerAddress for a purchase invoice.
        /// </summary>
        private static BusinessPartnerAddress? GetBillingAddress(Entities.PurchaseInvoiceMain inv)
        {
            var addresses = inv.BusinessPartner?.BPAddresses;
            if (addresses == null || !addresses.Any()) return null;

            return addresses.FirstOrDefault(a => a.Type == AddressType.Billing)
                ?? addresses.FirstOrDefault(a => a.IsDefault)
                ?? addresses.FirstOrDefault();
        }

        /// <summary>
        /// Returns true when the business partner has a valid GSTIN on their billing address.
        /// </summary>
        private static bool IsRegisteredDealer(Entities.SalesInvoiceMain inv)
        {
            var addr = GetBillingAddress(inv);
            return addr != null && !string.IsNullOrWhiteSpace(addr.GSTNo);
        }

        /// <summary>
        /// Returns the GST state code (int) from the billing address.
        /// </summary>
        private static int? GetBillStateCode(Entities.SalesInvoiceMain inv)
        {
            var addr = GetBillingAddress(inv);
            return addr?.State.HasValue == true ? (int?)addr.State.Value : null;
        }

        private static int? GetBillStateCode(Entities.PurchaseInvoiceMain inv)
        {
            var addr = GetBillingAddress(inv);
            return addr?.State.HasValue == true ? (int?)addr.State.Value : null;
        }

        // ════════════════════════════════════════════════════════════════════
        // PUBLIC — Summary (full GSTR-3B)
        // ════════════════════════════════════════════════════════════════════
        public async Task<Gstr3bResponseDto> GetSummaryAsync(string taxPeriod)
        {
            var companyId = _common.GetCompanyId();
            var finYearId = _common.GetFinancialYearId();
            var (from, to) = ParseTaxPeriod(taxPeriod);

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId)
                ?? throw new Exception("Company not found.");

            // ── Fetch raw data ─────────────────────────────────────────────
            var salesMains = await _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && x.FinYearId == finYearId
                         && !x.IsDeleted
                         && x.InvoiceDate >= from
                         && x.InvoiceDate <= to)
                .Include(x => x.Details!)
                    .ThenInclude(d => d.TaxDetails)
                .Include(x => x.BusinessPartner!)
                    .ThenInclude(bp => bp.BPAddresses)   // ← load addresses
                .AsNoTracking()
                .ToListAsync();

            var purchaseMains = await _context.PurchaseInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && x.FinYearId == finYearId
                         && !x.IsDeleted
                         && x.InvoiceDate >= from
                         && x.InvoiceDate <= to)
                .Include(x => x.Details!)
                    .ThenInclude(d => d.TaxDetails)
                .Include(x => x.BusinessPartner!)
                    .ThenInclude(bp => bp.BPAddresses)   // ← load addresses
                .AsNoTracking()
                .ToListAsync();

            // ── Section 3.1 — Outward Supplies ────────────────────────────
            var sec31 = new Section31Dto();

            foreach (var inv in salesMains)
            {
                foreach (var detail in inv.Details ?? Enumerable.Empty<Entities.SalesInvoiceDetail>())
                {
                    var taxRow = detail.TaxDetails?.FirstOrDefault();
                    if (detail.LineTotal == 0 && detail.TaxableAmount == 0) continue;

                    sec31.TaxableSupplies.TaxableValue += detail.TaxableAmount;
                    sec31.TaxableSupplies.IGSTAmount += taxRow?.IGSTAmount ?? 0;
                    sec31.TaxableSupplies.CGSTAmount += taxRow?.CGSTAmount ?? 0;
                    sec31.TaxableSupplies.SGSTAmount += taxRow?.SGSTAmount ?? 0;
                    sec31.TaxableSupplies.CessAmount += taxRow?.CessAmount ?? 0;
                }
            }

            // ── Section 3.2 — Inter-state to unregistered ─────────────────
            var sec32 = new Section32Dto();
            foreach (var inv in salesMains)
            {
                // Use BPAddresses to resolve state code and GSTIN
                int? billState = GetBillStateCode(inv);
                bool isInterState = inv.SalesStateCode.HasValue
                                 && billState.HasValue
                                 && inv.SalesStateCode != billState;
                bool isUnregistered = !IsRegisteredDealer(inv);

                if (isInterState && isUnregistered)
                    sec32.UnregisteredPersons += inv.NetTotal;
            }

            // ── Section 4 — ITC ────────────────────────────────────────────
            var sec4 = new Section4Dto();
            foreach (var inv in purchaseMains)
            {
                foreach (var detail in inv.Details ?? Enumerable.Empty<Entities.PurchaseInvoiceDetail>())
                {
                    var taxRow = detail.TaxDetails?.FirstOrDefault();
                    if (taxRow == null) continue;

                    sec4.InwardITCOthers.IGSTAmount += taxRow.IGSTAmount;
                    sec4.InwardITCOthers.CGSTAmount += taxRow.CGSTAmount;
                    sec4.InwardITCOthers.SGSTAmount += taxRow.SGSTAmount;
                    sec4.InwardITCOthers.CessAmount += taxRow.CessAmount;
                }
            }

            sec4.NetITCAvailable = new ITCRowDto
            {
                IGSTAmount = sec4.InwardITCOthers.IGSTAmount + sec4.ImportOfGoods.IGSTAmount
                           + sec4.ImportOfServices.IGSTAmount + sec4.InwardReverseCharge.IGSTAmount
                           - sec4.Rule42And43.IGSTAmount - sec4.OtherReversal.IGSTAmount,

                CGSTAmount = sec4.InwardITCOthers.CGSTAmount + sec4.ImportOfGoods.CGSTAmount
                           + sec4.ImportOfServices.CGSTAmount + sec4.InwardReverseCharge.CGSTAmount
                           - sec4.Rule42And43.CGSTAmount - sec4.OtherReversal.CGSTAmount,

                SGSTAmount = sec4.InwardITCOthers.SGSTAmount + sec4.ImportOfGoods.SGSTAmount
                           + sec4.ImportOfServices.SGSTAmount + sec4.InwardReverseCharge.SGSTAmount
                           - sec4.Rule42And43.SGSTAmount - sec4.OtherReversal.SGSTAmount,

                CessAmount = sec4.InwardITCOthers.CessAmount + sec4.ImportOfGoods.CessAmount
                           + sec4.ImportOfServices.CessAmount + sec4.InwardReverseCharge.CessAmount
                           - sec4.Rule42And43.CessAmount - sec4.OtherReversal.CessAmount
            };

            // ── Section 5 — Exempt purchases ──────────────────────────────
            var sec5 = new Section5Dto();
            foreach (var inv in purchaseMains)
            {
                foreach (var detail in inv.Details ?? Enumerable.Empty<Entities.PurchaseInvoiceDetail>())
                {
                    var taxRow = detail.TaxDetails?.FirstOrDefault();
                    bool hasZeroTax = taxRow == null
                                   || (taxRow.IGSTAmount == 0 && taxRow.CGSTAmount == 0
                                       && taxRow.SGSTAmount == 0);
                    if (hasZeroTax)
                        sec5.ExemptPurchases += detail.TaxableAmount;
                }
            }

            // ── Tax Payable ────────────────────────────────────────────────
            var taxPayable = new TaxPayableSummaryDto
            {
                IGST = new TaxPayableRowDto
                {
                    TaxPayable = sec31.TaxableSupplies.IGSTAmount + sec31.ZeroRatedSupplies.IGSTAmount + sec31.ReverseCharge.IGSTAmount,
                    ITCAvailable = sec4.NetITCAvailable.IGSTAmount
                },
                CGST = new TaxPayableRowDto
                {
                    TaxPayable = sec31.TaxableSupplies.CGSTAmount + sec31.NilExemptSupplies.CGSTAmount,
                    ITCAvailable = sec4.NetITCAvailable.CGSTAmount
                },
                SGST = new TaxPayableRowDto
                {
                    TaxPayable = sec31.TaxableSupplies.SGSTAmount + sec31.NilExemptSupplies.SGSTAmount,
                    ITCAvailable = sec4.NetITCAvailable.SGSTAmount
                },
                Cess = new TaxPayableRowDto
                {
                    TaxPayable = sec31.TaxableSupplies.CessAmount,
                    ITCAvailable = sec4.NetITCAvailable.CessAmount
                }
            };

            return new Gstr3bResponseDto
            {
                TaxPeriod = taxPeriod,
                CompanyName = company.CompanyName,
                GSTIN = company.GSTNumber ?? string.Empty,
                OutwardSupplies = sec31,
                InterStateSupplies = sec32,
                EligibleITC = sec4,
                ExemptSupplies = sec5,
                InterestLateFee = new Section51Dto(),
                TaxPayable = taxPayable
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // PUBLIC — Sales Invoice drill-down (GSTR-3B)
        // ════════════════════════════════════════════════════════════════════
        public async Task<List<Gstr3bInvoiceListDto>> GetSalesInvoicesAsync(string taxPeriod)
        {
            var companyId = _common.GetCompanyId();
            var finYearId = _common.GetFinancialYearId();
            var (from, to) = ParseTaxPeriod(taxPeriod);

            var invoices = await _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && x.FinYearId == finYearId
                         && !x.IsDeleted
                         && x.InvoiceDate >= from
                         && x.InvoiceDate <= to)
                .Include(x => x.BusinessPartner!)
                    .ThenInclude(bp => bp.BPAddresses)   // ← load addresses
                .Include(x => x.Details!)
                    .ThenInclude(d => d.TaxDetails)
                .AsNoTracking()
                .OrderBy(x => x.InvoiceDate)
                .ToListAsync();

            return invoices.Select(inv =>
            {
                int? billState = GetBillStateCode(inv);
                bool isInterState = inv.SalesStateCode.HasValue
                                 && billState.HasValue
                                 && inv.SalesStateCode != billState;

                var billingAddr = GetBillingAddress(inv);

                decimal igst = SumSalesTax(inv, t => t.IGSTAmount);
                decimal cgst = SumSalesTax(inv, t => t.CGSTAmount);
                decimal sgst = SumSalesTax(inv, t => t.SGSTAmount);
                decimal cess = SumSalesTax(inv, t => t.CessAmount);

                return new Gstr3bInvoiceListDto
                {
                    InvoiceType = "Sales",
                    InvoiceId = inv.InvoiceId,
                    InvoiceNo = inv.InvoiceNo,
                    InvoiceDate = inv.InvoiceDate,
                    PartyName = inv.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                    PartyGstin = billingAddr?.GSTNo ?? string.Empty,
                    TaxableAmount = inv.SubTotal,
                    IGSTAmount = igst,
                    CGSTAmount = cgst,
                    SGSTAmount = sgst,
                    CessAmount = cess,
                    NetTotal = inv.NetTotal,
                    IsInterState = isInterState
                };
            }).ToList();
        }

        // ════════════════════════════════════════════════════════════════════
        // PUBLIC — Purchase Invoice drill-down (GSTR-3B)
        // ════════════════════════════════════════════════════════════════════
        public async Task<List<Gstr3bInvoiceListDto>> GetPurchaseInvoicesAsync(string taxPeriod)
        {
            var companyId = _common.GetCompanyId();
            var finYearId = _common.GetFinancialYearId();
            var (from, to) = ParseTaxPeriod(taxPeriod);

            var invoices = await _context.PurchaseInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && x.FinYearId == finYearId
                         && !x.IsDeleted
                         && x.InvoiceDate >= from
                         && x.InvoiceDate <= to)
                .Include(x => x.BusinessPartner!)
                    .ThenInclude(bp => bp.BPAddresses)   // ← load addresses
                .Include(x => x.Details!)
                    .ThenInclude(d => d.TaxDetails)
                .AsNoTracking()
                .OrderBy(x => x.InvoiceDate)
                .ToListAsync();

            return invoices.Select(inv =>
            {
                int? billState = GetBillStateCode(inv);
                bool isInterState = inv.PurchaseStateCode.HasValue
                                 && billState.HasValue
                                 && inv.PurchaseStateCode != billState;

                var billingAddr = GetBillingAddress(inv);

                decimal igst = SumPurchaseTax(inv, t => t.IGSTAmount);
                decimal cgst = SumPurchaseTax(inv, t => t.CGSTAmount);
                decimal sgst = SumPurchaseTax(inv, t => t.SGSTAmount);
                decimal cess = SumPurchaseTax(inv, t => t.CessAmount);

                return new Gstr3bInvoiceListDto
                {
                    InvoiceType = "Purchase",
                    InvoiceId = inv.InvoiceId,
                    InvoiceNo = inv.InvoiceNo,
                    InvoiceDate = inv.InvoiceDate,
                    PartyName = inv.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                    PartyGstin = billingAddr?.GSTNo ?? string.Empty,
                    TaxableAmount = inv.SubTotal,
                    IGSTAmount = igst,
                    CGSTAmount = cgst,
                    SGSTAmount = sgst,
                    CessAmount = cess,
                    NetTotal = inv.NetTotal,
                    IsInterState = isInterState
                };
            }).ToList();
        }

        // ════════════════════════════════════════════════════════════════════
        // GSTR-1 — Summary
        // ════════════════════════════════════════════════════════════════════
        public async Task<Gstr1ResponseDto> GetGstr1SummaryAsync(string taxPeriod)
        {
            var companyId = _common.GetCompanyId();
            var finYearId = _common.GetFinancialYearId();
            var (from, to) = ParseTaxPeriod(taxPeriod);

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId)
                ?? throw new Exception("Company not found.");

            var salesMains = await _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && x.FinYearId == finYearId
                         && !x.IsDeleted
                         && x.InvoiceDate >= from
                         && x.InvoiceDate <= to)
                .Include(x => x.BusinessPartner!)
                    .ThenInclude(bp => bp.BPAddresses)   // ← load addresses
                .Include(x => x.Details!)
                    .ThenInclude(d => d.TaxDetails)
                .AsNoTracking()
                .OrderBy(x => x.InvoiceDate)
                .ToListAsync();

            var b2b = new List<Gstr1B2BRowDto>();
            var b2cl = new List<Gstr1B2CLRowDto>();
            var b2csMap = new Dictionary<(int? state, bool inter), Gstr1B2CSRowDto>();
            var nilExempt = new Gstr1NilExemptDto();

            foreach (var inv in salesMains)
            {
                // ── Resolve billing address from BPAddresses collection ────
                var billingAddr = GetBillingAddress(inv);
                int? billState = billingAddr?.State.HasValue == true
                                  ? (int?)billingAddr.State.Value
                                  : null;

                bool isInterState = inv.SalesStateCode.HasValue
                                  && billState.HasValue
                                  && inv.SalesStateCode != billState;
                bool isRegistered = billingAddr != null
                                  && !string.IsNullOrWhiteSpace(billingAddr.GSTNo);

                decimal igst = SumSalesTax(inv, t => t.IGSTAmount);
                decimal cgst = SumSalesTax(inv, t => t.CGSTAmount);
                decimal sgst = SumSalesTax(inv, t => t.SGSTAmount);
                decimal cess = SumSalesTax(inv, t => t.CessAmount);
                bool hasTax = (igst + cgst + sgst + cess) > 0;

                int? pos = billState ?? inv.SalesStateCode;   // Place of Supply

                // ── Nil / Exempt ───────────────────────────────────────────
                if (!hasTax)
                {
                    nilExempt.ExemptValue += inv.SubTotal;
                    continue;
                }

                // ── B2B — registered buyer ─────────────────────────────────
                if (isRegistered)
                {
                    b2b.Add(new Gstr1B2BRowDto
                    {
                        InvoiceId = inv.InvoiceId,
                        InvoiceNo = inv.InvoiceNo,
                        InvoiceDate = inv.InvoiceDate,
                        RecipientGSTIN = billingAddr!.GSTNo!,
                        RecipientName = inv.BusinessPartner!.BusinessPartnerName,
                        PlaceOfSupply = pos,
                        IsInterState = isInterState,
                        IsReverseCharge = false,
                        InvoiceValue = inv.NetTotal,
                        TaxableValue = inv.SubTotal,
                        IGSTAmount = igst,
                        CGSTAmount = cgst,
                        SGSTAmount = sgst,
                        CessAmount = cess
                    });
                    continue;
                }

                // ── B2CL — inter-state unregistered, invoice > ₹2.5L ──────
                if (isInterState && inv.NetTotal > B2CLThreshold)
                {
                    b2cl.Add(new Gstr1B2CLRowDto
                    {
                        InvoiceId = inv.InvoiceId,

                        InvoiceNo = inv.InvoiceNo,
                        InvoiceDate = inv.InvoiceDate,
                        PartyName = inv.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                        PlaceOfSupply = pos,
                        InvoiceValue = inv.NetTotal,
                        TaxableValue = inv.SubTotal,
                        IGSTAmount = igst,
                        CessAmount = cess
                    });
                    continue;
                }

                // ── B2CS — aggregate by (state, isInterState) ──────────────
                var key = (pos, isInterState);
                if (!b2csMap.TryGetValue(key, out var b2csRow))
                {
                    b2csRow = new Gstr1B2CSRowDto
                    {
                        PlaceOfSupply = pos,
                        StateName = pos.HasValue ? ResolveStateName(pos.Value) : "Unknown",
                        IsInterState = isInterState
                    };
                    b2csMap[key] = b2csRow;
                }
                b2csRow.TaxableValue += inv.SubTotal;
                b2csRow.IGSTAmount += igst;
                b2csRow.CGSTAmount += cgst;
                b2csRow.SGSTAmount += sgst;
                b2csRow.CessAmount += cess;
            }

            var totals = new Gstr1TotalsDto
            {
                TotalInvoices = salesMains.Count,
                TotalInvoiceValue = salesMains.Sum(x => x.NetTotal),
                TotalTaxableValue = b2b.Sum(x => x.TaxableValue)
                                  + b2cl.Sum(x => x.TaxableValue)
                                  + b2csMap.Values.Sum(x => x.TaxableValue),
                TotalIGST = b2b.Sum(x => x.IGSTAmount)
                                  + b2cl.Sum(x => x.IGSTAmount)
                                  + b2csMap.Values.Sum(x => x.IGSTAmount),
                TotalCGST = b2b.Sum(x => x.CGSTAmount)
                                  + b2csMap.Values.Sum(x => x.CGSTAmount),
                TotalSGST = b2b.Sum(x => x.SGSTAmount)
                                  + b2csMap.Values.Sum(x => x.SGSTAmount),
                TotalCess = b2b.Sum(x => x.CessAmount)
                                  + b2cl.Sum(x => x.CessAmount)
                                  + b2csMap.Values.Sum(x => x.CessAmount)
            };

            return new Gstr1ResponseDto
            {
                TaxPeriod = taxPeriod,
                CompanyName = company.CompanyName,
                GSTIN = company.GSTNumber ?? string.Empty,
                B2BSupplies = b2b,
                B2CLSupplies = b2cl,
                B2CSSupplies = b2csMap.Values.OrderBy(x => x.PlaceOfSupply).ToList(),
                NilExempt = nilExempt,
                Totals = totals
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // GSTR-1 — B2B drill-down
        // ════════════════════════════════════════════════════════════════════
        public async Task<List<Gstr1B2BRowDto>> GetGstr1B2BAsync(string taxPeriod)
        {
            var companyId = _common.GetCompanyId();
            var finYearId = _common.GetFinancialYearId();
            var (from, to) = ParseTaxPeriod(taxPeriod);

            var invoices = await _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && x.FinYearId == finYearId
                         && !x.IsDeleted
                         && x.InvoiceDate >= from
                         && x.InvoiceDate <= to)
                .Include(x => x.BusinessPartner!)
                    .ThenInclude(bp => bp.BPAddresses)   // ← load addresses
                .Include(x => x.Details!)
                    .ThenInclude(d => d.TaxDetails)
                .AsNoTracking()
                .OrderBy(x => x.InvoiceDate)
                .ToListAsync();

            // Filter to registered dealers only (in memory — after Include)
            return invoices
                .Where(inv => IsRegisteredDealer(inv))
                .Select(inv =>
                {
                    var billingAddr = GetBillingAddress(inv);
                    int? billState = billingAddr?.State.HasValue == true
                                       ? (int?)billingAddr.State.Value : null;
                    bool isInterState = inv.SalesStateCode.HasValue
                                    && billState.HasValue
                                    && inv.SalesStateCode != billState;
                    int? pos = billState ?? inv.SalesStateCode;

                    return new Gstr1B2BRowDto
                    {
                        InvoiceId = inv.InvoiceId,
                        InvoiceNo = inv.InvoiceNo,
                        InvoiceDate = inv.InvoiceDate,
                        RecipientGSTIN = billingAddr!.GSTNo!,
                        RecipientName = inv.BusinessPartner!.BusinessPartnerName,
                        PlaceOfSupply = pos,
                        IsInterState = isInterState,
                        IsReverseCharge = false,
                        InvoiceValue = inv.NetTotal,
                        TaxableValue = inv.SubTotal,
                        IGSTAmount = SumSalesTax(inv, t => t.IGSTAmount),
                        CGSTAmount = SumSalesTax(inv, t => t.CGSTAmount),
                        SGSTAmount = SumSalesTax(inv, t => t.SGSTAmount),
                        CessAmount = SumSalesTax(inv, t => t.CessAmount)
                    };
                })
                .ToList();
        }

        // ════════════════════════════════════════════════════════════════════
        // GSTR-9 — Annual Return Summary
        // Covers the full financial year derived from FinancialYear table
        // ════════════════════════════════════════════════════════════════════
        public async Task<Gstr9ResponseDto> GetGstr9SummaryAsync(int year)
        {
            var companyId = _common.GetCompanyId();
            var (from, to, fyLabel) = GetFinancialYearRange(year);

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId)
                ?? throw new Exception("Company not found.");

            // ── Fetch all sales & purchases for the full financial year ───
            var salesMains = await _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && !x.IsDeleted
                         && x.InvoiceDate >= from
                         && x.InvoiceDate <= to)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails)
                .AsNoTracking()
                .ToListAsync();

            var purchaseMains = await _context.PurchaseInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && !x.IsDeleted
                         && x.InvoiceDate >= from
                         && x.InvoiceDate <= to)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails)
                .AsNoTracking()
                .ToListAsync();

            // ════════════════════════════════════════════════════════════════
            // PART II — Outward & Inward Supplies
            // ════════════════════════════════════════════════════════════════
            var part2 = new Gstr9Part2Dto();

            foreach (var inv in salesMains)
            {
                bool isRegistered = !string.IsNullOrWhiteSpace(inv.BillAddress?.GSTNo);

                decimal igst = SumSalesTax(inv, t => t.IGSTAmount);
                decimal cgst = SumSalesTax(inv, t => t.CGSTAmount);
                decimal sgst = SumSalesTax(inv, t => t.SGSTAmount);
                decimal cess = SumSalesTax(inv, t => t.CessAmount);
                bool hasTax = (igst + cgst + sgst + cess) > 0;

                if (!hasTax)
                {
                    // Nil / Exempt — no tax
                    part2.ExemptSupplies += inv.SubTotal;
                    continue;
                }

                if (isRegistered)
                {
                    // 4A — B2B registered
                    part2.B2BSupplies.TaxableValue += inv.SubTotal;
                    part2.B2BSupplies.IGSTAmount += igst;
                    part2.B2BSupplies.CGSTAmount += cgst;
                    part2.B2BSupplies.SGSTAmount += sgst;
                    part2.B2BSupplies.CessAmount += cess;
                }
                else
                {
                    // 4B — B2C unregistered
                    part2.B2CSupplies.TaxableValue += inv.SubTotal;
                    part2.B2CSupplies.IGSTAmount += igst;
                    part2.B2CSupplies.CGSTAmount += cgst;
                    part2.B2CSupplies.SGSTAmount += sgst;
                    part2.B2CSupplies.CessAmount += cess;
                }
            }

            foreach (var inv in purchaseMains)
            {
                decimal igst = SumPurchaseTax(inv, t => t.IGSTAmount);
                decimal cgst = SumPurchaseTax(inv, t => t.CGSTAmount);
                decimal sgst = SumPurchaseTax(inv, t => t.SGSTAmount);
                decimal cess = SumPurchaseTax(inv, t => t.CessAmount);

                part2.InwardSupplies.TaxableValue += inv.SubTotal;
                part2.InwardSupplies.IGSTAmount += igst;
                part2.InwardSupplies.CGSTAmount += cgst;
                part2.InwardSupplies.SGSTAmount += sgst;
                part2.InwardSupplies.CessAmount += cess;
            }

            // ════════════════════════════════════════════════════════════════
            // PART III — ITC Availed
            // ════════════════════════════════════════════════════════════════
            var part3 = new Gstr9Part3Dto();

            foreach (var inv in purchaseMains)
            {
                foreach (var detail in inv.Details ?? Enumerable.Empty<Entities.PurchaseInvoiceDetail>())
                {
                    var taxRow = detail.TaxDetails?.FirstOrDefault();
                    if (taxRow == null) continue;

                    // 6E — All other ITC (domestic purchases)
                    part3.OtherITC.IGSTAmount += taxRow.IGSTAmount;
                    part3.OtherITC.CGSTAmount += taxRow.CGSTAmount;
                    part3.OtherITC.SGSTAmount += taxRow.SGSTAmount;
                    part3.OtherITC.CessAmount += taxRow.CessAmount;
                }
            }

            // 6J — Net ITC = all availed – reversed (no reversals in current scope)
            part3.NetITCAvailable = new ITCAmountDto
            {
                IGSTAmount = part3.OtherITC.IGSTAmount + part3.ImportOfGoods.IGSTAmount
                           + part3.ImportOfServices.IGSTAmount + part3.ReverseChargeReg.IGSTAmount
                           - part3.ITCReversed.IGSTAmount,
                CGSTAmount = part3.OtherITC.CGSTAmount + part3.ImportOfGoods.CGSTAmount
                           + part3.ImportOfServices.CGSTAmount + part3.ReverseChargeReg.CGSTAmount
                           - part3.ITCReversed.CGSTAmount,
                SGSTAmount = part3.OtherITC.SGSTAmount + part3.ImportOfGoods.SGSTAmount
                           + part3.ImportOfServices.SGSTAmount + part3.ReverseChargeReg.SGSTAmount
                           - part3.ITCReversed.SGSTAmount,
                CessAmount = part3.OtherITC.CessAmount + part3.ImportOfGoods.CessAmount
                           + part3.ImportOfServices.CessAmount + part3.ReverseChargeReg.CessAmount
                           - part3.ITCReversed.CessAmount
            };

            // ════════════════════════════════════════════════════════════════
            // PART IV — Tax Paid (9)
            // Tax payable = outward tax liability
            // Paid via ITC = net ITC available (up to payable)
            // Paid via Cash = remaining after ITC
            // ════════════════════════════════════════════════════════════════
            decimal totalOutwardIGST = part2.B2BSupplies.IGSTAmount + part2.B2CSupplies.IGSTAmount
                                     + part2.ZeroRated.IGSTAmount;
            decimal totalOutwardCGST = part2.B2BSupplies.CGSTAmount + part2.B2CSupplies.CGSTAmount;
            decimal totalOutwardSGST = part2.B2BSupplies.SGSTAmount + part2.B2CSupplies.SGSTAmount;
            decimal totalOutwardCess = part2.B2BSupplies.CessAmount + part2.B2CSupplies.CessAmount
                                     + part2.ZeroRated.CessAmount;

            var part4 = new Gstr9Part4Dto
            {
                IGST = BuildTaxPaidRow(totalOutwardIGST, part3.NetITCAvailable.IGSTAmount),
                CGST = BuildTaxPaidRow(totalOutwardCGST, part3.NetITCAvailable.CGSTAmount),
                SGST = BuildTaxPaidRow(totalOutwardSGST, part3.NetITCAvailable.SGSTAmount),
                Cess = BuildTaxPaidRow(totalOutwardCess, part3.NetITCAvailable.CessAmount)
            };

            // ════════════════════════════════════════════════════════════════
            // TOTALS
            // ════════════════════════════════════════════════════════════════
            var totals = new Gstr9TotalsDto
            {
                TotalSalesInvoices = salesMains.Count,
                TotalPurchaseInvoices = purchaseMains.Count,
                TotalOutwardSupplies = part2.B2BSupplies.TaxableValue
                                      + part2.B2CSupplies.TaxableValue
                                      + part2.ZeroRated.TaxableValue
                                      + part2.ExemptSupplies
                                      + part2.NilRatedSupplies
                                      + part2.NonGstSupplies,
                TotalInwardSupplies = part2.InwardSupplies.TaxableValue,
                TotalITCAvailed = part3.NetITCAvailable.TotalITC,
                TotalTaxPayable = totalOutwardIGST + totalOutwardCGST
                                      + totalOutwardSGST + totalOutwardCess,
                TotalTaxPaidViaITC = part4.IGST.PaidViaITC + part4.CGST.PaidViaITC
                                      + part4.SGST.PaidViaITC + part4.Cess.PaidViaITC,
                TotalTaxPaidViaCash = part4.IGST.PaidViaCash + part4.CGST.PaidViaCash
                                      + part4.SGST.PaidViaCash + part4.Cess.PaidViaCash,
                TotalIGST = totalOutwardIGST,
                TotalCGST = totalOutwardCGST,
                TotalSGST = totalOutwardSGST,
                TotalCess = totalOutwardCess
            };

            // ── Monthly breakdown ─────────────────────────────────────────
            var monthly = await GetGstr9MonthlyBreakdownAsync(year);

            return new Gstr9ResponseDto
            {
                FinancialYear = fyLabel,
                CompanyName = company.CompanyName,
                GSTIN = company.GSTNumber ?? string.Empty,
                OutwardInwardSupplies = part2,
                ITCAvailed = part3,
                TaxPaid = part4,
                PreviousFYTransactions = new Gstr9Part5Dto(),   // filled manually
                Totals = totals,
                MonthlyBreakdown = monthly
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // GSTR-9 — Monthly Breakdown drill-down
        // ════════════════════════════════════════════════════════════════════
        public async Task<List<Gstr9MonthlyBreakdownDto>> GetGstr9MonthlyBreakdownAsync(int year)
        {
            var companyId = _common.GetCompanyId();
            var (from, to, _) = GetFinancialYearRange(year);

            var salesMains = await _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && !x.IsDeleted
                         && x.InvoiceDate >= from
                         && x.InvoiceDate <= to)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails)
                .AsNoTracking()
                .ToListAsync();

            // Group by calendar month, ordered April→March (Indian FY)
            return salesMains
                .GroupBy(x => new { x.InvoiceDate.Year, x.InvoiceDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g =>
                {
                    decimal igst = g.Sum(inv => SumSalesTax(inv, t => t.IGSTAmount));
                    decimal cgst = g.Sum(inv => SumSalesTax(inv, t => t.CGSTAmount));
                    decimal sgst = g.Sum(inv => SumSalesTax(inv, t => t.SGSTAmount));
                    decimal cess = g.Sum(inv => SumSalesTax(inv, t => t.CessAmount));

                    return new Gstr9MonthlyBreakdownDto
                    {
                        Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM-yyyy}",
                        TaxableValue = g.Sum(x => x.SubTotal),
                        IGSTAmount = igst,
                        CGSTAmount = cgst,
                        SGSTAmount = sgst,
                        CessAmount = cess,
                        NetTotal = g.Sum(x => x.NetTotal),
                        InvoiceCount = g.Count()
                    };
                })
                .ToList();
        }


        // ════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════

        private static decimal SumSalesTax(
            Entities.SalesInvoiceMain inv,
            Func<Entities.SalesInvoiceTaxDetail, decimal> selector)
            => inv.Details?
                  .SelectMany(d => d.TaxDetails
                               ?? Enumerable.Empty<Entities.SalesInvoiceTaxDetail>())
                  .Sum(selector) ?? 0;

        private static decimal SumPurchaseTax(
            Entities.PurchaseInvoiceMain inv,
            Func<Entities.PurchaseInvoiceTaxDetail, decimal> selector)
            => inv.Details?
                  .SelectMany(d => d.TaxDetails
                               ?? Enumerable.Empty<Entities.PurchaseInvoiceTaxDetail>())
                  .Sum(selector) ?? 0;

        private static string ResolveStateName(int stateCode)
            => Enum.IsDefined(typeof(GstState), stateCode)
               ? ((GstState)stateCode).ToString().Replace("_", " ")
               : stateCode.ToString();

        private static (DateTime from, DateTime to, string label) GetFinancialYearRange(int year)
        {
            var from = new DateTime(year, 4, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(year + 1, 3, 31, 23, 59, 59, DateTimeKind.Utc);
            var label = $"{year}-{(year + 1) % 100:D2}";
            return (from, to, label);
        }

        /// <summary>
        /// Splits tax payable into ITC portion and cash portion.
        /// ITC used = min(taxPayable, itcAvailable); Cash = remainder.
        /// </summary>
        private static TaxPaidRowDto BuildTaxPaidRow(decimal taxPayable, decimal itcAvailable)
        {
            decimal paidViaITC = Math.Min(taxPayable, itcAvailable);
            decimal paidViaCash = Math.Max(0, taxPayable - itcAvailable);
            return new TaxPaidRowDto
            {
                TaxPayable = taxPayable,
                PaidViaITC = paidViaITC,
                PaidViaCash = paidViaCash
            };
        }

        private static (DateTime from, DateTime to) ParseTaxPeriod(string taxPeriod)
        {
            if (string.IsNullOrWhiteSpace(taxPeriod))
                throw new ArgumentException("TaxPeriod is required. Format: MM-YYYY");

            var parts = taxPeriod.Split('-');
            if (parts.Length != 2
                || !int.TryParse(parts[0], out int month)
                || !int.TryParse(parts[1], out int year)
                || month < 1 || month > 12)
                throw new ArgumentException($"Invalid TaxPeriod '{taxPeriod}'. Expected MM-YYYY.");

            var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(1).AddTicks(-1);
            return (from, to);
        }
    }
}  