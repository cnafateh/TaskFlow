using System.Diagnostics;
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

        string userName =
            context.HttpContext.User.Identity?.Name
            ?? "Anonymous";


        Stopwatch stopwatch =
            Stopwatch.StartNew();


        _logger.LogInformation(
            "Starting {Controller}.{Action} by {User}",
            controllerName,
            actionName,
            userName);


        ActionExecutedContext executedContext =
            await next();


        stopwatch.Stop();


        if (executedContext.Exception == null ||
            executedContext.ExceptionHandled)
        {
            _logger.LogInformation(
                "Completed {Controller}.{Action} by {User} in {ElapsedMilliseconds} ms",
                controllerName,
                actionName,
                userName,
                stopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogError(
                executedContext.Exception,
                "Failed {Controller}.{Action} by {User} after {ElapsedMilliseconds} ms",
                controllerName,
                actionName,
                userName,
                stopwatch.ElapsedMilliseconds);
        }
    }
}