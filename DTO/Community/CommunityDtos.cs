using MiTaller.DTO.Pager;
using MiTaller.Models.Community;

namespace MiTaller.DTO.Community
{
    public class SupportTicketDto
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public SupportTicketCategory Category { get; set; }
        public SupportTicketStatus Status { get; set; }
        public CommunityAudience Audience { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        // Admin views only: workshop or customer name
        public string? AccountName { get; set; }
        public bool Unread { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
    }

    public class SupportTicketDetailDto : SupportTicketDto
    {
        public List<SupportMessageDto> Messages { get; set; } = new();
    }

    public class SupportMessageDto
    {
        public int Id { get; set; }
        public bool FromAdmin { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class PostSupportTicketDto
    {
        public string Subject { get; set; } = string.Empty;
        public SupportTicketCategory Category { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class PostSupportMessageDto
    {
        public string Body { get; set; } = string.Empty;
        // Admin only: optional status change sent with the reply
        public SupportTicketStatus? Status { get; set; }
    }

    public class PutTicketStatusDto
    {
        public SupportTicketStatus Status { get; set; }
    }

    public class CommunityIdeaDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IdeaStatus Status { get; set; }
        public int Votes { get; set; }
        public bool VotedThisMonth { get; set; }
        public bool IsMine { get; set; }
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ImplementedAt { get; set; }
    }

    public class CommunityBoardDto
    {
        public int VotesPerMonth { get; set; }
        public int VotesLeft { get; set; }
        // Approved ideas, most voted first (the cloud)
        public List<CommunityIdeaDto> Ideas { get; set; } = new();
        public List<CommunityIdeaDto> Implemented { get; set; } = new();
        // Caller's own ideas in any status
        public List<CommunityIdeaDto> Mine { get; set; } = new();
    }

    public class PostCommunityIdeaDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AdminIdeaDto : CommunityIdeaDto
    {
        public CommunityAudience Audience { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public int VotesThisMonth { get; set; }
        public int? MergedIntoId { get; set; }
        public string? MergedIntoTitle { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public class PutIdeaDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PutIdeaStatusDto
    {
        public IdeaStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class MergeIdeaDto
    {
        public int TargetId { get; set; }
    }

    public class CommunitySettingsDto
    {
        public int VotesPerMonth { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CommunitySummaryDto
    {
        public int OpenTickets { get; set; }
        public int UnreadTickets { get; set; }
        public int PendingIdeas { get; set; }
        public int ApprovedIdeas { get; set; }
        public int ImplementedIdeas { get; set; }
        public int VotesThisMonth { get; set; }
    }

    public class SupportTicketsPageDto : PagerWithCountResponseDto<SupportTicketDto>
    {
    }
}
