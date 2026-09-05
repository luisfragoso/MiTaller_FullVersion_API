using MiTaller.DTO.Customer;
using MiTaller.Models.Inspections;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Inspections
{
    public class InspectionResultItemDto
    {
        [Required]
        public int TemplateItemId { get; set; }

        public InspectionItemValue Value { get; set; } = InspectionItemValue.Optimal;
    }

    public class InspectionSectionCommentDto
    {
        [Required]
        public int SectionId { get; set; }

        public string? Comment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generic (template-driven) inspection creation payload. Reuses the same
    /// customer/vehicle sub-DTOs the car inspection endpoint uses
    /// (PostCustomerFromWorkshopDto / PostVehicleFromWorkshopDto) so
    /// "create customer/vehicle if new" logic and shapes stay identical.
    /// </summary>
    public class PostInspectionDto
    {
        [Required]
        public Guid WorkshopId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        public bool IsNewCustomer { get; set; } = false;
        [Required]
        public bool IsNewVehicle { get; set; } = false;

        // Si `IsNewCustomer == true`, se usará este DTO para crear al cliente
        public PostCustomerFromWorkshopDto? Customer { get; set; }

        // Si `IsNewVehicle == true`, se usará este DTO para crear el vehículo
        public PostVehicleFromWorkshopDto? Vehicle { get; set; }

        // Si `IsNewCustomer == false` / `IsNewVehicle == false`, se tomarán estos Ids
        public Guid? CustomerId { get; set; }
        public int? VehicleId { get; set; }

        // Bound from form-data as Results[0].TemplateItemId, Results[0].Value, Results[1]...
        public List<InspectionResultItemDto> Results { get; set; } = new();

        // Bound from form-data as SectionComments[0].SectionId, SectionComments[0].Comment...
        public List<InspectionSectionCommentDto> SectionComments { get; set; } = new();

        public string? GeneralObservations { get; set; } = string.Empty;

        public List<IFormFile>? Files { get; set; } = new();
    }
}
