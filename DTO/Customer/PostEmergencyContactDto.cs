namespace MiTaller.DTO.Customer
{
    public class PostEmergencyContactDto
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool MustBeNotified { get; set; } = false;
    }
}
