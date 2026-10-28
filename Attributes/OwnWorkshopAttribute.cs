using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.Models.Auth;
using System.Security.Claims;

namespace MiTaller.Attributes
{
    /// <summary>
    /// 403 salvo que el {workshopId} de la ruta sea el taller del llamante (el propio taller o el de su empleado).
    /// </summary>
    public class OwnWorkshopAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (!context.RouteData.Values.TryGetValue("workshopId", out var raw)
                || !Guid.TryParse(raw?.ToString(), out var workshopId)
                || !Enum.TryParse<UserType>(user.FindFirst("UserType")?.Value, out var type)
                || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId))
            {
                context.Result = new ForbidResult();
                return;
            }

            var allowed = type switch
            {
                UserType.Workshop => callerId == workshopId,
                UserType.Employee => await context.HttpContext.RequestServices.GetRequiredService<DataContext>()
                    .WorkshopEmployees
                    .AnyAsync(we => we.EmployeeId == callerId && we.WorkshopId == workshopId && !we.IsDeleted),
                _ => false,
            };

            if (!allowed)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
