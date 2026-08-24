using MiTaller.Models.Auth;

namespace MiTaller.Models.Notification
{
    public class NotificationSettings
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public UserType UserType { get; set; }
        public bool Email {  get; set; } = true;
        public bool SMS { get; set; } = true;
        public bool Push { get; set; } = true;
    }
}
