using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Accident
{
    /// <summary>"Reportaron tu placa, ¿estás bien?" sent to the vehicle owner; dismissing it emails the contacts a false alarm.</summary>
    public class AccidentOwnerAlert
    {
        [Key]
        public int Id { get; set; }
        public int AccidentId { get; set; }
        [ForeignKey("AccidentId")]
        public Accident Accident { get; set; }
        public Guid OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public Customer.Customer Owner { get; set; }
        [MaxLength(200)]
        public string VehicleName { get; set; } = string.Empty;
        // Secret for the email link, which works without logging in
        [MaxLength(64)]
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? FalseAlarmAt { get; set; }
    }
}
