using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Workshop
{
    /// <summary>Whether a workshop offers roadside assistance and how far it goes; customers only see those that do.</summary>
    public class WorkshopRoadsideSettings
    {
        [Key]
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop Workshop { get; set; }
        public bool OffersRoadsideAssistance { get; set; }
        public int CoverageRadiusKm { get; set; } = 20;
        public bool Available24h { get; set; }
        [MaxLength(20)]
        public string EmergencyPhone { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
