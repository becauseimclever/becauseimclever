namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Application.Commissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>Handles unavailable storage and known, content-free commission failures.</summary>
public sealed class CommissionApiFilter : Attribute, IAsyncResourceFilter, IExceptionFilter
{
    /// <inheritdoc />
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.HttpContext.RequestServices.GetService<ICommissionService>() is null)
        {
            context.Result = new ObjectResult(new ProblemDetails { Status = 503, Title = "Commission storage is unavailable." }) { StatusCode = 503 };
            return;
        }

        await next();
    }

    /// <inheritdoc />
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not CommissionException exception)
        {
            return;
        }

        var (status, title) = exception.Code switch
        {
            "not_found" => (404, "Commission not found."),
            "conflict" => (409, "Commission has changed or conflicts with another entry."),
            "denied" or "forbidden" => (403, "Commission operation is forbidden."),
            _ => (400, "Invalid commission request."),
        };
        context.HttpContext.RequestServices.GetRequiredService<ILogger<CommissionApiFilter>>()
            .LogInformation("Commission operation failed with status {Status}", status);
        context.Result = new ObjectResult(new ProblemDetails { Status = status, Title = title }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
