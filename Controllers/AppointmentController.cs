using Microsoft.AspNetCore.Mvc;
using MiTaller.Data;
using Microsoft.EntityFrameworkCore;
using MiTaller.DTO;
using MiTaller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiTaller.DTO.Appointment;
using Microsoft.AspNetCore.Authorization;
using MiTaller.Attributes;
using MiTaller.DTO.Vehicle;
using Microsoft.Identity.Client;
using MiTaller.Models.Workshop;
using MiTaller.Models.Auth;
using MiTaller.Models.Notification;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly FirebaseNotificationService _firebaseNotificationService;

        public AppointmentController(DataContext context, FirebaseNotificationService firebaseNotificationService)
        {
            _context = context;
            _firebaseNotificationService = firebaseNotificationService;
        }

        [HttpGet("{appointmentId}")]
        public async Task<ActionResult<AppointmentResponseDto>> GetAppointmentById(int appointmentId)
        {
            try 
            { 
                var appointment = await _context.Appointments
                    .Where(a => a.Id == appointmentId)
                    .Include(a => a.Customer)
                    .Include(a => a.Vehicle)
                    .Include(a => a.Workshop)
                    .Select(a => new AppointmentResponseDto
                    {
                        Id = a.Id,
                        CustomerId = a.CustomerId,
                        CustomerName = a.Customer.FullName,
                        WorkshopId = a.WorkshopId,
                        WorkshopName = a.Workshop.WorkshopName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = a.Vehicle.Id,
                            Brand = a.Vehicle.BrandId == -1 ? a.Vehicle.OtherBrand : a.Vehicle.Brand.Name,
                            Model = a.Vehicle.VehicleModelId == -1 ? a.Vehicle.OtherVehicleModel : a.Vehicle.VehicleModel.Model,
                            Version = a.Vehicle.VehicleVersionId == -1 ? a.Vehicle.OtherVehicleVersion : a.Vehicle.VehicleVersion.Version,
                            Type = a.Vehicle.VehicleTypeId == -1 ? a.Vehicle.OtherVehicleType : a.Vehicle.VehicleType.Type,
                            Year = a.Vehicle.Year,
                            SerialNumber = a.Vehicle.SerialNumber,
                            Color = a.Vehicle.Color,
                            Plates = a.Vehicle.Plates,
                            RimRubber = a.Vehicle.RimRubber,
                            Kms = a.Vehicle.Kms,
                            VehicleFormat = a.Vehicle.VehicleFormat,
                        },
                        Date = a.Date,
                        Title = a.Title,
                        Description = a.Description,
                        AppointmentType = a.AppointmentType,
                        NotificationType = a.NotificationType,
                        Status = a.Status,
                    })
                    .FirstOrDefaultAsync();

                if (appointment == null) return NotFound("not-found");

                return Ok(appointment);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<AppointmentResponseDto>> GetAppointmentsByCustomer(Guid customerId)
        {
            try 
            {
                var appointments = await _context.Appointments
                    .Where(a => a.CustomerId == customerId)
                    .Include(a => a.Customer)
                    .Include(a => a.Vehicle)
                    .Include(a => a.Workshop)
                    .Select(a => new AppointmentResponseDto
                    {
                        Id = a.Id,
                        CustomerId = a.CustomerId,
                        CustomerName = a.Customer.FullName,
                        WorkshopId = a.WorkshopId,
                        WorkshopName = a.Workshop.WorkshopName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = a.Vehicle.Id,
                            Brand = a.Vehicle.BrandId == -1 ? a.Vehicle.OtherBrand : a.Vehicle.Brand.Name,
                            Model = a.Vehicle.VehicleModelId == -1 ? a.Vehicle.OtherVehicleModel : a.Vehicle.VehicleModel.Model,
                            Version = a.Vehicle.VehicleVersionId == -1 ? a.Vehicle.OtherVehicleVersion : a.Vehicle.VehicleVersion.Version,
                            Type = a.Vehicle.VehicleTypeId == -1 ? a.Vehicle.OtherVehicleType : a.Vehicle.VehicleType.Type,
                            Year = a.Vehicle.Year,
                            SerialNumber = a.Vehicle.SerialNumber,
                            Color = a.Vehicle.Color,
                            Plates = a.Vehicle.Plates,
                            RimRubber = a.Vehicle.RimRubber,
                            Kms = a.Vehicle.Kms,
                            VehicleFormat = a.Vehicle.VehicleFormat,
                        },
                        Date = a.Date,
                        Title = a.Title,
                        Description = a.Description,
                        AppointmentType = a.AppointmentType,
                        NotificationType = a.NotificationType,
                        Status = a.Status,
                        Image = a.Image,
                    })
                    .ToListAsync();

                if (appointments == null)
                {
                    return NotFound("not-found");
                }

                return Ok(appointments);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("workshop/{workshopId}")]
        public async Task<ActionResult<List<AppointmentResponseDto>>> GetAppointmentsByWorkshop(Guid workshopId)
        {
            try 
            { 
                var appointments = await _context.Appointments
                    .Where(a => a.WorkshopId == workshopId)
                    .Include(a => a.Customer)
                    .Include(a => a.Vehicle)
                    .Include(a => a.Workshop)
                    .Select(a => new AppointmentResponseDto
                    {
                        Id = a.Id,
                        CustomerId = a.CustomerId,
                        CustomerName = a.Customer.FullName,
                        WorkshopId = a.WorkshopId,
                        WorkshopName = a.Workshop.WorkshopName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = a.Vehicle.Id,
                            Brand = a.Vehicle.BrandId == -1 ? a.Vehicle.OtherBrand : a.Vehicle.Brand.Name,
                            Model = a.Vehicle.VehicleModelId == -1 ? a.Vehicle.OtherVehicleModel : a.Vehicle.VehicleModel.Model,
                            Version = a.Vehicle.VehicleVersionId == -1 ? a.Vehicle.OtherVehicleVersion : a.Vehicle.VehicleVersion.Version,
                            Type = a.Vehicle.VehicleTypeId == -1 ? a.Vehicle.OtherVehicleType : a.Vehicle.VehicleType.Type,
                            Year = a.Vehicle.Year,
                            SerialNumber = a.Vehicle.SerialNumber,
                            Color = a.Vehicle.Color,
                            Plates = a.Vehicle.Plates,
                            RimRubber = a.Vehicle.RimRubber,
                            Kms = a.Vehicle.Kms,
                            VehicleFormat = a.Vehicle.VehicleFormat,
                            Image = a.Vehicle.Image,
                        },
                        Date = a.Date,
                        Title = a.Title,
                        Description = a.Description,
                        AppointmentType = a.AppointmentType,
                        NotificationType = a.NotificationType,
                        Status = a.Status,
                    })
                    .ToListAsync();

                if (appointments == null)
                {
                    return NotFound("not-found");
                }

                return Ok(appointments);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> CreateAppointment([FromForm] PostAppointmentDto model)
        {
            try 
            { 
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == model.CustomerId && !c.IsDeleted);
                var workshop = await _context.Workshops.FirstOrDefaultAsync(w => w.Id == model.WorkshopId && !w.IsDeleted);
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == model.VehicleId && v.CustomerId == model.CustomerId && !v.IsDeleted);

                if (customer == null) return NotFound("not-found");
                if (workshop == null) return NotFound("not-found");
                if (vehicle == null) return NotFound("not-found");

                //var activeInspection = await _context.WorkshopVehicleInspections
                //    .Where(a => a.WorkshopId == model.WorkshopId
                //            && a.CustomerId == model.CustomerId
                //            && a.VehicleId == model.VehicleId
                //            && a.IsActive)
                //    .FirstOrDefaultAsync();

                byte[]? imageData = null;
                if (model.Image != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        await model.Image.CopyToAsync(ms);
                        imageData = ms.ToArray();
                    }
                }

                var appointment = new Appointment
                {
                    CustomerId = model.CustomerId,
                    WorkshopId = model.WorkshopId,
                    VehicleId = model.VehicleId,
                    Date = model.Date,
                    Title = model.Title,
                    Description = model.Description,
                    AppointmentType = model.AppointmentType,
                    NotificationType = model.NotificationType,
                    Image = imageData,
                    Status = "Pendiente",
                    //Status = model.Status
                };

                var formattedDate = appointment.Date.ToString("dd/MM/yy");
                var formattedTime = appointment.Date.ToString("HH:mm");

                if (model.UserType == UserType.Customer)
                {
                    // WorkshopInbox
                    var workshopInbox = new WorkshopInbox
                    {
                        WorkshopId = model.WorkshopId,
                        CustomerId = model.CustomerId,
                        VehicleId = model.VehicleId,
                        ParentModelType = "Appointment",
                        ParentModelId = appointment.Id,
                        Title = $"{customer.FullName} ha solicitado una cita para la fecha del {formattedDate} a las {formattedTime}",
                        Details = appointment.Description,
                    };
                    _context.WorkshopInbox.Add(workshopInbox);
                    await _context.SaveChangesAsync();

                    // TODO: Activar cuando se implementen las notificaciones push Firebase
                    // Enviar notificación push al taller
                    //await _firebaseNotificationService.SendNotificationToWorkshopAsync(
                    //    model.WorkshopId,
                    //    workshopInbox.Title,
                    //    workshopInbox.Details,
                    //    workshopInbox.ParentModelType,
                    //    new Dictionary<string, string> { { "appointmentId", appointment.Id.ToString() } }
                    //);
                }
                else
                {
                    // Customer Notifications
                    var notification = new Notifications
                    {
                        UserId = model.CustomerId,
                        UserType = UserType.Customer,
                        Title = $"{workshop.WorkshopName} te ha agendado una cita para la fecha del {formattedDate} a las {formattedTime}",
                        Content = appointment.Description,
                        Event = "AppointmentCreated"
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    // TODO: Activar cuando se implementen las notificaciones push Firebase
                    // Enviar notificación push al cliente
                    //await _firebaseNotificationService.SendNotificationToCustomerAsync(
                    //    model.CustomerId,
                    //    notification.Title,
                    //    notification.Content,
                    //    notification.Event,
                    //    new Dictionary<string, string> { { "appointmentId", appointment.Id.ToString() } }
                    //);
                }

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return Ok("appointment-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("{appointmentId}")]
        public async Task<ActionResult> UpdateAppointment(int appointmentId, [FromBody] PostAppointmentDto model)
        {
            try 
            { 
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null) return NotFound("not-found");

                var customerExists = await _context.Customers.AnyAsync(c => c.Id == model.CustomerId);
                var workshopExists = await _context.Workshops.AnyAsync(w => w.Id == model.WorkshopId);
                var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == model.VehicleId && v.CustomerId == model.CustomerId);

                if (!customerExists) return NotFound("not-found");
                if (!workshopExists) return NotFound("not-found");
                if (!vehicleExists) return NotFound("not-found");

                // Actualizar los datos de la cita
                appointment.CustomerId = model.CustomerId;
                appointment.WorkshopId = model.WorkshopId;
                appointment.VehicleId = model.VehicleId;
                appointment.Date = model.Date;
                appointment.Title = model.Title;
                appointment.AppointmentType = model.AppointmentType;
                appointment.NotificationType = model.NotificationType;
                appointment.Description = model.Description;
                //appointment.Status = model.Status;

                await _context.SaveChangesAsync();

                return Ok("appointment-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        private static readonly string[] ValidAppointmentStatuses =
            { "Pendiente", "Confirmada", "En taller", "Completada", "Cancelada" };

        [HttpPatch("{appointmentId}/status")]
        public async Task<ActionResult> UpdateAppointmentStatus(int appointmentId, [FromBody] UpdateAppointmentStatusDto model)
        {
            try
            {
                if (!ValidAppointmentStatuses.Contains(model.Status)) return BadRequest("invalid-status");

                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null) return NotFound("not-found");

                appointment.Status = model.Status;
                await _context.SaveChangesAsync();

                return Ok("appointment-status-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("{appointmentId}")]
        public async Task<ActionResult> DeleteAppointment(int appointmentId)
        {
            try 
            { 
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null) return NotFound("not-found");

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return Ok("appointment-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("vehicle-has-inspections")]
        public async Task<ActionResult> HasActiveInspection(CustomerWorkshopVehicleBodyDto  model)
        {
            try
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == model.CustomerId && !c.IsDeleted);
                var workshopExists = await _context.Workshops.AnyAsync(w => w.Id == model.WorkshopId && !w.IsDeleted);
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == model.VehicleId && v.CustomerId == model.CustomerId && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                dynamic? activeInspection ;

                string exists = string.Empty;

                if (!customerExists) return NotFound("not-found");
                if (!workshopExists) return NotFound("not-found");
                if (vehicle == null) return NotFound("not-found");

                if (vehicle.VehicleFormat == "Automóvil")
                {
                    activeInspection = await _context.WorkshopVehicleInspections
                    .Where(a => a.WorkshopId == model.WorkshopId
                            && a.CustomerId == model.CustomerId
                            && a.VehicleId == model.VehicleId
                            && a.IsActive)
                    .FirstOrDefaultAsync();

                    if (activeInspection == null) exists = "false";
                }
                if (vehicle.VehicleFormat == "Motocicleta")
                {
                    activeInspection = await _context.WorkshopMotocycleInspections
                    .Where(a => a.WorkshopId == model.WorkshopId
                            && a.CustomerId == model.CustomerId
                            && a.VehicleId == model.VehicleId
                            && a.IsActive)
                    .FirstOrDefaultAsync();

                    if (activeInspection == null) exists = "false";
                }


                return Ok("true");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}
