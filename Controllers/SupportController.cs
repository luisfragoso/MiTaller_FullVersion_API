using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Community;
using MiTaller.Models.Community;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    /// <summary>
    /// Support tickets of the caller's account (workshop incl. its employees, or customer). The account always comes
    /// from the token; a ticket of another account answers 404.
    /// </summary>
    [Route("api/support")]
    [ApiController]
    [Authorize]
    public class SupportController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly CommunityService _community;

        public SupportController(DataContext context, CommunityService community)
        {
            _context = context;
            _community = community;
        }

        [HttpGet("tickets")]
        public async Task<ActionResult<List<SupportTicketDto>>> GetTickets()
        {
            var caller = await _community.ResolveAsync(User);
            if (caller == null) return Forbid();

            var tickets = await _context.SupportTickets
                .Where(t => t.AccountId == caller.AccountId && t.Audience == caller.Audience)
                .OrderByDescending(t => t.LastMessageAt)
                .Select(t => new { Ticket = t, Last = t.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.Body).FirstOrDefault() })
                .ToListAsync();
            return Ok(tickets.Select(r => CommunityService.ToTicketDto(r.Ticket, r.Last ?? "", r.Ticket.UnreadByUser)).ToList());
        }

        [HttpGet("tickets/{id:int}")]
        public async Task<ActionResult<SupportTicketDetailDto>> GetTicket(int id)
        {
            var (ticket, error) = await FindOwnAsync(id);
            if (ticket == null) return error!;

            var unread = ticket.UnreadByUser;
            if (unread)
            {
                ticket.UnreadByUser = false;
                await _context.SaveChangesAsync();
            }
            return Ok(CommunityService.ToDetailDto(ticket, unread));
        }

        [HttpPost("tickets")]
        public async Task<ActionResult<SupportTicketDetailDto>> CreateTicket([FromBody] PostSupportTicketDto model)
        {
            var caller = await _community.ResolveAsync(User);
            if (caller == null) return Forbid();

            var subject = CommunityService.Clean(model.Subject, 3, 120);
            var message = CommunityService.Clean(model.Message, 3, 2000);
            if (subject == null) return BadRequest("invalid-subject");
            if (message == null) return BadRequest("invalid-message");
            if (!Enum.IsDefined(model.Category)) return BadRequest("invalid-category");

            var open = await _context.SupportTickets.CountAsync(t => t.AccountId == caller.AccountId
                && (t.Status == SupportTicketStatus.Open || t.Status == SupportTicketStatus.InProgress));
            if (open >= CommunityService.MaxOpenTickets) return BadRequest("too-many-open-tickets");

            var ticket = new SupportTicket
            {
                Audience = caller.Audience,
                AccountId = caller.AccountId,
                CreatedByUserId = caller.UserId,
                CreatedByName = caller.Name,
                Subject = subject,
                Category = model.Category,
            };
            ticket.Messages.Add(new SupportTicketMessage { AuthorUserId = caller.UserId, AuthorName = caller.Name, Body = message });
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();
            return Ok(CommunityService.ToDetailDto(ticket, false));
        }

        /// <summary>A reply to a resolved ticket reopens it; a closed ticket takes no more messages.</summary>
        [HttpPost("tickets/{id:int}/messages")]
        public async Task<ActionResult<SupportTicketDetailDto>> Reply(int id, [FromBody] PostSupportMessageDto model)
        {
            var (ticket, error) = await FindOwnAsync(id);
            if (ticket == null) return error!;
            if (ticket.Status == SupportTicketStatus.Closed) return BadRequest("ticket-closed");

            var body = CommunityService.Clean(model.Body, 1, 2000);
            if (body == null) return BadRequest("invalid-message");

            var caller = (await _community.ResolveAsync(User))!;
            ticket.Messages.Add(new SupportTicketMessage { AuthorUserId = caller.UserId, AuthorName = caller.Name, Body = body });
            if (ticket.Status == SupportTicketStatus.Resolved) ticket.Status = SupportTicketStatus.Open;
            ticket.UnreadByAdmin = true;
            ticket.LastMessageAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok(CommunityService.ToDetailDto(ticket, false));
        }

        [HttpPost("tickets/{id:int}/close")]
        public async Task<ActionResult<SupportTicketDetailDto>> Close(int id)
        {
            var (ticket, error) = await FindOwnAsync(id);
            if (ticket == null) return error!;

            ticket.Status = SupportTicketStatus.Closed;
            await _context.SaveChangesAsync();
            return Ok(CommunityService.ToDetailDto(ticket, false));
        }

        private async Task<(SupportTicket?, ActionResult?)> FindOwnAsync(int id)
        {
            var caller = await _community.ResolveAsync(User);
            if (caller == null) return (null, Forbid());

            var ticket = await _context.SupportTickets.Include(t => t.Messages)
                .FirstOrDefaultAsync(t => t.Id == id && t.AccountId == caller.AccountId && t.Audience == caller.Audience);
            return ticket == null ? (null, NotFound("not-found")) : (ticket, null);
        }
    }
}
