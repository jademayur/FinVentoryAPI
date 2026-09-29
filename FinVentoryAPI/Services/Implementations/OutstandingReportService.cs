using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.OutstandingReportDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class OutstandingReportService : IOutstandingReportService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public OutstandingReportService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<OutstandingReportResponseDto> GenerateAsync(OutstandingReportRequestDto req)
        {
            var companyId = _common.GetCompanyId();
            var finYearId = _common.GetFinancialYearId();
            var today = DateTime.Today;

            List<InvoiceOutstandingVm> invoices;

            if (req.PartyType == "Customer")
            {
                invoices = await LoadCustomerInvoicesAsync(companyId, finYearId, req.BusinessPartnerIds);
            }
            else
            {
                invoices = await LoadSupplierInvoicesAsync(companyId, finYearId, req.BusinessPartnerIds);
            }

            var pendingInvoices = invoices.Where(x => x.PendingAmount > 0.0001m).ToList();

            var meta = new OutstandingReportMetaDto
            {
                TotalInvoices = pendingInvoices.Count,
                TotalBilled = pendingInvoices.Sum(x => x.NetTotal),
                TotalPaid = pendingInvoices.Sum(x => x.PaidAmount),
                TotalOutstanding = pendingInvoices.Sum(x => x.PendingAmount),
                TotalRecords = pendingInvoices.Count,
                PageNumber = req.PageNumber,
                PageSize = req.PageSize,
                TotalPages = (int)Math.Ceiling((double)pendingInvoices.Count / req.PageSize)
            };

            object data = req.ReportType switch
            {
                "Overall" => BuildOverall(pendingInvoices, today, req),
                "BillWise" => BuildBillWise(pendingInvoices, today, req),
                "Aging" => BuildAging(pendingInvoices, today, req),
                _ => throw new ArgumentException($"Unknown ReportType: {req.ReportType}")
            };

            return new OutstandingReportResponseDto
            {
                ReportType = req.ReportType,
                PartyType = req.PartyType,
                Meta = meta,
                Data = data
            };
        }

        public async Task<OutstandingReportFilterOptionsDto> GetFilterOptionsAsync(string partyType)
        {
            var companyId = _common.GetCompanyId();
            var finYearId = _common.GetFinancialYearId();

            List<int> bpIds;

            if (partyType == "Supplier")
            {
                bpIds = await _context.PurchaseInvoiceMains
                    .Where(x => x.CompanyId == companyId
                             && x.FinYearId == finYearId
                             && !x.IsDeleted
                             && x.Status != "Cancelled")
                    .Select(x => x.BusinessPartnerId)
                    .Distinct()
                    .ToListAsync();
            }
            else
            {
                bpIds = await _context.SalesInvoiceMains
                    .Where(x => x.CompanyId == companyId
                             && x.FinYearId == finYearId
                             && !x.IsDeleted
                             && x.Status != "Cancelled")
                    .Select(x => x.BusinessPartnerId)
                    .Distinct()
                    .ToListAsync();
            }

            var businessPartners = await _context.BusinessPartners
                .Where(x => bpIds.Contains(x.BusinessPartnerId) && !x.IsDeleted)
                .Select(x => new IdNameDto
                {
                    Id = x.BusinessPartnerId,
                    Name = x.BusinessPartnerName
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return new OutstandingReportFilterOptionsDto
            {
                BusinessPartners = businessPartners
            };
        }

        // ══════════════════════════════════════════════════════════════
        // DATA LOADING
        // ══════════════════════════════════════════════════════════════

        private async Task<List<InvoiceOutstandingVm>> LoadCustomerInvoicesAsync(
            int companyId, int finYearId, List<int>? bpIds)
        {
            var query = _context.SalesInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && x.FinYearId == finYearId
                         && !x.IsDeleted
                         && x.Status != "Cancelled");

            if (bpIds?.Count > 0)
                query = query.Where(x => bpIds.Contains(x.BusinessPartnerId));

            var invoices = await query
                .Include(x => x.BusinessPartner)
                .ToListAsync();

            var invoiceIds = invoices.Select(x => x.InvoiceId).ToList();

            var allocations = await _context.IncomingPaymentAllocations
                .Where(a => invoiceIds.Contains(a.InvoiceId)
                         && a.Payment != null
                         && !a.Payment.IsDeleted
                         && a.Payment.Status != "Cancelled")
                .ToListAsync();

            var grouped = allocations.GroupBy(a => a.InvoiceId)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.AmountApplied));

            return invoices.Select(inv => new InvoiceOutstandingVm
            {
                InvoiceId = inv.InvoiceId,
                BusinessPartnerId = inv.BusinessPartnerId,
                InvoiceNo = inv.InvoiceNo,
                SupplierInvoiceNo = null,
                InvoiceDate = inv.InvoiceDate,
                DueDate = inv.DueDate,
                PartyName = inv.BusinessPartner?.BusinessPartnerName ?? "",
                GSTNo = null,
                NetTotal = inv.NetTotal,
                PaidAmount = grouped.GetValueOrDefault(inv.InvoiceId, 0m),
                PendingAmount = inv.NetTotal - grouped.GetValueOrDefault(inv.InvoiceId, 0m)
            }).ToList();
        }

        private async Task<List<InvoiceOutstandingVm>> LoadSupplierInvoicesAsync(
            int companyId, int finYearId, List<int>? bpIds)
        {
            var query = _context.PurchaseInvoiceMains
                .Where(x => x.CompanyId == companyId
                         && x.FinYearId == finYearId
                         && !x.IsDeleted
                         && x.Status != "Cancelled");

            if (bpIds?.Count > 0)
                query = query.Where(x => bpIds.Contains(x.BusinessPartnerId));

            var invoices = await query
                .Include(x => x.BusinessPartner)
                .ToListAsync();

            var invoiceIds = invoices.Select(x => x.InvoiceId).ToList();

            var allocations = await _context.OutgoingPaymentAllocations
                .Where(a => invoiceIds.Contains(a.BillId)
                         && a.Payment != null
                         && !a.Payment.IsDeleted
                         && a.Payment.Status != "Cancelled")
                .ToListAsync();

            var grouped = allocations.GroupBy(a => a.BillId)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.AmountApplied));

            return invoices.Select(inv => new InvoiceOutstandingVm
            {
                InvoiceId = inv.InvoiceId,
                BusinessPartnerId = inv.BusinessPartnerId,
                InvoiceNo = inv.InvoiceNo,
                SupplierInvoiceNo = inv.SupplierInvoiceNo,
                InvoiceDate = inv.InvoiceDate,
                DueDate = inv.DueDate,
                PartyName = inv.BusinessPartner?.BusinessPartnerName ?? "",
                GSTNo = null,
                NetTotal = inv.NetTotal,
                PaidAmount = grouped.GetValueOrDefault(inv.InvoiceId, 0m),
                PendingAmount = inv.NetTotal - grouped.GetValueOrDefault(inv.InvoiceId, 0m)
            }).ToList();
        }

        // ══════════════════════════════════════════════════════════════
        // BUILDERS
        // ══════════════════════════════════════════════════════════════

        private List<OverallOutstandingRowDto> BuildOverall(
            List<InvoiceOutstandingVm> invoices, DateTime today,
            OutstandingReportRequestDto req)
        {
            var result = invoices
                .GroupBy(x => x.BusinessPartnerId)
                .Select(g =>
                {
                    var first = g.First();
                    return new OverallOutstandingRowDto
                    {
                        BusinessPartnerId = g.Key,
                        PartyName = first.PartyName,
                        GSTNo = first.GSTNo,
                        InvoiceCount = g.Count(),
                        TotalBilled = g.Sum(x => x.NetTotal),
                        TotalPaid = g.Sum(x => x.PaidAmount),
                        TotalPending = g.Sum(x => x.PendingAmount),
                        MaxOverdueDays = g.Max(x => GetDaysOverdue(x.DueDate, today))
                    };
                })
                .OrderByDescending(x => x.TotalPending)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();

            return result;
        }

        private List<BillWiseOutstandingRowDto> BuildBillWise(
            List<InvoiceOutstandingVm> invoices, DateTime today,
            OutstandingReportRequestDto req)
        {
            return invoices
                .Select(x => new BillWiseOutstandingRowDto
                {
                    InvoiceId = x.InvoiceId,
                    InvoiceNo = x.InvoiceNo,
                    SupplierInvoiceNo = x.SupplierInvoiceNo,
                    InvoiceDate = x.InvoiceDate.ToString("dd MMM yy"),
                    DueDate = x.DueDate.ToString("dd MMM yy"),
                    PartyName = x.PartyName,
                    GSTNo = x.GSTNo,
                    InvoiceTotal = x.NetTotal,
                    PaidAmount = x.PaidAmount,
                    PendingAmount = x.PendingAmount,
                    DaysOverdue = GetDaysOverdue(x.DueDate, today),
                    AgingBucket = GetAgingBucket(GetDaysOverdue(x.DueDate, today))
                })
                .OrderBy(x => x.PartyName)
                .ThenByDescending(x => x.DaysOverdue)
                .ThenByDescending(x => x.PendingAmount)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();
        }

        private List<AgingOutstandingRowDto> BuildAging(
            List<InvoiceOutstandingVm> invoices, DateTime today,
            OutstandingReportRequestDto req)
        {
            var totalPending = invoices.Sum(x => x.PendingAmount);

            var buckets = new List<AgingOutstandingRowDto>
            {
                new() { BucketName = "Current", BucketOrder = 0 },
                new() { BucketName = "1-30 Days", BucketOrder = 1 },
                new() { BucketName = "31-60 Days", BucketOrder = 2 },
                new() { BucketName = "61-90 Days", BucketOrder = 3 },
                new() { BucketName = "90+ Days", BucketOrder = 4 }
            };

            foreach (var inv in invoices)
            {
                var days = GetDaysOverdue(inv.DueDate, today);
                var bucket = GetAgingBucket(days);
                var target = buckets.First(b => b.BucketName == bucket);
                target.InvoiceCount++;
                target.TotalAmount += inv.PendingAmount;
            }

            foreach (var b in buckets)
            {
                b.Percentage = totalPending > 0
                    ? Math.Round(b.TotalAmount / totalPending * 100, 2)
                    : 0;
            }

            return buckets;
        }

        // ══════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════

        private static int GetDaysOverdue(DateTime dueDate, DateTime today)
        {
            return (today - dueDate).Days;
        }

        private static string GetAgingBucket(int daysOverdue)
        {
            if (daysOverdue <= 0) return "Current";
            if (daysOverdue <= 30) return "1-30 Days";
            if (daysOverdue <= 60) return "31-60 Days";
            if (daysOverdue <= 90) return "61-90 Days";
            return "90+ Days";
        }

        // ══════════════════════════════════════════════════════════════
        // VIEW MODEL
        // ══════════════════════════════════════════════════════════════

        private class InvoiceOutstandingVm
        {
            public int InvoiceId { get; set; }
            public int BusinessPartnerId { get; set; }
            public string InvoiceNo { get; set; } = "";
            public string? SupplierInvoiceNo { get; set; }
            public DateTime InvoiceDate { get; set; }
            public DateTime DueDate { get; set; }
            public string PartyName { get; set; } = "";
            public string? GSTNo { get; set; }
            public decimal NetTotal { get; set; }
            public decimal PaidAmount { get; set; }
            public decimal PendingAmount { get; set; }
        }
    }
}
