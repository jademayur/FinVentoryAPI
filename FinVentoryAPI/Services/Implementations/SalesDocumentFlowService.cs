using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.SalesPipelineDTOs;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class SalesDocumentFlowService : ISalesDocumentFlowService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public SalesDocumentFlowService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<SalesDocumentFlowResponseDto> GetPipelineAsync(SalesDocumentFlowRequestDto req)
        {
            var companyId = _common.GetCompanyId();

            // 1. Get all quotations
            var query = _context.SalesQuotationMains
                .AsNoTracking()
                .Where(q => q.CompanyId == companyId && !q.IsDeleted);

            if (req.FromDate.HasValue)
                query = query.Where(q => q.QuotationDate >= req.FromDate.Value);
            if (req.ToDate.HasValue)
                query = query.Where(q => q.QuotationDate <= req.ToDate.Value.AddDays(1).AddTicks(-1));
            if (req.BusinessPartnerId.HasValue)
                query = query.Where(q => q.BusinessPartnerId == req.BusinessPartnerId.Value);

            var quotations = await query.ToListAsync();

            if (!quotations.Any())
                return EmptyResponse(req);

            var quotationIds = quotations.Select(q => q.QuotationId).ToList();

            // 2. Get all linked orders
            var orders = await _context.SalesOrderMains
                .AsNoTracking()
                .Where(o => o.CompanyId == companyId && !o.IsDeleted && o.QuotationId != null && quotationIds.Contains(o.QuotationId!.Value))
                .ToListAsync();

            var orderIds = orders.Select(o => o.OrderId).ToList();
            var quotationToOrders = orders.GroupBy(o => o.QuotationId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 3. Get all delivery details linked to these orders
            var deliveryDetails = await _context.GoodsDeliveryDetails
                .AsNoTracking()
                .Where(d => orderIds.Contains(d.OrderId))
                .ToListAsync();

            var deliveryIds = deliveryDetails.Select(d => d.DeliveryId).Distinct().ToList();
            var orderToDeliveryDetails = deliveryDetails.GroupBy(d => d.OrderId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 4. Get delivery mains
            var deliveries = await _context.GoodsDeliveryMains
                .AsNoTracking()
                .Where(d => deliveryIds.Contains(d.DeliveryId) && d.CompanyId == companyId && !d.IsDeleted)
                .ToDictionaryAsync(d => d.DeliveryId);

            var deliveryDetailIds = deliveryDetails.Select(d => d.DeliveryDetailId).ToList();

            // 5. Get invoice details linked to delivery details
            var invoiceDetails = await _context.SalesInvoiceDetails
                .AsNoTracking()
                .Where(d => d.DeliveryDetailId.HasValue && deliveryDetailIds.Contains(d.DeliveryDetailId!.Value))
                .ToListAsync();

            var invoiceIds = invoiceDetails.Select(d => d.InvoiceId).Distinct().ToList();
            var deliveryDetailToInvoiceDetails = invoiceDetails.GroupBy(d => d.DeliveryDetailId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 6. Get invoice mains
            var invoices = await _context.SalesInvoiceMains
                .AsNoTracking()
                .Where(i => invoiceIds.Contains(i.InvoiceId) && i.CompanyId == companyId && !i.IsDeleted)
                .ToDictionaryAsync(i => i.InvoiceId);

            // 7. Get payment allocations
            var paymentAllocations = await _context.IncomingPaymentAllocations
                .AsNoTracking()
                .Where(a => invoiceIds.Contains(a.InvoiceId))
                .ToListAsync();

            var paymentIds = paymentAllocations.Select(a => a.PaymentId).Distinct().ToList();

            var payments = await _context.IncomingPaymentMains
                .AsNoTracking()
                .Where(p => paymentIds.Contains(p.PaymentId) && p.CompanyId == companyId && !p.IsDeleted)
                .ToDictionaryAsync(p => p.PaymentId);

            // 8. Get business partners
            var bpIds = quotations.Select(q => q.BusinessPartnerId).Distinct().ToList();
            var bps = await _context.BusinessPartners
                .AsNoTracking()
                .Where(bp => bpIds.Contains(bp.BusinessPartnerId))
                .ToDictionaryAsync(bp => bp.BusinessPartnerId, bp => bp.BusinessPartnerName);

            // 9. Build rows
            var rows = new List<SalesDocumentFlowRowDto>();

            foreach (var q in quotations)
            {
                var qOrders = quotationToOrders.GetValueOrDefault(q.QuotationId, new());
                var orderAmount = qOrders.Sum(o => o.NetTotal);
                var orderNos = string.Join(", ", qOrders.Select(o => o.OrderNo));
                var orderStatuses = string.Join(", ", qOrders.Select(o => o.Status).Distinct());

                // Delivery tracking
                var allDeliveryDetails = qOrders
                    .SelectMany(o => orderToDeliveryDetails.GetValueOrDefault(o.OrderId, new()))
                    .ToList();
                var linkedDeliveryIds = allDeliveryDetails.Select(d => d.DeliveryId).Distinct().ToList();
                var linkedDeliveries = linkedDeliveryIds
                    .Select(id => deliveries.GetValueOrDefault(id))
                    .Where(d => d != null)
                    .ToList();
                var deliveryAmount = linkedDeliveries.Sum(d => d!.NetTotal);
                var deliveryNos = string.Join(", ", linkedDeliveries.Select(d => d!.DeliveryNo));
                var deliveryStatuses = string.Join(", ", linkedDeliveries.Select(d => d!.Status).Distinct());

                // Invoice tracking
                var allInvoiceDetails = allDeliveryDetails
                    .SelectMany(dd => deliveryDetailToInvoiceDetails.GetValueOrDefault(dd.DeliveryDetailId, new()))
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
                    .Where(a => linkedInvoiceIds.Contains(a.InvoiceId))
                    .Sum(a => a.AmountApplied);

                var outstanding = invoiceAmount - paymentAmount;

                // Determine current stage
                var stage = DetermineStage(q.Status, qOrders.Any(), linkedDeliveries.Any(), linkedInvoices.Any(), paymentAmount, invoiceAmount);

                rows.Add(new SalesDocumentFlowRowDto
                {
                    QuotationId = q.QuotationId,
                    QuotationNo = q.QuotationNo,
                    QuotationDate = q.QuotationDate,
                    QuotationStatus = q.Status,
                    QuotationAmount = q.NetTotal,
                    BusinessPartnerId = q.BusinessPartnerId,
                    CustomerName = bps.GetValueOrDefault(q.BusinessPartnerId, ""),
                    OrderCount = qOrders.Count,
                    OrderNos = string.IsNullOrEmpty(orderNos) ? null : orderNos,
                    OrderStatuses = string.IsNullOrEmpty(orderStatuses) ? null : orderStatuses,
                    OrderAmount = orderAmount,
                    DeliveryCount = linkedDeliveries.Count,
                    DeliveryNos = string.IsNullOrEmpty(deliveryNos) ? null : deliveryNos,
                    DeliveryStatuses = string.IsNullOrEmpty(deliveryStatuses) ? null : deliveryStatuses,
                    DeliveryAmount = deliveryAmount,
                    InvoiceCount = linkedInvoices.Count,
                    InvoiceNos = string.IsNullOrEmpty(invoiceNos) ? null : invoiceNos,
                    InvoiceStatuses = string.IsNullOrEmpty(invoiceStatuses) ? null : invoiceStatuses,
                    InvoiceAmount = invoiceAmount,
                    PaymentAmount = paymentAmount,
                    OutstandingAmount = outstanding,
                    CurrentStage = stage,
                });
            }

            // 10. Apply stage filter
            if (!string.IsNullOrEmpty(req.CurrentStage))
                rows = rows.Where(r => r.CurrentStage == req.CurrentStage).ToList();

            // 11. Apply quotation status filter
            if (!string.IsNullOrEmpty(req.QuotationStatus))
                rows = rows.Where(r => r.QuotationStatus == req.QuotationStatus).ToList();

            // 12. Sort
            rows = req.SortBy?.ToLower() switch
            {
                "quotationno" => req.SortDirection == "asc" ? rows.OrderBy(r => r.QuotationNo).ToList() : rows.OrderByDescending(r => r.QuotationNo).ToList(),
                "customername" => req.SortDirection == "asc" ? rows.OrderBy(r => r.CustomerName).ToList() : rows.OrderByDescending(r => r.CustomerName).ToList(),
                "quotationamount" => req.SortDirection == "asc" ? rows.OrderBy(r => r.QuotationAmount).ToList() : rows.OrderByDescending(r => r.QuotationAmount).ToList(),
                "orderamount" => req.SortDirection == "asc" ? rows.OrderBy(r => r.OrderAmount).ToList() : rows.OrderByDescending(r => r.OrderAmount).ToList(),
                "invoiceamount" => req.SortDirection == "asc" ? rows.OrderBy(r => r.InvoiceAmount).ToList() : rows.OrderByDescending(r => r.InvoiceAmount).ToList(),
                "outstandingamount" => req.SortDirection == "asc" ? rows.OrderBy(r => r.OutstandingAmount).ToList() : rows.OrderByDescending(r => r.OutstandingAmount).ToList(),
                "currentstage" => req.SortDirection == "asc" ? rows.OrderBy(r => r.CurrentStage).ToList() : rows.OrderByDescending(r => r.CurrentStage).ToList(),
                _ => req.SortDirection == "asc" ? rows.OrderBy(r => r.QuotationDate).ToList() : rows.OrderByDescending(r => r.QuotationDate).ToList(),
            };

            // 13. Pagination
            var totalRecords = rows.Count;
            var paged = rows
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();

            return new SalesDocumentFlowResponseDto
            {
                Data = paged,
                TotalRecords = totalRecords,
                PageNumber = req.PageNumber,
                PageSize = req.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / req.PageSize),
                TotalQuotationAmount = rows.Sum(r => r.QuotationAmount),
                TotalOrderAmount = rows.Sum(r => r.OrderAmount),
                TotalDeliveryAmount = rows.Sum(r => r.DeliveryAmount),
                TotalInvoiceAmount = rows.Sum(r => r.InvoiceAmount),
                TotalPaymentAmount = rows.Sum(r => r.PaymentAmount),
                TotalOutstanding = rows.Sum(r => r.OutstandingAmount),
            };
        }

        private static string DetermineStage(string quotationStatus, bool hasOrders, bool hasDeliveries, bool hasInvoices, decimal paymentAmount, decimal invoiceAmount)
        {
            if (quotationStatus == "Cancelled" || quotationStatus == "Rejected")
                return quotationStatus;

            if (hasInvoices)
            {
                if (invoiceAmount > 0 && paymentAmount >= invoiceAmount)
                    return "Paid";
                if (paymentAmount > 0)
                    return "Partial Payment";
                return "Invoice";
            }
            if (hasDeliveries) return "Delivery";
            if (hasOrders) return "Order";
            return "Quotation";
        }

        private static SalesDocumentFlowResponseDto EmptyResponse(SalesDocumentFlowRequestDto req) => new()
        {
            Data = new(),
            TotalRecords = 0,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = 0,
        };
    }
}
