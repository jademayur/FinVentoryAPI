using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountGroupDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class AccountGroupService : IAccountGroupService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public AccountGroupService(
            AppDbContext context,           
            Common common)
        {
            _context = context;          ;
            _common = common;
        }       

        // ========================================
        // CREATE
        // ========================================
        public async Task<AccountGroupResponseDto> CreateAsync(CreateAccountGroupDto dto)
        {
            var companyId = _common.GetCompanyId();

            var duplicate = await _context.AccountGroups
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.GroupName.ToLower() == dto.GroupName.ToLower() &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Account group already exists.");

            var accountGroup = new AccountGroup
            {
                CompanyId = companyId,
                GroupName = dto.GroupName,
                ParentGroupId = dto.ParentGroupId,
                GroupType = dto.GroupType,
                BalanceTo = dto.BalanceTo,
                SortOrder = dto.SortOrder,
                CreatedBy = dto.CreatedBy
            };

            _context.AccountGroups.Add(accountGroup);
            await _context.SaveChangesAsync();

            return await MapToResponseAsync(accountGroup.AccountGroupId);
        }

        // ========================================
        // UPDATE
        // ========================================
        public async Task<bool> UpdateAsync(int id, UpdateAccountGroupDto dto)
        {
            var companyId = _common.GetCompanyId();

            var accountGroup = await _context.AccountGroups
                .FirstOrDefaultAsync(x =>
                    x.AccountGroupId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (accountGroup == null)
                return false;

            var duplicate = await _context.AccountGroups
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.GroupName.ToLower() == dto.GroupName.ToLower() &&
                    x.AccountGroupId != id &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Another account group with same name already exists.");

            accountGroup.GroupName = dto.GroupName;
            accountGroup.ParentGroupId = dto.ParentGroupId;
            accountGroup.GroupType = dto.GroupType;
            accountGroup.BalanceTo = dto.BalanceTo;
            accountGroup.SortOrder = dto.SortOrder;
            accountGroup.IsActive = dto.IsActive;
            accountGroup.ModifiedBy = _common.GetUserId();
            accountGroup.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // ========================================
        // GET ALL
        // ========================================
        public async Task<List<AccountGroupResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var groups = await _context.AccountGroups
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.ParentGroup)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            return groups.Select(x => new AccountGroupResponseDto
            {
                AccountGroupId = x.AccountGroupId,
                GroupName = x.GroupName,
                ParentGroupId = x.ParentGroupId,
                ParentGroupName = x.ParentGroup?.GroupName,

                GroupTypeId = (int)x.GroupType,
                GroupTypeName = x.GroupType.ToString(),

                BalanceToId = (int)x.BalanceTo,
                BalanceToName = x.BalanceTo.ToString(),

                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate
            }).ToList();
        }

        // ========================================
        // GET BY ID
        // ========================================
        public async Task<AccountGroupResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var group = await _context.AccountGroups
                .Include(x => x.ParentGroup)
                .FirstOrDefaultAsync(x =>
                    x.AccountGroupId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (group == null)
                return null;

            return new AccountGroupResponseDto
            {
                AccountGroupId = group.AccountGroupId,
                GroupName = group.GroupName,
                ParentGroupId = group.ParentGroupId,
                ParentGroupName = group.ParentGroup?.GroupName,

                GroupTypeId = (int)group.GroupType,
                GroupTypeName = group.GroupType.ToString(),

                BalanceToId = (int)group.BalanceTo,
                BalanceToName = group.BalanceTo.ToString(),

                SortOrder = group.SortOrder,
                IsActive = group.IsActive,
                CreatedDate = group.CreatedDate
            };
        }

        // ========================================
        // SOFT DELETE
        // ========================================
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var group = await _context.AccountGroups
                .FirstOrDefaultAsync(x =>
                    x.AccountGroupId == id &&
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

        // ========================================
        // PRIVATE: Return Full Response After Create
        // ========================================
        private async Task<AccountGroupResponseDto> MapToResponseAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var group = await _context.AccountGroups
                .Include(x => x.ParentGroup)
                .FirstAsync(x =>
                    x.AccountGroupId == id &&
                    x.CompanyId == companyId);

            return new AccountGroupResponseDto
            {
                AccountGroupId = group.AccountGroupId,
                GroupName = group.GroupName,
                ParentGroupId = group.ParentGroupId,
                ParentGroupName = group.ParentGroup?.GroupName,

                GroupTypeId = (int)group.GroupType,
                GroupTypeName = group.GroupType.ToString(),

                BalanceToId = (int)group.BalanceTo,
                BalanceToName = group.BalanceTo.ToString(),

                SortOrder = group.SortOrder,
                IsActive = group.IsActive,
                CreatedDate = group.CreatedDate
            };
        }

        public async Task<PagedResponseDto<AccountGroupResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.AccountGroups
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.GroupName.ToLower().Contains(search));
            }

            // FILTERS
            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("groupTypeId"))
                {
                    //var groupTypeId = Convert.ToInt32(request.Filters["groupTypeId"]);
                    var groupTypeId = ((JsonElement)request.Filters["groupTypeId"]).GetInt32();
                    query = query.Where(x => (int)x.GroupType == groupTypeId);
                }

                if (request.Filters.ContainsKey("balanceToId"))
                {
                    //var balanceToId = Convert.ToInt32(request.Filters["balanceToId"]);
                    var balanceToId = ((JsonElement)request.Filters["balanceToId"]).GetInt32();
                    query = query.Where(x => (int)x.BalanceTo == balanceToId);
                }

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
                    case "groupname":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.GroupName)
                            : query.OrderBy(x => x.GroupName);
                        break;

                    case "grouptype":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.GroupType)
                            : query.OrderBy(x => x.GroupType);
                        break;

                    case "balanceto":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.BalanceTo)
                            : query.OrderBy(x => x.BalanceTo);
                        break;

                    case "isactive":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.IsActive)
                            : query.OrderBy(x => x.IsActive);
                        break;

                    case "sortorder":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.SortOrder)
                            : query.OrderBy(x => x.SortOrder);
                        break;

                    default:
                        query = query.OrderBy(x => x.GroupName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.GroupName);
            }

            // TOTAL RECORD COUNT
            var totalRecords = await query.CountAsync();

            // PAGINATION + DATA
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new AccountGroupResponseDto
                {
                    AccountGroupId = x.AccountGroupId,
                    GroupName = x.GroupName,

                    ParentGroupId = x.ParentGroupId,
                    ParentGroupName = x.ParentGroup != null
                        ? x.ParentGroup.GroupName
                        : null,

                    GroupTypeId = (int)x.GroupType,
                    GroupTypeName = EnumHelper.GetDisplayName(x.GroupType),

                    BalanceToId = (int)x.BalanceTo,
                    BalanceToName = EnumHelper.GetDisplayName(x.BalanceTo),

                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return new PagedResponseDto<AccountGroupResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };

        }
    }
}