namespace MiTaller.DTO.Customer
{
    public class PostCustomerFilesDto
    {
        public Guid CustomerId { get; set; }
        public ICollection<IFormFile> Files { get; set; } = new List<IFormFile>();
        public List<string> FileDescriptions { get; set; } = new List<string>();
    }
}
