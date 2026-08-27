using MiTaller.Models.Auth;
using MiTaller.Models.Audit;

namespace MiTaller.Models.Notification
{
    public class Notifications : INotAudited
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public UserType UserType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public bool IsNew { get; set; } = true;
        public string Event { get; set; } = string.Empty;
        public DateTime RegisterDate { get; set; } = DateTime.Now;
    }
}
