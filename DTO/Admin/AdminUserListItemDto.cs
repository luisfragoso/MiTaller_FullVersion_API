namespace MiTaller.DTO.Admin
{
    public class AdminUserListItemDto
    {
        public Guid Id { get; set; }
        public string UserType { get; set; } = string.Empty; // "Customer" | "Workshop"
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
