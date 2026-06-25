using FinVentoryAPI.Data;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
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
                var log = new AuditLog
                {
                    CompanyId = _common.GetCompanyId(),
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
                // Audit failure must NEVER break the main operation
                Console.Error.WriteLine($"[AuditLog] Failed to write log: {ex.Message}");
            }
        }
    }
}
