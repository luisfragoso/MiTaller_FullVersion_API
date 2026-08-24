namespace MiTaller.DTO.Customer
{
    public class CustomerInfoResponseDto
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Landline { get; set; } = string.Empty;
        public byte[]? ProfilePicture { get; set; }
    }
}
