using MiTaller.Models.Auth;

namespace MiTaller.DTO.Notifications
{
    public class GetUserNotificationDto
    {
        public Guid UserId { get; set; }
        public UserType UserType { get; set; }
    }
}
