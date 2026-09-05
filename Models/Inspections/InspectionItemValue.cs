namespace MiTaller.Models.Inspections
{
    /// <summary>
    /// Result value for a single item inside a generic (configurable) inspection.
    /// This is a separate enum from Models.Vehicle.VehicleCondition (used by the
    /// legacy car/moto hardcoded inspections) because it adds a 4th value,
    /// NotApplicable, needed by templates like the yacht one (e.g. "Mástil" only
    /// applies to sailing yachts). Keeping it separate avoids touching the
    /// existing 3-value enum that car/moto already rely on.
    /// </summary>
    public enum InspectionItemValue
    {
        Optimal = 0,          // Estado óptimo (Verde)
        NeedsAttention = 1,   // Requiere atención (Amarillo)
        UrgentAttention = 2,  // Atención inmediata (Rojo)
        NotApplicable = 3     // No aplica para este vehículo/template
    }
}
