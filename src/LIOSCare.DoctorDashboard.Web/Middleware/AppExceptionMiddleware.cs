using LIOSCare.DoctorDashboard.Application.Common;
using System.Text.Json;

namespace LIOSCare.DoctorDashboard.Web.Middleware;

public sealed class AppExceptionMiddleware(RequestDelegate next, ILogger<AppExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            logger.LogWarning(ex, "Application error {Status}: {Message}", ex.StatusCode, ex.Message);
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message, status = ex.StatusCode }));
                return;
            }
            context.Response.StatusCode = ex.StatusCode;
            context.Items["ErrorMessage"] = ex.Message;
            context.Response.Redirect("/home/error");
        }
        catch (Exception ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            logger.LogError(ex, "Unhandled exception on API path {Path}", context.Request.Path);
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "An unexpected error occurred.", status = 500 }));
        }
    }
}
