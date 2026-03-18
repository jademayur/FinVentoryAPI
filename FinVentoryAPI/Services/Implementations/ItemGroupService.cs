using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountDTOs;
using FinVentoryAPI.DTOs.AccountGroupDTOs;
using FinVentoryAPI.DTOs.ItemGroupDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Migrations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class ItemGroupService : IItemGroupService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        public ItemGroupService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<ItemGroupResponseDto> CreateAsync(CreateItemGroupDto dto)
        {
            var companyId = _common.GetCompanyId();

            var duplicate = await _context.ItemGroups
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.ItemGroupName.ToLower() == dto.ItemGroupName.ToLower() &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Item group already exists.");

            var itemGroup = new ItemGroup
            {
                CompanyId = companyId,
                ItemGroupName = dto.ItemGroupName,
                ParentGroupId = dto.ParentGroupId,
                GroupCode = dto.GroupCode,
                CreatedBy = _common.GetUserId()
            };

            _context.ItemGroups.Add(itemGroup);
            await _context.SaveChangesAsync();

            return new ItemGroupResponseDto
            {
                ItemGroupId = itemGroup.ItemGroupId,
                ItemGroupName = itemGroup.ItemGroupName,
                ParentGroupId = itemGroup.ParentGroupId,
                GroupCode = itemGroup.GroupCode,
                IsActive = itemGroup.IsActive,
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateItemGroupDto dto)
        {
            var companyId = _common.GetCompanyId();

            var itemGroup = await _context.ItemGroups
                .FirstOrDefaultAsync(x =>
                    x.ItemGroupId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (itemGroup == null)
                return false;

            var duplicate = await _context.ItemGroups
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.ItemGroupName.ToLower() == dto.ItemGroupName.ToLower() &&
                    x.ItemGroupId != id &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Item Group Name with same name already exists.");

            itemGroup.ItemGroupName = dto.ItemGroupName;
            itemGroup.ParentGroupId = dto.ParentGroupId;
            itemGroup.GroupCode = dto.GroupCode;
            itemGroup.IsActive = dto.IsActive;
            itemGroup.ModifiedBy = _common.GetUserId();
            itemGroup.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ItemGroupResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var groups = await _context.ItemGroups
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.ParentGroup)                
                .ToListAsync();

            return groups.Select(x => new ItemGroupResponseDto
            {
                ItemGroupId = x.ItemGroupId,
                ItemGroupName = x.ItemGroupName,
                ParentGroupId = x.ParentGroupId,
                ParentGroupName = x.ParentGroup?.ItemGroupName,

                GroupCode = x.GroupCode,
                IsActive = x.IsActive,
               
            }).ToList();
        }

        public async Task<ItemGroupResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var group = await _context.ItemGroups
                .Include(x => x.ParentGroup)
                .FirstOrDefaultAsync(x =>
                    x.ItemGroupId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (group == null)
                return null;

            return new ItemGroupResponseDto
            {
                ItemGroupId = group.ItemGroupId,
                ItemGroupName = group.ItemGroupName,
                ParentGroupId = group.ParentGroupId,
                ParentGroupName = group.ParentGroup?.ItemGroupName,
                GroupCode = group.GroupCode,
                IsActive = group.IsActive,
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var group = await _context.ItemGroups
                .FirstOrDefaultAsync(x =>
                    x.ItemGroupId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (group == null)
                return false;

            group.IsDeleted = true;
            group.IsActive = false;
            group.ModifiedBy = _common.GetUserId();
            group.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<PagedResponseDto<ItemGroupResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.ItemGroups
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.ItemGroupName.ToLower().Contains(search));
            }

            // FILTERS
            if (request.Filters != null)
            {              

                if (request.Filters.ContainsKey("isActive"))
                {
                    var isActive = ((JsonElement)request.Filters["isActive"]).GetBoolean();
                    //var isActive = Convert.ToBoolean(request.Filters["isActive"]);
                    query = query.Where(x => x.IsActive == isActive);
                }

                if (request.Filters.ContainsKey("parentGroupId"))
                {
                    var parentGroupId = ((JsonElement)request.Filters["parentGroupId"]).GetInt32();
                    //var parentGroupId = Convert.ToInt32(request.Filters["parentGroupId"]);
                    query = query.Where(x => x.ParentGroupId == parentGroupId);
                }
            }

            // SORTING
            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();

                switch (sort.Column.ToLower())
                {

                    case "parentgroup":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.ParentGroup)
                            : query.OrderBy(x => x.ParentGroup);
                        break;

                    case "isactive":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.IsActive)
                            : query.OrderBy(x => x.IsActive);
                        break;

                    case "groupcode":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.GroupCode)
                            : query.OrderBy(x => x.GroupCode);
                        break;

                    default:
                        query = query.OrderBy(x => x.ItemGroupName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.ItemGroupName);
            }

            // TOTAL RECORD COUNT
            var totalRecords = await query.CountAsync();

            // PAGINATION + DATA
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(group => new ItemGroupResponseDto
                {
                    ItemGroupId = group.ItemGroupId,
                    ItemGroupName = group.ItemGroupName,
                    ParentGroupId = group.ParentGroupId,
                    ParentGroupName = group.ParentGroup != null
                        ? group.ParentGroup.ItemGroupName
                        : null,
                    GroupCode = group.GroupCode,
                    IsActive = group.IsActive,
                })
                .ToListAsync();

            return new PagedResponseDto<ItemGroupResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };

        }
    }
}
