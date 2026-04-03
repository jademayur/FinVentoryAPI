using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesPersonDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FinVentoryAPI.Helpers;

namespace FinVentoryAPI.Services.Implementations
{
    public class SalesPersonService : ISalesPersonService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public SalesPersonService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<SalesPersonResponseDto> CreateAsync(SalesPersonCreateDto dto)
        {
            var companyId = _common.GetCompanyId();

            var duplicate = await _context.SalesPersons
                .AnyAsync(x => x.CompanyId == companyId
                            && x.SalesPersonName.ToLower() == dto.SalesPersonName.ToLower()
                            && !x.IsDeleted);

            if (duplicate) throw new Exception("Sales Person already exists.");

            var sp = new SalesPerson
            {
                CompanyId = companyId,
                SalesPersonCode = dto.SalesPersonCode,
                SalesPersonName = dto.SalesPersonName,
                Mobile = dto.Mobile,
                Email = dto.Email,
                CommissionPct = dto.CommissionPct,
                CreatedBy = _common.GetUserId()
            };

            _context.SalesPersons.Add(sp);
            await _context.SaveChangesAsync();
            return MapToResponse(sp);
        }

        public async Task<List<SalesPersonResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();
            var list = await _context.SalesPersons
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .ToListAsync();

            return list.Select(MapToResponse).ToList();
        }

        public async Task<SalesPersonResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var sp = await _context.SalesPersons
                .FirstOrDefaultAsync(x => x.SalesPersonId == id
                                       && x.CompanyId == companyId
                                       && !x.IsDeleted);

            return sp == null ? null : MapToResponse(sp);
        }

        public async Task<bool> UpdateAsync(int id, SalesPersonUpdateDto dto)
        {
            var companyId = _common.GetCompanyId();
            var sp = await _context.SalesPersons
                .FirstOrDefaultAsync(x => x.SalesPersonId == id
                                       && x.CompanyId == companyId
                                       && !x.IsDeleted);

            if (sp == null) return false;

            sp.SalesPersonCode = dto.SalesPersonCode;
            sp.SalesPersonName = dto.SalesPersonName;
            sp.Mobile = dto.Mobile;
            sp.Email = dto.Email;
            sp.CommissionPct = dto.CommissionPct;
            sp.IsActive = dto.IsActive;
            sp.ModifiedDate = DateTime.UtcNow;
            sp.ModifiedBy = _common.GetUserId();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var sp = await _context.SalesPersons
                .FirstOrDefaultAsync(x => x.SalesPersonId == id
                                       && x.CompanyId == companyId
                                       && !x.IsDeleted);

            if (sp == null) return false;

            sp.IsDeleted = true;
            sp.IsActive = false;
            sp.ModifiedDate = DateTime.UtcNow;
            sp.ModifiedBy = _common.GetUserId();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResponseDto<SalesPersonResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.SalesPersons
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.SalesPersonName.ToLower().Contains(search) ||
                    (x.SalesPersonCode ?? "").ToLower().Contains(search) ||
                    (x.Mobile ?? "").ToLower().Contains(search) ||
                    (x.Email ?? "").ToLower().Contains(search));
            }

            // 🎯 FILTERS
            if (request.Filters != null)
            {
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
                    case "salespersonname":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.SalesPersonName)
                            : query.OrderBy(x => x.SalesPersonName);
                        break;

                    case "salespersoncode":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.SalesPersonCode)
                            : query.OrderBy(x => x.SalesPersonCode);
                        break;

                    case "commissionpct":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.CommissionPct)
                            : query.OrderBy(x => x.CommissionPct);
                        break;

                    case "isactive":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.IsActive)
                            : query.OrderBy(x => x.IsActive);
                        break;

                    default:
                        query = query.OrderBy(x => x.SalesPersonName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.SalesPersonName);
            }

            // 📊 TOTAL COUNT
            var totalRecords = await query.CountAsync();

            // 📄 DATA
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new SalesPersonResponseDto
                {
                    SalesPersonId = x.SalesPersonId,
                    SalesPersonCode = x.SalesPersonCode,
                    SalesPersonName = x.SalesPersonName,
                    Mobile = x.Mobile,
                    Email = x.Email,
                    CommissionPct = x.CommissionPct,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return new PagedResponseDto<SalesPersonResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }

        private SalesPersonResponseDto MapToResponse(SalesPerson sp) => new()
        {
            SalesPersonId = sp.SalesPersonId,
            SalesPersonCode = sp.SalesPersonCode,
            SalesPersonName = sp.SalesPersonName,
            Mobile = sp.Mobile,
            Email = sp.Email,
            CommissionPct = sp.CommissionPct,
            IsActive = sp.IsActive
        };


    }
}
