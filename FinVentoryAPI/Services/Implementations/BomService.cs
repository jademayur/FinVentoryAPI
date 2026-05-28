using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.BomDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FinVentoryAPI.Enums;

namespace FinVentoryAPI.Services.Implementations
{
    public class BomService : IBomService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public BomService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ─────────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────────
        public async Task<BomResponseDto> CreateAsync(CreateBomDto dto)
        {
            var companyId = _common.GetCompanyId();

            // Duplicate check: same BomName for the same finished-good item
            var duplicate = await _context.BillOfMaterial
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.ItemId == dto.ItemId &&
                    x.BomName.ToLower() == dto.BomName.ToLower() &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("A BOM with the same name already exists for this item.");

            // If this BOM is default, clear existing default for the item
            if (dto.IsDefault)
                await ClearDefaultAsync(companyId, dto.ItemId, null);

            var bom = new BillOfMaterial
            {
                CompanyId = companyId,
                ItemId = dto.ItemId,
                BomCode = dto.BomCode,
                BomName = dto.BomName,
                Description = dto.Description,
                OutputQuantity = dto.OutputQuantity,
                BaseUnitId = dto.BaseUnitId,
                IsDefault = dto.IsDefault,
                CreatedBy = _common.GetUserId()
            };

            _context.BillOfMaterial.Add(bom);
            await _context.SaveChangesAsync();

            // Save lines
            if (dto.Lines?.Any() == true)
            {
                var lines = MapLineDtos(bom.BomId, dto.Lines);
                await _context.BomLines.AddRangeAsync(lines);
                await _context.SaveChangesAsync();
            }

            return (await GetByIdAsync(bom.BomId))!;
        }

        // ─────────────────────────────────────────────────────────
        // UPDATE
        // ─────────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateBomDto dto)
        {
            var companyId = _common.GetCompanyId();

            var bom = await _context.BillOfMaterial
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x =>
                    x.BomId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (bom == null)
                return false;

            // Duplicate name check (exclude self)
            var duplicate = await _context.BillOfMaterial
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.ItemId == bom.ItemId &&
                    x.BomName.ToLower() == dto.BomName.ToLower() &&
                    x.BomId != id &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("A BOM with the same name already exists for this item.");

            // Handle default flag
            if (dto.IsDefault && !bom.IsDefault)
                await ClearDefaultAsync(companyId, bom.ItemId, id);

            // Update header
            bom.BomCode = dto.BomCode;
            bom.BomName = dto.BomName;
            bom.Description = dto.Description;
            bom.OutputQuantity = dto.OutputQuantity;
            bom.BaseUnitId = dto.BaseUnitId;
            bom.IsDefault = dto.IsDefault;
            bom.IsActive = dto.IsActive;
            bom.ModifiedBy = _common.GetUserId();
            bom.ModifiedDate = DateTime.UtcNow;

            // Replace lines entirely
            _context.BomLines.RemoveRange(bom.Lines);

            if (dto.Lines?.Any() == true)
            {
                var newLines = MapLineDtos(bom.BomId, dto.Lines);
                await _context.BomLines.AddRangeAsync(newLines);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────
        // DELETE  (soft)
        // ─────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var bom = await _context.BillOfMaterial
                .FirstOrDefaultAsync(x =>
                    x.BomId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (bom == null)
                return false;

            bom.IsDeleted = true;
            bom.IsActive = false;
            bom.ModifiedBy = _common.GetUserId();
            bom.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────
        // GET BY ID
        // ─────────────────────────────────────────────────────────
        public async Task<BomResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var bom = await _context.BillOfMaterial
                .Include(x => x.FinishedGood)
                .Include(x => x.Lines)
                    .ThenInclude(l => l.Component)
                .FirstOrDefaultAsync(x =>
                    x.BomId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (bom == null)
                return null;

            return MapToResponseDto(bom);
        }

        // ─────────────────────────────────────────────────────────
        // PAGED LIST
        // ─────────────────────────────────────────────────────────
        public async Task<PagedResponseDto<BomListItemDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.BillOfMaterial
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.FinishedGood)
                .Include(x => x.Lines)
                .AsQueryable();

            // 🔍 Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x =>
                    x.BomName.ToLower().Contains(s) ||
                    (x.BomCode ?? "").ToLower().Contains(s) ||
                    (x.FinishedGood != null && x.FinishedGood.ItemName.ToLower().Contains(s)));
            }

