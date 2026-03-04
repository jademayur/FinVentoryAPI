using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountGroupDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace FinVentoryAPI.Services.Implementations
{
    public class AccountGroupService : IAccountGroupService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountGroupService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // ========================================
        // PRIVATE: Get CompanyId From Token
        // ========================================
        private int GetCompanyId()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User?.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(claim))
                throw new Exception("CompanyId not found in token.");

            return int.Parse(claim);
        }

        // ========================================
        // CREATE
        // ========================================
        public async Task<AccountGroupResponseDto> CreateAsync(CreateAccountGroupDto dto)
        {
            var companyId = GetCompanyId();

            var duplicate = await _context.AccountGroups
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.GroupName.ToLower() == dto.GroupName.ToLower() &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Account group already exists.");

            var accountGroup = new AccountGroup
            {
                GroupName = dto.GroupName,
                ParentGroupId = dto.ParentGroupId,
                GroupType = dto.GroupType,
                BalanceTo = dto.BalanceTo,
                SortOrder = dto.SortOrder
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
            var companyId = GetCompanyId();

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

            await _context.SaveChangesAsync();

            return true;
        }

        // ========================================
        // GET ALL
        // ========================================
        public async Task<List<AccountGroupResponseDto>> GetAllAsync()
        {
            var companyId = GetCompanyId();

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
            var companyId = GetCompanyId();

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
            var companyId = GetCompanyId();

            var group = await _context.AccountGroups
                .FirstOrDefaultAsync(x =>
                    x.AccountGroupId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (group == null)
                return false;

            group.IsDeleted = true;
            group.IsActive = false;

            await _context.SaveChangesAsync();

            return true;
        }

        // ========================================
        // PRIVATE: Return Full Response After Create
        // ========================================
        private async Task<AccountGroupResponseDto> MapToResponseAsync(int id)
        {
            var companyId = GetCompanyId();

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
            var companyId = GetCompanyId();

            var query = _context.AccountGroups
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.GroupName.ToLower().Contains(search));
            }

            // 🎯 FILTER
            if (request.GroupTypeId.HasValue)
                query = query.Where(x => (int)x.GroupType == request.GroupTypeId);

            if (request.BalanceToId.HasValue)
                query = query.Where(x => (int)x.BalanceTo == request.BalanceToId);

            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive);

            // 📊 SORT
            switch (request.SortBy?.ToLower())
            {
                case "groupname":
                    query = request.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.GroupName)
                        : query.OrderBy(x => x.GroupName);
                    break;

                case "isactive":
                    query = request.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.IsActive)
                        : query.OrderBy(x => x.IsActive);
                    break;

                case "grouptype":
                    query = request.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.GroupType)
                        : query.OrderBy(x => x.GroupType);
                    break;

                default:
                    query = query.OrderBy(x => x.GroupName);
                    break;
            }

            var totalRecords = await query.CountAsync();

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
                    GroupTypeName = x.GroupType.ToString(),

                    BalanceToId = (int)x.BalanceTo,
                    BalanceToName = x.BalanceTo.ToString(),

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