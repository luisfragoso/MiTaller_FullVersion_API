namespace MiTaller.DTO.Workshop
{
    // A vehicle of the customer that has at least one inspection at this workshop
    public class WorkshopCustomerInspectedVehicleDto
    {
        public int VehicleId { get; set; }
        public string VehicleFormat { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Plates { get; set; } = string.Empty;
        public int Inspections { get; set; }
        public DateTime LastInspectionDate { get; set; }
        public bool HasActiveInspection { get; set; }
    }
}
