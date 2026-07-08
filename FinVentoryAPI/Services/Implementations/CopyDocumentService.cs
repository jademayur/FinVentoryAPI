using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.CopyDocumentDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class CopyDocumentService : ICopyDocumentService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public CopyDocumentService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════════
        // 1. SALES ORDER → SALES INVOICE
        // ════════════════════════════════════════════════════

        public async Task<CopiedSalesInvoiceDto> CopySalesOrderToInvoiceAsync(
            int salesOrderId, CopyRequestDto? request = null)
        {
            //var companyId = _common.GetCompanyId();

            //// ── Load source ───────────────────────────────────
            //var order = await _context.SalesOrderMains
            //    .AsNoTracking()
            //    .AsSplitQuery()
            //    .Include(o => o.BusinessPartner)
            //    .Include(o => o.SalesAccount)
            //    .Include(o => o.BillAddress)
            //    .Include(o => o.Details!).ThenInclude(d => d.Item)
            //    .Include(o => o.Details!).ThenInclude(d => d.Hsn).ThenInclude(h => h!.tax)
            //    .FirstOrDefaultAsync(o =>
            //        o.OrderId == salesOrderId &&
            //        o.CompanyId == companyId &&
            //        !o.IsDeleted)
            //    ?? throw new Exception($"Sales Order {salesOrderId} not found.");

            //// ── Validate ──────────────────────────────────────
            //ValidateSourceStatus(order.Status, "Sales Order");

            //// ── Load already-copied quantities ────────────────
            //var copiedQtyMap = await GetAlreadyCopiedQtyMapAsync(
            //    DocumentType.SalesOrder, salesOrderId, companyId);

            //// ── Build detail lines ────────────────────────────
            //var details = new List<CopiedSalesInvoiceDetailDto>();

            //foreach (var line in order.Details ?? Enumerable.Empty<SalesOrderDetail>())
            //{
            //    var alreadyCopied = copiedQtyMap.GetValueOrDefault(line.DetailId, 0);
            //    var pendingQty = line.Qty - alreadyCopied;

            //    if (pendingQty <= 0) continue; // fully invoiced — skip

            //    var copyQty = ResolveCopyQty(
            //        line.DetailId, pendingQty, request?.QtyOverrides);

            //    details.Add(new CopiedSalesInvoiceDetailDto
            //    {
            //        SourceDetailId = line.DetailId,
            //        ItemId = line.ItemId,
            //        ItemName = line.Item?.ItemName ?? string.Empty,
            //        ItemCode = line.Item?.ItemCode,
            //        ItemManageBy = line.Item?.ItemManageBy.ToString(),
            //        HsnId = line.HsnId,
            //        HsnCode = line.HsnCode,
            //        PriceType = line.PriceType,
            //        OriginalQty = line.Qty,
            //        AlreadyCopiedQty = alreadyCopied,
            //        PendingQty = pendingQty,
            //        CopyQty = copyQty,
            //        Rate = line.Rate,
            //        DiscountRate = line.DiscountRate,
            //        AddisDiscountRate = line.AddisDiscountRate,
            //        IsTaxIncluded = line.IsTaxIncluded,
            //        IGSTRate = line.Hsn?.tax?.IGST ?? 0,
            //        CGSTRate = line.Hsn?.tax?.CGST ?? 0,
            //        SGSTRate = line.Hsn?.tax?.SGST ?? 0,
            //        CessRate = line.Hsn?.Cess ?? 0,
            //        IGSTPostingAccountId = line.Hsn?.tax?.IGSTPostingAccountId,
            //        CGSTPostingAccountId = line.Hsn?.tax?.CGSTPostingAccountId,
            //        SGSTPostingAccountId = line.Hsn?.tax?.SGSTPostingAccountId,
            //        CessPostingAccountId = line.Hsn?.CessPostingAc,
            //    });
            //}

            //if (details.Count == 0)
            //    throw new Exception(
            //        "All lines in this Sales Order have already been fully invoiced.");

            //return new CopiedSalesInvoiceDto
            //{
            //    SourceOrderId = order.OrderId,
            //    SourceOrderNo = order.OrderNo,
            //    SourceOrderDate = order.OrderDate,
            //    BusinessPartnerId = order.BusinessPartnerId,
            //    BusinessPartnerName = order.BusinessPartner?.BusinessPartnerName ?? string.Empty,
            //    LocationId = order.LocationId,
            //    SalesAccountId = order.SalesAccountId,
            //    SalesAccountName = order.SalesAccount?.AccountName ?? string.Empty,
            //    SalesStateCode = order.SalesStateCode,
            //    BillStateCode = order.BillStateCode,
            //    BillAddressId = order.BillAddressId,
            //    BillAddressLine = FormatAddress(order.BillAddress),
            //    Remarks = order.Remarks,
            //    Details = details,
            //};
            return new CopiedSalesInvoiceDto();
        }

        // ════════════════════════════════════════════════════
        // 2. SALES INVOICE → SALES RETURN
        // ════════════════════════════════════════════════════

        public async Task<CopiedSalesReturnDto> CopySalesInvoiceToReturnAsync(
            int salesInvoiceId, CopyRequestDto? request = null)
        {
            var companyId = _common.GetCompanyId();

            var invoice = await _context.SalesInvoiceMains
                .AsNoTracking()
                .AsSplitQuery()
                .Include(i => i.BusinessPartner)
                .Include(i => i.SalesAccount)
                .Include(i => i.BillAddress)
                .Include(i => i.Details!).ThenInclude(d => d.Item)
                .Include(i => i.Details!).ThenInclude(d => d.Hsn).ThenInclude(h => h!.tax)
                .FirstOrDefaultAsync(i =>
                    i.InvoiceId == salesInvoiceId &&
                    i.CompanyId == companyId &&
                    !i.IsDeleted)
                ?? throw new Exception($"Sales Invoice {salesInvoiceId} not found.");

            ValidateSourceStatus(invoice.Status, "Sales Invoice");

            var copiedQtyMap = await GetAlreadyCopiedQtyMapAsync(
                DocumentType.SalesInvoice, salesInvoiceId, companyId);

            var details = new List<CopiedSalesReturnDetailDto>();

            foreach (var line in invoice.Details ?? Enumerable.Empty<SalesInvoiceDetail>())
            {
                var alreadyCopied = copiedQtyMap.GetValueOrDefault(line.DetailId, 0);
                var pendingQty = line.Qty - alreadyCopied;

                if (pendingQty <= 0) continue;

                var copyQty = ResolveCopyQty(line.DetailId, pendingQty, request?.QtyOverrides);

                details.Add(new CopiedSalesReturnDetailDto
                {
                    SourceDetailId = line.DetailId,
                    ItemId = line.ItemId,
                    ItemName = line.Item?.ItemName ?? string.Empty,
                    ItemCode = line.Item?.ItemCode,
                    ItemManageBy = line.Item?.ItemManageBy.ToString(),
                    HsnId = line.HsnId,
                    HsnCode = line.HsnCode,
                    PriceType = line.PriceType,
                    OriginalQty = line.Qty,
                    AlreadyCopiedQty = alreadyCopied,
                    PendingQty = pendingQty,
                    CopyQty = copyQty,
                    Rate = line.Rate,
                    DiscountRate = line.DiscountRate,
                    AddisDiscountRate = line.AddisDiscountRate,
                    IsTaxIncluded = line.IsTaxIncluded,
                    IGSTRate = line.Hsn?.tax?.IGST ?? 0,
                    CGSTRate = line.Hsn?.tax?.CGST ?? 0,
                    SGSTRate = line.Hsn?.tax?.SGST ?? 0,
                    CessRate = line.Hsn?.Cess ?? 0,
                    IGSTPostingAccountId = line.Hsn?.tax?.IGSTPostingAccountId,
                    CGSTPostingAccountId = line.Hsn?.tax?.CGSTPostingAccountId,
                    SGSTPostingAccountId = line.Hsn?.tax?.SGSTPostingAccountId,
                    CessPostingAccountId = line.Hsn?.CessPostingAc,
                });
            }

            if (details.Count == 0)
                throw new Exception(
                    "All lines in this Sales Invoice have already been fully returned.");

            return new CopiedSalesReturnDto
            {
                SourceInvoiceId = invoice.InvoiceId,
                SourceInvoiceNo = invoice.InvoiceNo,
                SourceInvoiceDate = invoice.InvoiceDate,
                BusinessPartnerId = invoice.BusinessPartnerId,
                BusinessPartnerName = invoice.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                LocationId = invoice.LocationId,
                SalesAccountId = invoice.SalesAccountId,
                SalesAccountName = invoice.SalesAccount?.AccountName ?? string.Empty,
                SalesStateCode = invoice.SalesStateCode,
                BillStateCode = invoice.BillStateCode,
                BillAddressId = invoice.BillAddressId,
                BillAddressLine = FormatAddress(invoice.BillAddress),
                NoteType = "Credit",
                Remarks = invoice.Remarks,
                Details = details,
            };
        }

        // ════════════════════════════════════════════════════
        // 3. QUOTATION → SALES ORDER
        // ════════════════════════════════════════════════════

        public async Task<CopiedSalesOrderDto> CopyQuotationToSalesOrderAsync(
            int quotationId, CopyRequestDto? request = null)
        {
            //var companyId = _common.GetCompanyId();

            //var quotation = await _context.Quotations
            //    .AsNoTracking()
            //    .AsSplitQuery()
            //    .Include(q => q.BusinessPartner)
            //    .Include(q => q.BillAddress)
            //    .Include(q => q.Details!).ThenInclude(d => d.Item)
            //    .Include(q => q.Details!).ThenInclude(d => d.Hsn).ThenInclude(h => h!.tax)
            //    .FirstOrDefaultAsync(q =>
            //        q.QuotationId == quotationId &&
            //        q.CompanyId == companyId &&
            //        !q.IsDeleted)
            //    ?? throw new Exception($"Quotation {quotationId} not found.");

            //ValidateSourceStatus(quotation.Status, "Quotation");

            //var copiedQtyMap = await GetAlreadyCopiedQtyMapAsync(
            //    DocumentType.Quotation, quotationId, companyId);

            //var details = new List<CopiedSalesOrderDetailDto>();

            //foreach (var line in quotation.Details ?? Enumerable.Empty<QuotationDetail>())
            //{
            //    var alreadyCopied = copiedQtyMap.GetValueOrDefault(line.DetailId, 0);
            //    var pendingQty = line.Qty - alreadyCopied;

            //    if (pendingQty <= 0) continue;

            //    var copyQty = ResolveCopyQty(line.DetailId, pendingQty, request?.QtyOverrides);

            //    details.Add(new CopiedSalesOrderDetailDto
            //    {
            //        SourceDetailId = line.DetailId,
            //        ItemId = line.ItemId,
            //        ItemName = line.Item?.ItemName ?? string.Empty,
            //        ItemCode = line.Item?.ItemCode,
            //        ItemManageBy = line.Item?.ItemManageBy.ToString(),
            //        HsnId = line.HsnId,
            //        HsnCode = line.HsnCode,
            //        PriceType = line.PriceType,
            //        OriginalQty = line.Qty,
            //        AlreadyCopiedQty = alreadyCopied,
            //        PendingQty = pendingQty,
            //        CopyQty = copyQty,
            //        Rate = line.Rate,
            //        DiscountRate = line.DiscountRate,
            //        AddisDiscountRate = line.AddisDiscountRate,
            //        IsTaxIncluded = line.IsTaxIncluded,
            //        IGSTRate = line.Hsn?.tax?.IGST ?? 0,
            //        CGSTRate = line.Hsn?.tax?.CGST ?? 0,
            //        SGSTRate = line.Hsn?.tax?.SGST ?? 0,
            //        CessRate = line.Hsn?.Cess ?? 0,
            //        IGSTPostingAccountId = line.Hsn?.tax?.IGSTPostingAccountId,
            //        CGSTPostingAccountId = line.Hsn?.tax?.CGSTPostingAccountId,
            //        SGSTPostingAccountId = line.Hsn?.tax?.SGSTPostingAccountId,
            //        CessPostingAccountId = line.Hsn?.CessPostingAc,
            //    });
            //}

            //if (details.Count == 0)
            //    throw new Exception("All lines in this Quotation have already been ordered.");

            //return new CopiedSalesOrderDto
            //{
            //    SourceQuotationId = quotation.QuotationId,
            //    SourceQuotationNo = quotation.QuotationNo,
            //    SourceQuotationDate = quotation.QuotationDate,
            //    BusinessPartnerId = quotation.BusinessPartnerId,
            //    BusinessPartnerName = quotation.BusinessPartner?.BusinessPartnerName ?? string.Empty,
            //    LocationId = quotation.LocationId,
            //    SalesStateCode = quotation.SalesStateCode,
            //    BillStateCode = quotation.BillStateCode,
            //    BillAddressId = quotation.BillAddressId,
            //    BillAddressLine = FormatAddress(quotation.BillAddress),
            //    Remarks = quotation.Remarks,
            //    Details = details,
            //};
            return new CopiedSalesOrderDto();
        }

        // ════════════════════════════════════════════════════
        // 4. PURCHASE ORDER → PURCHASE INVOICE
        // ════════════════════════════════════════════════════

        public async Task<CopiedPurchaseInvoiceDto> CopyPurchaseOrderToInvoiceAsync(
            int purchaseOrderId, CopyRequestDto? request = null)
        {
            //var companyId = _common.GetCompanyId();

            //var order = await _context.PurchaseOrderMains
            //    .AsNoTracking()
            //    .AsSplitQuery()
            //    .Include(o => o.BusinessPartner)
            //    .Include(o => o.PurchaseAccount)
            //    .Include(o => o.BillAddress)
            //    .Include(o => o.Details!).ThenInclude(d => d.Item)
            //    .Include(o => o.Details!).ThenInclude(d => d.Hsn).ThenInclude(h => h!.tax)
            //    .FirstOrDefaultAsync(o =>
            //        o.OrderId == purchaseOrderId &&
            //        o.CompanyId == companyId &&
            //        !o.IsDeleted)
            //    ?? throw new Exception($"Purchase Order {purchaseOrderId} not found.");

            //ValidateSourceStatus(order.Status, "Purchase Order");

            //var copiedQtyMap = await GetAlreadyCopiedQtyMapAsync(
            //    DocumentType.PurchaseOrder, purchaseOrderId, companyId);

            //var details = new List<CopiedPurchaseInvoiceDetailDto>();

            //foreach (var line in order.Details ?? Enumerable.Empty<PurchaseOrderDetail>())
            //{
            //    var alreadyCopied = copiedQtyMap.GetValueOrDefault(line.DetailId, 0);
            //    var pendingQty = line.Qty - alreadyCopied;

            //    if (pendingQty <= 0) continue;

            //    var copyQty = ResolveCopyQty(line.DetailId, pendingQty, request?.QtyOverrides);

            //    details.Add(new CopiedPurchaseInvoiceDetailDto
            //    {
            //        SourceDetailId = line.DetailId,
            //        ItemId = line.ItemId,
            //        ItemName = line.Item?.ItemName ?? string.Empty,
            //        ItemCode = line.Item?.ItemCode,
            //        ItemManageBy = line.Item?.ItemManageBy.ToString(),
            //        HsnId = line.HsnId,
            //        HsnCode = line.HsnCode,
            //        PriceType = line.PriceType,
            //        OriginalQty = line.Qty,
            //        AlreadyCopiedQty = alreadyCopied,
            //        PendingQty = pendingQty,
            //        CopyQty = copyQty,
            //        Rate = line.Rate,
            //        DiscountRate = line.DiscountRate,
            //        AddisDiscountRate = line.AddisDiscountRate,
            //        IsTaxIncluded = line.IsTaxIncluded,
            //        IGSTRate = line.Hsn?.tax?.IGST ?? 0,
            //        CGSTRate = line.Hsn?.tax?.CGST ?? 0,
            //        SGSTRate = line.Hsn?.tax?.SGST ?? 0,
            //        CessRate = line.Hsn?.Cess ?? 0,
            //        IGSTPostingAccountId = line.Hsn?.tax?.IGSTPostingAccountId,
            //        CGSTPostingAccountId = line.Hsn?.tax?.CGSTPostingAccountId,
            //        SGSTPostingAccountId = line.Hsn?.tax?.SGSTPostingAccountId,
            //        CessPostingAccountId = line.Hsn?.CessPostingAc,
            //    });
            //}

            //if (details.Count == 0)
            //    throw new Exception(
            //        "All lines in this Purchase Order have already been fully invoiced.");

            //return new CopiedPurchaseInvoiceDto
            //{
            //    SourceOrderId = order.OrderId,
            //    SourceOrderNo = order.OrderNo,
            //    SourceOrderDate = order.OrderDate,
            //    BusinessPartnerId = order.BusinessPartnerId,
            //    BusinessPartnerName = order.BusinessPartner?.BusinessPartnerName ?? string.Empty,
            //    LocationId = order.LocationId,
            //    PurchaseAccountId = order.PurchaseAccountId,
            //    PurchaseAccountName = order.PurchaseAccount?.AccountName ?? string.Empty,
            //    PurchaseStateCode = order.PurchaseStateCode,
            //    BillStateCode = order.BillStateCode,
            //    BillAddressId = order.BillAddressId,
            //    BillAddressLine = FormatAddress(order.BillAddress),
            //    Remarks = order.Remarks,
            //    Details = details,
            //};
            return new CopiedPurchaseInvoiceDto();
        }

        // ════════════════════════════════════════════════════
        // 5. PURCHASE INVOICE → PURCHASE RETURN
        // ════════════════════════════════════════════════════

        public async Task<CopiedPurchaseReturnDto> CopyPurchaseInvoiceToReturnAsync(
            int purchaseInvoiceId, CopyRequestDto? request = null)
        {
            var companyId = _common.GetCompanyId();

            var invoice = await _context.PurchaseInvoiceMains
                .AsNoTracking()
                .AsSplitQuery()
                .Include(i => i.BusinessPartner)
                .Include(i => i.PurchaseAccount)
                .Include(i => i.BillAddress)
                .Include(i => i.Details!).ThenInclude(d => d.Item)
                .Include(i => i.Details!).ThenInclude(d => d.Hsn).ThenInclude(h => h!.tax)
                .FirstOrDefaultAsync(i =>
                    i.InvoiceId == purchaseInvoiceId &&
                    i.CompanyId == companyId &&
                    !i.IsDeleted)
                ?? throw new Exception($"Purchase Invoice {purchaseInvoiceId} not found.");

            ValidateSourceStatus(invoice.Status, "Purchase Invoice");

            var copiedQtyMap = await GetAlreadyCopiedQtyMapAsync(
                DocumentType.PurchaseInvoice, purchaseInvoiceId, companyId);

            var details = new List<CopiedPurchaseReturnDetailDto>();

            foreach (var line in invoice.Details ?? Enumerable.Empty<PurchaseInvoiceDetail>())
            {
                var alreadyCopied = copiedQtyMap.GetValueOrDefault(line.DetailId, 0);
                var pendingQty = line.Qty - alreadyCopied;

                if (pendingQty <= 0) continue;

                var copyQty = ResolveCopyQty(line.DetailId, pendingQty, request?.QtyOverrides);

                details.Add(new CopiedPurchaseReturnDetailDto
                {
                    SourceDetailId = line.DetailId,
                    ItemId = line.ItemId,
                    ItemName = line.Item?.ItemName ?? string.Empty,
                    ItemCode = line.Item?.ItemCode,
                    ItemManageBy = line.Item?.ItemManageBy.ToString(),
                    HsnId = line.HsnId,
                    HsnCode = line.HsnCode,
                    PriceType = line.PriceType,
                    OriginalQty = line.Qty,
                    AlreadyCopiedQty = alreadyCopied,
                    PendingQty = pendingQty,
                    CopyQty = copyQty,
                    Rate = line.Rate,
                    DiscountRate = line.DiscountRate,
                    AddisDiscountRate = line.AddisDiscountRate,
                    IsTaxIncluded = line.IsTaxIncluded,
                    IGSTRate = line.Hsn?.tax?.IGST ?? 0,
                    CGSTRate = line.Hsn?.tax?.CGST ?? 0,
                    SGSTRate = line.Hsn?.tax?.SGST ?? 0,
                    CessRate = line.Hsn?.Cess ?? 0,
                    IGSTPostingAccountId = line.Hsn?.tax?.IGSTPostingAccountId,
                    CGSTPostingAccountId = line.Hsn?.tax?.CGSTPostingAccountId,
                    SGSTPostingAccountId = line.Hsn?.tax?.SGSTPostingAccountId,
                    CessPostingAccountId = line.Hsn?.CessPostingAc,
                });
            }

            if (details.Count == 0)
                throw new Exception(
                    "All lines in this Purchase Invoice have already been fully returned.");

            return new CopiedPurchaseReturnDto
            {
                SourceInvoiceId = invoice.InvoiceId,
                SourceInvoiceNo = invoice.InvoiceNo,
                SourceInvoiceDate = invoice.InvoiceDate,
                BusinessPartnerId = invoice.BusinessPartnerId,
                BusinessPartnerName = invoice.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                LocationId = invoice.LocationId,
                PurchaseAccountId = invoice.PurchaseAccountId,
                PurchaseAccountName = invoice.PurchaseAccount?.AccountName ?? string.Empty,
                PurchaseStateCode = invoice.PurchaseStateCode,
                BillStateCode = invoice.BillStateCode,
                BillAddressId = invoice.BillAddressId,
                BillAddressLine = FormatAddress(invoice.BillAddress),
                NoteType = "Debit",
                Remarks = invoice.Remarks,
                Details = details,
            };
        }

        // ════════════════════════════════════════════════════
        // COPY LOG — Save (called after target doc is saved)
        // ════════════════════════════════════════════════════

        public async Task SaveCopyLogAsync(SaveCopyLogDto dto)
        {
            var entries = dto.Lines.Select(l => new DocumentCopyLog
            {
                SourceType = dto.SourceType,
                SourceId = dto.SourceId,
                SourceDetailId = l.SourceDetailId,
                SourceQty = l.SourceQty,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                TargetDetailId = l.TargetDetailId,
                CopiedQty = l.CopiedQty,
                ItemId = l.ItemId,
                CompanyId = dto.CompanyId,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.UtcNow,
            }).ToList();

            _context.DocumentCopyLogs.AddRange(entries);
            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════════
        // COPY LOG — Soft Delete (called when target doc deleted)
        // ════════════════════════════════════════════════════

        public async Task SoftDeleteCopyLogAsync(DocumentType targetType, int targetId)
        {
            var logs = await _context.DocumentCopyLogs
                .Where(l => l.TargetType == targetType &&
                            l.TargetId == targetId &&
                            !l.IsDeleted)
                .ToListAsync();

            foreach (var log in logs) log.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Returns a map of { SourceDetailId → totalAlreadyCopiedQty }
        /// by summing all non-deleted copy log entries for this source document.
        /// </summary>
        private async Task<Dictionary<int, decimal>> GetAlreadyCopiedQtyMapAsync(
            DocumentType sourceType, int sourceId, int companyId)
        {
            var logs = await _context.DocumentCopyLogs
                .AsNoTracking()
                .Where(l => l.SourceType == sourceType &&
                            l.SourceId == sourceId &&
                            l.CompanyId == companyId &&
                            !l.IsDeleted &&
                            l.SourceDetailId.HasValue)
                .GroupBy(l => l.SourceDetailId!.Value)
                .Select(g => new { DetailId = g.Key, TotalCopied = g.Sum(x => x.CopiedQty) })
                .ToListAsync();

            return logs.ToDictionary(x => x.DetailId, x => x.TotalCopied);
        }

        /// <summary>
        /// Validates that a source document has a copyable status.
        /// Throws if cancelled or closed.
        /// </summary>
        private static void ValidateSourceStatus(string status, string docType)
        {
            var blocked = new[] { "Cancelled", "Closed", "Rejected" };
            if (blocked.Contains(status, StringComparer.OrdinalIgnoreCase))
                throw new Exception(
                    $"{docType} is {status} and cannot be copied.");
        }

        /// <summary>
        /// Determines the qty to copy for a given source line.
        /// If a QtyOverride is provided and is valid (0 < override ≤ pendingQty),
        /// use the override. Otherwise fall back to the full pending qty.
        /// </summary>
        private static decimal ResolveCopyQty(
            int detailId, decimal pendingQty,
            Dictionary<int, decimal>? overrides)
        {
            if (overrides != null && overrides.TryGetValue(detailId, out var requested))
            {
                if (requested <= 0)
                    throw new Exception(
                        $"Copy qty for line {detailId} must be greater than 0.");
                if (requested > pendingQty)
                    throw new Exception(
                        $"Copy qty ({requested}) for line {detailId} " +
                        $"exceeds pending qty ({pendingQty}).");
                return requested;
            }
            return pendingQty;
        }

        /// <summary>
        /// Formats a BusinessPartnerAddress into a display string.
        /// Same pattern used across SalesReturnService etc.
        /// </summary>
        private static string? FormatAddress(BusinessPartnerAddress? addr) =>
            addr == null ? null :
            string.Join(", ", new[]
            {
                addr.AddressLine1, addr.AddressLine2, addr.City,
                addr.State.HasValue ? ((GstState)addr.State.Value).ToString() : null,
                addr.Pincode
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
