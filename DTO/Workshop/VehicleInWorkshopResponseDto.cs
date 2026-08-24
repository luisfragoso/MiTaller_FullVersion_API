using MiTaller.DTO.Vehicle;

namespace MiTaller.DTO.Workshop
{
    public class VehicleInWorkshopResponseDto
    {
        public int WorkshopInspectionId { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public VehicleResponseDto Vehicle { get; set; }
    }
}
