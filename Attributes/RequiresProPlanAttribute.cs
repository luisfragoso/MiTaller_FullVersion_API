using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiTaller.Data;
using MiTaller.Services;

namespace MiTaller.Attributes
{
    /// <summary>
    /// 403 "plan-requires-pro" when the caller's workshop is on the Gratis plan (trial over, nothing paid).
    /// Runs after the auth filters, so the caller is already known to own the workshop.
    /// </summary>
    public class RequiresProPlanAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var services = context.HttpContext.RequestServices;
            var caller = await CallerResolver.ResolveAsync(context.HttpContext.User, services.GetRequiredService<DataContext>());
            var workshopId = caller?.WorkshopId;

            if (workshopId != null && !await services.GetRequiredService<SubscriptionService>().IsProAsync(workshopId.Value))
            {
                context.Result = new ObjectResult(SubscriptionService.RequiresProError) { StatusCode = StatusCodes.Status403Forbidden };
                return;
            }

            await next();
        }
    }
}
