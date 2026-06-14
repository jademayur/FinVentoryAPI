namespace FinVentoryAPI.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(
            string module,
            string action,
            int? entityId = null,
            string? entityNo = null,
            object? oldValues = null,
            object? newValues = null,
            string? remarks = null);
    }
}
