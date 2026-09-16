using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AuditLogDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public AuditLogService(
            AppDbContext context,
            Common common,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _common = common;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
            string module,
            string action,
            int? entityId = null,
            string? entityNo = null,
            object? oldValues = null,
            object? newValues = null,
            string? remarks = null)
        {
            try
            {
                int? fyId = null;
                try { fyId = _common.GetFinancialYearId(); } catch { }

                var log = new AuditLog
                {
                    CompanyId = _common.GetCompanyId(),
                    FinancialYearId = fyId,
                    UserId = (int?)_common.GetUserId(),
                    Module = module,
                    Action = action,
                    EntityId = entityId,
                    EntityNo = entityNo,
                    OldValues = oldValues != null
                        ? JsonSerializer.Serialize(oldValues, _jsonOptions) : null,
                    NewValues = newValues != null
                        ? JsonSerializer.Serialize(newValues, _jsonOptions) : null,
                    Remarks = remarks,
                    IpAddress = _httpContextAccessor.HttpContext?
                        .Connection.RemoteIpAddress?.ToString(),
                    CreatedDate = DateTime.UtcNow
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[AuditLog] Failed to write log: {ex.Message}");
            }
        }

        public async Task<PagedResponseDto<AuditLogResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.AuditLogs
                .Where(x => x.CompanyId == companyId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    (x.EntityNo != null && x.EntityNo.ToLower().Contains(search)) ||
                    x.Module.ToLower().Contains(search) ||
                    x.Action.ToLower().Contains(search));
            }

            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("module"))
                {
                    var val = ((JsonElement)request.Filters["module"]).GetString();
                    if (!string.IsNullOrWhiteSpace(val))
                        query = query.Where(x => x.Module == val);
                }
                if (request.Filters.ContainsKey("action"))
                {
                    var val = ((JsonElement)request.Filters["action"]).GetString();
                    if (!string.IsNullOrWhiteSpace(val))
                        query = query.Where(x => x.Action == val);
                }
                if (request.Filters.ContainsKey("entityNo"))
                {
                    var val = ((JsonElement)request.Filters["entityNo"]).GetString();
                    if (!string.IsNullOrWhiteSpace(val))
                        query = query.Where(x => x.EntityNo != null && x.EntityNo.Contains(val));
                }
                if (request.Filters.ContainsKey("financialYearId"))
                {
                    var val = ((JsonElement)request.Filters["financialYearId"]).GetInt32();
                    query = query.Where(x => x.FinancialYearId == val);
                }
                if (request.Filters.ContainsKey("dateFrom"))
                {
                    var val = ((JsonElement)request.Filters["dateFrom"]).GetString();
                    if (DateTime.TryParse(val, out var from))
                        query = query.Where(x => x.CreatedDate >= from);
                }
                if (request.Filters.ContainsKey("dateTo"))
                {
                    var val = ((JsonElement)request.Filters["dateTo"]).GetString();
                    if (DateTime.TryParse(val, out var to))
                        query = query.Where(x => x.CreatedDate <= to.AddDays(1));
                }
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Direction == "desc"
                    ? query.OrderByDescending(x => x.CreatedDate)
                    : query.OrderBy(x => x.CreatedDate);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedDate);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new AuditLogResponseDto
                {
                    AuditLogId = x.AuditLogId,
                    CompanyId = x.CompanyId,
                    FinancialYearId = x.FinancialYearId,
                    UserId = x.UserId,
                    Module = x.Module,
                    Action = x.Action,
                    EntityId = x.EntityId,
                    EntityNo = x.EntityNo,
                    OldValues = x.OldValues,
                    NewValues = x.NewValues,
                    Remarks = x.Remarks,
                    IpAddress = x.IpAddress,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            return new PagedResponseDto<AuditLogResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }
    }
}
