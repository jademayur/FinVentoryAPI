using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SeriesDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class DocumentSeriesService : IDocumentSeriesService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DocumentSeriesService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCompanyId()
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst("CompanyId")?.Value;

            if (claim is null || !int.TryParse(claim, out var companyId))
                throw new UnauthorizedAccessException("CompanyId claim is missing or invalid.");

            return companyId;
        }

        public async Task<IEnumerable<SeriesResponseDto>> GetAllAsync()
        {
            var companyId = GetCompanyId();

            return await _context.DocumentSeries
                .Where(s => s.CompanyId == companyId)
                .Select(s => MapToResponseDto(s))
                .ToListAsync();
        }

        public async Task<SeriesResponseDto?> GetByIdAsync(int seriesId)
        {
            var companyId = GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.CompanyId == companyId);

            return series is null ? null : MapToResponseDto(series);
        }

        public async Task<SeriesResponseDto> CreateAsync(CreateSeriesDto dto)
        {
            var companyId = GetCompanyId();

            if (dto.IsDefault)
                await ClearDefaultAsync(companyId, dto.DocumentType);

            var series = new DocumentSeries
            {
                CompanyId = companyId,
                DocumentType = dto.DocumentType,
                SeriesName = dto.SeriesName,
                Prefix = dto.Prefix,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsDefault = dto.IsDefault,
                IsManual = dto.IsManual,
                CreatedAt = DateTime.UtcNow
            };

            _context.DocumentSeries.Add(series);
            await _context.SaveChangesAsync();

            return MapToResponseDto(series);
        }

        public async Task<SeriesResponseDto?> UpdateAsync(int seriesId, UpdateSeriesDto dto)
        {
            var companyId = GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.CompanyId == companyId);

            if (series is null) return null;

            if (series.IsLocked)
                throw new InvalidOperationException("Cannot update a locked series.");

            if (dto.IsDefault && !series.IsDefault)
                await ClearDefaultAsync(companyId, dto.DocumentType);

            series.DocumentType = dto.DocumentType;
            series.SeriesName = dto.SeriesName;
            series.Prefix = dto.Prefix;
            series.StartDate = dto.StartDate;
            series.EndDate = dto.EndDate;
            series.IsDefault = dto.IsDefault;
            series.IsManual = dto.IsManual;
            series.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return MapToResponseDto(series);
        }

        public async Task<bool> DeleteAsync(int seriesId)
        {
            var companyId = GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.CompanyId == companyId);

            if (series is null) return false;

            if (series.IsLocked)
                throw new InvalidOperationException("Cannot delete a locked series.");

            _context.DocumentSeries.Remove(series);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SeriesResponseDto?> GetDefaultSeriesAsync(string documentType)
        {
            var companyId = GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s =>
                    s.CompanyId == companyId &&
                    s.DocumentType == documentType &&
                    s.IsDefault &&
                    s.IsActive);

            return series is null ? null : MapToResponseDto(series);
        }

        public async Task<bool> SetAsDefaultAsync(int seriesId)
        {
            var companyId = GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.CompanyId == companyId);

            if (series is null) return false;

            await ClearDefaultAsync(companyId, series.DocumentType);

            series.IsDefault = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateNextNumberAsync(int seriesId)
        {
            var companyId = GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.CompanyId == companyId);

            if (series is null)
                throw new KeyNotFoundException($"Series {seriesId} not found.");

            if (!series.IsActive)
                throw new InvalidOperationException("Cannot generate number from an inactive series.");

            if (series.IsLocked)
                throw new InvalidOperationException("Series is locked.");

            var docNumber = $"{series.Prefix}{series.NextNumber:D5}";

            series.NextNumber++;
            await _context.SaveChangesAsync();

            return docNumber;
        }

        public async Task<PagedResponseDto<SeriesResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = GetCompanyId();

            var query = _context.DocumentSeries
                .Where(x => x.CompanyId == companyId)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.SeriesName!.ToLower().Contains(search) ||
                    x.Prefix.ToLower().Contains(search) ||
                    x.DocumentType.ToLower().Contains(search));
            }

            // FILTERS
            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("isActive"))
                {
                    var isActive = ((JsonElement)request.Filters["isActive"]).GetBoolean();
                    query = query.Where(x => x.IsActive == isActive);
                }

                if (request.Filters.ContainsKey("documentType"))
                {
                    var docType = ((JsonElement)request.Filters["documentType"]).GetString();
                    if (!string.IsNullOrWhiteSpace(docType))
                        query = query.Where(x => x.DocumentType == docType);
                }
            }

            // SORTING
            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();

                switch (sort.Column.ToLower())
                {
                    case "seriesname":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.SeriesName)
                            : query.OrderBy(x => x.SeriesName);
                        break;

                    case "documenttype":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.DocumentType)
                            : query.OrderBy(x => x.DocumentType);
                        break;

                    case "prefix":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.Prefix)
                            : query.OrderBy(x => x.Prefix);
                        break;

                    case "startdate":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.StartDate)
                            : query.OrderBy(x => x.StartDate);
                        break;

                    case "nextnumber":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.NextNumber)
                            : query.OrderBy(x => x.NextNumber);
                        break;

                    case "isactive":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.IsActive)
                            : query.OrderBy(x => x.IsActive);
                        break;

                    default:
                        query = query.OrderBy(x => x.SeriesName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.SeriesName);
            }

            // TOTAL RECORD COUNT
            var totalRecords = await query.CountAsync();

            // PAGINATION + DATA
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new SeriesResponseDto
                {
                    SeriesId = x.SeriesId,
                    SeriesName = x.SeriesName ?? string.Empty,
                    Prefix = x.Prefix,
                    DocumentType = x.DocumentType,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    NextNumber = x.NextNumber,
                    IsDefault = x.IsDefault,
                    IsLocked = x.IsLocked,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return new PagedResponseDto<SeriesResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private async Task ClearDefaultAsync(int companyId, string documentType)
        {
            var existing = await _context.DocumentSeries
                .Where(s => s.CompanyId == companyId &&
                            s.DocumentType == documentType &&
                            s.IsDefault)
                .ToListAsync();

            foreach (var s in existing)
                s.IsDefault = false;
        }

        private static SeriesResponseDto MapToResponseDto(DocumentSeries s) => new()
        {
            SeriesId = s.SeriesId,
            SeriesName = s.SeriesName ?? string.Empty,
            Prefix = s.Prefix,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            NextNumber = s.NextNumber,
            IsDefault = s.IsDefault,
            IsLocked = s.IsLocked
        };
    }
}
