namespace MiTaller.DTO.Vehicle
{
    public class VehicleByCustomerResponseDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Plates { get; set; } = string.Empty;
        public string RimRubber { get; set; } = string.Empty;
        public string Kms { get; set; } = string.Empty;
        public string VehicleFormat {  get; set; } = string.Empty;
        public byte[]? Image { get; set; }
        public List<VehicleHistoryDto> VehicleHistory { get; set; } = new List<VehicleHistoryDto>();
    }

}
