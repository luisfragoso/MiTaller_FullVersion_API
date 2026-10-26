using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MiTaller.Attributes
{
    /// <summary>
    /// Bloquea (410 Gone) un endpoint legacy de Auto/Moto que escribe: esas inspecciones
    /// ya viven en el motor genérico (api/inspections) y una escritura aquí se perdería.
    /// </summary>
    public class LegacyWriteRetiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.Result = new ObjectResult("legacy-endpoint-retired") { StatusCode = StatusCodes.Status410Gone };
        }
    }
}
