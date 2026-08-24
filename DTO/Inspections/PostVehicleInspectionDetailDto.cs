namespace MiTaller.DTO.Inspections
{
    public class PostVehicleInspectionDetailDto
    {
        public int VehicleInspectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
