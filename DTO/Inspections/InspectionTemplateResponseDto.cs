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
        /// Spanish catalog name, stable across languages (the apps key icons by it).
        public string Key { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<InspectionTemplateItemResponseDto> Items { get; set; } = new();
    }

    public class InspectionTemplateItemResponseDto
    {
        /// Spanish catalog name, stable across languages (the apps key icons by it).
        public string Key { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool AllowNotApplicable { get; set; }
    }
}
