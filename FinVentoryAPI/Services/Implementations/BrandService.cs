using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountGroupDTOs;
using FinVentoryAPI.DTOs.BrandDTOs;
using FinVentoryAPI.DTOs.ItemGroupDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class BrandService: IBrandService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public BrandService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<BrandResponseDto> CreateAsync(CreateBrandDto dto)
        {
            var companyId = _common.GetCompanyId();

            var duplicate = await _context.Brands
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.BrandName.ToLower() == dto.BrandName.ToLower() &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Brand Name already exists.");

            var brand = new Brand
            {
                CompanyId = companyId,
                BrandName = dto.BrandName,
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return new BrandResponseDto
            {
                BrandId = brand.BrandId,
                BrandName = brand.BrandName,
                IsActive = brand.IsActive,
            };           
        }

        public async Task<bool> UpdateAsync(int id, UpdateBrandDto dto)
        {
            var companyId = _common.GetCompanyId();

            var itemGroup = await _context.Brands
                .FirstOrDefaultAsync(x =>
                    x.BrandId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (itemGroup == null)
                return false;

            var duplicate = await _context.Brands
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.BrandName.ToLower() == dto.BrandName.ToLower() &&
                    x.BrandId != id &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Brand Name with same name already exists.");

            itemGroup.BrandName = dto.BrandName;          
            itemGroup.IsActive = dto.IsActive;
            itemGroup.ModifiedBy = _common.GetUserId();
            itemGroup.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<BrandResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var groups = await _context.Brands
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)                
                .ToListAsync();

            return groups.Select(x => new BrandResponseDto
            {
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                IsActive = x.IsActive,

            }).ToList();
        }

        public async Task<BrandResponseDto> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var brand = await _context.Brands               
                .FirstOrDefaultAsync(x =>
                    x.BrandId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (brand == null)
                return null;

            return new BrandResponseDto
            {
                BrandId = brand.BrandId,
                BrandName = brand.BrandName,
                IsActive = brand.IsActive,
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var brand = await _context.Brands
                .FirstOrDefaultAsync(x =>
                    x.BrandId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (brand == null)
                return false;

            brand.IsDeleted = true;
            brand.IsActive = false;
            brand.ModifiedBy = _common.GetUserId();
            brand.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResponseDto<BrandResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.Brands
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.BrandName.ToLower().Contains(search));
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
            
            }

            // SORTING
            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();

                switch (sort.Column.ToLower())
                {

                    case "isactive":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.IsActive)
                            : query.OrderBy(x => x.IsActive);
                        break;                                           

                    default:
                        query = query.OrderBy(x => x.BrandName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.BrandName);
            }

            // TOTAL RECORD COUNT
            var totalRecords = await query.CountAsync();

            // PAGINATION + DATA
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(group => new BrandResponseDto
                {
                    BrandId = group.BrandId,
                    BrandName = group.BrandName,                    
                    IsActive = group.IsActive,
                })
                .ToListAsync();

            return new PagedResponseDto<BrandResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };

        }
    }
}
