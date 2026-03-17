using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountGroupDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.WarehouseDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class WarehouseService : IWarehouseService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public WarehouseService(
            AppDbContext context,
            Common common)
        {
            _context = context; ;
            _common = common;
        }

        // ========================================
        // CREATE
        // ========================================
        public async Task<WarehouseResponseDto> CreateAsync(CreateWarehouseDto dto)
        {
            var companyId = _common.GetCompanyId();

            var duplicate = await _context.Warehouses
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.WarehouseName.ToLower() == dto.WarehouseName.ToLower() &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Warehouse already exists.");

            var warehouse = new Warehouse
            {
                CompanyId = companyId,
                WarehouseName = dto.WarehouseName,
                ParentWarehouseId = dto.ParentWarehouseId,
                WarehouseCode = dto.WarehouseCode,
                Address = dto.Address,
                City = dto.City,
                ContactPerson = dto.ContactPerson,
                MobileNo = dto.MobileNo,
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            return new WarehouseResponseDto
            {
                WarehouseId = warehouse.WarehouseId,
                WarehouseName = warehouse.WarehouseName,
                WarehouseCode = warehouse.WarehouseCode,
                ParentWarehouseId = warehouse.ParentWarehouseId,
                ParentWarehouseName = warehouse.ParentWarehouse != null
                   ? warehouse.ParentWarehouse.WarehouseName
                   : null,
                IsActive = warehouse.IsActive,
            };
        }

        // ========================================
        // UPDATE
        // ========================================
        public async Task<bool> UpdateAsync(int id, UpdateWarehouseDto dto)
        {
            var companyId = _common.GetCompanyId();

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (warehouse == null)
                return false;

            var duplicate = await _context.Warehouses
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.WarehouseName.ToLower() == dto.WarehouseName.ToLower() &&
                    x.WarehouseId != id &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Warehouse with same name already exists.");

            warehouse.WarehouseName=dto.WarehouseName;
            warehouse.WarehouseCode=dto.WarehouseCode;
            warehouse.ParentWarehouseId=dto.ParentWarehouseId;
            warehouse.Address=dto.Address;
            warehouse.City=dto.City;
            warehouse.ContactPerson=dto.ContactPerson;
            warehouse.MobileNo=dto.MobileNo;
            warehouse.IsActive = dto.IsActive;
            warehouse.ModifiedBy = _common.GetUserId();
            warehouse.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // ========================================
        // GET ALL
        // ========================================
        public async Task<List<WarehouseResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var warehouses = await _context.Warehouses
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.ParentWarehouse)                
                .ToListAsync();

            return warehouses.Select(x => new WarehouseResponseDto
            {               
                WarehouseId = x.WarehouseId,
                WarehouseName = x.WarehouseName,
                WarehouseCode = x.WarehouseCode,
                ParentWarehouseId = x.ParentWarehouseId,
                ParentWarehouseName = x.ParentWarehouse != null
                    ? x.ParentWarehouse.WarehouseName
                    : null,
                IsActive = x.IsActive,              
            }).ToList();
        }

        // ========================================
        // GET BY ID
        // ========================================
        public async Task<WarehouseResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var warehouse = await _context.Warehouses
                .Include(x => x.ParentWarehouse)
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (warehouse == null)
                return null;

            return new WarehouseResponseDto
            {
                WarehouseId = warehouse.WarehouseId,
                WarehouseName = warehouse.WarehouseName,
                WarehouseCode = warehouse.WarehouseCode,
                ParentWarehouseId = warehouse.ParentWarehouseId,
                ParentWarehouseName = warehouse.ParentWarehouse != null
                    ? warehouse.ParentWarehouse.WarehouseName
                    : null,
                IsActive = warehouse.IsActive,
            };
        }

        // ========================================
        // SOFT DELETE
        // ========================================
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var group = await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == id &&
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
           
          

        public async Task<PagedResponseDto<WarehouseResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.Warehouses
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.WarehouseName.ToLower().Contains(search));
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
                        query = query.OrderBy(x => x.WarehouseName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.WarehouseName);
            }

            // TOTAL RECORD COUNT
            var totalRecords = await query.CountAsync();

            // PAGINATION + DATA
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new WarehouseResponseDto
                {
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.WarehouseName,
                    WarehouseCode = x.WarehouseCode,
                    ParentWarehouseId = x.ParentWarehouseId,
                    ParentWarehouseName = x.ParentWarehouse != null
                    ? x.ParentWarehouse.WarehouseName
                    : null,
                    IsActive = x.IsActive,
                })
                .ToListAsync();

            return new PagedResponseDto<WarehouseResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };

        }
    }
}
