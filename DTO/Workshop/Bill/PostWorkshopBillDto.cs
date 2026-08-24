using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Workshop.Bill
{
    public class PostWorkshopBillDto
    {
        public Guid WorkshopId { get; set; }
        public string Description { get; set; } = string.Empty;
        public float Amount { get; set; }
    }
}
