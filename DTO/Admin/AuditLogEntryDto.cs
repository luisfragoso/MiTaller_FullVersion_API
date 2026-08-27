namespace MiTaller.DTO.Admin
{
    public class AuditLogEntryDto
    {
        public long Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public Guid? ChangedByUserId { get; set; }
    }

    public class AuditLogFilterRequestDto
    {
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public Guid? ChangedByUserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
