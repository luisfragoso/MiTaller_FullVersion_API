namespace MiTaller.Models.Settings
{
    /// <summary>Single row (Id 1): public contact addresses, editable from the Portal Admin.</summary>
    public class ContactSettings
    {
        public int Id { get; set; }
        public string InfoEmail { get; set; } = "";
        public string SupportEmail { get; set; } = "";
        public DateTime? UpdatedAt { get; set; }
    }
}
