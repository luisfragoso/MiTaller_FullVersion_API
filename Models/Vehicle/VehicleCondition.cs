namespace MiTaller.Models.Vehicle
{
    public enum VehicleCondition
    {
        Optimal = 0,         // Estado óptimo (Verde)
        NeedsAttention = 1,  // Requiere atención (Amarillo)
        UrgentAttention = 2  // Atención inmediata (Rojo)
    }
}
