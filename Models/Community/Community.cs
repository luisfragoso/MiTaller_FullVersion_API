using System.ComponentModel.DataAnnotations;
using MiTaller.Models.Audit;

namespace MiTaller.Models.Community
{
    // Workshops (owner + employees) and customers each get their own tickets list and idea cloud.
    public enum CommunityAudience { Workshops, Customers }

    public enum SupportTicketCategory { Problem, Question, Account, Billing, Other }

    public enum SupportTicketStatus { Open, InProgress, Resolved, Closed }

    public enum IdeaStatus { Pending, Approved, Rejected, Implemented, Merged }

    public class SupportTicket
    {
        public int Id { get; set; }
        public CommunityAudience Audience { get; set; }
        // Workshop id (also for tickets opened by its employees) or customer id
        public Guid AccountId { get; set; }
        public Guid CreatedByUserId { get; set; }
        [MaxLength(150)]
        public string CreatedByName { get; set; } = string.Empty;
        [MaxLength(120)]
        public string Subject { get; set; } = string.Empty;
        public SupportTicketCategory Category { get; set; }
        public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
        public bool UnreadByUser { get; set; }
        public bool UnreadByAdmin { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastMessageAt { get; set; } = DateTime.Now;
        public List<SupportTicketMessage> Messages { get; set; } = new();
    }

    public class SupportTicketMessage : INotAudited
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public SupportTicket? Ticket { get; set; }
        public bool FromAdmin { get; set; }
        public Guid AuthorUserId { get; set; }
        [MaxLength(150)]
        public string AuthorName { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class CommunityIdea
    {
        public int Id { get; set; }
        public CommunityAudience Audience { get; set; }
        public Guid AccountId { get; set; }
        public Guid CreatedByUserId { get; set; }
        [MaxLength(60)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        public IdeaStatus Status { get; set; } = IdeaStatus.Pending;
        public int? MergedIntoId { get; set; }
        // Shown to the author, e.g. why it was rejected
        [MaxLength(500)]
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ReviewedAt { get; set; }
        public DateTime? ImplementedAt { get; set; }
        public List<CommunityVote> Votes { get; set; } = new();
    }

    /// <summary>One row per idea, account and month (yyyyMM); the account has VotesPerMonth rows per month.</summary>
    public class CommunityVote : INotAudited
    {
        public int Id { get; set; }
        public int IdeaId { get; set; }
        public CommunityIdea? Idea { get; set; }
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }
        public int Month { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
