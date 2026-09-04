using MiTaller.DTO.Vehicle;
using MiTaller.DTO.Workshop.Services;
using MiTaller.Models;
using MiTaller.Models.Workshop;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.DTO.Quotation
{
    public class QuotationResponseDto
    {
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        public string WorkshopName { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public VehicleResponseDto? Vehicle { get; set; }
        public int? WorkshopVehicleInspectionId { get; set; }
        public string Description { get; set; } = string.Empty;
        public  float PriceOfLabor { get; set; } = 0;
        public float PriceOfSpareParts { get; set; } = 0;
        public string Title { get; set; } = string.Empty;
        public List<WorkshopServiceResponseDto>? Services { get; set; } = new List<WorkshopServiceResponseDto>();
        public string Status { get; set; } = "Pendiente"; // Estados: "Pendiente", "Aprobada", "Rechazada"
        public DateTime CreatedAt { get; set; }
    }
}