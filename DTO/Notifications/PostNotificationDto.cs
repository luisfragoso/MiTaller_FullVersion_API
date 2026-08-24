using MiTaller.Models.Auth;

namespace MiTaller.DTO.Notifications
{
    public class PostNotificationDto
    {
        public Guid UserId { get; set; }
        public UserType UserType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public bool IsNew { get; set; } = true;
        public string Event { get; set; } = string.Empty;
        public string RegisterDate { get; set; } = string.Empty;
    }
}
