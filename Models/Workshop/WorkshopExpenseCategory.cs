using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Workshop
{
    /// <summary>Categoría de gasto propia de un taller; ocultarla la quita del alta sin tocar gastos viejos.</summary>
    public class WorkshopExpenseCategory
    {
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop Workshop { get; set; }
        [MaxLength(40)]
        public string Name { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
