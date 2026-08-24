namespace MiTaller.DTO.Workshop.Employee
{
    public class WorkshopEmployeeResponseDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public float Salary { get; set; } = 0;
        public string Role { get; set; } = string.Empty;
        public string Permissions { get; set; } = string.Empty;
    }
}
