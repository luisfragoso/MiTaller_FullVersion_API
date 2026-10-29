using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Community;
using MiTaller.Models.Community;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    /// <summary>Portal Admin: support tickets (reply, status) and moderation of Comunidad ideas.</summary>
    [Route("api/admin/community")]
    [ApiController]
    [Authorize(Policy = "PlatformAdmin")]
    [EnableCors("AdminPortal")]
    public class AdminCommunityController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly CommunityService _community;

        public AdminCommunityController(DataContext context, CommunityService community)
        {
            _context = context;
            _community = community;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<CommunitySummaryDto>> GetSummary()
        {
            var month = CommunityService.CurrentMonth;
            return Ok(new CommunitySummaryDto
            {
                OpenTickets = await _context.SupportTickets.CountAsync(t => t.Status == SupportTicketStatus.Open || t.Status == SupportTicketStatus.InProgress),
                UnreadTickets = await _context.SupportTickets.CountAsync(t => t.UnreadByAdmin),
                PendingIdeas = await _context.CommunityIdeas.CountAsync(i => i.Status == IdeaStatus.Pending),
                ApprovedIdeas = await _context.CommunityIdeas.CountAsync(i => i.Status == IdeaStatus.Approved),
                ImplementedIdeas = await _context.CommunityIdeas.CountAsync(i => i.Status == IdeaStatus.Implemented),
                VotesThisMonth = await _context.CommunityVotes.CountAsync(v => v.Month == month),
            });
        }

        /// <summary>Newest activity first. [search] matches subject, author or workshop/customer name.</summary>
        [HttpGet("tickets")]
        public async Task<ActionResult<SupportTicketsPageDto>> GetTickets(
            [FromQuery] CommunityAudience? audience, [FromQuery] SupportTicketStatus? status, [FromQuery] bool unreadOnly = false,
            [FromQuery] string? search = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.SupportTickets.AsQueryable();
            if (audience != null) query = query.Where(t => t.Audience == audience);
            if (status != null) query = query.Where(t => t.Status == status);
            if (unreadOnly) query = query.Where(t => t.UnreadByAdmin);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(t => t.Subject.Contains(term) || t.CreatedByName.Contains(term)
                    || _context.Workshops.Any(w => w.Id == t.AccountId && w.WorkshopName.Contains(term))
                    || _context.Customers.Any(c => c.Id == t.AccountId && c.FullName.Contains(term)));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(t => t.LastMessageAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new { Ticket = t, Last = t.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.Body).FirstOrDefault() })
                .ToListAsync();
            var names = await _community.AccountNamesAsync(rows.Select(r => (r.Ticket.Audience, r.Ticket.AccountId)));

            return Ok(new SupportTicketsPageDto
            {
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                TotalElements = total,
                Elements = rows.Select(r => CommunityService.ToTicketDto(r.Ticket, r.Last ?? "", r.Ticket.UnreadByAdmin,
                    names.GetValueOrDefault(r.Ticket.AccountId, "(cuenta eliminada)"))).ToList(),
            });
        }

        [HttpGet("tickets/{id:int}")]
        public async Task<ActionResult<SupportTicketDetailDto>> GetTicket(int id)
        {
            var ticket = await _context.SupportTickets.Include(t => t.Messages).FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound("not-found");

            var unread = ticket.UnreadByAdmin;
            if (unread)
            {
                ticket.UnreadByAdmin = false;
                await _context.SaveChangesAsync();
            }
            return Ok(await DetailAsync(ticket, unread));
        }

        /// <summary>Replies to the user and notifies them; an Open ticket moves to InProgress unless a status is given.</summary>
        [HttpPost("tickets/{id:int}/messages")]
        public async Task<ActionResult<SupportTicketDetailDto>> Reply(int id, [FromBody] PostSupportMessageDto model)
        {
            var ticket = await _context.SupportTickets.Include(t => t.Messages).FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound("not-found");

            var body = CommunityService.Clean(model.Body, 1, 2000);
            if (body == null) return BadRequest("invalid-message");
            if (model.Status != null && !Enum.IsDefined(model.Status.Value)) return BadRequest("invalid-status");

            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId);
            var adminName = await _context.Admins.Where(a => a.Id == adminId).Select(a => a.FullName).FirstOrDefaultAsync();
            ticket.Messages.Add(new SupportTicketMessage
            {
                FromAdmin = true,
                AuthorUserId = adminId,
                AuthorName = string.IsNullOrWhiteSpace(adminName) ? "Admin" : adminName,
                Body = body,
            });
            ticket.Status = model.Status ?? (ticket.Status == SupportTicketStatus.Open ? SupportTicketStatus.InProgress : ticket.Status);
            ticket.UnreadByUser = true;
            ticket.UnreadByAdmin = false;
            ticket.LastMessageAt = DateTime.Now;
            _community.Notify(ticket.Audience, ticket.AccountId, $"Respondimos tu ticket #{ticket.Id}", ticket.Subject, "SupportTicketReply");
            await _context.SaveChangesAsync();
            return Ok(await DetailAsync(ticket, false));
        }

        [HttpPut("tickets/{id:int}/status")]
        public async Task<ActionResult<SupportTicketDetailDto>> SetTicketStatus(int id, [FromBody] PutTicketStatusDto model)
        {
            var ticket = await _context.SupportTickets.Include(t => t.Messages).FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound("not-found");
            if (!Enum.IsDefined(model.Status)) return BadRequest("invalid-status");

            if (ticket.Status != model.Status && model.Status is SupportTicketStatus.Resolved or SupportTicketStatus.Closed)
            {
                _community.Notify(ticket.Audience, ticket.AccountId,
                    $"Tu ticket #{ticket.Id} está {CommunityService.StatusLabel(model.Status).ToLower()}", ticket.Subject, "SupportTicketStatus");
            }
            ticket.Status = model.Status;
            await _context.SaveChangesAsync();
            return Ok(await DetailAsync(ticket, false));
        }

        [HttpGet("ideas")]
        public async Task<ActionResult<List<AdminIdeaDto>>> GetIdeas([FromQuery] CommunityAudience? audience, [FromQuery] IdeaStatus? status)
        {
            var month = CommunityService.CurrentMonth;
            var query = _context.CommunityIdeas.AsQueryable();
            if (audience != null) query = query.Where(i => i.Audience == audience);
            if (status != null) query = query.Where(i => i.Status == status);

            var rows = await query
                .Select(i => new
                {
                    Idea = i,
                    Votes = i.Votes.Count,
                    VotesThisMonth = i.Votes.Count(v => v.Month == month),
                    MergedIntoTitle = _context.CommunityIdeas.Where(t => t.Id == i.MergedIntoId).Select(t => t.Title).FirstOrDefault(),
                })
                .ToListAsync();
            var names = await _community.AccountNamesAsync(rows.Select(r => (r.Idea.Audience, r.Idea.AccountId)));

            return Ok(rows
                .OrderBy(r => r.Idea.Status == IdeaStatus.Pending ? 0 : 1)
                .ThenByDescending(r => r.Votes)
                .ThenByDescending(r => r.Idea.CreatedAt)
                .Select(r => new AdminIdeaDto
                {
                    Id = r.Idea.Id,
                    Title = r.Idea.Title,
                    Description = r.Idea.Description,
                    Status = r.Idea.Status,
                    Votes = r.Votes,
                    AdminNote = r.Idea.AdminNote,
                    CreatedAt = r.Idea.CreatedAt,
                    ImplementedAt = r.Idea.ImplementedAt,
                    Audience = r.Idea.Audience,
                    AccountName = names.GetValueOrDefault(r.Idea.AccountId, "(cuenta eliminada)"),
                    VotesThisMonth = r.VotesThisMonth,
                    MergedIntoId = r.Idea.MergedIntoId,
                    MergedIntoTitle = r.MergedIntoTitle,
                    ReviewedAt = r.Idea.ReviewedAt,
                })
                .ToList());
        }

        /// <summary>Fixes wording/typos before or after approving; the author is not notified.</summary>
        [HttpPut("ideas/{id:int}")]
        public async Task<IActionResult> UpdateIdea(int id, [FromBody] PutIdeaDto model)
        {
            var idea = await _context.CommunityIdeas.FindAsync(id);
            if (idea == null) return NotFound("not-found");

            var title = CommunityService.Clean(model.Title, 3, 60);
            var description = CommunityService.Clean(model.Description, 0, 1000);
            if (title == null) return BadRequest("invalid-title");
            if (description == null) return BadRequest("invalid-description");

            idea.Title = title;
            idea.Description = description;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Approve (publish), reject (with an optional note for the author) or mark implemented; Approved also undoes
        /// a rejection or an "implemented" mark. Merged ideas can't change.
        /// </summary>
        [HttpPut("ideas/{id:int}/status")]
        public async Task<IActionResult> SetIdeaStatus(int id, [FromBody] PutIdeaStatusDto model)
        {
            var idea = await _context.CommunityIdeas.FindAsync(id);
            if (idea == null) return NotFound("not-found");

            var allowed = (idea.Status, model.Status) switch
            {
                (IdeaStatus.Merged, _) => false,
                (_, IdeaStatus.Pending or IdeaStatus.Merged) => false,
                (IdeaStatus.Pending, IdeaStatus.Implemented) => false,
                var (from, to) => from != to,
            };
            if (!allowed) return BadRequest("invalid-status");

            var note = model.Note?.Trim();
            if (note?.Length > 500) return BadRequest("invalid-note");

            var previous = idea.Status;
            idea.Status = model.Status;
            idea.ReviewedAt = DateTime.Now;
            idea.ImplementedAt = model.Status == IdeaStatus.Implemented ? DateTime.Now : null;
            if (model.Status == IdeaStatus.Rejected) idea.AdminNote = string.IsNullOrEmpty(note) ? null : note;
            else if (model.Status == IdeaStatus.Approved) idea.AdminNote = null;

            var (title, content) = model.Status switch
            {
                IdeaStatus.Approved when previous != IdeaStatus.Implemented =>
                    ("Tu idea ya está en Comunidad", $"\"{idea.Title}\" ya se puede votar."),
                IdeaStatus.Rejected =>
                    ("Tu idea no se publicó", string.IsNullOrEmpty(note) ? $"\"{idea.Title}\"" : $"\"{idea.Title}\": {note}"),
                IdeaStatus.Implemented =>
                    ("¡Hicimos tu idea realidad!", $"\"{idea.Title}\" ya está disponible en MiTaller. Gracias por proponerla."),
                _ => ((string?)null, (string?)null),
            };
            if (title != null) _community.Notify(idea.Audience, idea.AccountId, title, content!, "CommunityIdea");

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Folds a duplicate into [MergeIdeaDto.TargetId]: its votes move to the target (a vote the same account already
        /// gave the target that month is dropped, which gives that vote back) and the duplicate leaves the cloud.
        /// </summary>
        [HttpPost("ideas/{id:int}/merge")]
        public async Task<IActionResult> Merge(int id, [FromBody] MergeIdeaDto model)
        {
            var idea = await _context.CommunityIdeas.Include(i => i.Votes).FirstOrDefaultAsync(i => i.Id == id);
            var target = await _context.CommunityIdeas.Include(i => i.Votes).FirstOrDefaultAsync(i => i.Id == model.TargetId);
            if (idea == null) return NotFound("not-found");
            if (target == null || target.Id == idea.Id || target.Audience != idea.Audience
                || idea.Status == IdeaStatus.Merged
                || target.Status is not (IdeaStatus.Approved or IdeaStatus.Implemented))
            {
                return BadRequest("invalid-target");
            }

            var existing = target.Votes.Select(v => (v.AccountId, v.Month)).ToHashSet();
            foreach (var vote in idea.Votes.ToList())
            {
                if (existing.Add((vote.AccountId, vote.Month))) vote.IdeaId = target.Id;
                else _context.CommunityVotes.Remove(vote);
            }

            idea.Status = IdeaStatus.Merged;
            idea.MergedIntoId = target.Id;
            idea.ReviewedAt = DateTime.Now;
            idea.AdminNote = $"Ya existía una idea igual: \"{target.Title}\". Tus votos pasaron a esa idea.";
            _community.Notify(idea.Audience, idea.AccountId, "Unimos tu idea con otra igual",
                $"\"{idea.Title}\" ahora suma votos en \"{target.Title}\".", "CommunityIdea");
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<SupportTicketDetailDto> DetailAsync(SupportTicket ticket, bool unread)
        {
            var names = await _community.AccountNamesAsync(new[] { (ticket.Audience, ticket.AccountId) });
            return CommunityService.ToDetailDto(ticket, unread, names.GetValueOrDefault(ticket.AccountId, "(cuenta eliminada)"));
        }
    }
}
