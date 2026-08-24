namespace MiTaller.DTO.Vehicle
{
    public class VehicleHistoryDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
