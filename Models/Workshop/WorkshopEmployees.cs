using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Workshop
{
    public class WorkshopEmployees
    {
        [Key]
        public Guid Id { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop Workshop { get; set; }
        public Guid? EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public float Salary { get; set; } = 0;
        public string Role { get; set; } = string.Empty;
        public string Permissions { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
    }
}
