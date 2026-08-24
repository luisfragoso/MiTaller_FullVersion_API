namespace MiTaller.Models.Domain
{
    public class Brand
    {
        public int Id { get; set; } // -1 = Other
        public string Type { get; set; } = string.Empty; // Automovil - Motocicleta
        public string Name { get; set; } = string.Empty;
    }
}
