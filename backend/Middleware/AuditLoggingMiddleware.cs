using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using VinhKhanhNarration.Api.BUS;

namespace VinhKhanhNarration.Api.Middleware;

public class AuditLoggingMiddleware
{
    private static readonly HashSet<string> MutatingMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH", "DELETE" };

    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(
        RequestDelegate next,
        ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        await _next(context);
        if (!MutatingMethods.Contains(context.Request.Method)) return;
        if (context.Request.Path.StartsWithSegments("/swagger")) return;

        try
        {
            var (actorType, actorId) = ResolveActor(context.User);
            var guestSessionId = ResolveGuestSession(context);
            if (actorType == "System" && !string.IsNullOrWhiteSpace(guestSessionId))
                actorType = "Guest";

            var route = context.GetRouteData().Values;
            var entityId = route.Values
                .Select(value => value?.ToString())
                .Select(value => long.TryParse(value, out var id) ? id : (long?)null)
                .FirstOrDefault(value => value.HasValue);
            var controller = route.TryGetValue("controller", out var controllerName)
                ? controllerName?.ToString()
                : null;
            var action = route.TryGetValue("action", out var actionName)
                ? actionName?.ToString()
                : null;

            using var scope = scopeFactory.CreateScope();
            var audit = scope.ServiceProvider.GetRequiredService<AuditLogBUS>();
            audit.Write(
                actorType,
                actorId,
                guestSessionId,
                $"{context.Request.Method} {context.Request.Path}",
                controller,
                entityId,
                new
                {
                    ControllerAction = action,
                    StatusCode = context.Response.StatusCode,
                    TraceId = context.TraceIdentifier
                },
                context.Connection.RemoteIpAddress?.ToString(),
                context.Request.Headers["User-Agent"].ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not write audit log for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
    }

    private static (string ActorType, long? ActorId) ResolveActor(ClaimsPrincipal user)
    {
        if (long.TryParse(user.FindFirstValue("adminId"), out var adminId))
            return ("Admin", adminId);
        if (long.TryParse(user.FindFirstValue("vendorUserId"), out var vendorId))
            return ("Vendor", vendorId);
        return ("System", null);
    }

    private static string? ResolveGuestSession(HttpContext context)
    {
        var route = context.GetRouteData().Values;
        if (route.TryGetValue("guestSessionId", out var routeValue))
            return routeValue?.ToString();
        if (context.Request.Query.TryGetValue("guestSessionId", out var queryValue))
            return queryValue.FirstOrDefault();
        if (context.Request.Headers.TryGetValue("X-Guest-Session-Id", out var headerValue))
            return headerValue.FirstOrDefault();
        return null;
    }
}
