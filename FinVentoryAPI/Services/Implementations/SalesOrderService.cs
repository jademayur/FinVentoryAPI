using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesOrderDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public SalesOrderService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════════
        // CREATE
        // ════════════════════════════════════════════════════
        public async Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            // Validate quotation reference when provided
            string? quotationNo = null;
            if (dto.QuotationId.HasValue)
            {
                var quotation = await _context.SalesQuotationMains
                    .FirstOrDefaultAsync(x =>
                        x.QuotationId == dto.QuotationId &&
                        x.CompanyId == companyId &&
                        !x.IsDeleted)
                    ?? throw new Exception("Referenced quotation not found.");

                if (quotation.BusinessPartnerId != dto.BusinessPartnerId)
                    throw new Exception("Quotation does not belong to the selected customer.");

                if (quotation.Status == "Cancelled")
                    throw new Exception("Cannot create order from a cancelled quotation.");

                quotationNo = quotation.QuotationNo;
            }

            var orderNo = await GenerateOrderNoAsync(companyId, finYearId);

            var main = new SalesOrderMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                OrderNo = orderNo,
                OrderDate = dto.OrderDate,
                DeliveryDate = dto.DeliveryDate,
                QuotationId = dto.QuotationId,
                QuotationNo = quotationNo,
                QuotationDate = dto.QuotationDate,
                RefNo = dto.RefNo,
                RefDate = dto.RefDate,
                BusinessPartnerId = dto.BusinessPartnerId,
                LocationId = dto.LocationId,
                RoundOff = dto.RoundOff,
                Remarks = dto.Remarks,
                Status = "Draft",
                SalesStateCode = dto.SalesStateCode,
                BillStateCode = dto.BillStateCode,
                ContactPersonId = dto.ContactPersonId,
                SalesPersonId = dto.SalesPersonId,
                BillAddressId = dto.BillAddressId,
                ShipAddressId = dto.ShipAddressId,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                Details = new List<SalesOrderDetail>(),
                TaxDetails = new List<SalesOrderTaxDetail>()
            };

            decimal totalSubTotal = 0;
            decimal totalTaxAmount = 0;
            decimal totalCessAmount = 0;

            foreach (var lineDto in dto.Details)
            {
                var detail = await BuildDetailWithTaxAsync(
                    lineDto, dto.SalesStateCode, dto.BillStateCode);

                totalSubTotal += detail.TaxableAmount;
                totalCessAmount += detail.CessAmount;
                totalTaxAmount += detail.LineTaxAmount - detail.CessAmount;

                foreach (var td in detail.TaxDetails ?? Enumerable.Empty<SalesOrderTaxDetail>())
                {
                    td.Order = main;
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
                _context.SalesOrderMains.Add(main);
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(main.OrderId)
                ?? throw new Exception("Failed to retrieve saved order.");
        }

        // ════════════════════════════════════════════════════
        // UPDATE  (Draft only)
        // ════════════════════════════════════════════════════
        public async Task<bool> UpdateAsync(int id, UpdateSalesOrderMainDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await _context.SalesOrderMains
                .Include(m => m.Details!).ThenInclude(d => d.TaxDetails)
                .Include(m => m.TaxDetails)
                .FirstOrDefaultAsync(x =>
                    x.OrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Sales Order not found.");

            if (main.Status != "Draft")
                throw new Exception("Only Draft orders can be updated.");

            await ValidateHeaderAsync(
                dto.BusinessPartnerId, dto.LocationId, companyId,
                dto.SalesStateCode, dto.BillStateCode,
                dto.ContactPersonId, dto.SalesPersonId,
                dto.BillAddressId, dto.ShipAddressId);

            // Build incoming detail objects (fresh tax calc)
            var incomingDetails = new List<SalesOrderDetail>();
            foreach (var lineDto in dto.Details)
            {
                var createDto = new CreateSalesOrderDetailDto
                {
                    ItemId = lineDto.ItemId,
                    PriceType = lineDto.PriceType,
                    Qty = lineDto.Qty,
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
                incomingDetails.Add(await BuildDetailWithTaxAsync(
                    createDto, dto.SalesStateCode, dto.BillStateCode));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                main.OrderDate = dto.OrderDate;
                main.DeliveryDate = dto.DeliveryDate;
                main.QuotationDate = dto.QuotationDate;
                main.RefNo = dto.RefNo;
                main.RefDate = dto.RefDate;
                main.BusinessPartnerId = dto.BusinessPartnerId;
                main.LocationId = dto.LocationId;
                main.RoundOff = dto.RoundOff;
                main.Remarks = dto.Remarks;
                main.SalesStateCode = dto.SalesStateCode;
                main.BillStateCode = dto.BillStateCode;
                main.ContactPersonId = dto.ContactPersonId;
                main.SalesPersonId = dto.SalesPersonId;
                main.BillAddressId = dto.BillAddressId;
                main.ShipAddressId = dto.ShipAddressId;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

                var existingDetails = main.Details!.ToList();
                decimal totalSubTotal = 0;
                decimal totalTaxAmount = 0;
                decimal totalCessAmount = 0;

                for (int i = 0; i < incomingDetails.Count; i++)
                {
                    var incoming = incomingDetails[i];
                    var incomingDto = dto.Details[i];

                    if (i < existingDetails.Count)
                    {
                        var existing = existingDetails[i];

                        existing.ItemId = incoming.ItemId;
                        existing.HsnId = incoming.HsnId;
                        existing.HsnCode = incoming.HsnCode;
                        existing.PriceType = incoming.PriceType;
                        existing.Qty = incoming.Qty;
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

                     

                        // Wipe existing tax rows and re-insert fresh
                        if (existing.TaxDetails != null && existing.TaxDetails.Any())
                        {
                            _context.SalesOrderTaxDetails.RemoveRange(existing.TaxDetails);
                            await SaveChangesAsync(); // flush deletes before re-insert
                        }


                        foreach (var inTax in incoming.TaxDetails ?? new())
                        {
                            _context.SalesOrderTaxDetails.Add(new SalesOrderTaxDetail
                            {
                                OrderId = main.OrderId,
                                OrderDetailId = existing.OrderDetailId,
                                TaxId = inTax.TaxId,
                                IGSTRate = inTax.IGSTRate,
                                CGSTRate = inTax.CGSTRate,
                                SGSTRate = inTax.SGSTRate,
                                TaxableAmount = inTax.TaxableAmount,
                                IGSTAmount = inTax.IGSTAmount,
                                CGSTAmount = inTax.CGSTAmount,
                                SGSTAmount = inTax.SGSTAmount,
                                CessRate = inTax.CessRate,
                                CessAmount = inTax.CessAmount,
                                TotalTaxAmount = inTax.TotalTaxAmount
                            });
                        }

                        totalSubTotal += existing.TaxableAmount;
                        totalCessAmount += existing.CessAmount;
                        totalTaxAmount += existing.LineTaxAmount - existing.CessAmount;
                    }
                    else
                    {
                        incoming.OrderId = main.OrderId;
                        var newTaxList = incoming.TaxDetails ?? new();
                        incoming.TaxDetails = null;

                        _context.SalesOrderDetails.Add(incoming);
                        await SaveChangesAsync();

                        foreach (var inTax in newTaxList)
                        {
                            inTax.OrderId = main.OrderId;
                            inTax.OrderDetailId = incoming.OrderDetailId;
                            _context.SalesOrderTaxDetails.Add(inTax);
                        }

                        totalSubTotal += incoming.TaxableAmount;
                        totalCessAmount += incoming.CessAmount;
                        totalTaxAmount += incoming.LineTaxAmount - incoming.CessAmount;
                    }
                }

                // Remove surplus lines
                if (existingDetails.Count > incomingDetails.Count)
                {
                    var surplus = existingDetails.Skip(incomingDetails.Count).ToList();
                    foreach (var sd in surplus)
                        if (sd.TaxDetails != null && sd.TaxDetails.Any())
                            _context.SalesOrderTaxDetails.RemoveRange(sd.TaxDetails);

                    _context.SalesOrderDetails.RemoveRange(surplus);
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

            var main = await _context.SalesOrderMains
                .FirstOrDefaultAsync(x =>
                    x.OrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Sales Order not found.");

            if (main.Status != "Draft")
                throw new Exception("Only Draft orders can be deleted.");

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

            var main = await _context.SalesOrderMains
                .FirstOrDefaultAsync(x =>
                    x.OrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Sales Order not found.");

            if (main.Status != "Draft")
                throw new Exception("Only Draft orders can be confirmed.");

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

            var main = await _context.SalesOrderMains
                .FirstOrDefaultAsync(x =>
                    x.OrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Sales Order not found.");

            if (main.Status == "Cancelled")
                throw new Exception("Order is already cancelled.");

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
        public async Task<List<SalesOrderResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var list = await _context.SalesOrderMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.ContactPerson)
                .Include(x => x.SalesPerson)
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            return list.Select(MapToResponseDto).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET BY ID
        // ════════════════════════════════════════════════════
        public async Task<SalesOrderResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.SalesOrderMains
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.BusinessPartner)
                .Include(x => x.Location)
                .Include(x => x.ContactPerson)
                .Include(x => x.SalesPerson)
                .Include(x => x.BillAddress)
                .Include(x => x.ShipAddress)
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .Include(x => x.Details!).ThenInclude(d => d.TaxDetails!).ThenInclude(td => td.Tax)
                .Include(x => x.TaxDetails!).ThenInclude(td => td.Tax)
                .FirstOrDefaultAsync(x =>
                    x.OrderId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            return main == null ? null : MapToResponseDto(main);
        }

        // ════════════════════════════════════════════════════
        // GET PAGED
        // ════════════════════════════════════════════════════
        public async Task<PagedResponseDto<SalesOrderResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.SalesOrderMains
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
                    x.OrderNo.ToLower().Contains(search) ||
                    x.BusinessPartner!.BusinessPartnerName.ToLower().Contains(search) ||
                    (x.QuotationNo != null && x.QuotationNo.ToLower().Contains(search)));
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
                    query = query.Where(x => x.OrderDate >= fromDate);
                }
                if (request.Filters.ContainsKey("toDate"))
                {
                    var toDate = ((JsonElement)request.Filters["toDate"]).GetDateTime();
                    query = query.Where(x => x.OrderDate <= toDate);
                }
                if (request.Filters.ContainsKey("salesPersonId"))
                {
                    var spId = ((JsonElement)request.Filters["salesPersonId"]).GetInt32();
                    query = query.Where(x => x.SalesPersonId == spId);
                }
                if (request.Filters.ContainsKey("quotationId"))
                {
                    var qId = ((JsonElement)request.Filters["quotationId"]).GetInt32();
                    query = query.Where(x => x.QuotationId == qId);
                }
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "orderno" => sort.Direction == "desc" ? query.OrderByDescending(x => x.OrderNo) : query.OrderBy(x => x.OrderNo),
                    "orderdate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.OrderDate) : query.OrderBy(x => x.OrderDate),
                    "deliverydate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.DeliveryDate) : query.OrderBy(x => x.DeliveryDate),
                    "businesspartnername" => sort.Direction == "desc" ? query.OrderByDescending(x => x.BusinessPartner!.BusinessPartnerName) : query.OrderBy(x => x.BusinessPartner!.BusinessPartnerName),
                    "salesperson" => sort.Direction == "desc" ? query.OrderByDescending(x => x.SalesPerson!.SalesPersonName) : query.OrderBy(x => x.SalesPerson!.SalesPersonName),
                    "status" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "nettotal" => sort.Direction == "desc" ? query.OrderByDescending(x => x.NetTotal) : query.OrderBy(x => x.NetTotal),
                    _ => query.OrderByDescending(x => x.OrderDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.OrderDate);
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

            return new PagedResponseDto<SalesOrderResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapToResponseDto).ToList()
            };
        }

        // ════════════════════════════════════════════════════
        // GET QUOTATIONS FOR CUSTOMER  (picker dropdown)
        // Returns only Draft/Sent quotations not already
        // converted to a Confirmed order.
        // ════════════════════════════════════════════════════
        public async Task<List<QuotationPickerDto>> GetQuotationsForCustomerAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();

            // Collect quotation IDs already used in Confirmed orders
            var usedQuotationIds = await _context.SalesOrderMains
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.QuotationId != null &&
                    x.Status == "Confirmed" &&
                    !x.IsDeleted)
                .Select(x => x.QuotationId!.Value)
                .ToListAsync();

            var list = await _context.SalesQuotationMains
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.BusinessPartnerId == businessPartnerId &&
                    (x.Status == "Draft" || x.Status == "Sent") &&
                    !x.IsDeleted &&
                    !usedQuotationIds.Contains(x.QuotationId))
                .Include(x => x.SalesPerson)
                .OrderByDescending(x => x.QuotationDate)
                .ToListAsync();

            return list.Select(x => new QuotationPickerDto
            {
                QuotationId = x.QuotationId,
                QuotationNo = x.QuotationNo,
                QuotationDate = x.QuotationDate,
                ValidUntilDate = x.ValidUntilDate,
                NetTotal = x.NetTotal,
                SalesPersonId = x.SalesPersonId,
                SalesPersonName = x.SalesPerson?.SalesPersonName
            }).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET QUOTATION PREFILL  (fills form from quotation)
        // ════════════════════════════════════════════════════
        public async Task<QuotationPrefillDto> GetQuotationPrefillAsync(int quotationId)
        {
            var companyId = _common.GetCompanyId();

            var q = await _context.SalesQuotationMains
                .AsNoTracking()
                .Include(x => x.Details!).ThenInclude(d => d.Item)
                .Include(x => x.Details!).ThenInclude(d => d.Hsn)
                .FirstOrDefaultAsync(x =>
                    x.QuotationId == quotationId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted)
                ?? throw new Exception("Quotation not found.");

            return new QuotationPrefillDto
            {
                QuotationId = q.QuotationId,
                QuotationNo = q.QuotationNo,
                QuotationDate = q.QuotationDate,
                ValidUntilDate = q.ValidUntilDate,
                BusinessPartnerId = q.BusinessPartnerId,
                LocationId = q.LocationId,
                ContactPersonId = q.ContactPersonId,
                SalesPersonId = q.SalesPersonId,
                BillAddressId = q.BillAddressId,
                ShipAddressId = q.ShipAddressId,
                SalesStateCode = q.SalesStateCode,
                BillStateCode = q.BillStateCode,
                SubTotal = q.SubTotal,
                TaxAmount = q.TaxAmount,
                CessAmount = q.CessAmount,
                RoundOff = q.RoundOff,
                NetTotal = q.NetTotal,
                Remarks = q.Remarks,
                Details = q.Details?.Select(d => new QuotationPrefillDetailDto
                {
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
                    IsTaxIncluded = d.IsTaxIncluded,
                    TaxableAmount = d.TaxableAmount,
                    LineTaxAmount = d.LineTaxAmount,
                    LineTotal = d.LineTotal
                }).ToList() ?? new()
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Build detail + tax  (same logic as quotation)
        // ════════════════════════════════════════════════════
        private async Task<SalesOrderDetail> BuildDetailWithTaxAsync(
      CreateSalesOrderDetailDto lineDto,
      int? salesStateCode, int? billStateCode)
        {
            var item = await _context.Items
                .Include(i => i.Hsn).ThenInclude(h => h!.tax)
                .FirstOrDefaultAsync(i => i.ItemId == lineDto.ItemId && !i.IsDeleted)
                ?? throw new Exception($"Item {lineDto.ItemId} not found.");

            bool hasHsn = item.HSNCodeId != 0 && item.Hsn != null;
            bool useManualTax = lineDto.ManualTaxId.HasValue;

            int resolvedTaxId;
            decimal igstRate, cgstRate, sgstRate, cessRate;
            int hsnId;
            string hsnCode;

            if (useManualTax)
            {
                var manualTax = await _context.Taxes
                    .FirstOrDefaultAsync(t => t.TaxId == lineDto.ManualTaxId && !t.IsDeleted)
                    ?? throw new Exception($"Tax {lineDto.ManualTaxId} not found.");

                resolvedTaxId = manualTax.TaxId;
                igstRate = lineDto.ManualIgstRate ?? manualTax.IGST;
                cgstRate = lineDto.ManualCgstRate ?? manualTax.CGST;
                sgstRate = lineDto.ManualSgstRate ?? manualTax.SGST;
                cessRate = lineDto.ManualCessRate ?? 0;
                hsnId = item.Hsn?.HsnId ?? 0;
                hsnCode = item.Hsn?.HsnName ?? string.Empty;
            }
            else
            {
                if (!hasHsn)
                    throw new Exception(
                        $"Item '{item.ItemName}' has no HSN Code assigned. Please select a tax manually.");

                if (item.Hsn!.tax == null)
                    throw new Exception(
                        $"HSN '{item.Hsn.HsnName}' has no Tax assigned.");

                var hsn = item.Hsn;
                var tax = hsn.tax!;

                resolvedTaxId = tax.TaxId;
                igstRate = tax.IGST;
                cgstRate = tax.CGST;
                sgstRate = tax.SGST;
                cessRate = hsn.Cess ?? 0;
                hsnId = hsn.HsnId;
                hsnCode = hsn.HsnName;
            }

            bool isIntraState = (salesStateCode.HasValue && billStateCode.HasValue)
                ? salesStateCode.Value == billStateCode.Value
                : true;

            decimal effectiveIgst = isIntraState ? 0 : igstRate;
            decimal effectiveCgst = isIntraState ? cgstRate : 0;
            decimal effectiveSgst = isIntraState ? sgstRate : 0;

            decimal grossAmount = lineDto.Rate * lineDto.Qty;
            decimal discountAmt = Math.Round(grossAmount * lineDto.DiscountRate / 100, 2);
            decimal afterFirst = grossAmount - discountAmt;
            decimal addisDiscAmt = Math.Round(afterFirst * lineDto.AddisDiscountRate / 100, 2);
            decimal taxableAmount = afterFirst - addisDiscAmt;

            if (lineDto.IsTaxIncluded)
            {
                decimal totalTaxRate = isIntraState
                    ? (effectiveCgst + effectiveSgst)
                    : effectiveIgst;

                if (totalTaxRate > 0)
                    taxableAmount = Math.Round(taxableAmount / (1 + totalTaxRate / 100), 2);
            }

            decimal igstAmount = effectiveIgst > 0 ? Math.Round(taxableAmount * effectiveIgst / 100, 2) : 0;
            decimal cgstAmount = effectiveCgst > 0 ? Math.Round(taxableAmount * effectiveCgst / 100, 2) : 0;
            decimal sgstAmount = effectiveSgst > 0 ? Math.Round(taxableAmount * effectiveSgst / 100, 2) : 0;
            decimal cessAmt = cessRate > 0 ? Math.Round(taxableAmount * cessRate / 100, 2) : 0;
            decimal lineTaxAmt = igstAmount + cgstAmount + sgstAmount + cessAmt;

            return new SalesOrderDetail
            {
                ItemId = lineDto.ItemId,
                HsnId = hsnId,
                HsnCode = hsnCode,
                PriceType = lineDto.PriceType,
                Qty = lineDto.Qty,
                Rate = lineDto.Rate,
                DiscountRate = lineDto.DiscountRate,
                AddisDiscountRate = lineDto.AddisDiscountRate,
                DiscountAmount = discountAmt,
                AddisDiscountAmount = addisDiscAmt,
                IsTaxIncluded = lineDto.IsTaxIncluded,
                TaxableAmount = taxableAmount,
                CessRate = cessRate,
                CessAmount = cessAmt,
                LineTaxAmount = lineTaxAmt,
                LineTotal = taxableAmount + lineTaxAmt,
                TaxDetails = new List<SalesOrderTaxDetail>
        {
            new()
            {
                TaxId          = resolvedTaxId,
                IGSTRate       = effectiveIgst,
                CGSTRate       = effectiveCgst,
                SGSTRate       = effectiveSgst,
                TaxableAmount  = taxableAmount,
                IGSTAmount     = igstAmount,
                CGSTAmount     = cgstAmount,
                SGSTAmount     = sgstAmount,
                CessRate       = cessRate,
                CessAmount     = cessAmt,
                TotalTaxAmount = lineTaxAmt
            }
        }
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Validate header  (identical rules)
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
        // PRIVATE — Generate order number
        // ════════════════════════════════════════════════════
        private async Task<string> GenerateOrderNoAsync(int companyId, int finYearId)
        {
            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(x => x.FinancialYearId == finYearId);

            var yearLabel = financialYear != null
                ? $"{financialYear.StartDate.Year % 100}{financialYear.EndDate.Year % 100}"
                : finYearId.ToString();

            var count = await _context.SalesOrderMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"SO-{yearLabel}-{(count + 1):D4}";
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Map to response DTO
        // ════════════════════════════════════════════════════
        private SalesOrderResponseDto MapToResponseDto(SalesOrderMain main)
        {
            return new SalesOrderResponseDto
            {
                OrderId = main.OrderId,
                FinYearId = main.FinYearId,
                OrderNo = main.OrderNo,
                OrderDate = main.OrderDate,
                DeliveryDate = main.DeliveryDate,
                Status = main.Status,

                QuotationId = main.QuotationId,
                QuotationNo = main.QuotationNo,
                QuotationDate = main.QuotationDate,
                RefNo = main.RefNo,
                RefDate = main.RefDate,

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
                Remarks = main.Remarks,

                CreatedBy = (int)(main.CreatedBy ?? 0),
                CreatedDate = main.CreatedDate,
                ModifiedBy = main.ModifiedBy,
                ModifiedDate = main.ModifiedDate,

                Details = main.Details?.Select(d => new SalesOrderDetailResponseDto
                {
                    OrderDetailId = d.OrderDetailId,
                    OrderId = d.OrderId,
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

        private static List<SalesOrderTaxDetailResponseDto> MapTaxDetails(
            IEnumerable<SalesOrderTaxDetail>? rows) =>
            rows?.Select(MapTaxDetailDto).ToList() ?? new();

        private static SalesOrderTaxDetailResponseDto MapTaxDetailDto(SalesOrderTaxDetail td) =>
            new()
            {
                TaxDetailId = td.TaxDetailId,
                OrderDetailId = td.OrderDetailId,
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
