using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiTaller.Data;

namespace MiTaller.Attributes
{
    /// <summary>
    /// 403 salvo que alguna de las propiedades indicadas ("WorkshopId" / "CustomerId") de los argumentos de la acción
    /// (body, query o parámetro simple) sea la identidad del llamante; si ninguna viene, también 403.
    /// </summary>
    public class CallerMatchesAttribute : ActionFilterAttribute
    {
        private readonly string[] _props;

        public CallerMatchesAttribute(params string[] props)
        {
            _props = props;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<DataContext>();
            var caller = await CallerResolver.ResolveAsync(context.HttpContext.User, db);

            var allowed = caller != null && _props.Any(prop =>
            {
                var expected = prop.Equals("WorkshopId", StringComparison.OrdinalIgnoreCase)
                    ? caller.Value.WorkshopId
                    : caller.Value.CustomerId;
                return expected != null && Values(context, prop).Any(v => v == expected);
            });

            if (!allowed)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }

        private static IEnumerable<Guid> Values(ActionExecutingContext context, string prop)
        {
            foreach (var (name, arg) in context.ActionArguments)
            {
                if (arg is Guid g && name.Equals(prop, StringComparison.OrdinalIgnoreCase))
                {
                    yield return g;
                    continue;
                }

                var value = arg?.GetType().GetProperty(prop)?.GetValue(arg);
                if (value is Guid id) yield return id;
            }
        }
    }
}
