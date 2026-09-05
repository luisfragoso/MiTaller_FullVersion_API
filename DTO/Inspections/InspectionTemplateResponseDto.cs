namespace MiTaller.DTO.Inspections
{
    public class InspectionTemplateResponseDto
    {
        public int Id { get; set; }
        public string VehicleFormat { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<InspectionTemplateSectionResponseDto> Sections { get; set; } = new();
    }

    public class InspectionTemplateSectionResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<InspectionTemplateItemResponseDto> Items { get; set; } = new();
    }

    public class InspectionTemplateItemResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool AllowNotApplicable { get; set; }
    }
}
