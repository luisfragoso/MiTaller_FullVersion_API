using MiTaller.Models.Workshop;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models
{
    public class Quotation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer.Customer? Customer { get; set; }  // Opcional para evitar problemas de eliminación

        [Required]
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop.Workshop? Workshop { get; set; }  // Opcional

        [Required]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle.Vehicle? Vehicle { get; set; }  // Opcional

        [Required]
        public List<QuotationService> Services { get; set; } = new List<QuotationService>(); // Lista de servicios en la cotización

        public string Description { get; set; } = string.Empty;

        public float PriceOfLabor { get; set; } = 0;
        public float PriceOfSpareParts { get; set; } = 0;

        [Required]
        public string Status { get; set; } = "Pendiente"; // Estados: "Pendiente", "Aprobada", "Rechazada"

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class QuotationService
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuotationId { get; set; }
        [ForeignKey("QuotationId")]
        public Quotation Quotation { get; set; }

        [Required]
        public int WorkshopServiceId { get; set; }
        [ForeignKey("WorkshopServiceId")]
        public WorkshopServices Service { get; set; }

        public bool IsSelected { get; set; } = true;  // Define si el servicio está activo en la cotización

        public float? Price { get; set; } = 0;  // Precio individual del servicio en la cotización
    }
}