            // 🎯 Filters
            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("itemId"))
                {
                    var itemId = ((JsonElement)request.Filters["itemId"]).GetInt32();
                    query = query.Where(x => x.ItemId == itemId);
                }

                if (request.Filters.ContainsKey("isActive"))
                {
                    var isActive = ((JsonElement)request.Filters["isActive"]).GetBoolean();
                    query = query.Where(x => x.IsActive == isActive);
                }

                if (request.Filters.ContainsKey("isDefault"))
                {
                    var isDefault = ((JsonElement)request.Filters["isDefault"]).GetBoolean();
                    query = query.Where(x => x.IsDefault == isDefault);
                }
            }

            // 🔽 Sorting
            if (request.Sorts?.Any() == true)
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "bomname" => sort.Direction == "desc" ? query.OrderByDescending(x => x.BomName) : query.OrderBy(x => x.BomName),
                    "bomcode" => sort.Direction == "desc" ? query.OrderByDescending(x => x.BomCode) : query.OrderBy(x => x.BomCode),
                    "itemname" => sort.Direction == "desc" ? query.OrderByDescending(x => x.FinishedGood!.ItemName) : query.OrderBy(x => x.FinishedGood!.ItemName),
                    "createddate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
                    "isactive" => sort.Direction == "desc" ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                    _ => query.OrderBy(x => x.BomName)
                };
            }
            else
            {
                query = query.OrderBy(x => x.BomName);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new BomListItemDto
                {
                    BomId = x.BomId,
                    BomCode = x.BomCode,
                    BomName = x.BomName,
                    ItemId = x.ItemId,
                    ItemName = x.FinishedGood != null ? x.FinishedGood.ItemName : null,
                    OutputQuantity = x.OutputQuantity,
                    IsDefault = x.IsDefault,
                    IsActive = x.IsActive,
                    LineCount = x.Lines.Count,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            return new PagedResponseDto<BomListItemDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }

        // ─────────────────────────────────────────────────────────
        // GET BY ITEM ID
        // ─────────────────────────────────────────────────────────
        public async Task<List<BomResponseDto>> GetByItemIdAsync(int itemId)
        {
            var companyId = _common.GetCompanyId();

            var boms = await _context.BillOfMaterial
                .Where(x => x.CompanyId == companyId && x.ItemId == itemId && !x.IsDeleted)
                .Include(x => x.FinishedGood)
                .Include(x => x.Lines)
                    .ThenInclude(l => l.Component)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.BomName)
                .ToListAsync();

            return boms.Select(MapToResponseDto).ToList();
        }

        // ─────────────────────────────────────────────────────────
        // SET DEFAULT
        // ─────────────────────────────────────────────────────────
        public async Task<bool> SetDefaultAsync(int bomId)
        {
            var companyId = _common.GetCompanyId();

            var bom = await _context.BillOfMaterial
                .FirstOrDefaultAsync(x =>
                    x.BomId == bomId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (bom == null)
                return false;

            await ClearDefaultAsync(companyId, bom.ItemId, bomId);

            bom.IsDefault = true;
            bom.ModifiedBy = _common.GetUserId();
            bom.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────

        /// <summary>Clears IsDefault on all BOMs for the item except the one being set.</summary>
        private async Task ClearDefaultAsync(int companyId, int itemId, int? excludeBomId)
        {
            var others = await _context.BillOfMaterial
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.ItemId == itemId &&
                    x.IsDefault &&
                    !x.IsDeleted &&
                    (excludeBomId == null || x.BomId != excludeBomId))
                .ToListAsync();

            foreach (var b in others)
            {
                b.IsDefault = false;
                b.ModifiedBy = _common.GetUserId();
                b.ModifiedDate = DateTime.UtcNow;
            }
        }

        private static List<BomLine> MapLineDtos(int bomId, List<CreateBomLineDto> dtos) =>
            dtos.Select(l => new BomLine
            {
                BomId = bomId,
                ItemId = l.ItemId,
                Quantity = l.Quantity,
                UnitId = l.UnitId,
                ConversionFactor = l.ConversionFactor,
                WastagePercent = l.WastagePercent,
                Notes = l.Notes,
                SortOrder = l.SortOrder
            }).ToList();

        private static BomResponseDto MapToResponseDto(BillOfMaterial bom) =>
            new()
            {
                BomId = bom.BomId,
                CompanyId = bom.CompanyId,
                ItemId = bom.ItemId,
                ItemName = bom.FinishedGood?.ItemName,
                ItemCode = bom.FinishedGood?.ItemCode,
                BomCode = bom.BomCode,
                BomName = bom.BomName,
                Description = bom.Description,
                OutputQuantity = bom.OutputQuantity,
                BaseUnitId = (int) bom.BaseUnitId,                  
                IsDefault = bom.IsDefault,
                IsActive = bom.IsActive,
                CreatedDate = bom.CreatedDate,
                UpdatedDate = bom.ModifiedDate,
                Lines = bom.Lines
                    .OrderBy(l => l.SortOrder)
                    .Select(l => new BomLineResponseDto
                    {
                        BomLineId = l.BomLineId,
                        BomId = l.BomId,
                        ItemId = l.ItemId,
                        ItemName = l.Component?.ItemName,
                        ItemCode = l.Component?.ItemCode,
                        Quantity = l.Quantity,
                        UnitId = l.UnitId,
                        ConversionFactor = l.ConversionFactor,
                        WastagePercent = l.WastagePercent,
                        Notes = l.Notes,
                        SortOrder = l.SortOrder
                    }).ToList()
            };
    }
}
