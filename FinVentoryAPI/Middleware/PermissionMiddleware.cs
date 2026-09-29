using FinVentoryAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace FinVentoryAPI.Middleware
{
    /// <summary>
    /// Enforces RoleRights at the API level.
    /// applAdmin bypasses all checks (full access, no RoleRights rows required).
    /// Any other role is checked against RoleRights for the menu screen the
    /// request belongs to (MenuItemId header, with route→ControllerName fallback).
    /// Endpoints that don't map to any MenuItem (lookups, dashboard, enums, ...)
    /// only require authentication (which UseAuthorization already enforced).
    /// </summary>
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionMiddleware> _logger;

        public PermissionMiddleware(RequestDelegate next, ILogger<PermissionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                await _next(context);
                return;
            }

            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            // applAdmin: implicit full rights — never consult RoleRights.
            var roleName = context.User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.Equals(roleName, "applAdmin", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (!int.TryParse(context.User.FindFirst("RoleId")?.Value, out var roleId) || roleId == 0)
            {
                await WriteForbidden(context, "Invalid role.");
                return;
            }

            // 1) MenuItemId header (set by the frontend for the current screen)
            int? menuItemId = null;
            if (context.Request.Headers.TryGetValue("MenuItemId", out var headerValues)
                && int.TryParse(headerValues.FirstOrDefault(), out var headerMenuId))
            {
                menuItemId = headerMenuId;
            }

            // 2) Fallback: /api/{Controller} → MenuItems.ControllerName (case-insensitive)
            if (!menuItemId.HasValue)
            {
                var segments = (context.Request.Path.Value ?? string.Empty)
                    .Split('/', StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length >= 2
                    && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
                {
                    var controller = segments[1].ToLowerInvariant();
                    menuItemId = await db.MenuItems
                        .Where(mi => mi.IsActive
                                    && mi.ControllerName != null
                                    && mi.ControllerName.ToLower() == controller)
                        .Select(mi => (int?)mi.MenuItemId)
                        .FirstOrDefaultAsync();
                }
            }

            // 3) Not a menu-backed screen (dashboard, lookups, enums, reports
            //    without a menu row, ...) → authentication is enough.
            if (!menuItemId.HasValue)
            {
                await _next(context);
                return;
            }

            var right = await db.RoleRights
                .FirstOrDefaultAsync(r => r.RoleId == roleId
                                       && r.MenuItemId == menuItemId.Value);

            var allowed = context.Request.Method.ToUpperInvariant() switch
            {
                "GET" => right?.CanView == true,
                "POST" => right?.CanAdd == true,
                "PUT" or "PATCH" => right?.CanEdit == true,
                "DELETE" => right?.CanDelete == true,
                _ => false
            };

            if (!allowed)
            {
                _logger.LogWarning(
                    "Permission denied: RoleId={RoleId}, MenuItemId={MenuItemId}, {Method} {Path}",
                    roleId, menuItemId, context.Request.Method, context.Request.Path);
                await WriteForbidden(context, "You don't have permission for this action.");
                return;
            }

            await _next(context);
        }

        private static async Task WriteForbidden(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                message
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
