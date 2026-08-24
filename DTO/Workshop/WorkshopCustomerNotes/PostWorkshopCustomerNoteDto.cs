namespace MiTaller.DTO.Workshop.WorkshopCustomerNotes
{
    public class PostWorkshopCustomerNoteDto
    {
        public Guid WorkshopId { get; set; }
        public Guid CustomerId { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
