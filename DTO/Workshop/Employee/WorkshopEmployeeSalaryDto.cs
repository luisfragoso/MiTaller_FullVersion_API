namespace MiTaller.DTO.Workshop.Employee
{
    public class WorkshopEmployeeSalaryDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public float Salary { get; set; }
    }
}
