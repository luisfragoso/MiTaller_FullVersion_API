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
    /// "Comunidad": improvement ideas of the caller's audience (workshops or customers). New ideas wait for admin
    /// approval; each account gets VotesPerMonth votes per calendar month, at most one per idea.
    /// </summary>
    [Route("api/community")]
    [ApiController]
    [Authorize]
    public class CommunityController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly CommunityService _community;

        public CommunityController(DataContext context, CommunityService community)
        {
            _context = context;
            _community = community;
        }

        [HttpGet("ideas")]
        public async Task<ActionResult<CommunityBoardDto>> GetBoard()
        {
            var caller = await _community.ResolveAsync(User);
            if (caller == null) return Forbid();
            return Ok(await BuildBoardAsync(caller));
        }

        [HttpPost("ideas")]
        public async Task<ActionResult<CommunityBoardDto>> CreateIdea([FromBody] PostCommunityIdeaDto model)
        {
            var caller = await _community.ResolveAsync(User);
            if (caller == null) return Forbid();

            var title = CommunityService.Clean(model.Title, 3, 60);
            var description = CommunityService.Clean(model.Description, 0, 1000);
            if (title == null) return BadRequest("invalid-title");
            if (description == null) return BadRequest("invalid-description");

            var pending = await _context.CommunityIdeas.CountAsync(i => i.AccountId == caller.AccountId && i.Status == IdeaStatus.Pending);
            if (pending >= CommunityService.MaxPendingIdeas) return BadRequest("too-many-pending-ideas");

            _context.CommunityIdeas.Add(new CommunityIdea
            {
                Audience = caller.Audience,
                AccountId = caller.AccountId,
                CreatedByUserId = caller.UserId,
                Title = title,
                Description = description,
            });
            await _context.SaveChangesAsync();
            return Ok(await BuildBoardAsync(caller));
        }

        [HttpPost("ideas/{id:int}/vote")]
        public async Task<ActionResult<CommunityBoardDto>> Vote(int id)
        {
            var caller = await _community.ResolveAsync(User);
            if (caller == null) return Forbid();

            var open = await _context.CommunityIdeas.AnyAsync(i => i.Id == id && i.Audience == caller.Audience && i.Status == IdeaStatus.Approved);
            if (!open) return BadRequest("idea-not-open");

            var month = CommunityService.CurrentMonth;
            if (await _context.CommunityVotes.AnyAsync(v => v.IdeaId == id && v.AccountId == caller.AccountId && v.Month == month))
                return BadRequest("already-voted");
            if (await UsedVotesAsync(caller.AccountId, month) >= CommunityService.VotesPerMonth) return BadRequest("no-votes-left");

            _context.CommunityVotes.Add(new CommunityVote { IdeaId = id, AccountId = caller.AccountId, UserId = caller.UserId, Month = month });
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Unique (idea, account, month): a double click from two tabs
                return BadRequest("already-voted");
            }
            return Ok(await BuildBoardAsync(caller));
        }

        /// <summary>Takes back this month's vote; votes from earlier months stay.</summary>
        [HttpDelete("ideas/{id:int}/vote")]
        public async Task<ActionResult<CommunityBoardDto>> Unvote(int id)
        {
            var caller = await _community.ResolveAsync(User);
            if (caller == null) return Forbid();

            var month = CommunityService.CurrentMonth;
            var vote = await _context.CommunityVotes.FirstOrDefaultAsync(v => v.IdeaId == id && v.AccountId == caller.AccountId && v.Month == month);
            if (vote == null) return BadRequest("not-voted");

            _context.CommunityVotes.Remove(vote);
            await _context.SaveChangesAsync();
            return Ok(await BuildBoardAsync(caller));
        }

        // Votes on ideas that got implemented or rejected this month are given back
        private Task<int> UsedVotesAsync(Guid accountId, int month) =>
            _context.CommunityVotes.CountAsync(v => v.AccountId == accountId && v.Month == month && v.Idea!.Status == IdeaStatus.Approved);

        private async Task<CommunityBoardDto> BuildBoardAsync(CommunityService.Requester caller)
        {
            var month = CommunityService.CurrentMonth;
            var ideas = await _context.CommunityIdeas
                .Where(i => i.Audience == caller.Audience
                    && (i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented || i.AccountId == caller.AccountId))
                .Select(i => new
                {
                    Idea = i,
                    Votes = i.Votes.Count,
                    Voted = i.Votes.Any(v => v.AccountId == caller.AccountId && v.Month == month),
                })
                .ToListAsync();

            CommunityIdeaDto ToDto(CommunityIdea i, int votes, bool voted) => new()
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Status = i.Status,
                Votes = votes,
                VotedThisMonth = voted,
                IsMine = i.AccountId == caller.AccountId,
                // The note explains a rejection or merge to its author only
                AdminNote = i.AccountId == caller.AccountId ? i.AdminNote : null,
                CreatedAt = i.CreatedAt,
                ImplementedAt = i.ImplementedAt,
            };

            var usedVotes = await UsedVotesAsync(caller.AccountId, month);
            return new CommunityBoardDto
            {
                VotesPerMonth = CommunityService.VotesPerMonth,
                VotesLeft = Math.Max(0, CommunityService.VotesPerMonth - usedVotes),
                Ideas = ideas.Where(r => r.Idea.Status == IdeaStatus.Approved)
                    .OrderByDescending(r => r.Votes).ThenBy(r => r.Idea.CreatedAt)
                    .Select(r => ToDto(r.Idea, r.Votes, r.Voted)).ToList(),
                Implemented = ideas.Where(r => r.Idea.Status == IdeaStatus.Implemented)
                    .OrderByDescending(r => r.Idea.ImplementedAt)
                    .Select(r => ToDto(r.Idea, r.Votes, r.Voted)).ToList(),
                Mine = ideas.Where(r => r.Idea.AccountId == caller.AccountId)
                    .OrderByDescending(r => r.Idea.CreatedAt)
                    .Select(r => ToDto(r.Idea, r.Votes, r.Voted)).ToList(),
            };
        }
    }
}
