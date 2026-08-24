using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.Models.Auth;
using System.Security.Claims;

namespace MiTaller.Attributes
{
    /// <summary>
    /// Atributo de autorización que verifica los permisos de los empleados (tipo 2).
    /// Si el usuario NO es empleado, permite el acceso.
    /// Si el usuario ES empleado, verifica que tenga los permisos requeridos en WorkshopEmployees.Permissions.
    /// </summary>
    public class AuthorizeEmployeeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] _requiredPermissions;

        /// <summary>
        /// Constructor que acepta los permisos requeridos para el endpoint.
        /// Los permisos pueden ser múltiples, separados por comas.
        /// Ejemplo: [AuthorizeNonEmployee("Administrador")] o [AuthorizeNonEmployee("Administrador", "Registrar vehículos")]
        /// </summary>
        public AuthorizeEmployeeAttribute(params string[] requiredPermissions)
        {
            _requiredPermissions = requiredPermissions ?? Array.Empty<string>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Verificar si el usuario está autenticado
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Obtener el claim de UserType del JWT
            var userTypeClaim = context.HttpContext.User.FindFirst("UserType")?.Value;

            if (string.IsNullOrEmpty(userTypeClaim))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Si el usuario NO es Employee, permitir el acceso
            if (!Enum.TryParse<UserType>(userTypeClaim, out var userType) || userType != UserType.Employee)
            {
                return; // Permitir acceso a Customer y Workshop
            }

            // Si es Employee, verificar permisos
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var employeeId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Obtener el DataContext del servicio
            var dataContext = context.HttpContext.RequestServices.GetRequiredService<DataContext>();

            // Buscar el empleado en WorkshopEmployees
            var workshopEmployee = dataContext.WorkshopEmployees
                .Where(we => we.EmployeeId == employeeId && !we.IsDeleted)
                .FirstOrDefault();

            if (workshopEmployee == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Verificar si el empleado tiene alguno de los permisos requeridos
            var employeePermissions = workshopEmployee.Permissions?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList() ?? new List<string>();

            // Si no hay permisos requeridos especificados, denegar acceso a empleados
            if (_requiredPermissions.Length == 0)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Verificar si el empleado tiene al menos uno de los permisos requeridos
            var hasRequiredPermission = _requiredPermissions.Any(required =>
                employeePermissions.Contains(required, StringComparer.OrdinalIgnoreCase));

            if (!hasRequiredPermission)
            {
                context.Result = new ForbidResult();
                return;
            }

            // El empleado tiene los permisos necesarios, permitir acceso
        }
    }
}

