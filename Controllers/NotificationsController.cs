using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Customer;
using MiTaller.DTO.Notifications;
using MiTaller.DTO.Pager;
using MiTaller.DTO.Vehicle;
using MiTaller.DTO.Workshop;
using MiTaller.Models.Auth;
using MiTaller.Models.Customer;
using MiTaller.Models.Notification;
using MiTaller.Models.Workshop;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly DataContext _context;

        public NotificationsController(UserManager<BaseIdentityUser> userManager, DataContext dataContext)
        {
            _userManager = userManager;
            _context = dataContext;
        }

        [HttpPost("get-user-notifications")]
        public async Task<ActionResult<IEnumerable<Notifications>>> GetUserNotifications(GetUserNotificationDto model)
        {
            try
            {
                var existingUser = await _userManager.Users
                    .AnyAsync(u => u.Id == model.UserId && u.UserType == model.UserType && !u.IsDeleted);

                if (!existingUser)
                {
                    return BadRequest("not-found");
                }

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == model.UserId && n.UserType == model.UserType)
                    .OrderByDescending(w => w.RegisterDate)
                    .ToListAsync();

                return Ok(notifications);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-user-notifications-pager")]
        public async Task<ActionResult<PagerResponseDto<Notifications>>> GetUserNotifications(Guid userUID, UserType userType, PagerBodyDto pager)
        {

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userUID && !x.IsDeleted && x.UserType == userType);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Filtrar notificaciones del usuario
                var query = _context.Notifications
                    .Where(x => x.UserId == user.Id);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                // Traer todo sin ordenar ni paginar aún
                var rawNotifications = await query.ToListAsync();

                // Ordenar y paginar en memoria
                var notifications = rawNotifications
                    .OrderByDescending(x => x.RegisterDate)
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToList();

                var response = new PagerResponseDto<Notifications>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = notifications
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error getting user notifications: " + ex.Message);
            }
        }

        [HttpPut("mark-one-as-read/{notificationId}")]
        public async Task<ActionResult> UpdateOneNotificationAsViewed(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .Where(n => n.Id == notificationId)
                    .FirstOrDefaultAsync();

                if (notification == null)
                {
                    return NotFound("not-found");
                }

                notification.IsRead = true;

                await _context.SaveChangesAsync();

                return Ok("notification-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("mark-all-as-read")]
        public async Task<ActionResult> UpdateAllNotificationAsViewed(GetUserNotificationDto model)
        {
            try
            {
                var existingUser = await _userManager.Users
                    .AnyAsync(u => u.Id == model.UserId && u.UserType == model.UserType && !u.IsDeleted);

                if (!existingUser)
                {
                    return BadRequest("not-found");
                }

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == model.UserId && n.UserType == model.UserType)
                    .ToListAsync();

                if (notifications == null)
                {
                    return NotFound("not-found");
                }

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();

                return Ok("notifications-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpDelete("delete-one/{notificationId}")]
        public async Task<ActionResult> DeleteOneNotification(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .Where(n => n.Id == notificationId)
                    .FirstOrDefaultAsync();

                if (notification == null)
                {
                    return NotFound("not-found");
                }

                _context.Notifications.Remove(notification);

                await _context.SaveChangesAsync();

                return Ok("notification-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpDelete("delete-all")]
        public async Task<ActionResult> DeleteAllNotifications(GetUserNotificationDto model)
        {
            try
            {
                var existingUser = await _userManager.Users
                    .AnyAsync(u => u.Id == model.UserId && u.UserType == model.UserType && !u.IsDeleted);

                if (!existingUser)
                {
                    return BadRequest("not-found");
                }

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == model.UserId && n.UserType == model.UserType)
                    .ToListAsync();

                if (notifications == null || !notifications.Any())
                {
                    return NotFound("not-found");
                }

                _context.Notifications.RemoveRange(notifications);

                await _context.SaveChangesAsync();

                return Ok("notifications-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }


        [HttpPost("get-user-notification-settings")]
        public async Task<ActionResult<NotificationResponseDto>> GetUserNotificationSettings(GetUserNotificationDto model)
        {
            try
            {
                var existingUser = await _userManager.Users
                    .AnyAsync(u => u.Id == model.UserId && u.UserType == model.UserType && !u.IsDeleted);

                if (!existingUser)
                {
                    return BadRequest("not-found");
                }

                var notificationSettings = await _context.NotificationSettings
                    .Where(n => n.UserId == model.UserId && n.UserType == model.UserType)
                    .FirstOrDefaultAsync();

                if (notificationSettings == null)
                {
                    return BadRequest("not-found");
                }

                var notificationSettingsDto = new NotificationResponseDto
                {
                    UserId = notificationSettings.UserId,
                    UserType = notificationSettings.UserType,
                    Email = notificationSettings.Email,
                    SMS = notificationSettings.SMS,
                    Push = notificationSettings.Push,
                };

                return Ok(notificationSettingsDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("user-notification-settings")]
        public async Task<ActionResult> UddateUserNotificationSettings(PostNotificationSettingsDto model)
        {
            try
            {
                var existingUser = await _userManager.Users
                    .AnyAsync(u => u.Id == model.UserId && u.UserType == model.UserType && !u.IsDeleted);

                if (!existingUser)
                {
                    return BadRequest("not-found");
                }

                var notificationSettings = await _context.NotificationSettings
                    .Where(n => n.UserId == model.UserId && n.UserType == model.UserType)
                    .FirstOrDefaultAsync();

                if (notificationSettings == null)
                {
                    return NotFound("not-found");
                }

                notificationSettings.Email = model.Email;
                notificationSettings.SMS = model.SMS;
                notificationSettings.Push = model.Push;

                await _context.SaveChangesAsync();

                return Ok("notification-settings-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}
