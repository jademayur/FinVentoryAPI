using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.DocumentTypeDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public DocumentTypeService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<DocumentTypeResponseDto> CreateAsync(CreateDocumentTypeDto dto)
        {
            var companyId = _common.GetCompanyId();

            var duplicate = await _context.DocumentTypes
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.TypeName.ToLower() == dto.TypeName.ToLower() &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Document Type already exists.");

            var entity = new DocType
            {
                CompanyId = companyId,
                TypeName = dto.TypeName,
                CreatedBy = _common.GetUserId()
            };

            _context.DocumentTypes.Add(entity);
            await _context.SaveChangesAsync();

            return new DocumentTypeResponseDto
            {
                DocumentTypeId = entity.DocumentTypeId,
                TypeName = entity.TypeName,
                IsActive = entity.IsActive,
            };
        }

        public async Task<List<DocumentTypeResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            return await _context.DocumentTypes
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .OrderBy(x => x.TypeName)
                .Select(x => new DocumentTypeResponseDto
                {
                    DocumentTypeId = x.DocumentTypeId,
                    TypeName = x.TypeName,
                    IsActive = x.IsActive,
                })
                .ToListAsync();
        }

        public async Task<DocumentTypeResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var entity = await _context.DocumentTypes
                .FirstOrDefaultAsync(x =>
                    x.DocumentTypeId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (entity == null)
                return null;

            return new DocumentTypeResponseDto
            {
                DocumentTypeId = entity.DocumentTypeId,
                TypeName = entity.TypeName,
                IsActive = entity.IsActive,
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateDocumentTypeDto dto)
        {
            var companyId = _common.GetCompanyId();

            var entity = await _context.DocumentTypes
                .FirstOrDefaultAsync(x =>
                    x.DocumentTypeId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (entity == null)
                return false;

            var duplicate = await _context.DocumentTypes
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.TypeName.ToLower() == dto.TypeName.ToLower() &&
                    x.DocumentTypeId != id &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Document Type with same name already exists.");

            entity.TypeName = dto.TypeName;
            entity.IsActive = dto.IsActive;
            entity.ModifiedBy = _common.GetUserId();
            entity.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var entity = await _context.DocumentTypes
                .FirstOrDefaultAsync(x =>
                    x.DocumentTypeId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (entity == null)
                return false;

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.ModifiedBy = _common.GetUserId();
            entity.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResponseDto<DocumentTypeResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.DocumentTypes
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.TypeName.ToLower().Contains(search));
            }

            if (request.Filters != null && request.Filters.ContainsKey("isActive"))
            {
                var isActive = ((JsonElement)request.Filters["isActive"]).GetBoolean();
                query = query.Where(x => x.IsActive == isActive);
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Direction == "desc"
                    ? query.OrderByDescending(x => x.TypeName)
                    : query.OrderBy(x => x.TypeName);
            }
            else
            {
                query = query.OrderBy(x => x.TypeName);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new DocumentTypeResponseDto
                {
                    DocumentTypeId = x.DocumentTypeId,
                    TypeName = x.TypeName,
                    IsActive = x.IsActive,
                })
                .ToListAsync();

            return new PagedResponseDto<DocumentTypeResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }
    }
}
