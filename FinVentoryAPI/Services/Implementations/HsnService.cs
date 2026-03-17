using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.HsnDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.TaxTDOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class HsnService : IHsnService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public HsnService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<HsnResponseDto> CreateAsync(CreateHsnDto dto)
        {
            var CompanyId = _common.GetCompanyId();

            var duplicate = await _context.Hsns
                .AnyAsync(x =>
                x.CompanyId == CompanyId &&
                x.HsnName.ToLower() == dto.HsnName.ToLower() &&
                !x.IsDeleted);

            if (duplicate)
                throw new Exception("HSN already exists. ");

            var hsn = new Hsn
            {
                CompanyId = CompanyId,
                HsnName = dto.HsnName,
                HSNType = dto.HsnType,
                Description = dto.Description,
                TaxId = dto.TaxId,
                Cess = dto.Cess,
                CreatedBy = _common.GetUserId()

            };

            _context.Hsns.Add(hsn);
            await _context.SaveChangesAsync();

            return new HsnResponseDto
            {
                HsnId = hsn.HsnId,
                HsnName = hsn.HsnName,
                HsnType = hsn.HSNType,
                Description = hsn.Description,
                TaxId = dto.TaxId,
                Cess = dto.Cess
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateHsnDto dto)
        {
            var CompanyId = _common.GetCompanyId();

            var hsn = await _context.Hsns
                .FirstOrDefaultAsync(x =>
                   x.HsnId == id &&
                   x.CompanyId == CompanyId &&
                   !x.IsDeleted);
            if (hsn == null)
            {
                return false;
            }

            var duplicate = await _context.Hsns
               .AnyAsync(x =>
                   x.CompanyId == CompanyId &&
                   x.HsnName.ToLower() == dto.HsnName.ToLower() &&
                   x.HsnId != id &&
                   !x.IsDeleted);

            if (duplicate)
                throw new Exception("Another HSN with same name already exists.");
            hsn.HsnId = dto.HsnId;
            hsn.HsnName = dto.HsnName;
            hsn.HSNType = dto.HsnType;
            hsn.Description = dto.Description;
            hsn.TaxId = dto.TaxId;
            hsn.Cess = dto.Cess;
            hsn.IsActive = dto.IsActive;
            hsn.ModifiedBy = _common.GetUserId();
            hsn.ModifiedDate = dto.ModifiedDate;


            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<HsnResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var hsn = await _context.Hsns
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .OrderBy(x => x.HsnId)
                .ToListAsync();

            return hsn.Select(x => new HsnResponseDto
            {
                HsnId = x.HsnId,
                HsnName = x.HsnName,
                HsnType = x.HSNType,
                Description = x.Description,
                TaxId = x.TaxId,
                Cess = x.Cess,
                IsActive = x.IsActive

            }).ToList();
        }

        public async Task<HsnResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var hsn = await _context.Hsns
            .FirstOrDefaultAsync(x =>
                x.HsnId == id &&
                x.CompanyId == companyId &&
                !x.IsDeleted);

            if (hsn == null)
                return null;

            return new HsnResponseDto
            {
                HsnId = hsn.HsnId,
                HsnName = hsn.HsnName,
                HsnType = hsn.HSNType,
                Description = hsn.Description,
                TaxId = hsn.TaxId,
                Cess = hsn.Cess,
                IsActive = hsn.IsActive
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var tax = await _context.Hsns
                .FirstOrDefaultAsync(x =>
                    x.HsnId  == id &&
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

        public async Task<PagedResponseDto<HsnResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.Hsns
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();

                query = query.Where(x =>
                    x.HsnName.ToLower().Contains(search));
            }

            // FILTERS
            if (request.Filters != null)
            {
                if (request.Filters.TryGetValue("hsnType", out var value))
                {
                    var hsnType = ((JsonElement)value).GetString();
                    query = query.Where(x => x.HSNType == hsnType);
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
                    case "hsnname":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.HsnName)
                            : query.OrderBy(x => x.HsnName);
                        break;

                    case "hsntype":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.HSNType)
                            : query.OrderBy(x => x.HSNType);
                        break;

                    case "taxname":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.tax.TaxName)
                            : query.OrderBy(x => x.TaxId);
                        break;

                    case "isactive":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.IsActive)
                            : query.OrderBy(x => x.IsActive);
                        break;

                    default:
                        query = query.OrderBy(x => x.HsnName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.HsnName);
            }

            // TOTAL COUNT
            var totalRecords = await query.CountAsync();

            // PAGINATION
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new HsnResponseDto
                {
                    HsnId = x.HsnId,
                   HsnName = x.HsnName,
                   Description = x.Description,
                   HsnType = x.HSNType,
                   TaxId = x.TaxId,
                   TaxName = x.tax.TaxName,
                   Cess = x.Cess,
                    IsActive = x.IsActive,

                })
                .ToListAsync();

            return new PagedResponseDto<HsnResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }
    }
}
