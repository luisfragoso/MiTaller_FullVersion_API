using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.Models.Workshop;

namespace MiTaller.Services
{
    public static class ExpenseCategoryService
    {
        public const int MaxNameLength = 40;

        public static readonly string[] Defaults =
            { "Renta", "Servicios", "Refacciones", "Salarios", "Equipo", "Otro" };

        /// <summary>Categorías del taller; la primera vez le crea las de siempre para que no empiece en blanco.</summary>
        public static async Task<List<WorkshopExpenseCategory>> EnsureForWorkshopAsync(DataContext db, Guid workshopId)
        {
            var categories = await db.WorkshopExpenseCategories
                .Where(c => c.WorkshopId == workshopId)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
                .ToListAsync();
            if (categories.Count > 0) return categories;

            categories = Defaults
                .Select((name, i) => new WorkshopExpenseCategory { WorkshopId = workshopId, Name = name, SortOrder = i })
                .ToList();
            db.WorkshopExpenseCategories.AddRange(categories);
            await db.SaveChangesAsync();
            return categories;
        }

        /// <summary>La categoría tal como la tiene el taller (ocultas incluidas, para no romper gastos viejos), o "Otro".</summary>
        public static async Task<string> NormalizeAsync(DataContext db, Guid workshopId, string? category)
        {
            var name = category?.Trim();
            if (string.IsNullOrEmpty(name)) return "Otro";
            var categories = await EnsureForWorkshopAsync(db, workshopId);
            return categories.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase))?.Name
                ?? "Otro";
        }
    }
}
