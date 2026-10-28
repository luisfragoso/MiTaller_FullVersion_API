using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.Models.Auth;
using System.Security.Claims;

namespace MiTaller.Attributes
{
    public enum ResourceKind
    {
        Customer, Vehicle, CustomerFile, Appointment, Quotation, Review, Notification, CustomerTag,
        Inspection, InspectionFile, WorkshopBill, WorkshopIncome, WorkshopNote, WorkshopCustomerNote,
        WorkshopEmployee, WorkshopFile, WorkshopService,
        LegacyCarInspection, LegacyCarFile, LegacyCarHistory, LegacyMotoInspection, LegacyMotoFile, LegacyMotoHistory,
    }

    /// <summary>
    /// 403 salvo que el llamante sea dueño del recurso de la ruta: su cliente, su taller (o un empleado de ese taller),
    /// o cualquier taller cuando AnyWorkshop (lecturas que el flujo de búsqueda de clientes necesita).
    /// Si el recurso no existe deja pasar para que la acción responda su propio 404.
    /// </summary>
    public class OwnsResourceAttribute : ActionFilterAttribute
    {
        private readonly ResourceKind _kind;
        private readonly string _param;

        public bool AllowCustomer { get; set; } = true;
        public bool AllowWorkshop { get; set; } = true;
        public bool AnyWorkshop { get; set; }

