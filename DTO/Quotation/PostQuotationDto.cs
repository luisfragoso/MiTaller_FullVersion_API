using MiTaller.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Quotation
{
    public class PostQuotationDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid WorkshopId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public List<QuotationServiceDto> Services { get; set; } = new List<QuotationServiceDto>();
        public UserType UserType { get; set; }

        public string Description { get; set; } = string.Empty;

        public float PriceOfLabor { get; set; } = 0;
        public float PriceOfSpareParts { get; set; } = 0;
        public string Status {  get; set; } = string.Empty;
    }

    public class QuotationServiceDto
    {
        [Required]
        public int ServiceId { get; set; }

        public float? Price { get; set; } = 0;
    }
}
