using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiTaller.Data;

namespace MiTaller.Attributes
{
    /// <summary>
    /// 403 salvo que el llamante sea un taller o un empleado activo de uno (clientes y admin no).
    /// </summary>
    public class WorkshopCallerAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<DataContext>();
            var caller = await CallerResolver.ResolveAsync(context.HttpContext.User, db);
            if (caller?.WorkshopId == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
