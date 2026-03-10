using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Migrations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using Account = FinVentoryAPI.Entities.Account;

namespace FinVentoryAPI.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountService(AppDbContext context,  IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = new HttpContextAccessor();
        }

        private int GetCompanyId()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User?.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(claim))
                throw new Exception("CompanyId not found in token.");

            return int.Parse(claim);
        }


        public async Task<AccountResponseDto> CreateAsync(CreateAccountDto dto)
        {
            var CompanyId = GetCompanyId();
                         
            var duplicate = await _context.Accounts          
                .AnyAsync(x =>
                 x.CompanyId == CompanyId &&
                 x.AccountName.ToLower() == dto.AccountName.ToLower() &&
                 !x.IsDeleted);

            if (duplicate)
                throw new Exception("Account group already exists.");

            var account = new Account
            {
                AccountName = dto.AccountName,
                AccountGroupId = dto.AccountGroupId,
                AccountCode = dto.AccountCode,
                AccountType = (Enums.AccountType)(int)dto.AccountType,
                BookType = (Enums.BookType?)(int?)dto.BookType,
                BookSubType = (Enums.BookSubType?)(int?)dto.BookSubType,
                CompanyId = CompanyId
            };


            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return await MapToResponseAsync(account.AccountId);

        }

        public async Task<bool> UpdateAsync(int id,UpdateAccountDto dto) 
        {
            var CompanyId = GetCompanyId();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(x =>
                   x.AccountId == id &&
                   x.CompanyId == CompanyId &&
                   !x.IsDeleted);
            if (account == null) {
                return false;
            }



            var duplicate = await _context.Accounts
               .AnyAsync(x =>
                   x.CompanyId == CompanyId &&
                   x.AccountName.ToLower() == dto.AccountName.ToLower() &&
                   x.AccountId != id &&
                   !x.IsDeleted);

            if (duplicate) 
                throw new Exception("Another account with same name already exists.");

            account.AccountName = dto.AccountName;
            account.AccountCode = dto.AccountCode;
            account.AccountGroupId = dto.AccountGroupId;
            account.AccountType = dto.AccountType;
            account.BookType = dto.BookType;
            account.BookSubType = dto.BookSubType;
            account.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return true;

        }

        public async Task<List<AccountResponseDto>> GetAllAsync()
        {
            var companyId = GetCompanyId();

            var accounts = await _context.Accounts
                .Where(x =>x.CompanyId == companyId && !x.IsDeleted )
                .Include(x => x.AccountGroup)
                .OrderBy(x => x.AccountId)
                .ToListAsync();

            return accounts.Select(x => new AccountResponseDto
            {
                AccountId = x.AccountId,
                AccountName = x.AccountName,
                AccountCode = x.AccountCode,
                AccountGroupId = x.AccountGroupId,
                AccountGroupName = x.AccountGroup.GroupName,
                AccountTypeId = (int) x.AccountType,
                AccountTypeName= x.AccountType.ToString(),
                BookTypeId = (int?)  x.BookType,
                BookTypeName = x.BookType.ToString(),
                BookSubTypeId = (int?) x.BookSubType,
                BookSubTypeName = x.BookSubType.ToString(),
                IsActive = x.IsActive

            }).ToList();
        }

        public async Task<AccountResponseDto?> GetByIdAsync(int id)
        {
            var companyId = GetCompanyId();

            var account = await _context.Accounts
            .Include(x => x.AccountGroup)
            .FirstOrDefaultAsync(x =>
                x.AccountId == id &&
                x.CompanyId == companyId &&
                !x.IsDeleted);

            if (account == null)
                return null;

            return new AccountResponseDto
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                AccountCode = account.AccountCode,
                AccountGroupId = account.AccountGroupId,
                AccountGroupName = account.AccountGroup.GroupName,
                AccountTypeId = (int)account.AccountType,
                AccountTypeName = account.AccountType.ToString(),
                BookTypeId = (int?)account.BookType,
                BookTypeName = account.BookType.ToString(),
                BookSubTypeId = (int?)account.BookSubType,
                BookSubTypeName = account.BookSubType.ToString(),
                IsActive = account.IsActive

            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = GetCompanyId();

            var group = await _context.Accounts
                .FirstOrDefaultAsync(x =>
                    x.AccountId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (group == null)
                return false;

            group.IsDeleted = true;
            group.IsActive = false;

            await _context.SaveChangesAsync();

            return true;
        }


        private async Task<AccountResponseDto> MapToResponseAsync (int id)
        {
            var companyId = GetCompanyId();

            var account = await _context.Accounts
            .Include(x => x.AccountGroup)
            .FirstOrDefaultAsync(x =>
                x.AccountId == id &&
                x.CompanyId == companyId &&
                !x.IsDeleted);
            return new AccountResponseDto
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                AccountCode = account.AccountCode,
                AccountGroupId = account.AccountGroupId,
                AccountGroupName = account.AccountGroup.GroupName,
                AccountTypeId = (int)account.AccountType,
                AccountTypeName = account.AccountType.ToString(),
                BookTypeId = (int?)account.BookType,
                BookTypeName = account.BookType.ToString(),
                BookSubTypeId = (int?)account.BookSubType,
                BookSubTypeName = account.BookSubType.ToString(),
                IsActive = account.IsActive

            };

        }

        public async Task<PagedResponseDto<AccountResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = GetCompanyId();

            var query = _context.Accounts
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.AccountName.ToLower().Contains(search)
                                      || x.AccountCode!.ToLower().Contains(search));
            }

            // FILTERS
            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("accountTypeId"))
                {
                    var accountTypeId = Convert.ToInt32(request.Filters["accountTypeId"]);
                    query = query.Where(x => (int)x.AccountType == accountTypeId);
                }

                if (request.Filters.ContainsKey("accountGroupId"))
                {
                    var groupId = Convert.ToInt32(request.Filters["accountGroupId"]);
                    query = query.Where(x => x.AccountGroupId == groupId);
                }

                if (request.Filters.ContainsKey("isActive"))
                {
                    var isActive = Convert.ToBoolean(request.Filters["isActive"]);
                    query = query.Where(x => x.IsActive == isActive);
                }

                if (request.Filters.ContainsKey("bookTypeId"))
                {
                    var bookTypeId = Convert.ToInt32(request.Filters["bookTypeId"]);
                    query = query.Where(x => (int)x.BookType == bookTypeId);
                }
            }

            // SORTING
            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();

                switch (sort.Column.ToLower())
                {
                    case "accountname":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.AccountName)
                            : query.OrderBy(x => x.AccountName);
                        break;

                    case "accountcode":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.AccountCode)
                            : query.OrderBy(x => x.AccountCode);
                        break;

                    case "accounttypename":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.AccountType)
                            : query.OrderBy(x => x.AccountType);
                        break;

                    case "isactive":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.IsActive)
                            : query.OrderBy(x => x.IsActive);
                        break;

                    default:
                        query = query.OrderBy(x => x.AccountName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.AccountName);
            }

            // TOTAL COUNT
            var totalRecords = await query.CountAsync();

            // PAGINATION + DATA
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new AccountResponseDto
                {
                    AccountId = x.AccountId,
                    AccountName = x.AccountName,
                    AccountCode = x.AccountCode,
                    AccountGroupId = x.AccountGroupId,               
                    AccountGroupName = x.AccountGroup != null ? x.AccountGroup.GroupName : null,
                    AccountTypeId = (int)x.AccountType,
                    AccountTypeName = x.AccountType.ToString(),
                    BookTypeId = (int?)x.BookType,
                    BookTypeName = x.BookType.ToString(),
                    BookSubTypeId = (int?)x.BookSubType,
                    BookSubTypeName = x.BookSubType.ToString(),
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            return new PagedResponseDto<AccountResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }


    }
}
