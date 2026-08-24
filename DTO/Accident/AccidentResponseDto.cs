namespace MiTaller.DTO.Accident
{
    public class AccidentResponseDto
    {
        public int Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Plates { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
