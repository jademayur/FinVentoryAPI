using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PurchaseDocumentFlowDTOs;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class PurchaseDocumentFlowService : IPurchaseDocumentFlowService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public PurchaseDocumentFlowService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<PurchaseDocumentFlowResponseDto> GetFlowAsync(PurchaseDocumentFlowRequestDto req)
        {
            var companyId = _common.GetCompanyId();

            // 1. Get all purchase orders
            var query = _context.PurchaseOrderMains
                .AsNoTracking()
                .Where(o => o.CompanyId == companyId && !o.IsDeleted);

            if (req.FromDate.HasValue)
                query = query.Where(o => o.OrderDate >= req.FromDate.Value);
            if (req.ToDate.HasValue)
                query = query.Where(o => o.OrderDate <= req.ToDate.Value.AddDays(1).AddTicks(-1));
            if (req.BusinessPartnerId.HasValue)
                query = query.Where(o => o.BusinessPartnerId == req.BusinessPartnerId.Value);

            var orders = await query.ToListAsync();

            if (!orders.Any())
                return EmptyResponse(req);

            var orderIds = orders.Select(o => o.OrderId).ToList();

            // 2. Get GRN details linked to these orders
            var grnDetails = await _context.GRNDetails
                .AsNoTracking()
                .Where(d => d.PurchaseOrderId.HasValue && orderIds.Contains(d.PurchaseOrderId!.Value))
                .ToListAsync();

            var grnIds = grnDetails.Select(d => d.GRNId).Distinct().ToList();
            var orderToGrnDetails = grnDetails.GroupBy(d => d.PurchaseOrderId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 3. Get GRN mains
            var grns = await _context.GRNMains
                .AsNoTracking()
                .Where(g => grnIds.Contains(g.GRNId) && g.CompanyId == companyId && !g.IsDeleted)
                .ToDictionaryAsync(g => g.GRNId);

            var grnDetailIds = grnDetails.Select(d => d.GRNDetailId).ToList();

            // 4. Get invoice details linked to GRN
            var invoiceDetails = await _context.PurchaseInvoiceDetails
                .AsNoTracking()
                .Where(d => d.GRNId.HasValue && grnIds.Contains(d.GRNId!.Value))
                .ToListAsync();

            var invoiceIds = invoiceDetails.Select(d => d.InvoiceId).Distinct().ToList();
            var grnDetailToInvoiceDetails = invoiceDetails.GroupBy(d => d.GRNId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 5. Get invoice mains
            var invoices = await _context.PurchaseInvoiceMains
                .AsNoTracking()
                .Where(i => invoiceIds.Contains(i.InvoiceId) && i.CompanyId == companyId && !i.IsDeleted)
                .ToDictionaryAsync(i => i.InvoiceId);

            // 6. Get payment allocations
            var paymentAllocations = await _context.OutgoingPaymentAllocations
                .AsNoTracking()
                .Where(a => invoiceIds.Contains(a.BillId))
                .ToListAsync();

            var paymentIds = paymentAllocations.Select(a => a.PaymentId).Distinct().ToList();

            var payments = await _context.OutgoingPaymentMains
                .AsNoTracking()
                .Where(p => paymentIds.Contains(p.PaymentId) && p.CompanyId == companyId && !p.IsDeleted)
                .ToDictionaryAsync(p => p.PaymentId);

            // 7. Get business partners
            var bpIds = orders.Select(o => o.BusinessPartnerId).Distinct().ToList();
            var bps = await _context.BusinessPartners
                .AsNoTracking()
                .Where(bp => bpIds.Contains(bp.BusinessPartnerId))
                .ToDictionaryAsync(bp => bp.BusinessPartnerId, bp => bp.BusinessPartnerName);

            // 8. Build rows
            var rows = new List<PurchaseDocumentFlowRowDto>();

            foreach (var o in orders)
            {
                var oGrnDetails = orderToGrnDetails.GetValueOrDefault(o.OrderId, new());
                var linkedGrnIds = oGrnDetails.Select(d => d.GRNId).Distinct().ToList();
                var linkedGrns = linkedGrnIds
                    .Select(id => grns.GetValueOrDefault(id))
                    .Where(g => g != null)
                    .ToList();
                var grnAmount = linkedGrns.Sum(g => g!.NetTotal);
                var grnNos = string.Join(", ", linkedGrns.Select(g => g!.GRNNo));
                var grnStatuses = string.Join(", ", linkedGrns.Select(g => g!.Status).Distinct());

                // Invoice tracking
                var allInvoiceDetails = linkedGrnIds
                    .SelectMany(grnId => grnDetailToInvoiceDetails.GetValueOrDefault(grnId, new()))
                    .ToList();
                var linkedInvoiceIds = allInvoiceDetails.Select(d => d.InvoiceId).Distinct().ToList();
                var linkedInvoices = linkedInvoiceIds
                    .Select(id => invoices.GetValueOrDefault(id))
                    .Where(i => i != null)
                    .ToList();
                var invoiceAmount = linkedInvoices.Sum(i => i!.NetTotal);
                var invoiceNos = string.Join(", ", linkedInvoices.Select(i => i!.InvoiceNo));
                var invoiceStatuses = string.Join(", ", linkedInvoices.Select(i => i!.Status).Distinct());

                // Payment tracking
                var paymentAmount = paymentAllocations
                    .Where(a => linkedInvoiceIds.Contains(a.BillId))
                    .Sum(a => a.AmountApplied);

                var outstanding = invoiceAmount - paymentAmount;

                var stage = DetermineStage(o.Status, linkedGrns.Any(), linkedInvoices.Any(), paymentAmount, invoiceAmount);

                rows.Add(new PurchaseDocumentFlowRowDto
                {
                    OrderId = o.OrderId,
                    OrderNo = o.OrderNo,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.Status,
                    OrderAmount = o.NetTotal,
                    BusinessPartnerId = o.BusinessPartnerId,
                    SupplierName = bps.GetValueOrDefault(o.BusinessPartnerId, ""),
                    GrnCount = linkedGrns.Count,
                    GrnNos = string.IsNullOrEmpty(grnNos) ? null : grnNos,
                    GrnStatuses = string.IsNullOrEmpty(grnStatuses) ? null : grnStatuses,
                    GrnAmount = grnAmount,
                    InvoiceCount = linkedInvoices.Count,
                    InvoiceNos = string.IsNullOrEmpty(invoiceNos) ? null : invoiceNos,
                    InvoiceStatuses = string.IsNullOrEmpty(invoiceStatuses) ? null : invoiceStatuses,
                    InvoiceAmount = invoiceAmount,
                    PaymentAmount = paymentAmount,
                    OutstandingAmount = outstanding,
                    CurrentStage = stage,
                });
            }

            // 9. Apply stage filter
            if (!string.IsNullOrEmpty(req.CurrentStage))
                rows = rows.Where(r => r.CurrentStage == req.CurrentStage).ToList();

            // 10. Apply order status filter
            if (!string.IsNullOrEmpty(req.OrderStatus))
                rows = rows.Where(r => r.OrderStatus == req.OrderStatus).ToList();

            // 11. Sort
            rows = req.SortBy?.ToLower() switch
            {
                "orderno" => req.SortDirection == "asc" ? rows.OrderBy(r => r.OrderNo).ToList() : rows.OrderByDescending(r => r.OrderNo).ToList(),
                "suppliername" => req.SortDirection == "asc" ? rows.OrderBy(r => r.SupplierName).ToList() : rows.OrderByDescending(r => r.SupplierName).ToList(),
                "orderamount" => req.SortDirection == "asc" ? rows.OrderBy(r => r.OrderAmount).ToList() : rows.OrderByDescending(r => r.OrderAmount).ToList(),
                "grnamount" => req.SortDirection == "asc" ? rows.OrderBy(r => r.GrnAmount).ToList() : rows.OrderByDescending(r => r.GrnAmount).ToList(),
                "invoiceamount" => req.SortDirection == "asc" ? rows.OrderBy(r => r.InvoiceAmount).ToList() : rows.OrderByDescending(r => r.InvoiceAmount).ToList(),
                "outstandingamount" => req.SortDirection == "asc" ? rows.OrderBy(r => r.OutstandingAmount).ToList() : rows.OrderByDescending(r => r.OutstandingAmount).ToList(),
                "currentstage" => req.SortDirection == "asc" ? rows.OrderBy(r => r.CurrentStage).ToList() : rows.OrderByDescending(r => r.CurrentStage).ToList(),
                _ => req.SortDirection == "asc" ? rows.OrderBy(r => r.OrderDate).ToList() : rows.OrderByDescending(r => r.OrderDate).ToList(),
            };

            // 12. Pagination
            var totalRecords = rows.Count;
            var paged = rows
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();

            return new PurchaseDocumentFlowResponseDto
            {
                Data = paged,
                TotalRecords = totalRecords,
                PageNumber = req.PageNumber,
                PageSize = req.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / req.PageSize),
                TotalOrderAmount = rows.Sum(r => r.OrderAmount),
                TotalGrnAmount = rows.Sum(r => r.GrnAmount),
                TotalInvoiceAmount = rows.Sum(r => r.InvoiceAmount),
                TotalPaymentAmount = rows.Sum(r => r.PaymentAmount),
                TotalOutstanding = rows.Sum(r => r.OutstandingAmount),
            };
        }

        private static string DetermineStage(string orderStatus, bool hasGrns, bool hasInvoices, decimal paymentAmount, decimal invoiceAmount)
        {
            if (orderStatus == "Cancelled")
                return "Cancelled";

            if (hasInvoices)
            {
                if (invoiceAmount > 0 && paymentAmount >= invoiceAmount)
                    return "Paid";
                if (paymentAmount > 0)
                    return "Partial Payment";
                return "Invoice";
            }
            if (hasGrns) return "GRN";
            return "Order";
        }

        private static PurchaseDocumentFlowResponseDto EmptyResponse(PurchaseDocumentFlowRequestDto req) => new()
        {
            Data = new(),
            TotalRecords = 0,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = 0,
        };
    }
}
