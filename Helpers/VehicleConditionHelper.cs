using MiTaller.Models.Vehicle;

namespace MiTaller.Helpers
{
    public static class VehicleConditionHelper
    {
        public static string ToText(this VehicleCondition condition) => condition switch
        {
            VehicleCondition.Optimal => "Estado óptimo",
            VehicleCondition.NeedsAttention => "Requiere atención",
            VehicleCondition.UrgentAttention => "Atención inmediata",
            _ => "Desconocido"
        };

        public static string ToColor(this VehicleCondition condition) => condition switch
        {
            VehicleCondition.Optimal => QuestPDF.Helpers.Colors.Green.Lighten2,
            VehicleCondition.NeedsAttention => QuestPDF.Helpers.Colors.Yellow.Lighten2,
            VehicleCondition.UrgentAttention => QuestPDF.Helpers.Colors.Red.Lighten2,
            _ => QuestPDF.Helpers.Colors.Grey.Lighten2
        };
    }
}