        public OwnsResourceAttribute(ResourceKind kind, string param)
        {
            _kind = kind;
            _param = param;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<DataContext>();
            var caller = await CallerResolver.ResolveAsync(context.HttpContext.User, db);
            var raw = context.RouteData.Values.TryGetValue(_param, out var v) ? v?.ToString() : null;

            if (caller == null || raw == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            if (AnyWorkshop && caller.Value.WorkshopId != null)
            {
                await next();
                return;
            }

            var owner = await ResolveOwnerAsync(db, raw);
            if (owner == null)
            {
                await next();
                return;
            }

            var (customerId, workshopId) = owner.Value;
            var allowed =
                (AllowCustomer && caller.Value.CustomerId != null && customerId == caller.Value.CustomerId)
                || (AllowWorkshop && caller.Value.WorkshopId != null && workshopId == caller.Value.WorkshopId);

            if (!allowed)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }

        private async Task<(Guid? CustomerId, Guid? WorkshopId)?> ResolveOwnerAsync(DataContext db, string raw)
        {
            var isInt = int.TryParse(raw, out var i);
            var isGuid = Guid.TryParse(raw, out var g);

            async Task<(Guid?, Guid?)?> First<T>(IQueryable<T> q, Func<T, (Guid?, Guid?)> map) where T : class
            {
                var row = await q.FirstOrDefaultAsync();
                return row == null ? null : map(row);
            }

            switch (_kind)
            {
                case ResourceKind.Customer when isGuid:
                    return await db.Customers.AnyAsync(c => c.Id == g) ? (g, null) : null;
                case ResourceKind.Vehicle when isInt:
                    return await First(db.Vehicles.Where(x => x.Id == i).Select(x => new { x.CustomerId }), x => (x.CustomerId, null));
                case ResourceKind.CustomerFile when isGuid:
                    return await First(db.CustomerFiles.Where(x => x.Id == g).Select(x => new { x.CustomerId }), x => (x.CustomerId, null));
                case ResourceKind.Appointment when isInt:
                    return await First(db.Appointments.Where(x => x.Id == i).Select(x => new { x.CustomerId, x.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                case ResourceKind.Quotation when isInt:
                    return await First(db.Quotations.Where(x => x.Id == i).Select(x => new { x.CustomerId, x.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                case ResourceKind.Review when isInt:
                    return await First(db.Reviews.Where(x => x.Id == i).Select(x => new { x.CustomerId, x.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                case ResourceKind.Notification when isInt:
                    // UserId is the customer's or the workshop's account id
                    return await First(db.Notifications.Where(x => x.Id == i).Select(x => new { x.UserId }), x => (x.UserId, x.UserId));
                case ResourceKind.CustomerTag when isInt:
                    return await First(db.CustomerAssociatedTags.Where(x => x.Id == i).Select(x => new { x.WorkshopId }), x => (null, x.WorkshopId));
                case ResourceKind.Inspection when isInt:
                    return await First(db.Inspections.Where(x => x.Id == i).Select(x => new { x.CustomerId, x.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                case ResourceKind.InspectionFile when isInt:
                    return await First(db.InspectionFiles.Where(x => x.Id == i).Select(x => new { x.Inspection.CustomerId, x.Inspection.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                case ResourceKind.WorkshopBill when isInt:
                    return await First(db.WorkshopBills.Where(x => x.Id == i).Select(x => new { x.WorkshopId }), x => (null, x.WorkshopId));
                case ResourceKind.WorkshopIncome when isInt:
                    return await First(db.WorkshopIncomes.Where(x => x.Id == i).Select(x => new { x.WorkshopId }), x => (null, x.WorkshopId));
                case ResourceKind.WorkshopNote when isInt:
                    return await First(db.WorkshopNotes.Where(x => x.Id == i).Select(x => new { x.WorkshopId }), x => (null, x.WorkshopId));
                case ResourceKind.WorkshopCustomerNote when isInt:
                    return await First(db.WorkshopCustomerNotes.Where(x => x.Id == i).Select(x => new { x.WorkshopId }), x => (null, x.WorkshopId));
                case ResourceKind.WorkshopEmployee when isGuid:
                    return await First(db.WorkshopEmployees.Where(x => x.Id == g).Select(x => new { x.WorkshopId }), x => (null, x.WorkshopId));
                case ResourceKind.WorkshopFile when isGuid:
                    return await First(db.WorkshopFiles.Where(x => x.Id == g).Select(x => new { x.WorkshopId }), x => (null, x.WorkshopId));
                case ResourceKind.WorkshopService when isInt:
                    return await First(db.WorkshopServices.Where(x => x.Id == i).Select(x => new { x.WorkshopId }), x => (null, x.WorkshopId));
                case ResourceKind.LegacyCarInspection when isInt:
                    return await First(db.WorkshopVehicleInspections.Where(x => x.Id == i).Select(x => new { x.CustomerId, x.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                case ResourceKind.LegacyCarFile when isInt:
                    return await First(db.WorkshopVehicleFiles.Where(x => x.Id == i)
                        .Join(db.WorkshopVehicleInspections, f => f.WorkshopVehicleInspectionId, w => w.Id, (f, w) => new { w.CustomerId, w.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                case ResourceKind.LegacyCarHistory when isInt:
                    return await First(db.VehicleInspectionHistory.Where(x => x.Id == i)
                        .Join(db.WorkshopVehicleInspections, h => h.VehicleInspectionId, w => w.Id, (h, w) => new { w.CustomerId, w.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                case ResourceKind.LegacyMotoInspection when isInt:
                    return await First(db.WorkshopMotocycleInspections.Where(x => x.Id == i).Select(x => new { x.CustomerId, x.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                case ResourceKind.LegacyMotoFile when isInt:
                    return await First(db.MotocycleInspectionFiles.Where(x => x.Id == i)
                        .Join(db.WorkshopMotocycleInspections, f => f.WorkshopMotocycleInspectionId, w => w.Id, (f, w) => new { w.CustomerId, w.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                case ResourceKind.LegacyMotoHistory when isInt:
                    return await First(db.MotocycleInspectionHistory.Where(x => x.Id == i)
                        .Join(db.WorkshopMotocycleInspections, h => h.MotocycleInspectionId, w => w.Id, (h, w) => new { w.CustomerId, w.WorkshopId }), x => (x.CustomerId, x.WorkshopId));
                default:
                    return null;
            }
        }
    }

    public static class CallerResolver
    {
        /// <summary>(CustomerId, WorkshopId) del token; un empleado resuelve al taller donde trabaja.</summary>
        public static async Task<(Guid? CustomerId, Guid? WorkshopId)?> ResolveAsync(ClaimsPrincipal user, DataContext db)
        {
            if (!Enum.TryParse<UserType>(user.FindFirst("UserType")?.Value, out var type)
                || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                return null;

            return type switch
            {
                UserType.Customer => (id, null),
                UserType.Workshop => (null, id),
                UserType.Employee => await db.WorkshopEmployees
                    .Where(we => we.EmployeeId == id && !we.IsDeleted)
                    .Select(we => (Guid?)we.WorkshopId)
                    .FirstOrDefaultAsync() is Guid w ? (null, w) : null,
                _ => null,
            };
        }

        public static async Task<bool> IsOwnAccountAsync(ClaimsPrincipal user, DataContext db, Guid accountId)
        {
            var caller = await ResolveAsync(user, db);
            return caller != null && (caller.Value.CustomerId == accountId || caller.Value.WorkshopId == accountId);
        }
    }
}
