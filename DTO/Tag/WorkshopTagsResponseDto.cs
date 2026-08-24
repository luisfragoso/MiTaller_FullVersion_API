namespace MiTaller.DTO.Tag
{
    public class WorkshopTagsResponseDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HexColor { get; set; } = string.Empty;
        public int? AssignedCount { get; set; }
    }
}
