using MiTaller.Models.Auth;

namespace MiTaller.DTO.Notifications
{
    public class NotificationSettingsResponseDto
    {
        public Guid UserId { get; set; }
        public UserType UserType { get; set; }
        public bool Email { get; set; } = true;
        public bool SMS { get; set; } = true;
        public bool Push { get; set; } = true;
    }
}
