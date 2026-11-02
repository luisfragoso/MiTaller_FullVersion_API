namespace MiTaller.Models.Domain
{
    /// <summary>
    /// Quién capturó un cliente o vehículo: el propio cliente o un taller (responsable de esos datos ante la LFPDPPP).
    /// Unknown = registros anteriores a que se guardara el origen.
    /// </summary>
    public enum RecordOrigin
    {
        Unknown = 0,
        Self = 1,
        Workshop = 2,
        Admin = 3,
    }
}
