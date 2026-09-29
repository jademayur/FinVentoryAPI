using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SeriesDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class DocumentSeriesService : IDocumentSeriesService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public DocumentSeriesService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<IEnumerable<SeriesResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            return await _context.DocumentSeries
                .Where(s => s.CompanyId == companyId && !s.IsDeleted)
                .Select(s => MapToResponseDto(s))
                .ToListAsync();
        }

        public async Task<SeriesResponseDto?> GetByIdAsync(int seriesId)
        {
            var companyId = _common.GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.CompanyId == companyId && !s.IsDeleted);

            return series is null ? null : MapToResponseDto(series);
        }

        public async Task<SeriesResponseDto> CreateAsync(CreateSeriesDto dto)
        {
            var companyId = dto.CompanyId ?? _common.GetCompanyId();

            if (dto.IsDefault)
                await ClearDefaultAsync(companyId, dto.DocumentType);

            var series = new DocumentSeries
            {
                CompanyId = companyId,
                FinancialYearId = dto.FinancialYearId,
                ModuleId = dto.ModuleId,
                DocumentType = dto.DocumentType,
                SeriesCode = dto.SeriesCode,
                SeriesName = dto.SeriesName,
                Prefix = dto.Prefix,
                Suffix = dto.Suffix,
                Format = dto.Format,
                DocumentLength = dto.DocumentLength,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                StartFromNumber = dto.StartFromNumber,
                NextNumber = dto.StartFromNumber,
                IsDefault = dto.IsDefault,
                IsManual = dto.IsManual,
                CreatedBy = _common.GetUserId()
            };

            _context.DocumentSeries.Add(series);
            await _context.SaveChangesAsync();

            return MapToResponseDto(series);
        }

        public async Task<SeriesResponseDto?> UpdateAsync(int seriesId, UpdateSeriesDto dto)
        {
            var companyId = _common.GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.CompanyId == companyId && !s.IsDeleted);

            if (series is null) return null;

            if (series.IsLocked)
                throw new InvalidOperationException("Cannot update a locked series.");

            if (dto.IsDefault && !series.IsDefault)
                await ClearDefaultAsync(companyId, dto.DocumentType);

            series.FinancialYearId = dto.FinancialYearId;
            series.ModuleId = dto.ModuleId;
            series.DocumentType = dto.DocumentType;
            series.SeriesCode = dto.SeriesCode;
            series.SeriesName = dto.SeriesName;
            series.Prefix = dto.Prefix;
            series.Suffix = dto.Suffix;
            series.Format = dto.Format;
            series.DocumentLength = dto.DocumentLength;
            series.StartFromNumber = dto.StartFromNumber;
            series.IsDefault = dto.IsDefault;
            series.IsManual = dto.IsManual;
            series.IsActive = dto.IsActive;
            series.ModifiedBy = _common.GetUserId();
            series.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponseDto(series);
        }

        public async Task<bool> DeleteAsync(int seriesId)
        {
            var companyId = _common.GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.CompanyId == companyId);

            if (series is null) return false;

            if (series.IsLocked)
                throw new InvalidOperationException("Cannot delete a locked series.");

            series.IsDeleted = true;
            series.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SeriesResponseDto?> GetDefaultSeriesAsync(string documentType)
        {
            var companyId = _common.GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s =>
                    s.CompanyId == companyId &&
                    s.DocumentType == documentType &&
                    s.IsDefault &&
                    s.IsActive &&
                    !s.IsDeleted);

            return series is null ? null : MapToResponseDto(series);
        }

        public async Task<bool> SetAsDefaultAsync(int seriesId)
        {
            var companyId = _common.GetCompanyId();

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
            var companyId = _common.GetCompanyId();

            var series = await _context.DocumentSeries
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.CompanyId == companyId);

            if (series is null)
                throw new KeyNotFoundException($"Series {seriesId} not found.");

            if (!series.IsActive)
                throw new InvalidOperationException("Cannot generate number from an inactive series.");

            if (series.IsLocked)
                throw new InvalidOperationException("Series is locked.");

            var docNumber = FormatDocumentNumber(series);

            series.NextNumber++;
            await _context.SaveChangesAsync();

            return docNumber;
        }

        public async Task<PagedResponseDto<SeriesResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.DocumentSeries
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.SeriesName!.ToLower().Contains(search) ||
                    x.SeriesCode!.ToLower().Contains(search) ||
                    x.Prefix.ToLower().Contains(search) ||
                    x.DocumentType.ToLower().Contains(search));
            }

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

                if (request.Filters.ContainsKey("financialYearId"))
                {
                    var fyId = ((JsonElement)request.Filters["financialYearId"]).GetInt32();
                    query = query.Where(x => x.FinancialYearId == fyId);
                }

                if (request.Filters.ContainsKey("moduleId"))
                {
                    var modId = ((JsonElement)request.Filters["moduleId"]).GetInt32();
                    query = query.Where(x => x.ModuleId == modId);
                }
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "seriesname" => sort.Direction == "desc" ? query.OrderByDescending(x => x.SeriesName) : query.OrderBy(x => x.SeriesName),
                    "documenttype" => sort.Direction == "desc" ? query.OrderByDescending(x => x.DocumentType) : query.OrderBy(x => x.DocumentType),
                    "seriescode" => sort.Direction == "desc" ? query.OrderByDescending(x => x.SeriesCode) : query.OrderBy(x => x.SeriesCode),
                    "prefix" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Prefix) : query.OrderBy(x => x.Prefix),
                    "nextnumber" => sort.Direction == "desc" ? query.OrderByDescending(x => x.NextNumber) : query.OrderBy(x => x.NextNumber),
                    _ => query.OrderBy(x => x.SeriesName)
                };
            }
            else
            {
                query = query.OrderBy(x => x.DocumentType).ThenBy(x => x.SeriesName);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => MapToResponseDto(x))
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

        private static string FormatDocumentNumber(DocumentSeries s)
        {
            var numStr = s.NextNumber.ToString().PadLeft(s.DocumentLength, '0');
            var prefix = s.Prefix ?? "";
            var suffix = s.Suffix ?? "";

            if (!string.IsNullOrEmpty(s.Format))
            {
                var fy = s.FinancialYearId?.ToString() ?? "";
                var result = s.Format
                    .Replace("{PREFIX}", prefix)
                    .Replace("{SUFFIX}", suffix)
                    .Replace("{NUMBER}", numStr)
                    .Replace("{FY}", fy);
                return result;
            }

            return $"{prefix}{numStr}{suffix}";
        }

        private static SeriesResponseDto MapToResponseDto(DocumentSeries s) => new()
        {
            SeriesId = s.SeriesId,
            CompanyId = s.CompanyId,
            FinancialYearId = s.FinancialYearId,
            ModuleId = s.ModuleId,
            DocumentType = s.DocumentType,
            SeriesCode = s.SeriesCode,
            SeriesName = s.SeriesName ?? string.Empty,
            Prefix = s.Prefix,
            Suffix = s.Suffix,
            Format = s.Format,
            DocumentLength = s.DocumentLength,
            StartFromNumber = s.StartFromNumber,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            NextNumber = s.NextNumber,
            IsDefault = s.IsDefault,
            IsManual = s.IsManual,
            IsActive = s.IsActive,
            IsLocked = s.IsLocked,
        };
    }
}
