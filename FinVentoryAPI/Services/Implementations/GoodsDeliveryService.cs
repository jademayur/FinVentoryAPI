using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.GoodsDeliveryDTOs;
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
    public class GoodsDeliveryService : IGoodsDeliveryService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public GoodsDeliveryService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════════
        // CREATE
        // ════════════════════════════════════════════════════
        public async Task<GoodsDeliveryResponseDto> CreateAsync(CreateGoodsDeliveryMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            // ── Validate source orders & collect distinct order IDs ───────────────
            var orderIds = dto.Details.Select(d => d.OrderId).Distinct().ToList();
            await ValidateSourceOrdersAsync(orderIds, dto.BusinessPartnerId, companyId);

            // ── Validate each line: delivery qty must not exceed pending qty ───────
            await ValidateDeliveryQtysAsync(dto.Details, companyId);

            var deliveryNo = await GenerateDeliveryNoAsync(companyId, finYearId);

            var main = new GoodsDeliveryMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                DeliveryNo = deliveryNo,
                DeliveryDate = dto.DeliveryDate,
                RefNo = dto.RefNo,
                RefDate = dto.RefDate,
                Remarks = dto.Remarks,
                Status = "Draft",
                BusinessPartnerId = dto.BusinessPartnerId,
                LocationId = dto.LocationId,
                ContactPersonId = dto.ContactPersonId,
                SalesPersonId = dto.SalesPersonId,
                BillAddressId = dto.BillAddressId,
                ShipAddressId = dto.ShipAddressId,
                SalesStateCode = dto.SalesStateCode,
                BillStateCode = dto.BillStateCode,
                RoundOff = dto.RoundOff,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                Details = new List<GoodsDeliveryDetail>(),
                TaxDetails = new List<GoodsDeliveryTaxDetail>(),
                // OrderLinks = new List<GoodsDeliveryOrderLink>()
            };

            // ── Build order-link junction rows ────────────────────────────────────
            //foreach (var orderId in orderIds)
            //    main.OrderLinks.Add(new GoodsDeliveryOrderLink { OrderId = orderId });

            // ── Build detail lines ────────────────────────────────────────────────
            decimal totalSubTotal = 0, totalTaxAmount = 0, totalCessAmount = 0;

            foreach (var lineDto in dto.Details)
            {
                var detail = await BuildDetailWithTaxAsync(lineDto, dto.SalesStateCode, dto.BillStateCode, companyId);

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                foreach (var td in detail.TaxDetails ?? Enumerable.Empty<GoodsDeliveryTaxDetail>())
                {
                    td.Delivery = main;
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
                _context.GoodsDeliveryMains.Add(main);
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(main.DeliveryId)
                ?? throw new Exception("Failed to retrieve saved delivery.");
        }

        // ════════════════════════════════════════════════════
        // UPDATE  (Draft only)
        // ════════════════════════════════════════════════════
        public async Task<bool> UpdateAsync(int id, UpdateGoodsDeliveryMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.GoodsDeliveryMains
                .Include(m => m.Details!).ThenInclude(d => d.TaxDetails)
                .Include(m => m.TaxDetails)
                //.Include(m => m.OrderLinks)
                .FirstOrDefaultAsync(x =>
                    x.DeliveryId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Goods Delivery not found.");

            if (main.Status != "Draft")
                throw new Exception("Only Draft deliveries can be updated.");

            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            var orderIds = dto.Details.Select(d => d.OrderId).Distinct().ToList();
            await ValidateSourceOrdersAsync(orderIds, dto.BusinessPartnerId, companyId);

            // Validate delivery qtys; exclude the current delivery from "already delivered" count
            await ValidateDeliveryQtysAsync(
                dto.Details.Select(d => new CreateGoodsDeliveryDetailDto
                {
                    OrderId = d.OrderId,
                    OrderDetailId = d.OrderDetailId,
                    ItemId = d.ItemId,
                    PriceType = d.PriceType,
                    DeliveryQty = d.DeliveryQty,
                    Rate = d.Rate,
                    DiscountRate = d.DiscountRate,
                    AddisDiscountRate = d.AddisDiscountRate,
                    IsTaxIncluded = d.IsTaxIncluded,
                    ManualTaxId = d.ManualTaxId,
                    ManualIgstRate = d.ManualIgstRate,
                    ManualCgstRate = d.ManualCgstRate,
                    ManualSgstRate = d.ManualSgstRate,
                    ManualCessRate = d.ManualCessRate
                }).ToList(),
                companyId,
                excludeDeliveryId: id);

            // ── Build fresh detail objects ────────────────────────────────────────
            var incomingDetails = new List<GoodsDeliveryDetail>();
            foreach (var lineDto in dto.Details)
            {
                var createDto = new CreateGoodsDeliveryDetailDto
                {
                    OrderId = lineDto.OrderId,
                    OrderDetailId = lineDto.OrderDetailId,
                    ItemId = lineDto.ItemId,
                    PriceType = lineDto.PriceType,
                    DeliveryQty = lineDto.DeliveryQty,
                    Rate = lineDto.Rate,
                    DiscountRate = lineDto.DiscountRate,
                    AddisDiscountRate = lineDto.AddisDiscountRate,
                    IsTaxIncluded = lineDto.IsTaxIncluded,
                    ManualTaxId = lineDto.ManualTaxId,
                    ManualIgstRate = lineDto.ManualIgstRate,
                    ManualCgstRate = lineDto.ManualCgstRate,
                    ManualSgstRate = lineDto.ManualSgstRate,
                    ManualCessRate = lineDto.ManualCessRate
                };
                incomingDetails.Add(await BuildDetailWithTaxAsync(createDto, dto.SalesStateCode, dto.BillStateCode, companyId));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Header fields
                main.DeliveryDate = dto.DeliveryDate;
                main.RefNo = dto.RefNo;
                main.RefDate = dto.RefDate;
                main.Remarks = dto.Remarks;
                main.BusinessPartnerId = dto.BusinessPartnerId;
                main.LocationId = dto.LocationId;
                main.ContactPersonId = dto.ContactPersonId;
                main.SalesPersonId = dto.SalesPersonId;
                main.BillAddressId = dto.BillAddressId;
                main.ShipAddressId = dto.ShipAddressId;
                main.SalesStateCode = dto.SalesStateCode;
                main.BillStateCode = dto.BillStateCode;
                main.RoundOff = dto.RoundOff;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

                // ── Sync order links ──────────────────────────────────────────────
                //var existingLinks = main.OrderLinks?.ToList() ?? new();
                //var toRemoveLinks = existingLinks.Where(l => !orderIds.Contains(l.OrderId)).ToList();
                //var toAddLinks = orderIds
                //    .Where(oid => !existingLinks.Any(l => l.OrderId == oid))
                //    .Select(oid => new GoodsDeliveryOrderLink { DeliveryId = id, OrderId = oid })
                //    .ToList();

                //_context.GoodsDeliveryOrderLinks.RemoveRange(toRemoveLinks);
                //_context.GoodsDeliveryOrderLinks.AddRange(toAddLinks);

                // ── Sync detail lines (same positional merge pattern as SalesOrder) ─
                var existingDetails = main.Details!.ToList();
                decimal totalSubTotal = 0, totalTaxAmount = 0, totalCessAmount = 0;

                for (int i = 0; i < incomingDetails.Count; i++)
                {
                    var incoming = incomingDetails[i];

                    if (i < existingDetails.Count)
                    {
                        var existing = existingDetails[i];

                        existing.OrderId = incoming.OrderId;
                        existing.OrderDetailId = incoming.OrderDetailId;
                        existing.ItemId = incoming.ItemId;
                        existing.HsnId = incoming.HsnId;
                        existing.HsnCode = incoming.HsnCode;
                        existing.PriceType = incoming.PriceType;
                        existing.OrderedQty = incoming.OrderedQty;
                        existing.PreviouslyDeliveredQty = incoming.PreviouslyDeliveredQty;
                        existing.DeliveryQty = incoming.DeliveryQty;
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
                                inTax.DeliveryId = main.DeliveryId;
                                inTax.DeliveryDetailId = existing.DeliveryDetailId;
                                _context.GoodsDeliveryTaxDetails.Add(inTax);
                            }
                        }

                        if (existingTaxList.Count > incomingTaxList.Count)
                            _context.GoodsDeliveryTaxDetails.RemoveRange(
                                existingTaxList.Skip(incomingTaxList.Count));

                        totalSubTotal += existing.TaxableAmount;
                        totalCessAmount += existing.CessAmount;
                        totalTaxAmount += existing.LineTaxAmount - existing.CessAmount;
                    }
                    else
                    {
                        incoming.DeliveryId = main.DeliveryId;
                        var newTaxList = incoming.TaxDetails ?? new();
                        incoming.TaxDetails = null;

                        _context.GoodsDeliveryDetails.Add(incoming);
                        await SaveChangesAsync();

                        foreach (var inTax in newTaxList)
                        {
                            inTax.DeliveryId = main.DeliveryId;
                            inTax.DeliveryDetailId = incoming.DeliveryDetailId;
                            _context.GoodsDeliveryTaxDetails.Add(inTax);
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
                            _context.GoodsDeliveryTaxDetails.RemoveRange(sd.TaxDetails);

                    _context.GoodsDeliveryDetails.RemoveRange(surplus);
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

            var main = await _context.GoodsDeliveryMains
                .FirstOrDefaultAsync(x =>
                    x.DeliveryId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Goods Delivery not found.");

            if (main.Status != "Draft")
                throw new Exception("Only Draft deliveries can be deleted.");

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

            var main = await _context.GoodsDeliveryMains
                .FirstOrDefaultAsync(x =>
                    x.DeliveryId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Goods Delivery not found.");

            if (main.Status != "Draft")
                throw new Exception("Only Draft deliveries can be confirmed.");

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

            var main = await _context.GoodsDeliveryMains
                .FirstOrDefaultAsync(x =>
                    x.DeliveryId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Goods Delivery not found.");

            if (main.Status == "Cancelled")
                throw new Exception("Delivery is already cancelled.");

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
        public async Task<List<GoodsDeliveryResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var list = await _context.GoodsDeliveryMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.ContactPerson)
                .Include(x => x.SalesPerson)
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                //.Include(x => x.OrderLinks!).ThenInclude(l => l.Order)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.Order)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .OrderByDescending(x => x.DeliveryDate)
                .ToListAsync();

            return list.Select(MapToResponseDto).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET BY ID
        // ════════════════════════════════════════════════════
        public async Task<GoodsDeliveryResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.GoodsDeliveryMains
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.ContactPerson)
                .Include(x => x.SalesPerson)
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                // .Include(x => x.OrderLinks!).ThenInclude(l => l.Order)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.Order)
                .Include(x => x.Details!).ThenInclude(d => d.OrderDetail)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .FirstOrDefaultAsync(x =>
                    x.DeliveryId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            return main == null ? null : MapToResponseDto(main);
        }

        // ════════════════════════════════════════════════════
        // GET PAGED
        // ════════════════════════════════════════════════════
        public async Task<PagedResponseDto<GoodsDeliveryResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.GoodsDeliveryMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.ContactPerson)
                .Include(x => x.SalesPerson)
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                .Include(x => x.Details!)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.DeliveryNo.ToLower().Contains(search) ||
                    x.BusinessPartner!.BusinessPartnerName.ToLower().Contains(search) ||
                    (x.RefNo != null && x.RefNo.ToLower().Contains(search)));
            }

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
                    query = query.Where(x => x.DeliveryDate >= fromDate);
                }
                if (request.Filters.ContainsKey("toDate"))
                {
                    var toDate = ((JsonElement)request.Filters["toDate"]).GetDateTime();
                    query = query.Where(x => x.DeliveryDate <= toDate);
                }
                if (request.Filters.ContainsKey("salesPersonId"))
                {
                    var spId = ((JsonElement)request.Filters["salesPersonId"]).GetInt32();
                    query = query.Where(x => x.SalesPersonId == spId);
                }
                //if (request.Filters.ContainsKey("orderId"))
                //{
                //    var orderId = ((JsonElement)request.Filters["orderId"]).GetInt32();
                //    query = query.Where(x =>
                //        x.OrderLinks!.Any(l => l.OrderId == orderId));
                //}
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "deliveryno" => sort.Direction == "desc" ? query.OrderByDescending(x => x.DeliveryNo) : query.OrderBy(x => x.DeliveryNo),
                    "deliverydate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.DeliveryDate) : query.OrderBy(x => x.DeliveryDate),
                    "businesspartnername" => sort.Direction == "desc" ? query.OrderByDescending(x => x.BusinessPartner!.BusinessPartnerName) : query.OrderBy(x => x.BusinessPartner!.BusinessPartnerName),
                    "salesperson" => sort.Direction == "desc" ? query.OrderByDescending(x => x.SalesPerson!.SalesPersonName) : query.OrderBy(x => x.SalesPerson!.SalesPersonName),
                    "status" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "nettotal" => sort.Direction == "desc" ? query.OrderByDescending(x => x.NetTotal) : query.OrderBy(x => x.NetTotal),
                    _ => query.OrderByDescending(x => x.DeliveryDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.DeliveryDate);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                //.Include(x => x.OrderLinks!).ThenInclude(l => l.Order)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .ToListAsync();

            return new PagedResponseDto<GoodsDeliveryResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapToResponseDto).ToList()
            };
        }

        // ════════════════════════════════════════════════════
        // GET ORDERS FOR CUSTOMER  (picker dropdown)
        // Returns only Confirmed orders that still have
        // pending (un-delivered) qty on at least one line.
        // ════════════════════════════════════════════════════
        public async Task<List<OrderPickerDto>> GetOrdersForCustomerAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();

            var orders = await _context.SalesOrderMains
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.BusinessPartnerId == businessPartnerId &&
                    x.Status == "Confirmed" &&
                    !x.IsDeleted)
                .Include(x => x.SalesPerson)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            // Fetch already-confirmed delivery quantities per order detail
            var deliveredQtyMap = await GetDeliveredQtyMapAsync(
                orders.SelectMany(o => o.Details!.Select(d => d.OrderDetailId)).ToList(),
                companyId);

            var result = new List<OrderPickerDto>();

            foreach (var order in orders)
            {
                var detailDtos = new List<OrderPickerDetailDto>();

                foreach (var d in order.Details ?? Enumerable.Empty<SalesOrderDetail>())
                {
                    var delivered = deliveredQtyMap.GetValueOrDefault(d.OrderDetailId, 0m);
                    var pending = d.Qty - delivered;

                    detailDtos.Add(new OrderPickerDetailDto
                    {
                        OrderDetailId = d.OrderDetailId,
                        ItemId = d.ItemId,
                        ItemName = d.Item?.ItemName ?? string.Empty,
                        ItemCode = d.Item?.ItemCode,
                        HsnCode = d.HsnCode,
                        PriceType = d.PriceType,
                        OrderedQty = d.Qty,
                        DeliveredQty = delivered,
                        PendingQty = pending,
                        Rate = d.Rate,
                        DiscountRate = d.DiscountRate,
                        AddisDiscountRate = d.AddisDiscountRate,
                        IsTaxIncluded = d.IsTaxIncluded
                    });
                }

                // Include only orders that have at least one line with pending qty
                if (detailDtos.Any(d => d.PendingQty > 0))
                {
                    result.Add(new OrderPickerDto
                    {
                        OrderId = order.OrderId,
                        OrderNo = order.OrderNo,
                        OrderDate = order.OrderDate,
                        DeliveryDate = order.DeliveryDate,
                        NetTotal = order.NetTotal,
                        SalesPersonId = order.SalesPersonId,
                        SalesPersonName = order.SalesPerson?.SalesPersonName,
                        IsFullyDelivered = false,
                        Details = detailDtos
                    });
                }
            }

            return result;
        }

        // ════════════════════════════════════════════════════
        // GET DELIVERY PREFILL  (fills form from selected orders)
        // ════════════════════════════════════════════════════
        public async Task<DeliveryPrefillDto> GetDeliveryPrefillAsync(List<int> orderIds)
        {
            var companyId = _common.GetCompanyId();

            if (!orderIds.Any())
                throw new Exception("At least one order must be selected.");

            var orders = await _context.SalesOrderMains
    .AsNoTracking()
    .Where(x =>
        orderIds.Contains(x.OrderId) &&
        x.CompanyId == companyId &&
        x.Status == "Confirmed" &&
        !x.IsDeleted)
    .Include(x => x.Details!).ThenInclude(d => d.Item)
    .Include(x => x.Details!).ThenInclude(d => d.Hsn).ThenInclude(h => h!.tax)  // ✅ added .tax
    .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax) // ✅ added to detect manual tax
    .ToListAsync();

            if (orders.Count != orderIds.Count)
                throw new Exception("One or more orders not found or not in Confirmed status.");

            // All orders must belong to the same business partner
            var bpIds = orders.Select(o => o.BusinessPartnerId).Distinct().ToList();
            if (bpIds.Count > 1)
                throw new Exception("All selected orders must belong to the same customer.");

            var firstOrder = orders[0];

            // Fetch already-delivered qtys for all relevant order details
            var allDetailIds = orders
                .SelectMany(o => o.Details!.Select(d => d.OrderDetailId))
                .ToList();

            var deliveredQtyMap = await GetDeliveredQtyMapAsync(allDetailIds, companyId);

            var prefillDetails = new List<DeliveryPrefillDetailDto>();

            foreach (var order in orders)
            {
                foreach (var d in order.Details ?? Enumerable.Empty<SalesOrderDetail>())
                {
                    var delivered = deliveredQtyMap.GetValueOrDefault(d.OrderDetailId, 0m);
                    var pending = d.Qty - delivered;

                    if (pending <= 0) continue;

                    bool isIntra = (firstOrder.SalesStateCode.HasValue && firstOrder.BillStateCode.HasValue)
                        ? firstOrder.SalesStateCode.Value == firstOrder.BillStateCode.Value
                        : true;

                    // ── Detect whether the order line used a manual / HSN-overridden tax ──
                    // The order line's snapshot tax (TaxDetails[0]) reflects whatever was
                    // actually applied — manual override (incl. 0%) or HSN-derived tax.
                    var orderTax = d.TaxDetails?.FirstOrDefault();
                    bool hasHsn = d.HsnId != 0;

                    // If the order line's stored tax doesn't match the item's current
                    // HSN-derived tax, treat it as a manual override and carry it forward.
                    var hsnTax = d.Hsn?.tax;

                    decimal hsnIgst = isIntra ? 0 : (hsnTax?.IGST ?? 0);
                    decimal hsnCgst = isIntra ? (hsnTax?.CGST ?? 0) : 0;
                    decimal hsnSgst = isIntra ? (hsnTax?.SGST ?? 0) : 0;
                    decimal hsnCess = d.Hsn?.Cess ?? 0;

                    bool isManualTax = !hasHsn; // no HSN ⇒ definitely manual
                    int? manualTaxId = null;

                    if (orderTax != null)
                    {
                        bool ratesDiffer =
                            orderTax.IGSTRate != hsnIgst ||
                            orderTax.CGSTRate != hsnCgst ||
                            orderTax.SGSTRate != hsnSgst ||
                            orderTax.CessRate != hsnCess ||
                            orderTax.TaxId != (hsnTax?.TaxId ?? 0);

                        if (!hasHsn || ratesDiffer)
                        {
                            isManualTax = true;
                            manualTaxId = orderTax.TaxId; // 0 ⇒ zero/export tax
                        }
                    }

                    prefillDetails.Add(new DeliveryPrefillDetailDto
                    {
                        OrderId = order.OrderId,
                        OrderNo = order.OrderNo,
                        OrderDetailId = d.OrderDetailId,
                        ItemId = d.ItemId,
                        ItemName = d.Item?.ItemName ?? string.Empty,
                        ItemCode = d.Item?.ItemCode,
                        HsnId = d.HsnId,
                        HsnCode = d.HsnCode,
                        PriceType = d.PriceType,
                        OrderedQty = d.Qty,
                        PreviouslyDeliveredQty = delivered,
                        PendingQty = pending,
                        SuggestedDeliveryQty = pending,
                        Rate = d.Rate,
                        DiscountRate = d.DiscountRate,
                        AddisDiscountRate = d.AddisDiscountRate,
                        IsTaxIncluded = d.IsTaxIncluded,

                        // Tax rates — reflect whatever was actually applied on the order
                        // (manual override or HSN-derived), so the Angular form recalcs correctly.
                        CgstRate = isManualTax ? (orderTax?.CGSTRate ?? 0) : hsnCgst,
                        SgstRate = isManualTax ? (orderTax?.SGSTRate ?? 0) : hsnSgst,
                        IgstRate = isManualTax ? (orderTax?.IGSTRate ?? 0) : hsnIgst,
                        CessRate = isManualTax ? (orderTax?.CessRate ?? 0) : hsnCess,

                        // Manual-tax passthrough so the Angular form pre-selects the
                        // same tax mode the order line used.
                        IsManualTax = isManualTax,
                        ManualTaxId = manualTaxId,
                    });
                }
            }



            return new DeliveryPrefillDto
            {
                BusinessPartnerId = firstOrder.BusinessPartnerId,
                LocationId = firstOrder.LocationId,
                ContactPersonId = firstOrder.ContactPersonId,
                SalesPersonId = firstOrder.SalesPersonId,
                BillAddressId = firstOrder.BillAddressId,
                ShipAddressId = firstOrder.ShipAddressId,
                SalesStateCode = firstOrder.SalesStateCode,
                BillStateCode = firstOrder.BillStateCode,
                Details = prefillDetails
            };
        }

        public async Task<List<DeliveryPickerDto>> GetDeliveriesForCustomerAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();

            var deliveries = await _context.GoodsDeliveryMains
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.BusinessPartnerId == businessPartnerId &&
                    x.Status == "Confirmed" &&
                    !x.IsDeleted)
                .Include(x => x.SalesPerson)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .OrderByDescending(x => x.DeliveryDate)
                .ToListAsync();

            // Get already-invoiced qty per delivery detail
            var allDetailIds = deliveries
                .SelectMany(d => d.Details!.Select(x => x.DeliveryDetailId))
                .ToList();

            var invoicedQtyMap = await GetInvoicedQtyByDeliveryDetailMapAsync(allDetailIds, companyId);

            var result = new List<DeliveryPickerDto>();

            foreach (var delivery in deliveries)
            {
                var detailDtos = new List<DeliveryPickerDetailDto>();

                foreach (var d in delivery.Details ?? Enumerable.Empty<GoodsDeliveryDetail>())
                {
                    var invoiced = invoicedQtyMap.GetValueOrDefault(d.DeliveryDetailId, 0m);
                    var pending = d.DeliveryQty - invoiced;

                    detailDtos.Add(new DeliveryPickerDetailDto
                    {
                        DeliveryDetailId = d.DeliveryDetailId,
                        ItemId = d.ItemId,
                        ItemName = d.Item?.ItemName ?? string.Empty,
                        ItemCode = d.Item?.ItemCode,
                        HsnCode = d.HsnCode,
                        PriceType = d.PriceType,
                        DeliveredQty = d.DeliveryQty,
                        InvoicedQty = invoiced,
                        PendingQty = pending,
                        Rate = d.Rate,
                        DiscountRate = d.DiscountRate,
                        AddisDiscountRate = d.AddisDiscountRate,
                        IsTaxIncluded = d.IsTaxIncluded
                    });
                }

                // Only include deliveries that have at least one line with pending qty
                if (detailDtos.Any(d => d.PendingQty > 0))
                {
                    result.Add(new DeliveryPickerDto
                    {
                        DeliveryId = delivery.DeliveryId,
                        DeliveryNo = delivery.DeliveryNo,
                        DeliveryDate = delivery.DeliveryDate,
                        NetTotal = delivery.NetTotal,
                        SalesPersonId = delivery.SalesPersonId,
                        SalesPersonName = delivery.SalesPerson?.SalesPersonName,
                        Details = detailDtos.Where(d => d.PendingQty > 0).ToList()
                    });
                }
            }

            return result;
        }

        private async Task<Dictionary<int, decimal>> GetInvoicedQtyByDeliveryDetailMapAsync(
            List<int> deliveryDetailIds, int companyId)
        {
            if (!deliveryDetailIds.Any())
                return new Dictionary<int, decimal>();

            return await _context.SalesInvoiceDetails
                .Where(d =>
                    d.DeliveryDetailId.HasValue &&
                    deliveryDetailIds.Contains(d.DeliveryDetailId.Value) &&
                    d.Invoice!.CompanyId == companyId &&
                    d.Invoice.Status != "Cancelled" &&
                    !d.Invoice.IsDeleted)
                .GroupBy(d => d.DeliveryDetailId!.Value)
                .Select(g => new { DeliveryDetailId = g.Key, Total = g.Sum(x => x.Qty) })
                .ToDictionaryAsync(x => x.DeliveryDetailId, x => x.Total);
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Build detail + tax
        // ════════════════════════════════════════════════════
        private async Task<GoodsDeliveryDetail> BuildDetailWithTaxAsync(
            CreateGoodsDeliveryDetailDto lineDto,
            int? salesStateCode, int? billStateCode, int companyId)
        {
            var item = await _context.Items
                .Include(i => i.Hsn).ThenInclude(h => h!.tax)
                .FirstOrDefaultAsync(i => i.ItemId == lineDto.ItemId && !i.IsDeleted)
                ?? throw new Exception($"Item {lineDto.ItemId} not found.");

            bool isIntraState = (salesStateCode.HasValue && billStateCode.HasValue)
                ? salesStateCode.Value == billStateCode.Value
                : true;

            int hsnId = 0;
            string? hsnCode = null;
            decimal igstRate, cgstRate, sgstRate, cessRate;
            int taxId;

            bool hasHsn = item.HSNCodeId != 0 && item.Hsn != null;

            if (lineDto.ManualTaxId.HasValue)
            {
                // ── Manual tax override (with or without HSN) ──
                hsnId = hasHsn ? item.Hsn!.HsnId : 0;
                hsnCode = hasHsn ? item.Hsn!.HsnName : null;

                if (lineDto.ManualTaxId.Value == 0)
                {
                    // 0% / export / exempt
                    taxId = 0;
                    igstRate = 0;
                    cgstRate = 0;
                    sgstRate = 0;
                    cessRate = 0;
                }
                else
                {
                    var manualTax = await _context.Taxes
                        .FirstOrDefaultAsync(t => t.TaxId == lineDto.ManualTaxId.Value)
                        ?? throw new Exception($"Tax {lineDto.ManualTaxId.Value} not found.");

                    taxId = manualTax.TaxId;
                    igstRate = manualTax.IGST;
                    cgstRate = manualTax.CGST;
                    sgstRate = manualTax.SGST;
                    cessRate = 0;
                }
            }
            else
            {
                // ── Auto: derive from item's HSN ──
                if (item.HSNCodeId == 0) throw new Exception($"Item '{item.ItemName}' has no HSN Code assigned.");
                if (item.Hsn == null) throw new Exception($"Item '{item.ItemName}' — HSN (Id: {item.HSNCodeId}) not found.");
                if (item.Hsn.tax == null) throw new Exception($"HSN '{item.Hsn.HsnName}' has no Tax assigned.");

                var hsn = item.Hsn;
                var tax = hsn.tax;

                hsnId = hsn.HsnId;
                hsnCode = hsn.HsnName;
                taxId = tax.TaxId;
                igstRate = tax.IGST;
                cgstRate = tax.CGST;
                sgstRate = tax.SGST;
                cessRate = hsn.Cess ?? 0;
            }

            // ── Fetch source order detail to snapshot orderedQty and previouslyDeliveredQty ──
            var orderDetail = await _context.SalesOrderDetails
                .FirstOrDefaultAsync(d => d.OrderDetailId == lineDto.OrderDetailId)
                ?? throw new Exception($"Order detail {lineDto.OrderDetailId} not found.");

            var deliveredQtyMap = await GetDeliveredQtyMapAsync(
                new List<int> { lineDto.OrderDetailId }, companyId, excludeDeliveryId: null);
            var previouslyDelivered = deliveredQtyMap.GetValueOrDefault(lineDto.OrderDetailId, 0m);

            // ── Tax calculation (same pattern as SalesOrderService) ──
            decimal grossAmount = lineDto.Rate * lineDto.DeliveryQty;
            decimal discountAmt = Math.Round(grossAmount * lineDto.DiscountRate / 100, 2);
            decimal afterFirst = grossAmount - discountAmt;
            decimal addisDiscAmt = Math.Round(afterFirst * lineDto.AddisDiscountRate / 100, 2);
            decimal taxableAmount = afterFirst - addisDiscAmt;

            decimal effIgstRate = isIntraState ? 0 : igstRate;
            decimal effCgstRate = isIntraState ? cgstRate : 0;
            decimal effSgstRate = isIntraState ? sgstRate : 0;

            if (lineDto.IsTaxIncluded)
            {
                decimal totalTaxRate = isIntraState
                    ? (effCgstRate + effSgstRate)
                    : effIgstRate;
                if (totalTaxRate > 0)
                    taxableAmount = Math.Round(taxableAmount / (1 + totalTaxRate / 100), 2);
            }

            decimal igstAmount = effIgstRate > 0 ? Math.Round(taxableAmount * effIgstRate / 100, 2) : 0;
            decimal cgstAmount = effCgstRate > 0 ? Math.Round(taxableAmount * effCgstRate / 100, 2) : 0;
            decimal sgstAmount = effSgstRate > 0 ? Math.Round(taxableAmount * effSgstRate / 100, 2) : 0;
            decimal cessAmount = cessRate > 0 ? Math.Round(taxableAmount * cessRate / 100, 2) : 0;
            decimal lineTaxAmt = igstAmount + cgstAmount + sgstAmount + cessAmount;

            return new GoodsDeliveryDetail
            {
                OrderId = lineDto.OrderId,
                OrderDetailId = lineDto.OrderDetailId,
                ItemId = lineDto.ItemId,
                HsnId = hsnId,
                HsnCode = hsnCode,
                PriceType = lineDto.PriceType,
                OrderedQty = orderDetail.Qty,
                PreviouslyDeliveredQty = previouslyDelivered,
                DeliveryQty = lineDto.DeliveryQty,
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
                TaxDetails = new List<GoodsDeliveryTaxDetail>
                {
                    new()
                    {
                        TaxId = taxId,
                        IGSTRate = effIgstRate,
                        CGSTRate = effCgstRate,
                        SGSTRate = effSgstRate,
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
        // PRIVATE — Validate source orders
        // All orders must be Confirmed, belong to same customer
        // ════════════════════════════════════════════════════
        private async Task ValidateSourceOrdersAsync(
            List<int> orderIds, int businessPartnerId, int companyId)
        {
            var orders = await _context.SalesOrderMains
                .Where(x =>
                    orderIds.Contains(x.OrderId) &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                .ToListAsync();

            if (orders.Count != orderIds.Count)
                throw new Exception("One or more source orders not found.");

            foreach (var order in orders)
            {
                if (order.Status != "Confirmed")
                    throw new Exception($"Order {order.OrderNo} is not Confirmed. Only Confirmed orders can be delivered.");

                if (order.BusinessPartnerId != businessPartnerId)
                    throw new Exception($"Order {order.OrderNo} does not belong to the selected customer.");
            }
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Validate delivery quantities
        // Ensures user cannot over-deliver any order line
        // ════════════════════════════════════════════════════
        private async Task ValidateDeliveryQtysAsync(
            List<CreateGoodsDeliveryDetailDto> lines,
            int companyId,
            int? excludeDeliveryId = null)
        {
            var detailIds = lines.Select(l => l.OrderDetailId).Distinct().ToList();
            var deliveredQtyMap = await GetDeliveredQtyMapAsync(detailIds, companyId, excludeDeliveryId);

            var orderDetails = await _context.SalesOrderDetails
                .Where(d => detailIds.Contains(d.OrderDetailId))
                .ToListAsync();

            foreach (var line in lines)
            {
                if (line.DeliveryQty <= 0)
                    throw new Exception($"Delivery quantity for item must be greater than zero.");

                var orderDetail = orderDetails.FirstOrDefault(d => d.OrderDetailId == line.OrderDetailId)
                    ?? throw new Exception($"Order detail {line.OrderDetailId} not found.");

                var alreadyDelivered = deliveredQtyMap.GetValueOrDefault(line.OrderDetailId, 0m);
                var pendingQty = orderDetail.Qty - alreadyDelivered;

                if (line.DeliveryQty > pendingQty)
                    throw new Exception(
                        $"Delivery quantity ({line.DeliveryQty}) exceeds pending quantity ({pendingQty}) " +
                        $"for order detail {line.OrderDetailId}.");
            }
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Get already-delivered qty per order detail
        // Counts only Confirmed deliveries (Draft/Cancelled excluded)
        // ════════════════════════════════════════════════════
        private async Task<Dictionary<int, decimal>> GetDeliveredQtyMapAsync(
            List<int> orderDetailIds,
            int companyId,
            int? excludeDeliveryId = null)
        {
            if (!orderDetailIds.Any())
                return new Dictionary<int, decimal>();

            var query = _context.GoodsDeliveryDetails
                .Where(d =>
                    orderDetailIds.Contains(d.OrderDetailId!.Value) &&
                     d.Delivery!.CompanyId == companyId &&
                    d.Delivery!.Status == "Confirmed" &&
                    !d.Delivery.IsDeleted);

            if (excludeDeliveryId.HasValue)
                query = query.Where(d => d.DeliveryId != excludeDeliveryId.Value);

            return await query
                .GroupBy(d => d.OrderDetailId!.Value)
                .Select(g => new { OrderDetailId = g.Key, TotalDelivered = g.Sum(x => x.DeliveryQty) })
                .ToDictionaryAsync(x => x.OrderDetailId, x => x.TotalDelivered);
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Validate header (same rules as SalesOrder)
        // ════════════════════════════════════════════════════
        private async Task ValidateHeaderAsync(
            int businessPartnerId, int locationId, int companyId,
            int? salesStateCode, int? billStateCode,
            int? contactPersonId, int? salesPersonId,
            int? billAddressId, int? shipAddressId)
        {
            var bpExists = await _context.BusinessPartners
                .AnyAsync(x =>
                    x.BusinessPartnerId == businessPartnerId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!bpExists) throw new Exception("Business Partner not found.");

            var locationExists = await _context.Locations
                .AnyAsync(x => x.LocationId == locationId && x.CompanyId == companyId);
            if (!locationExists) throw new Exception("Location not found.");

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
                if (!spExists) throw new Exception("Sales Person not found.");
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
        // PRIVATE — Generate delivery number
        // ════════════════════════════════════════════════════
        private async Task<string> GenerateDeliveryNoAsync(int companyId, int finYearId)
        {
            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(x => x.FinancialYearId == finYearId);

            var yearLabel = financialYear != null
                ? $"{financialYear.StartDate.Year % 100}{financialYear.EndDate.Year % 100}"
                : finYearId.ToString();

            var count = await _context.GoodsDeliveryMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"GD-{yearLabel}-{(count + 1):D4}";
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Map to response DTO
        // ════════════════════════════════════════════════════
        private GoodsDeliveryResponseDto MapToResponseDto(GoodsDeliveryMain main)
        {
            return new GoodsDeliveryResponseDto
            {
                DeliveryId = main.DeliveryId,
                FinYearId = main.FinYearId,
                DeliveryNo = main.DeliveryNo,
                DeliveryDate = main.DeliveryDate,
                Status = main.Status,
                RefNo = main.RefNo,
                RefDate = main.RefDate,
                Remarks = main.Remarks,

                BusinessPartnerId = main.BusinessPartnerId,
                BusinessPartnerName = main.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                BusinessPartnerCode = main.BusinessPartner?.BusinessPartnerCode ?? string.Empty,

                LocationId = main.LocationId,
                LocationName = main.Location?.LocationName ?? string.Empty,

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

                //LinkedOrders = main.OrderLinks?.Select(l => new LinkedOrderSummaryDto
                //{
                //    OrderId = l.OrderId,
                //    OrderNo = l.Order?.OrderNo ?? string.Empty,
                //    OrderDate = l.Order?.OrderDate ?? DateTime.MinValue
                //}).ToList() ?? new(),

                Details = main.Details?.Select(d => new GoodsDeliveryDetailResponseDto
                {
                    DeliveryDetailId = d.DeliveryDetailId,
                    DeliveryId = d.DeliveryId,
                    OrderId = d.OrderId,
                    OrderNo = d.Order?.OrderNo ?? string.Empty,
                    OrderDetailId = d.OrderDetailId ?? 0,
                    ItemId = d.ItemId,
                    ItemName = d.Item?.ItemName ?? string.Empty,
                    ItemCode = d.Item?.ItemCode,
                    HsnId = d.HsnId,
                    HsnCode = d.HsnCode,
                    PriceType = d.PriceType,
                    OrderedQty = d.OrderedQty,
                    PreviouslyDeliveredQty = d.PreviouslyDeliveredQty,
                    DeliveryQty = d.DeliveryQty,
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

        private static List<GoodsDeliveryTaxDetailResponseDto> MapTaxDetails(
            IEnumerable<GoodsDeliveryTaxDetail>? rows) =>
            rows?.Select(MapTaxDetailDto).ToList() ?? new();

        private static GoodsDeliveryTaxDetailResponseDto MapTaxDetailDto(GoodsDeliveryTaxDetail td) =>
            new()
            {
                TaxDetailId = td.TaxDetailId,
                DeliveryDetailId = td.DeliveryDetailId,
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