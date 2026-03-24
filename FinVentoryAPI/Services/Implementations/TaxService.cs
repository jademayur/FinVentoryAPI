using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.TaxTDOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace FinVentoryAPI.Services.Implementations
{
    public class TaxService : ITaxService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public TaxService(AppDbContext context, Common common)
        {
            _context = context;
           _common = common;
        }
        
        public async Task<TaxResponseDto> CreateAsync(CreateTaxDto dto)
        {
            var CompanyId = _common.GetCompanyId();

            var duplicate = await _context.Taxes
                .AnyAsync(x =>
                x.CompanyId == CompanyId &&
                x.TaxName.ToLower() == dto.TaxName.ToLower() &&
                !x.IsDeleted);

            if(duplicate)
                throw new Exception("Tax Name already exists. ");

            var tax = new Tax
            {
                CompanyId = CompanyId,
                TaxName = dto.TaxName,
                TaxType = dto.TaxType,
                TaxRate = dto.TaxRate,
                IGST = dto.IGST,
                SGST = dto.SGST,
                CGST = dto.CGST,
                IGSTPostingAccountId = dto.IGSTPostingAccountId,
                SGSTPostingAccountId = dto.SGSTPostingAccountId,
                CGSTPostingAccountId = dto.CGSTPostingAccountId,
                CreatedBy = _common.GetUserId(),

            };

            _context.Taxes.Add(tax);
            await _context.SaveChangesAsync();

            return new TaxResponseDto
            {
                TaxName = tax.TaxName,
                TaxType = tax.TaxType,
                TaxRate = tax.TaxRate,
                IGST = tax.IGST,
                SGST = tax.SGST,
                CGST = tax.CGST,
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateTaxDto dto)
        {
            var CompanyId = _common.GetCompanyId();

            var tax = await _context.Taxes
                .FirstOrDefaultAsync(x =>
                   x.TaxId == id &&
                   x.CompanyId == CompanyId &&
                   !x.IsDeleted);
            if (tax == null)
            {
                return false;
            }

            var duplicate = await _context.Taxes
               .AnyAsync(x =>
                   x.CompanyId == CompanyId &&
                   x.TaxName.ToLower() == dto.TaxName.ToLower() &&
                   x.TaxId != id &&
                   !x.IsDeleted);

            if (duplicate)
                throw new Exception("Another Tax with same name already exists.");

            tax.TaxName = dto.TaxName;
            tax.TaxRate = dto.TaxRate;
            tax.TaxType = dto.TaxType;
            tax.IGST = dto.IGST;
            tax.CGST = dto.CGST;
            tax.SGST = dto.SGST;
            tax.IsActive = dto.IsActive;
            tax.ModifiedBy = _common.GetUserId();
            tax.ModifiedDate = DateTime.UtcNow;
            tax.IGSTPostingAccountId = dto.IGSTPostingAccountId;
            tax.SGSTPostingAccountId = dto.SGSTPostingAccountId;
            tax.CGSTPostingAccountId = dto.CGSTPostingAccountId;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<TaxResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var tax = await _context.Taxes
                .Include(x => x.IGSTAccount)
                .Include(x => x.CGSTAccount)
                .Include(x => x.SGSTAccount)
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .OrderBy(x => x.TaxId)
                .ToListAsync();

            return tax.Select(x => new TaxResponseDto
            {
               TaxId= x.TaxId,
               TaxName = x.TaxName,
               TaxType = x.TaxType,
               TaxRate = x.TaxRate,
               IGST = x.IGST,
               CGST = x.CGST,
               SGST = x.SGST,
               IsActive = x.IsActive,
               IGSTPostingAccountId = x.IGSTPostingAccountId,
               IGSTPostingAccountName = x.IGSTAccount.AccountName,
               CGSTPostingAccountId = x.CGSTPostingAccountId,
               CGSTPostingAccountName = x.CGSTAccount.AccountName,
                SGSTPostingAccountId = x.SGSTPostingAccountId,
                SGSTPostingAccountName = x.SGSTAccount.AccountName,


            }).ToList();
        }

        public async Task<TaxResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var tax = await _context.Taxes
                .Include(x => x.IGSTAccount)
                .Include(x => x.CGSTAccount)
                .Include(x => x.SGSTAccount)
                .FirstOrDefaultAsync(x =>
                    x.TaxId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (tax == null)
                return null;

            return new TaxResponseDto
            {
                TaxId = tax.TaxId,
                TaxName = tax.TaxName,
                TaxType = tax.TaxType,
                TaxRate = tax.TaxRate,
                IGST = tax.IGST,
                CGST = tax.CGST,
                SGST = tax.SGST,
                IsActive = tax.IsActive,
                IGSTPostingAccountId = tax.IGSTPostingAccountId,
                CGSTPostingAccountId = tax.CGSTPostingAccountId,
                SGSTPostingAccountId = tax.SGSTPostingAccountId,
                IGSTPostingAccountName = tax.IGSTAccount != null ? tax.IGSTAccount.AccountName : null,
                CGSTPostingAccountName = tax.CGSTAccount != null ? tax.CGSTAccount.AccountName : null,
                SGSTPostingAccountName = tax.SGSTAccount != null ? tax.SGSTAccount.AccountName : null,
            };
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var tax = await _context.Taxes
                .FirstOrDefaultAsync(x =>
                    x.TaxId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (tax == null)
                return false;

            tax.IsDeleted = true;
            tax.IsActive = false;
            tax.ModifiedBy = _common.GetUserId();
            tax.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResponseDto<TaxResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.Taxes
                .Include(x => x.IGSTAccount)
                .Include(x => x.CGSTAccount)
                .Include(x => x.SGSTAccount)
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();

                query = query.Where(x =>
                    x.TaxName.ToLower().Contains(search));
            }

            // FILTERS
            if (request.Filters != null)
            {
                if (request.Filters.TryGetValue("taxType", out var value))
                {
                    var taxType = ((JsonElement)value).GetString();
                    query = query.Where(x => x.TaxType == taxType);
                }

                if (request.Filters.ContainsKey("isActive"))
                {
                    var isActive = ((JsonElement)request.Filters["isActive"]).GetBoolean();
                    query = query.Where(x => x.IsActive == isActive);
                }
               
            }

            // SORTING
            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();

                switch (sort.Column.ToLower())
                {
                    case "taxname":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.TaxName)
                            : query.OrderBy(x => x.TaxName);
                        break;

                    case "taxtype":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.TaxType)
                            : query.OrderBy(x => x.TaxType);
                        break;

                    case "taxrate":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.TaxRate)
                            : query.OrderBy(x => x.TaxRate);
                        break;

                    case "isactive":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.IsActive)
                            : query.OrderBy(x => x.IsActive);
                        break;

                    default:
                        query = query.OrderBy(x => x.TaxName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.TaxName);
            }

            // TOTAL COUNT
            var totalRecords = await query.CountAsync();

            // PAGINATION
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new TaxResponseDto
                {
                    TaxId = x.TaxId,
                    TaxName = x.TaxName,
                    TaxType  = x.TaxType,
                    TaxRate = x.TaxRate,
                    CGST = x.CGST,
                    SGST = x.SGST,
                    IGST  = x.IGST,
                    IsActive = x.IsActive,
                    IGSTPostingAccountId = x.IGSTPostingAccountId,
                    SGSTPostingAccountId = x.SGSTPostingAccountId,
                    CGSTPostingAccountId = x.CGSTPostingAccountId,
                    IGSTPostingAccountName = x.IGSTAccount != null ? x.IGSTAccount.AccountName : null,
                    CGSTPostingAccountName = x.CGSTAccount != null ? x.CGSTAccount.AccountName : null,
                    SGSTPostingAccountName = x.SGSTAccount != null ? x.SGSTAccount.AccountName : null,

                })
                .ToListAsync();

            return new PagedResponseDto<TaxResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }


    }
}
