using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountDTOs;
using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class ItemService : IItemService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public ItemService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ✅ CREATE
        public async Task<ItemResponseDto> CreateAsync(CreateItemDto dto)
        {
            var companyId = _common.GetCompanyId();

            var duplicate = await _context.Items
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.ItemName.ToLower() == dto.ItemName.ToLower() &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Item already exists.");

            var item = new Item
            {
                CompanyId = companyId,

                ItemName = dto.ItemName,
                ItemCode = dto.ItemCode,
                Description = dto.Description,
                Barcode = dto.Barcode,

                ItemType = dto.ItemType,
                ItemCategory = dto.ItemCategory,

                ItemGroupId = dto.ItemGroupId,
                BrandId = dto.BrandId,
                HSNCodeId = dto.HSNCodeId,

                BaseUnitId = dto.BaseUnitId,
                AlternateUnitId = dto.AlternateUnitId,
                ConversionFactor = dto.ConversionFactor,

                AllowNagativeStock = dto.AllowNagativeStock,
                ItemManageBy = dto.ItemManageBy,
                CostingMethod = dto.CostingMethod,

                InventoryAccountId = dto.InventoryAccountId,
                COGSAccountId = dto.COGSAccountId,
                SalesAccountId = dto.SalesAccountId,
                PurchaseAccountId = dto.PurchaseAccountId,

                CreatedBy = _common.GetUserId()
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            // 💰 Prices
            if (dto.Prices != null && dto.Prices.Any())
            {
                var prices = dto.Prices.Select(p => new ItemPrice
                {
                    ItemId = item.ItemId,
                    PriceType = p.PriceType,
                    Rate = p.Rate,
                    IsTaxIncluded = p.IsTaxIncluded
                }).ToList();

                _context.ItemsPrices.AddRange(prices);
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(item.ItemId);
        }

        // ✅ UPDATE
        public async Task<bool> UpdateAsync(int id, UpdateItemDto dto)
        {
            var companyId = _common.GetCompanyId();

            var item = await _context.Items
                .Include(x => x.Prices)
                .FirstOrDefaultAsync(x =>
                    x.ItemId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (item == null)
                return false;

            var duplicate = await _context.Items
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.ItemName.ToLower() == dto.ItemName.ToLower() &&
                    x.ItemId != id &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Item with same name already exists.");

            // Update
            item.ItemName = dto.ItemName;
            item.ItemCode = dto.ItemCode;
            item.Description = dto.Description;
            item.Barcode = dto.Barcode;

            item.ItemType = dto.ItemType;
            item.ItemCategory = dto.ItemCategory;

            item.ItemGroupId = dto.ItemGroupId;
            item.BrandId = dto.BrandId;
            item.HSNCodeId = dto.HSNCodeId;

            item.BaseUnitId = dto.BaseUnitId;
            item.AlternateUnitId = dto.AlternateUnitId;
            item.ConversionFactor = dto.ConversionFactor;

            item.AllowNagativeStock = dto.AllowNagativeStock;
            item.ItemManageBy = dto.ItemManageBy;
            item.CostingMethod = dto.CostingMethod;

            item.InventoryAccountId = dto.InventoryAccountId;
            item.COGSAccountId = dto.COGSAccountId;
            item.SalesAccountId = dto.SalesAccountId;
            item.PurchaseAccountId = dto.PurchaseAccountId;

            item.IsActive = dto.IsActive;
            item.ModifiedBy = _common.GetUserId();
            item.ModifiedDate = DateTime.UtcNow;

            // 🔥 Replace Prices
            if (dto.Prices != null)
            {
                _context.ItemsPrices.RemoveRange(item.Prices);

                var newPrices = dto.Prices.Select(p => new ItemPrice
                {
                    ItemId = item.ItemId,
                    PriceType = p.PriceType,
                    Rate = p.Rate,
                    IsTaxIncluded = p.IsTaxIncluded
                });

                await _context.ItemsPrices.AddRangeAsync(newPrices);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var item = await _context.Items
                .FirstOrDefaultAsync(x =>
                    x.ItemId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (item == null)
                return false;

            item.IsDeleted = true;
            item.IsActive = false;
            item.ModifiedBy = _common.GetUserId();
            item.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ GET BY ID
        // Fix GetByIdAsync — maps ALL fields so edit mode shows correct data
        // File: Services/Implementations/ItemService.cs

        public async Task<ItemResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var item = await _context.Items
                .Include(x => x.Prices)
                .Include(x => x.ItemGroup)
                .Include(x => x.Brand)
                .FirstOrDefaultAsync(x =>
                    x.ItemId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (item == null)
                return null;

            return new ItemResponseDto
            {
                // ── Identity ──────────────────────────────────────────
                ItemId = item.ItemId,
                CompanyId = item.CompanyId,

                // ── Basic Info ────────────────────────────────────────
                ItemName = item.ItemName,
                ItemCode = item.ItemCode ?? "",
                Description = item.Description,
                Barcode = item.Barcode,
                ItemType = item.ItemType,
                ItemCategory = item.ItemCategory,

                // ── Classification ────────────────────────────────────
                ItemGroupId = item.ItemGroupId,
                ItemGroupName = item.ItemGroup?.ItemGroupName,
                BrandId = item.BrandId,
                BrandName = item.Brand?.BrandName,
                HSNCodeId = item.HSNCodeId,

                // ── Units ─────────────────────────────────────────────
                BaseUnitId = item.BaseUnitId,
                AlternateUnitId = item.AlternateUnitId,
                ConversionFactor = item.ConversionFactor,

                // ── Inventory ─────────────────────────────────────────
                AllowNagativeStock = item.AllowNagativeStock,
                ItemManageBy = item.ItemManageBy,
                CostingMethod = item.CostingMethod,

                // ── Accounting ────────────────────────────────────────
                InventoryAccountId = item.InventoryAccountId,
                COGSAccountId = item.COGSAccountId,
                SalesAccountId = item.SalesAccountId,
                PurchaseAccountId = item.PurchaseAccountId,

                // ── Status & Audit ────────────────────────────────────
                IsActive = item.IsActive,
                CreatedDate = item.CreatedDate,
                UpdatedDate = item.ModifiedDate,

                // ── Prices ────────────────────────────────────────────
                Prices = item.Prices?.Select(p => new ItemPriceResponseDto
                {
                    ItemPriceId = p.ItemPriceId,
                    ItemId = p.ItemId,
                    PriceType = p.PriceType,
                    Rate = p.Rate,
                    IsTaxIncluded = p.IsTaxIncluded
                }).ToList()
            };
        }

        // ✅ GET ALL
        public async Task<List<ItemResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var items = await _context.Items
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .ToListAsync();

            return items.Select(x => new ItemResponseDto
            {
                ItemId = x.ItemId,
                ItemName = x.ItemName,
                ItemCode = x.ItemCode,
                ItemType = x.ItemType,
                ItemCategory = x.ItemCategory
            }).ToList();
        }

        // ✅ PAGED
        public async Task<PagedResponseDto<ItemResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.Items
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.ItemGroup)
                .AsQueryable();

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();

                query = query.Where(x =>
                    x.ItemName.ToLower().Contains(search) ||
                    (x.ItemCode ?? "").ToLower().Contains(search) ||
                    (x.Barcode ?? "").ToLower().Contains(search));
            }

            // 🎯 FILTERS
            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("itemCategory"))
                {
                    var category = ((JsonElement)request.Filters["itemCategory"]).GetInt32();
                    query = query.Where(x => (int)x.ItemCategory == category);
                }

                if (request.Filters.ContainsKey("itemType"))
                {
                    var type = ((JsonElement)request.Filters["itemType"]).GetInt32();
                    query = query.Where(x => (int)x.ItemType == type);
                }

                if (request.Filters.ContainsKey("itemGroupId"))
                {
                    var groupId = ((JsonElement)request.Filters["itemGroupId"]).GetInt32();
                    query = query.Where(x => x.ItemGroupId == groupId);
                }

                if (request.Filters.ContainsKey("isActive"))
                {
                    var isActive = ((JsonElement)request.Filters["isActive"]).GetBoolean();
                    query = query.Where(x => x.IsActive == isActive);
                }
            }

            // 🔽 SORTING
            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();

                switch (sort.Column.ToLower())
                {
                    case "itemname":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.ItemName)
                            : query.OrderBy(x => x.ItemName);
                        break;

                    case "itemcode":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.ItemCode)
                            : query.OrderBy(x => x.ItemCode);
                        break;

                    case "itemcategory":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.ItemCategory)
                            : query.OrderBy(x => x.ItemCategory);
                        break;

                    case "itemtype":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.ItemType)
                            : query.OrderBy(x => x.ItemType);
                        break;

                    case "isactive":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.IsActive)
                            : query.OrderBy(x => x.IsActive);
                        break;

                    default:
                        query = query.OrderBy(x => x.ItemName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.ItemName);
            }

            // 📊 TOTAL COUNT
            var totalRecords = await query.CountAsync();

            // 📄 DATA
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ItemResponseDto
                {
                    ItemId = x.ItemId,
                    CompanyId = x.CompanyId,

                    ItemCode = x.ItemCode ?? "",
                    ItemName = x.ItemName,                   
                    Description = x.Description,
                    Barcode = x.Barcode,

                    ItemType = x.ItemType,
                    ItemCategory = x.ItemCategory,

                    ItemGroupId = x.ItemGroupId,
                    ItemGroupName = x.ItemGroup != null ? x.ItemGroup.ItemGroupName : null,

                    BrandId = x.BrandId,
                    HSNCodeId = x.HSNCodeId,

                    BaseUnitId = x.BaseUnitId,
                    AlternateUnitId = x.AlternateUnitId,
                    ConversionFactor = x.ConversionFactor,

                    AllowNagativeStock = x.AllowNagativeStock,
                    ItemManageBy = x.ItemManageBy,
                    CostingMethod = x.CostingMethod,

                    InventoryAccountId = x.InventoryAccountId,
                    COGSAccountId = x.COGSAccountId,
                    SalesAccountId = x.SalesAccountId,
                    PurchaseAccountId = x.PurchaseAccountId,

                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.ModifiedDate
                })
                .ToListAsync();

            return new PagedResponseDto<ItemResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }

        public async Task<List<ItemResponseDto>> GetItemListAsync()
        {
            var companyId = _common.GetCompanyId();

            var data = await _context.Items
                .Where(x => x.CompanyId == companyId
                         && !x.IsDeleted
                         && x.ItemType == ItemType.Goods) // ✅ Correct enum usage
                .Select(x => new ItemResponseDto
                {
                    ItemId = x.ItemId,
                    CompanyId = x.CompanyId,

                    ItemCode = x.ItemCode ?? "",
                    ItemName = x.ItemName,
                    Description = x.Description,
                    Barcode = x.Barcode,

                    ItemType = x.ItemType,
                    ItemCategory = x.ItemCategory,

                    ItemGroupId = x.ItemGroupId,
                    ItemGroupName = x.ItemGroup != null ? x.ItemGroup.ItemGroupName : null,

                    BrandId = x.BrandId,
                    HSNCodeId = x.HSNCodeId,

                    IsActive = x.IsActive
                })
                .OrderBy(x => x.ItemName)
                .ToListAsync();

            return data;
        }
    }
}