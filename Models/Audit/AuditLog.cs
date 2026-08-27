using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Audit
{
    public enum AuditChangeType
    {
        Added,
        Modified,
        Deleted
    }

    // Registro genérico de "quién cambió qué" en cualquier tabla de la plataforma.
    // Se llena automáticamente vía AuditSaveChangesInterceptor - nada la escribe a mano.
    public class AuditLog : INotAudited
    {
        [Key]
        public long Id { get; set; }

        public string EntityName { get; set; } = string.Empty;

        // String en vez de Guid/int porque las entidades auditadas mezclan ambos tipos de PK.
        public string EntityId { get; set; } = string.Empty;

        public string PropertyName { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public AuditChangeType ChangeType { get; set; }

        public DateTime ChangedAt { get; set; }

        // Null cuando el cambio no vino de una request autenticada (seeding, jobs futuros).
        public Guid? ChangedByUserId { get; set; }
    }
}
