namespace MiTaller.Models.Audit
{
    // Marca una entidad para que AuditSaveChangesInterceptor la ignore por completo.
    // Úsalo en tablas que ya son de solo-auditoría/historial por diseño (evita
    // auditar un historial), o en tablas de alto volumen sin valor de auditoría.
    public interface INotAudited
    {
    }
}
