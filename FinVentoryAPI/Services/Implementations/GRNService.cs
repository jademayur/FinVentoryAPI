using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.GRNDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class GRNService : IGRNService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public GRNService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════════
        // CREATE
        // ════════════════════════════════════════════════════
        public async Task<GRNResponseDto> CreateAsync(CreateGRNMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, companyId,
                dto.PurchaseStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.ContactPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            // ── Validate source POs ───────────────────────────────────────────
            var poIds = dto.Details
                .Where(d => d.PurchaseOrderId.HasValue)
                .Select(d => d.PurchaseOrderId!.Value)
                .Distinct()
                .ToList();

            if (poIds.Any())
                await ValidateSourcePurchaseOrdersAsync(poIds, dto.BusinessPartnerId, companyId);

            // ── Validate received qtys (no over-receiving) ────────────────────
            await ValidateReceivedQtysAsync(dto.Details, companyId);

            var grnNo = await GenerateGRNNoAsync(companyId, finYearId);

            var main = new GRNMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                GRNNo = grnNo,
                GRNDate = dto.GRNDate,
                SupplierInvoiceNo = dto.SupplierInvoiceNo,
                SupplierInvoiceDate = dto.SupplierInvoiceDate,
                RefNo = dto.RefNo,
                RefDate = dto.RefDate,
                Remarks = dto.Remarks,
                Status = "Draft",
                BusinessPartnerId = dto.BusinessPartnerId,
                LocationId = dto.LocationId,
                ContactPersonId = dto.ContactPersonId,                
                BillAddressId = dto.BillAddressId,
                ShipAddressId = dto.ShipAddressId,
                PurchaseStateCode = dto.PurchaseStateCode,
                BillStateCode = dto.BillStateCode,
                RoundOff = dto.RoundOff,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                Details = new List<GRNDetail>(),
                TaxDetails = new List<GRNTaxDetail>()
            };

            // ── Build detail lines ────────────────────────────────────────────
            decimal totalSubTotal = 0, totalTaxAmount = 0, totalCessAmount = 0;

            foreach (var lineDto in dto.Details)
            {
                var detail = await BuildDetailWithTaxAsync(
                    lineDto, dto.PurchaseStateCode, dto.BillStateCode, companyId);

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                foreach (var td in detail.TaxDetails ?? Enumerable.Empty<GRNTaxDetail>())
                {
                    td.GRN = main;
                    td.Detail = detail;
                    main.TaxDetails!.Add(td);
                }

                main.Details!.Add(detail);
            }

            main.SubTotal = totalSubTotal;
            main.TaxAmount = totalTaxAmount;
            main.CessAmount = totalCessAmount;
            main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.GRNMains.Add(main);
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(main.GRNId)
                ?? throw new Exception("Failed to retrieve saved GRN.");
        }

        // ════════════════════════════════════════════════════
        // UPDATE  (Draft only)
        // ════════════════════════════════════════════════════
        public async Task<bool> UpdateAsync(int id, UpdateGRNMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.GRNMains
                .Include(m => m.Details!).ThenInclude(d => d.TaxDetails)
                .Include(m => m.TaxDetails)
                .FirstOrDefaultAsync(x =>
                    x.GRNId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("GRN not found.");

            if (main.Status != "Draft")
                throw new Exception("Only Draft GRNs can be updated.");

            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, companyId,
                dto.PurchaseStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.ContactPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            var poIds = dto.Details
                .Where(d => d.PurchaseOrderId.HasValue)
                .Select(d => d.PurchaseOrderId!.Value)
                .Distinct()
                .ToList();

            if (poIds.Any())
                await ValidateSourcePurchaseOrdersAsync(poIds, dto.BusinessPartnerId, companyId);

            // Validate received qtys; exclude current GRN from already-received count
            await ValidateReceivedQtysAsync(
                dto.Details.Select(d => new CreateGRNDetailDto
                {
                    PurchaseOrderId = d.PurchaseOrderId,
                    PurchaseOrderDetailId = d.PurchaseOrderDetailId,
                    ItemId = d.ItemId,
                    PriceType = d.PriceType,
                    ReceivedQty = d.ReceivedQty,
                    Rate = d.Rate,
                    DiscountRate = d.DiscountRate,
                    AddisDiscountRate = d.AddisDiscountRate,
                    IsTaxIncluded = d.IsTaxIncluded
                }).ToList(),
                companyId,
                excludeGRNId: id);

            // ── Build fresh detail objects ─────────────────────────────────────
            var incomingDetails = new List<GRNDetail>();
            foreach (var lineDto in dto.Details)
            {
                var createDto = new CreateGRNDetailDto
                {
                    PurchaseOrderId = lineDto.PurchaseOrderId,
                    PurchaseOrderDetailId = lineDto.PurchaseOrderDetailId,
                    ItemId = lineDto.ItemId,
                    PriceType = lineDto.PriceType,
                    ReceivedQty = lineDto.ReceivedQty,
                    Rate = lineDto.Rate,
                    DiscountRate = lineDto.DiscountRate,
                    AddisDiscountRate = lineDto.AddisDiscountRate,
                    IsTaxIncluded = lineDto.IsTaxIncluded
                };
                incomingDetails.Add(await BuildDetailWithTaxAsync(
                    createDto, dto.PurchaseStateCode, dto.BillStateCode, companyId));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Header fields
                main.GRNDate = dto.GRNDate;
                main.SupplierInvoiceNo = dto.SupplierInvoiceNo;
                main.SupplierInvoiceDate = dto.SupplierInvoiceDate;
                main.RefNo = dto.RefNo;
                main.RefDate = dto.RefDate;
                main.Remarks = dto.Remarks;
                main.BusinessPartnerId = dto.BusinessPartnerId;
                main.LocationId = dto.LocationId;
                main.ContactPersonId = dto.ContactPersonId;               
                main.BillAddressId = dto.BillAddressId;
                main.ShipAddressId = dto.ShipAddressId;
                main.PurchaseStateCode = dto.PurchaseStateCode;
                main.BillStateCode = dto.BillStateCode;
                main.RoundOff = dto.RoundOff;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

                // ── Sync detail lines (positional merge) ─────────────────────
                var existingDetails = main.Details!.ToList();
                decimal totalSubTotal = 0, totalTaxAmount = 0, totalCessAmount = 0;

                for (int i = 0; i < incomingDetails.Count; i++)
                {
                    var incoming = incomingDetails[i];

                    if (i < existingDetails.Count)
                    {
                        var existing = existingDetails[i];

                        existing.PurchaseOrderId = incoming.PurchaseOrderId;
                        existing.PurchaseOrderDetailId = incoming.PurchaseOrderDetailId;
                        existing.ItemId = incoming.ItemId;
                        existing.HsnId = incoming.HsnId;
                        existing.HsnCode = incoming.HsnCode;
                        existing.PriceType = incoming.PriceType;
                        existing.OrderedQty = incoming.OrderedQty;
                        existing.PreviouslyReceivedQty = incoming.PreviouslyReceivedQty;
                        existing.ReceivedQty = incoming.ReceivedQty;
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

                        // Sync tax details for this line
                        var existingTaxList = existing.TaxDetails?.ToList() ?? new();
                        var incomingTaxList = incoming.TaxDetails ?? new();

                        for (int t = 0; t < incomingTaxList.Count; t++)
                        {
                            var inTax = incomingTaxList[t];
                            if (t < existingTaxList.Count)
                            {
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
                            }
                            else
                            {
                                inTax.GRNId = main.GRNId;
                                inTax.GRNDetailId = existing.GRNDetailId;
                                _context.GRNTaxDetails.Add(inTax);
                            }
                        }

                        if (existingTaxList.Count > incomingTaxList.Count)
                            _context.GRNTaxDetails.RemoveRange(
                                existingTaxList.Skip(incomingTaxList.Count));

                        totalSubTotal += existing.TaxableAmount;
                        totalCessAmount += existing.CessAmount;
                        totalTaxAmount += existing.LineTaxAmount - existing.CessAmount;
                    }
                    else
                    {
                        // New line beyond existing count
                        incoming.GRNId = main.GRNId;
                        var newTaxList = incoming.TaxDetails ?? new();
                        incoming.TaxDetails = null;

                        _context.GRNDetails.Add(incoming);
                        await SaveChangesAsync();

                        foreach (var inTax in newTaxList)
                        {
                            inTax.GRNId = main.GRNId;
                            inTax.GRNDetailId = incoming.GRNDetailId;
                            _context.GRNTaxDetails.Add(inTax);
                        }

                        totalSubTotal += incoming.TaxableAmount;
                        totalCessAmount += incoming.CessAmount;
                        totalTaxAmount += incoming.LineTaxAmount - incoming.CessAmount;
                    }
                }

                // Remove surplus detail lines
                if (existingDetails.Count > incomingDetails.Count)
                {
                    var surplus = existingDetails.Skip(incomingDetails.Count).ToList();
                    foreach (var sd in surplus)
                        if (sd.TaxDetails != null && sd.TaxDetails.Any())
                            _context.GRNTaxDetails.RemoveRange(sd.TaxDetails);

                    _context.GRNDetails.RemoveRange(surplus);
                }

                main.SubTotal = totalSubTotal;
                main.TaxAmount = totalTaxAmount;
                main.CessAmount = totalCessAmount;
                main.NetTotal = totalSubTotal + totalTaxAmount + totalCessAmount + dto.RoundOff;

                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            _context.ChangeTracker.Clear();
            return true;
        }

        // ════════════════════════════════════════════════════
        // DELETE  (Draft only — soft delete)
        // ════════════════════════════════════════════════════
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.GRNMains
                .FirstOrDefaultAsync(x =>
                    x.GRNId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("GRN not found.");

            if (main.Status != "Draft")
                throw new Exception("Only Draft GRNs can be deleted.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                main.IsDeleted = true;
                main.IsActive = false;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

                await SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ════════════════════════════════════════════════════
        // CONFIRM  (Draft → Confirmed)
        // ════════════════════════════════════════════════════
        public async Task<bool> ConfirmAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.GRNMains
                .FirstOrDefaultAsync(x =>
                    x.GRNId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("GRN not found.");

            if (main.Status != "Draft")
                throw new Exception("Only Draft GRNs can be confirmed.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                main.Status = "Confirmed";
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

                await SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ════════════════════════════════════════════════════
        // CANCEL  (Draft or Confirmed → Cancelled)
        // ════════════════════════════════════════════════════
        public async Task<bool> CancelAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.GRNMains
                .FirstOrDefaultAsync(x =>
                    x.GRNId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("GRN not found.");

            if (main.Status == "Cancelled")
                throw new Exception("GRN is already cancelled.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                main.Status = "Cancelled";
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

                await SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ════════════════════════════════════════════════════
        // GET ALL
        // ════════════════════════════════════════════════════
        public async Task<List<GRNResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var list = await _context.GRNMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.ContactPerson)                
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.PurchaseOrder)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .OrderByDescending(x => x.GRNDate)
                .ToListAsync();

            return list.Select(MapToResponseDto).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET BY ID
        // ════════════════════════════════════════════════════
        public async Task<GRNResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.GRNMains
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.ContactPerson)               
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.PurchaseOrder)
                .Include(x => x.Details!).ThenInclude(d => d.PurchaseOrderDetail)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .FirstOrDefaultAsync(x =>
                    x.GRNId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            return main == null ? null : MapToResponseDto(main);
        }

        // ════════════════════════════════════════════════════
        // GET PAGED
        // ════════════════════════════════════════════════════
        public async Task<PagedResponseDto<GRNResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.GRNMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.ContactPerson)                
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                .Include(x => x.Details!)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.GRNNo.ToLower().Contains(search) ||
                    x.BusinessPartner!.BusinessPartnerName.ToLower().Contains(search) ||
                    (x.SupplierInvoiceNo != null && x.SupplierInvoiceNo.ToLower().Contains(search)) ||
                    (x.RefNo != null && x.RefNo.ToLower().Contains(search)));
            }

            // Filters
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
                    query = query.Where(x => x.GRNDate >= fromDate);
                }
                if (request.Filters.ContainsKey("toDate"))
                {
                    var toDate = ((JsonElement)request.Filters["toDate"]).GetDateTime();
                    query = query.Where(x => x.GRNDate <= toDate);
                }
               
                if (request.Filters.ContainsKey("purchaseOrderId"))
                {
                    var poId = ((JsonElement)request.Filters["purchaseOrderId"]).GetInt32();
                    query = query.Where(x => x.Details!.Any(d => d.PurchaseOrderId == poId));
                }
            }

            // Sorting
            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "grnno" => sort.Direction == "desc" ? query.OrderByDescending(x => x.GRNNo) : query.OrderBy(x => x.GRNNo),
                    "grndate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.GRNDate) : query.OrderBy(x => x.GRNDate),
                    "businesspartnername" => sort.Direction == "desc" ? query.OrderByDescending(x => x.BusinessPartner!.BusinessPartnerName) : query.OrderBy(x => x.BusinessPartner!.BusinessPartnerName),
                     "status" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "nettotal" => sort.Direction == "desc" ? query.OrderByDescending(x => x.NetTotal) : query.OrderBy(x => x.NetTotal),
                    _ => query.OrderByDescending(x => x.GRNDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.GRNDate);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .ToListAsync();

            return new PagedResponseDto<GRNResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapToResponseDto).ToList()
            };
        }

        // ════════════════════════════════════════════════════
        // GET POs FOR SUPPLIER  (picker dropdown)
        // Returns only Confirmed POs with pending received qty.
        // ════════════════════════════════════════════════════
        public async Task<List<PurchaseOrderPickerDto>> GetPurchaseOrdersForSupplierAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();

            var orders = await _context.PurchaseOrderMains
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.BusinessPartnerId == businessPartnerId &&
                    x.Status == "Confirmed" &&
                    !x.IsDeleted)                
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            var receivedQtyMap = await GetReceivedQtyMapAsync(
                orders.SelectMany(o => o.Details!.Select(d => d.OrderDetailId)).ToList(),
                companyId);

            var result = new List<PurchaseOrderPickerDto>();

            foreach (var order in orders)
            {
                var detailDtos = new List<PurchaseOrderPickerDetailDto>();

                foreach (var d in order.Details ?? Enumerable.Empty<PurchaseOrderDetail>())
                {
                    var received = receivedQtyMap.GetValueOrDefault(d.OrderDetailId, 0m);
                    var pending = d.Qty - received;

                    detailDtos.Add(new PurchaseOrderPickerDetailDto
                    {
                        PurchaseOrderDetailId = d.OrderDetailId,
                        ItemId = d.ItemId,
                        ItemName = d.Item?.ItemName ?? string.Empty,
                        ItemCode = d.Item?.ItemCode,
                        HsnCode = d.HsnCode,
                        PriceType = d.PriceType,
                        OrderedQty = d.Qty,
                        ReceivedQty = received,
                        PendingQty = pending,
                        Rate = d.Rate,
                        DiscountRate = d.DiscountRate,
                        AddisDiscountRate = d.AddisDiscountRate,
                        IsTaxIncluded = d.IsTaxIncluded
                    });
                }

                if (detailDtos.Any(d => d.PendingQty > 0))
                {
                    result.Add(new PurchaseOrderPickerDto
                    {
                        PurchaseOrderId = order.OrderId,
                        PurchaseOrderNo = order.OrderNo,
                        OrderDate = order.OrderDate,
                        ExpectedDeliveryDate = order.DeliveryDate,
                        NetTotal = order.NetTotal,                       
                        IsFullyReceived = false,
                        Details = detailDtos
                    });
                }
            }

            return result;
        }

        // ════════════════════════════════════════════════════
        // GET GRN PREFILL  (fills form from selected POs)
        // ════════════════════════════════════════════════════
        public async Task<GRNPrefillDto> GetGRNPrefillAsync(List<int> purchaseOrderIds)
        {
            var companyId = _common.GetCompanyId();

            if (!purchaseOrderIds.Any())
                throw new Exception("At least one purchase order must be selected.");

            var orders = await _context.PurchaseOrderMains
                .AsNoTracking()
                .Where(x =>
                    purchaseOrderIds.Contains(x.OrderId) &&
                    x.CompanyId == companyId &&
                    x.Status == "Confirmed" &&
                    !x.IsDeleted)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn).ThenInclude(h => h!.tax)
                .ToListAsync();

            if (orders.Count != purchaseOrderIds.Count)
                throw new Exception("One or more purchase orders not found or not in Confirmed status.");

            // All POs must belong to the same supplier
            var bpIds = orders.Select(o => o.BusinessPartnerId).Distinct().ToList();
            if (bpIds.Count > 1)
                throw new Exception("All selected purchase orders must belong to the same supplier.");

            var firstOrder = orders[0];

            var allDetailIds = orders
                .SelectMany(o => o.Details!.Select(d => d.OrderDetailId))
                .ToList();

            var receivedQtyMap = await GetReceivedQtyMapAsync(allDetailIds, companyId);

            var prefillDetails = new List<GRNPrefillDetailDto>();

            foreach (var order in orders)
            {
                foreach (var d in order.Details ?? Enumerable.Empty<PurchaseOrderDetail>())
                {
                    var received = receivedQtyMap.GetValueOrDefault(d.OrderDetailId, 0m);
                    var pending = d.Qty - received;

                    if (pending <= 0) continue;

                    var hsnTax = d.Hsn?.tax;

                    // For purchase (inbound): our state vs supplier state
                    bool isIntra = (firstOrder.PurchaseStateCode.HasValue && firstOrder.BillStateCode.HasValue)
                        ? firstOrder.PurchaseStateCode.Value == firstOrder.BillStateCode.Value
                        : true;

                    prefillDetails.Add(new GRNPrefillDetailDto
                    {
                        PurchaseOrderId = order.OrderId,
                        PurchaseOrderNo = order.OrderNo,
                        PurchaseOrderDetailId = d.OrderDetailId,
                        ItemId = d.ItemId,
                        ItemName = d.Item?.ItemName ?? string.Empty,
                        ItemCode = d.Item?.ItemCode,
                        HsnId = d.HsnId,
                        HsnCode = d.HsnCode,
                        PriceType = d.PriceType,
                        OrderedQty = d.Qty,
                        PreviouslyReceivedQty = received,
                        PendingQty = pending,
                        SuggestedReceivedQty = pending,
                        Rate = d.Rate,
                        DiscountRate = d.DiscountRate,
                        AddisDiscountRate = d.AddisDiscountRate,
                        IsTaxIncluded = d.IsTaxIncluded,
                        CgstRate = isIntra ? (hsnTax?.CGST ?? 0) : 0,
                        SgstRate = isIntra ? (hsnTax?.SGST ?? 0) : 0,
                        IgstRate = !isIntra ? (hsnTax?.IGST ?? 0) : 0,
                        CessRate = d.Hsn?.Cess ?? 0
                    });
                }
            }

            return new GRNPrefillDto
            {
                BusinessPartnerId = firstOrder.BusinessPartnerId,
                LocationId = firstOrder.LocationId,
                ContactPersonId = firstOrder.ContactPersonId,               
                BillAddressId = firstOrder.BillAddressId,
                ShipAddressId = firstOrder.ShipAddressId,
                PurchaseStateCode = firstOrder.PurchaseStateCode,
                BillStateCode = firstOrder.BillStateCode,
                Details = prefillDetails
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Build detail + tax
        // ════════════════════════════════════════════════════
        private async Task<GRNDetail> BuildDetailWithTaxAsync(
            CreateGRNDetailDto lineDto,
            int? purchaseStateCode, int? billStateCode, int companyId)
        {
            var item = await _context.Items
                .Include(i => i.Hsn).ThenInclude(h => h!.tax)
                .FirstOrDefaultAsync(i => i.ItemId == lineDto.ItemId && !i.IsDeleted)
                ?? throw new Exception($"Item {lineDto.ItemId} not found.");

            if (item.HSNCodeId == 0) throw new Exception($"Item '{item.ItemName}' has no HSN Code assigned.");
            if (item.Hsn == null) throw new Exception($"Item '{item.ItemName}' — HSN (Id: {item.HSNCodeId}) not found.");
            if (item.Hsn.tax == null) throw new Exception($"HSN '{item.Hsn.HsnName}' has no Tax assigned.");

            var hsn = item.Hsn;
            var tax = hsn.tax;

            // Default PO snapshot values when no PO is linked (free GRN)
            decimal orderedQty = 0;
            decimal previouslyReceived = 0;

            if (lineDto.PurchaseOrderDetailId.HasValue)
            {
                var orderDetail = await _context.PurchaseOrderDetails
                    .FirstOrDefaultAsync(d => d.OrderDetailId == lineDto.PurchaseOrderDetailId)
                    ?? throw new Exception($"Purchase order detail {lineDto.PurchaseOrderDetailId} not found.");

                orderedQty = orderDetail.Qty;

                var receivedQtyMap = await GetReceivedQtyMapAsync(
                    new List<int> { lineDto.PurchaseOrderDetailId.Value }, companyId);
                previouslyReceived = receivedQtyMap.GetValueOrDefault(lineDto.PurchaseOrderDetailId.Value, 0m);
            }

            // Tax calculation — identical logic to GoodsDeliveryService
            decimal grossAmount = lineDto.Rate * lineDto.ReceivedQty;
            decimal discountAmt = Math.Round(grossAmount * lineDto.DiscountRate / 100, 2);
            decimal afterFirst = grossAmount - discountAmt;
            decimal addisDiscAmt = Math.Round(afterFirst * lineDto.AddisDiscountRate / 100, 2);
            decimal taxableAmount = afterFirst - addisDiscAmt;

            bool isIntraState = (purchaseStateCode.HasValue && billStateCode.HasValue)
                ? purchaseStateCode.Value == billStateCode.Value
                : true;

            if (lineDto.IsTaxIncluded)
            {
                decimal totalTaxRate = isIntraState
                    ? (tax.CGST + tax.SGST)
                    : tax.IGST;
                if (totalTaxRate > 0)
                    taxableAmount = Math.Round(taxableAmount / (1 + totalTaxRate / 100), 2);
            }

            decimal igstAmount = !isIntraState ? Math.Round(taxableAmount * tax.IGST / 100, 2) : 0;
            decimal cgstAmount = isIntraState ? Math.Round(taxableAmount * tax.CGST / 100, 2) : 0;
            decimal sgstAmount = isIntraState ? Math.Round(taxableAmount * tax.SGST / 100, 2) : 0;
            decimal cessRate = hsn.Cess ?? 0;
            decimal cessAmount = Math.Round(taxableAmount * cessRate / 100, 2);
            decimal lineTaxAmt = igstAmount + cgstAmount + sgstAmount + cessAmount;

            return new GRNDetail
            {
                PurchaseOrderId = lineDto.PurchaseOrderId,
                PurchaseOrderDetailId = lineDto.PurchaseOrderDetailId,
                ItemId = lineDto.ItemId,
                HsnId = hsn.HsnId,
                HsnCode = hsn.HsnName,
                PriceType = lineDto.PriceType,
                OrderedQty = orderedQty,
                PreviouslyReceivedQty = previouslyReceived,
                ReceivedQty = lineDto.ReceivedQty,
                Rate = lineDto.Rate,
                DiscountRate = lineDto.DiscountRate,
                AddisDiscountRate = lineDto.AddisDiscountRate,
                DiscountAmount = discountAmt,
                AddisDiscountAmount = addisDiscAmt,
                IsTaxIncluded = lineDto.IsTaxIncluded,
                TaxableAmount = taxableAmount,
                CessRate = cessRate,
                CessAmount = cessAmount,
                LineTaxAmount = lineTaxAmt,
                LineTotal = taxableAmount + lineTaxAmt,
                TaxDetails = new List<GRNTaxDetail>
                {
                    new()
                    {
                        TaxId = tax.TaxId,
                        IGSTRate = isIntraState ? 0 : tax.IGST,
                        CGSTRate = isIntraState ? tax.CGST : 0,
                        SGSTRate = isIntraState ? tax.SGST : 0,
                        TaxableAmount = taxableAmount,
                        IGSTAmount = igstAmount,
                        CGSTAmount = cgstAmount,
                        SGSTAmount = sgstAmount,
                        CessRate = cessRate,
                        CessAmount = cessAmount,
                        TotalTaxAmount = lineTaxAmt
                    }
                }
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Validate source POs
        // All POs must be Confirmed and belong to same supplier
        // ════════════════════════════════════════════════════
        private async Task ValidateSourcePurchaseOrdersAsync(
            List<int> poIds, int businessPartnerId, int companyId)
        {
            var orders = await _context.PurchaseOrderMains
                .Where(x =>
                    poIds.Contains(x.OrderId) &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                .ToListAsync();

            if (orders.Count != poIds.Count)
                throw new Exception("One or more source purchase orders not found.");

            foreach (var order in orders)
            {
                if (order.Status != "Confirmed")
                    throw new Exception($"Purchase Order {order.OrderNo} is not Confirmed. Only Confirmed POs can be received.");

                if (order.BusinessPartnerId != businessPartnerId)
                    throw new Exception($"Purchase Order {order.OrderNo} does not belong to the selected supplier.");
            }
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Validate received quantities
        // Ensures no over-receiving of any PO line
        // ════════════════════════════════════════════════════
        private async Task ValidateReceivedQtysAsync(
            List<CreateGRNDetailDto> lines,
            int companyId,
            int? excludeGRNId = null)
        {
            var detailIds = lines
                .Where(l => l.PurchaseOrderDetailId.HasValue)
                .Select(l => l.PurchaseOrderDetailId!.Value)
                .Distinct()
                .ToList();

            if (!detailIds.Any()) return;  // Free GRNs (no PO link) skip over-receiving check

            var receivedQtyMap = await GetReceivedQtyMapAsync(detailIds, companyId, excludeGRNId);

            var orderDetails = await _context.PurchaseOrderDetails
                .Where(d => detailIds.Contains(d.OrderDetailId))
                .ToListAsync();

            foreach (var line in lines)
            {
                if (line.ReceivedQty <= 0)
                    throw new Exception("Received quantity must be greater than zero.");

                if (!line.PurchaseOrderDetailId.HasValue) continue;

                var orderDetail = orderDetails.FirstOrDefault(
                    d => d.OrderDetailId == line.PurchaseOrderDetailId)
                    ?? throw new Exception($"Purchase order detail {line.PurchaseOrderDetailId} not found.");

                var alreadyReceived = receivedQtyMap.GetValueOrDefault(line.PurchaseOrderDetailId.Value, 0m);
                var pendingQty = orderDetail.Qty - alreadyReceived;

                if (line.ReceivedQty > pendingQty)
                    throw new Exception(
                        $"Received quantity ({line.ReceivedQty}) exceeds pending quantity ({pendingQty}) " +
                        $"for purchase order detail {line.PurchaseOrderDetailId}.");
            }
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Get already-received qty per PO detail
        // Counts only Confirmed GRNs (Draft/Cancelled excluded)
        // ════════════════════════════════════════════════════
        private async Task<Dictionary<int, decimal>> GetReceivedQtyMapAsync(
            List<int> purchaseOrderDetailIds,
            int companyId,
            int? excludeGRNId = null)
        {
            if (!purchaseOrderDetailIds.Any())
                return new Dictionary<int, decimal>();

            var query = _context.GRNDetails
                .Where(d =>
                    d.PurchaseOrderDetailId.HasValue &&
                    purchaseOrderDetailIds.Contains(d.PurchaseOrderDetailId!.Value) &&
                    d.GRN!.CompanyId == companyId &&
                    d.GRN!.Status == "Confirmed" &&
                    !d.GRN.IsDeleted);

            if (excludeGRNId.HasValue)
                query = query.Where(d => d.GRNId != excludeGRNId.Value);

            return await query
                .GroupBy(d => d.PurchaseOrderDetailId!.Value)
                .Select(g => new
                {
                    PurchaseOrderDetailId = g.Key,
                    TotalReceived = g.Sum(x => x.ReceivedQty)
                })
                .ToDictionaryAsync(x => x.PurchaseOrderDetailId, x => x.TotalReceived);
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Validate header
        // ════════════════════════════════════════════════════
        private async Task ValidateHeaderAsync(
            int businessPartnerId, int locationId, int companyId,
            int? purchaseStateCode, int? billStateCode,
            int? contactPersonId, int? purchasePersonId,
            int? billAddressId, int? shipAddressId)
        {
            var bpExists = await _context.BusinessPartners
                .AnyAsync(x =>
                    x.BusinessPartnerId == businessPartnerId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!bpExists) throw new Exception("Business Partner (Supplier) not found.");

            var locationExists = await _context.Locations
                .AnyAsync(x => x.LocationId == locationId && x.CompanyId == companyId);
            if (!locationExists) throw new Exception("Location not found.");

            if (purchaseStateCode.HasValue && !Enum.IsDefined(typeof(GstState), purchaseStateCode.Value))
                throw new Exception("Invalid Purchase State Code.");

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

        // ════════════════════════════════════════════════════
        // PRIVATE — Generate GRN number
        // ════════════════════════════════════════════════════
        private async Task<string> GenerateGRNNoAsync(int companyId, int finYearId)
        {
            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(x => x.FinancialYearId == finYearId);

            var yearLabel = financialYear != null
                ? $"{financialYear.StartDate.Year % 100}{financialYear.EndDate.Year % 100}"
                : finYearId.ToString();

            var count = await _context.GRNMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"GRN-{yearLabel}-{(count + 1):D4}";
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Map to response DTO
        // ════════════════════════════════════════════════════
        private GRNResponseDto MapToResponseDto(GRNMain main)
        {
            return new GRNResponseDto
            {
                GRNId = main.GRNId,
                FinYearId = main.FinYearId,
                GRNNo = main.GRNNo,
                GRNDate = main.GRNDate,
                Status = main.Status,
                SupplierInvoiceNo = main.SupplierInvoiceNo,
                SupplierInvoiceDate = main.SupplierInvoiceDate,
                RefNo = main.RefNo,
                RefDate = main.RefDate,
                Remarks = main.Remarks,

                BusinessPartnerId = main.BusinessPartnerId,
                BusinessPartnerName = main.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                BusinessPartnerCode = main.BusinessPartner?.BusinessPartnerCode ?? string.Empty,

                LocationId = main.LocationId,
                LocationName = main.Location?.LocationName ?? string.Empty,

                PurchaseStateCode = main.PurchaseStateCode,
                PurchaseStateName = main.PurchaseStateCode.HasValue
                    ? ((GstState)main.PurchaseStateCode.Value).ToString() : null,

                BillStateCode = main.BillStateCode,
                BillStateName = main.BillStateCode.HasValue
                    ? ((GstState)main.BillStateCode.Value).ToString() : null,

                ContactPersonId = main.ContactPersonId,
                ContactPersonName = main.ContactPerson?.Name,
                ContactPersonMobile = main.ContactPerson?.Mobile,
                             

                BillAddressId = main.BillAddressId,
                BillAddressLine = FormatAddress(main.BillAddress),
                ShipAddressId = main.ShipAddressId,
                ShipAddressLine = FormatAddress(main.ShipAddress),

                SubTotal = main.SubTotal,
                TaxAmount = main.TaxAmount,
                CessAmount = main.CessAmount,
                RoundOff = main.RoundOff,
                NetTotal = main.NetTotal,
                Discount = main.Details?.Sum(d => d.DiscountAmount + d.AddisDiscountAmount) ?? 0,

                CreatedBy = (int)(main.CreatedBy ?? 0),
                CreatedDate = main.CreatedDate,
                ModifiedBy = main.ModifiedBy,
                ModifiedDate = main.ModifiedDate,

                Details = main.Details?.Select(d => new GRNDetailResponseDto
                {
                    GRNDetailId = d.GRNDetailId,
                    GRNId = d.GRNId,
                    PurchaseOrderId = d.PurchaseOrderId,
                    PurchaseOrderNo = d.PurchaseOrder?.OrderNo,
                    PurchaseOrderDetailId = d.PurchaseOrderDetailId,
                    ItemId = d.ItemId,
                    ItemName = d.Item?.ItemName ?? string.Empty,
                    ItemCode = d.Item?.ItemCode,
                    HsnId = d.HsnId,
                    HsnCode = d.HsnCode,
                    PriceType = d.PriceType,
                    OrderedQty = d.OrderedQty,
                    PreviouslyReceivedQty = d.PreviouslyReceivedQty,
                    ReceivedQty = d.ReceivedQty,
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
                }).ToList() ?? new(),

                TaxDetails = main.Details?
                    .Where(d => d.TaxDetails != null)
                    .SelectMany(d => d.TaxDetails!.Select(MapTaxDetailDto))
                    .ToList() ?? new()
            };
        }

        private static string? FormatAddress(BusinessPartnerAddress? addr) =>
            addr == null ? null :
            string.Join(", ", new[]
            {
                addr.AddressLine1, addr.AddressLine2, addr.City,
                addr.State.HasValue ? ((GstState)addr.State.Value).ToString() : null,
                addr.Pincode
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

        private static List<GRNTaxDetailResponseDto> MapTaxDetails(
            IEnumerable<GRNTaxDetail>? rows) =>
            rows?.Select(MapTaxDetailDto).ToList() ?? new();

        private static GRNTaxDetailResponseDto MapTaxDetailDto(GRNTaxDetail td) =>
            new()
            {
                TaxDetailId = td.TaxDetailId,
                GRNDetailId = td.GRNDetailId,
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
                TotalTaxAmount = td.TotalTaxAmount
            };

        private async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Database error: {inner}");
            }
        }
    }
}
