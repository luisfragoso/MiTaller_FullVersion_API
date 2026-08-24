namespace MiTaller.DTO.Inspections
{
    public class WorkshopVehicleFileDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}
