using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskFlow.Web.Filters;

public class ActionLoggingFilter : IAsyncActionFilter
{
    private readonly ILogger<ActionLoggingFilter> _logger;

    public ActionLoggingFilter(
        ILogger<ActionLoggingFilter> logger)
    {
        _logger = logger;
    }


    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        string controllerName =
            context.RouteData.Values["controller"]?
                .ToString() ?? "Unknown";

        string actionName =
            context.RouteData.Values["action"]?
                .ToString() ?? "Unknown";

        string userId =
            context.HttpContext.User
                .FindFirstValue(
                    ClaimTypes.NameIdentifier)
            ?? "Anonymous";

        string traceId =
            context.HttpContext.TraceIdentifier;


        Stopwatch stopwatch =
            Stopwatch.StartNew();


        _logger.LogDebug(
            "Starting {Controller}.{Action} by user {UserId}. TraceId: {TraceId}",
            controllerName,
            actionName,
            userId,
            traceId);


        ActionExecutedContext executedContext =
            await next();


        stopwatch.Stop();


        if (executedContext.Exception == null ||
            executedContext.ExceptionHandled)
        {
            _logger.LogInformation(
                "Completed {Controller}.{Action} by user {UserId} in {ElapsedMilliseconds} ms with status {StatusCode}. TraceId: {TraceId}",
                controllerName,
                actionName,
                userId,
                stopwatch.ElapsedMilliseconds,
                context.HttpContext.Response.StatusCode,
                traceId);
        }
        else
        {
            _logger.LogError(
                executedContext.Exception,
                "Failed {Controller}.{Action} by user {UserId} after {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                controllerName,
                actionName,
                userId,
                stopwatch.ElapsedMilliseconds,
                traceId);
        }
    }
}