// Services/Implementations/DocumentSeriesMappingService.cs
using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.DocumentSeriesMappingDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class DocumentSeriesMappingService : IDocumentSeriesMappingService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public DocumentSeriesMappingService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ── GET ALL ──────────────────────────────────────────
        public async Task<List<DocumentSeriesMappingResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            return await _context.DocumentSeriesMappings
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.Account)
                .Include(x => x.Series)
                .Select(x => MapToDto(x))
                .ToListAsync();
        }

        // ── GET BY ID ────────────────────────────────────────
        public async Task<DocumentSeriesMappingResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var mapping = await _context.DocumentSeriesMappings
                .Include(x => x.Account)
                .Include(x => x.Series)
                .FirstOrDefaultAsync(x =>
                    x.MappingId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            return mapping == null ? null : MapToDto(mapping);
        }

        // ── CREATE ───────────────────────────────────────────
        public async Task<DocumentSeriesMappingResponseDto> CreateAsync(
            CreateDocumentSeriesMappingDto dto)
        {
            var companyId = _common.GetCompanyId();

            // Validate account exists
            var accountExists = await _context.Accounts
                .AnyAsync(x =>
                    x.AccountId == dto.AccountId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!accountExists)
                throw new Exception("Account not found.");

            // Validate series exists and belongs to this company
            var seriesExists = await _context.DocumentSeries
                .AnyAsync(x =>
                    x.SeriesId == dto.SeriesId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!seriesExists)
                throw new Exception("Document Series not found.");

            // Check duplicate — one series per account
            var duplicate = await _context.DocumentSeriesMappings
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.AccountId == dto.AccountId &&
                    !x.IsDeleted);
            if (duplicate)
                throw new Exception(
                    "A series is already mapped to this Account. " +
                    "Please update the existing mapping instead.");

            var mapping = new DocumentSeriesMapping
            {
                CompanyId = companyId,
                AccountId = dto.AccountId,
                SeriesId = dto.SeriesId,
                CreatedBy = _common.GetUserId(),
                CreatedDate = DateTime.UtcNow
            };

            _context.DocumentSeriesMappings.Add(mapping);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(mapping.MappingId)
                ?? throw new Exception("Failed to retrieve saved mapping.");
        }

        // ── UPDATE ───────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, CreateDocumentSeriesMappingDto dto)
        {
            var companyId = _common.GetCompanyId();

            var mapping = await _context.DocumentSeriesMappings
                .FirstOrDefaultAsync(x =>
                    x.MappingId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (mapping == null) return false;

            // Check duplicate for new accountId (exclude self)
            var duplicate = await _context.DocumentSeriesMappings
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.AccountId == dto.AccountId &&
                    x.MappingId != id &&
                    !x.IsDeleted);
            if (duplicate)
                throw new Exception(
                    "A series is already mapped to this Account.");

            mapping.AccountId = dto.AccountId;
            mapping.SeriesId = dto.SeriesId;
            mapping.ModifiedBy = _common.GetUserId();
            mapping.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── DELETE ───────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var mapping = await _context.DocumentSeriesMappings
                .FirstOrDefaultAsync(x =>
                    x.MappingId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (mapping == null) return false;

            mapping.IsDeleted = true;
            mapping.IsActive = false;
            mapping.ModifiedBy = _common.GetUserId();
            mapping.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── PRIVATE MAP ──────────────────────────────────────
        private static DocumentSeriesMappingResponseDto MapToDto(
            DocumentSeriesMapping x) => new()
            {
                MappingId = x.MappingId,
                AccountId = x.AccountId,
                AccountName = x.Account?.AccountName ?? string.Empty,
                SeriesId = x.SeriesId,
                SeriesName = x.Series?.SeriesName ?? string.Empty,
                DocumentType = x.Series?.DocumentType ?? string.Empty,
                Prefix = x.Series?.Prefix ?? string.Empty,
                NextNumber = x.Series?.NextNumber ?? 1,
                IsDefault = x.Series?.IsDefault ?? false,
                StartDate = x.Series?.StartDate ?? DateTime.MinValue,
                EndDate = x.Series?.EndDate ?? DateTime.MinValue,
                IsLocked = x.Series?.IsLocked ?? false
            };
    }
}