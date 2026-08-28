using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiTaller.Models.Audit;

namespace MiTaller.Services.Audit
{
    // Registra automáticamente en AuditLog cualquier cambio (alta/edición/baja) en
    // cualquier entidad de DataContext, sin que cada controlador tenga que hacerlo a
    // mano. Ver Models/Audit/INotAudited.cs para excluir una entidad completa.
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Columnas que nunca deben aparecer como texto en el log, aunque cambien.
        private static readonly HashSet<string> ExcludedPropertyNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "PasswordResetCode",
            "PasswordResetCodeExpires",
            "EmailVerificationCode",
            "EmailVerificationExpires",
            "NormalizedEmail",
            "NormalizedUserName",
            "NormalizedPhoneNumber",
            "AccessFailedCount",
            "LockoutEnd",
            // Cambia en cada login - es ruido de actividad rutinaria, no un
            // cambio de datos digno de auditoría. Se muestra aparte como
            // "Última conexión" en el detalle del usuario.
            "LastLoginAt",
        };

        // Filas de auditoría de entidades nuevas cuyo EntityId todavía no se conocía
        // en SavingChanges (PK autogenerado por la BD) - se completan en SavedChanges.
        private readonly List<(AuditLog Log, EntityEntry Entry)> _pendingAddedFixups = new();

        public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            CollectAuditEntries(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            CollectAuditEntries(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            ResolvePendingFixups(eventData.Context);
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            await ResolvePendingFixupsAsync(eventData.Context, cancellationToken);
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void CollectAuditEntries(DbContext? context)
        {
            if (context == null) return;

            var changedByUserId = GetCurrentUserId();
            var now = DateTime.Now;
            var newEntries = new List<AuditLog>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is INotAudited) continue;

                if (entry.State != EntityState.Added &&
                    entry.State != EntityState.Modified &&
                    entry.State != EntityState.Deleted)
                {
                    continue;
                }

                var entityName = entry.Metadata.ClrType.Name;
                var changeType = entry.State switch
                {
                    EntityState.Added => AuditChangeType.Added,
                    EntityState.Deleted => AuditChangeType.Deleted,
                    _ => AuditChangeType.Modified,
                };

                // Si la PK todavía no existe (autogenerada por la BD en un insert),
                // se resuelve después en SavedChanges; si ya se conoce, se usa directo.
                var entityId = entry.State == EntityState.Added && !entry.IsKeySet
                    ? null
                    : GetPrimaryKeyValue(entry);

                var primaryKeyProperties = entry.Metadata.FindPrimaryKey()?.Properties
                    ?? Enumerable.Empty<Microsoft.EntityFrameworkCore.Metadata.IProperty>();

                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(byte[])) continue;
                    if (ExcludedPropertyNames.Contains(property.Metadata.Name)) continue;
                    // La propia PK ya queda registrada en EntityId; además, en un insert
                    // con clave autogenerada su valor real todavía no existe aquí.
                    if (primaryKeyProperties.Contains(property.Metadata)) continue;

                    string? oldValue = null;
                    string? newValue = null;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValue = FormatValue(property.CurrentValue);
                            if (newValue == null) continue;
                            break;
                        case EntityState.Deleted:
                            oldValue = FormatValue(property.OriginalValue);
                            if (oldValue == null) continue;
                            break;
                        default: // Modified
                            if (!property.IsModified) continue;
                            oldValue = FormatValue(property.OriginalValue);
                            newValue = FormatValue(property.CurrentValue);
                            if (oldValue == newValue) continue;
                            break;
                    }

                    var log = new AuditLog
                    {
                        EntityName = entityName,
                        EntityId = entityId ?? string.Empty,
                        PropertyName = property.Metadata.Name,
                        OldValue = oldValue,
                        NewValue = newValue,
                        ChangeType = changeType,
                        ChangedAt = now,
                        ChangedByUserId = changedByUserId,
                    };

                    newEntries.Add(log);

                    if (entityId == null)
                    {
                        _pendingAddedFixups.Add((log, entry));
                    }
                }
            }

            if (newEntries.Count > 0)
            {
                context.Set<AuditLog>().AddRange(newEntries);
            }
        }

        private void ResolvePendingFixups(DbContext? context)
        {
            if (context == null || _pendingAddedFixups.Count == 0) return;

            foreach (var (log, entry) in _pendingAddedFixups)
            {
                log.EntityId = GetPrimaryKeyValue(entry);
            }
            _pendingAddedFixups.Clear();

            // Segunda escritura, pequeña, solo para las filas de auditoría afectadas.
            // Las entidades originales ya quedaron en estado Unchanged, así que este
            // SaveChanges no vuelve a generar diffs de negocio - solo persiste el
            // EntityId ya resuelto (AuditLog está marcado INotAudited).
            context.SaveChanges();
        }

        private async Task ResolvePendingFixupsAsync(DbContext? context, CancellationToken cancellationToken)
        {
            if (context == null || _pendingAddedFixups.Count == 0) return;

            foreach (var (log, entry) in _pendingAddedFixups)
            {
                log.EntityId = GetPrimaryKeyValue(entry);
            }
            _pendingAddedFixups.Clear();

            await context.SaveChangesAsync(cancellationToken);
        }

        private static string GetPrimaryKeyValue(EntityEntry entry)
        {
            var key = entry.Metadata.FindPrimaryKey();
            if (key == null) return string.Empty;

            var values = key.Properties.Select(p =>
                entry.Property(p.Name).CurrentValue?.ToString() ?? string.Empty);

            return string.Join(",", values);
        }

        private static string? FormatValue(object? value)
        {
            if (value == null) return null;
            if (value is DateTime dt) return dt.ToString("O");
            if (value is DateTimeOffset dto) return dto.ToString("O");
            return value.ToString();
        }

        private Guid? GetCurrentUserId()
        {
            var idClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : null;
        }
    }
}
