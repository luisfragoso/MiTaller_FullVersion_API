using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Community;
using MiTaller.Models.Auth;
using MiTaller.Models.Community;
using MiTaller.Models.Notification;

namespace MiTaller.Services
{
    /// <summary>Who is calling (workshop account, its employee or a customer) and shared helpers for support tickets and ideas.</summary>
    public class CommunityService
    {
        public const int VotesPerMonth = 3;
        public const int MaxOpenTickets = 5;
        public const int MaxPendingIdeas = 5;

        private readonly DataContext _context;

        public CommunityService(DataContext context)
        {
            _context = context;
        }

        public record Requester(CommunityAudience Audience, Guid AccountId, Guid UserId, string Name);

        public static int CurrentMonth => DateTime.Now.Year * 100 + DateTime.Now.Month;

        /// <summary>Null for admins or an employee without a workshop.</summary>
        public async Task<Requester?> ResolveAsync(ClaimsPrincipal user)
        {
            if (!Enum.TryParse<UserType>(user.FindFirst("UserType")?.Value, out var type)
                || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                return null;

            switch (type)
            {
                case UserType.Workshop:
                    var owner = await _context.Workshops.Where(w => w.Id == id).Select(w => w.AssociateFullName).FirstOrDefaultAsync();
                    return owner == null ? null : new Requester(CommunityAudience.Workshops, id, id, owner);
                case UserType.Employee:
                    var employee = await _context.WorkshopEmployees
                        .Where(we => we.EmployeeId == id && !we.IsDeleted)
                        .Select(we => new { we.WorkshopId, we.FullName })
                        .FirstOrDefaultAsync();
                    return employee == null ? null : new Requester(CommunityAudience.Workshops, employee.WorkshopId, id, employee.FullName);
                case UserType.Customer:
                    var customer = await _context.Customers.Where(c => c.Id == id).Select(c => c.FullName).FirstOrDefaultAsync();
                    return customer == null ? null : new Requester(CommunityAudience.Customers, id, id, customer);
                default:
                    return null;
            }
        }

        /// <summary>Queues an in-app notification for the workshop or customer account (caller saves).</summary>
        public void Notify(CommunityAudience audience, Guid accountId, string title, string content, string eventName) =>
            _context.Notifications.Add(new Notifications
            {
                UserId = accountId,
                UserType = audience == CommunityAudience.Workshops ? UserType.Workshop : UserType.Customer,
                Title = title,
                Content = content,
                Event = eventName,
            });

        public async Task<Dictionary<Guid, string>> AccountNamesAsync(IEnumerable<(CommunityAudience Audience, Guid Id)> accounts)
        {
            var workshopIds = accounts.Where(a => a.Audience == CommunityAudience.Workshops).Select(a => a.Id).Distinct().ToList();
            var customerIds = accounts.Where(a => a.Audience == CommunityAudience.Customers).Select(a => a.Id).Distinct().ToList();
            var names = await _context.Workshops.Where(w => workshopIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, w => w.WorkshopName);
            foreach (var c in await _context.Customers.Where(c => customerIds.Contains(c.Id)).Select(c => new { c.Id, c.FullName }).ToListAsync())
                names[c.Id] = c.FullName;
            return names;
        }

        public static SupportTicketDto ToTicketDto(SupportTicket t, string lastMessage, bool unread, string? accountName = null) =>
            Fill(new SupportTicketDto(), t, lastMessage, unread, accountName);

        /// <summary>[accountName] is only passed by admin views; users see "Equipo MiTaller" instead of the admin's name.</summary>
        public static SupportTicketDetailDto ToDetailDto(SupportTicket t, bool unread, string? accountName = null)
        {
            var messages = t.Messages.OrderBy(m => m.CreatedAt).ToList();
            var dto = Fill(new SupportTicketDetailDto(), t, messages.LastOrDefault()?.Body ?? "", unread, accountName);
            dto.Messages = messages.Select(m => new SupportMessageDto
            {
                Id = m.Id,
                FromAdmin = m.FromAdmin,
                AuthorName = m.FromAdmin && accountName == null ? "Equipo MiTaller" : m.AuthorName,
                Body = m.Body,
                CreatedAt = m.CreatedAt,
            }).ToList();
            return dto;
        }

        private static T Fill<T>(T dto, SupportTicket t, string lastMessage, bool unread, string? accountName) where T : SupportTicketDto
        {
            dto.Id = t.Id;
            dto.Subject = t.Subject;
            dto.Category = t.Category;
            dto.Status = t.Status;
            dto.Audience = t.Audience;
            dto.CreatedByName = t.CreatedByName;
            dto.AccountName = accountName;
            dto.Unread = unread;
            dto.LastMessage = lastMessage;
            dto.CreatedAt = t.CreatedAt;
            dto.LastMessageAt = t.LastMessageAt;
            return dto;
        }

        public static string? Clean(string? text, int min, int max)
        {
            var value = text?.Trim() ?? "";
            return value.Length < min || value.Length > max ? null : value;
        }

        public static string StatusLabel(SupportTicketStatus status) => status switch
        {
            SupportTicketStatus.Open => "Abierto",
            SupportTicketStatus.InProgress => "En proceso",
            SupportTicketStatus.Resolved => "Resuelto",
            _ => "Cerrado",
        };
    }
}
