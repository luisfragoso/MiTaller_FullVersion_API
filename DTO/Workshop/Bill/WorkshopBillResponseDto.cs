namespace MiTaller.DTO.Workshop.Bill
{
    public class WorkshopBillResponseDto
    {
        public int Id { get; set; }
        public Guid? WorkshopId { get; set; }
        public string Description { get; set; } = string.Empty;
        public float Amount { get; set; }
    }
}
