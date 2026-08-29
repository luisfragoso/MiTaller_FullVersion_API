using MiTaller.DTO.Inspections;
using MiTaller.Helpers;
using MiTaller.Models.Vehicle;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MiTaller.Services.Documents
{
    /// <summary>
    /// Shared visual building blocks for the vehicle/motorcycle inspection
    /// PDFs, so both documents render with one consistent, on-brand design
    /// instead of duplicating layout code.
    /// </summary>
    public static class InspectionDocumentComponents
    {
        public const string BrandRed = "#F93232";
        public const string BrandRedDark = "#B8291F";
        public const string Ink = "#1F2430";
        public const string Muted = "#6B7280";
        public const string Surface = "#F6F7F9";
        public const string Border = "#E5E7EB";

        public static void Header(
            IContainer container,
            string documentTitle,
            string workshopName,
            string folio,
            DateTime date)
        {
            container
                .Background(BrandRed)
                .Padding(20)
                .Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("MiTaller").FontSize(20).Bold().FontColor(Colors.White);
                        col.Item().Text(workshopName).FontSize(9).FontColor(Colors.White);
                    });

                    row.ConstantItem(180).Column(col =>
                    {
                        col.Item().AlignRight().Text(documentTitle.ToUpper())
                            .FontSize(11).Bold().FontColor(Colors.White).LetterSpacing(0.05f);
                        col.Item().AlignRight().Text($"Folio: {folio}").FontSize(9).FontColor(Colors.White);
                        col.Item().AlignRight().Text($"{date:dd/MM/yyyy}").FontSize(9).FontColor(Colors.White);
                    });
                });
        }

        public static void Footer(IContainer container)
        {
            container
                .BorderTop(1)
                .BorderColor(Border)
                .PaddingTop(6)
                .Row(row =>
                {
                    row.RelativeItem().Text("Generado por MiTaller").FontSize(8).FontColor(Muted);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.DefaultTextStyle(style => style.FontSize(8).FontColor(Muted));
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
        }

        public static void SectionTitle(IContainer container, string title)
        {
            container.Row(row =>
            {
                row.AutoItem().Width(3).Height(14).Background(BrandRed);
                row.RelativeItem().PaddingLeft(8).AlignMiddle()
                    .Text(title).FontSize(13).Bold().FontColor(Ink);
            });
        }

        public static void InfoCard(
            IContainer container,
            string title,
            (string label, string value)[] fields,
            int columns = 3)
        {
            container.PreventPageBreak().Column(outer =>
            {
                outer.Item().Element(c => SectionTitle(c, title));

                outer.Item().PaddingTop(8)
                    .Background(Surface)
                    .BorderLeft(3).BorderColor(BrandRed)
                    .Padding(12)
                    .Column(col =>
                    {
                        foreach (var chunk in fields.Chunk(columns))
                        {
                            col.Item().PaddingBottom(8).Row(row =>
                            {
                                foreach (var (label, value) in chunk)
                                {
                                    row.RelativeItem().Column(fieldCol =>
                                    {
                                        fieldCol.Item().Text(label.ToUpper())
                                            .FontSize(7).Bold().FontColor(Muted).LetterSpacing(0.03f);
                                        fieldCol.Item().PaddingTop(1)
                                            .Text(string.IsNullOrWhiteSpace(value) ? "-" : value)
                                            .FontSize(10).Bold().FontColor(Ink);
                                    });
                                }

                                for (var i = chunk.Length; i < columns; i++)
                                    row.RelativeItem();
                            });
                        }
                    });
            });
        }

        public static void ConditionSection(
            IContainer container,
            string title,
            (string label, VehicleCondition condition)[] items,
            string? comments)
        {
            container.PreventPageBreak().Column(col =>
            {
                col.Item().Element(c => SectionTitle(c, title));

                col.Item().PaddingTop(8).Border(1).BorderColor(Border).Column(listCol =>
                {
                    for (var i = 0; i < items.Length; i++)
                    {
                        var (label, condition) = items[i];
                        listCol.Item()
                            .Background(i % 2 == 0 ? Colors.White : Surface)
                            .Padding(8)
                            .Row(row =>
                            {
                                row.RelativeItem(2).AlignMiddle().Text(label).FontSize(9.5f).FontColor(Ink);
                                row.RelativeItem(1).Row(badgeRow =>
                                {
                                    badgeRow.RelativeItem();
                                    badgeRow.AutoItem().Element(c => ConditionBadge(c, condition));
                                });
                            });
                    }
                });

                if (!string.IsNullOrWhiteSpace(comments))
                {
                    col.Item().PaddingTop(8).Element(c => NoteBox(c, comments));
                }
            });
        }

        private static void ConditionBadge(IContainer container, VehicleCondition condition)
        {
            var (background, foreground, border) = condition switch
            {
                VehicleCondition.Optimal => ("#E8F5E9", "#2E7D32", "#A5D6A7"),
                VehicleCondition.NeedsAttention => ("#FFF8E1", "#B78103", "#FFE082"),
                VehicleCondition.UrgentAttention => ("#FFEBEE", "#C62828", "#EF9A9A"),
                _ => ("#F5F5F5", "#616161", "#E0E0E0"),
            };

            container
                .Background(background)
                .Border(1).BorderColor(border)
                .PaddingHorizontal(8).PaddingVertical(3)
                .Text(condition.ToText()).FontSize(8).Bold().FontColor(foreground);
        }

        public static void NoteBox(IContainer container, string text)
        {
            container
                .Background(Surface)
                .BorderLeft(3).BorderColor(Border)
                .Padding(8)
                .Text($"Comentarios: {text}").FontSize(9).Italic().FontColor(Muted);
        }

        /// <summary>
        /// The chasis-diagram photo the client app uploads is just another
        /// row in the same Photos list, distinguishable only by its
        /// "chassis_" filename prefix (there's no dedicated field for it).
        /// </summary>
        public static bool IsChasisDiagram(WorkshopVehicleFileDto file) =>
            file.FileName.StartsWith("chassis_", StringComparison.OrdinalIgnoreCase);

        private static bool IsRenderableImage(WorkshopVehicleFileDto file)
        {
            if (file.FileType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                return true;

            // Some uploads were stored with a generic "application/octet-stream"
            // type, so fall back to the file extension instead of dropping them.
            var name = file.FileName.ToLowerInvariant();
            return name.EndsWith(".png") || name.EndsWith(".jpg") ||
                   name.EndsWith(".jpeg") || name.EndsWith(".webp");
        }

        public static void ChasisDiagram(IContainer container, WorkshopVehicleFileDto diagram)
        {
            // The diagram the app generates is a very wide, mostly-blank
            // canvas with a small drawing centered in it, so it needs a wide
            // box (not a square) to render at a readable size.
            container
                .Border(1).BorderColor(Border)
                .Padding(12)
                .MaxHeight(240)
                .AlignCenter()
                .Image(diagram.FileData, ImageScaling.FitArea);
        }

        public static void PhotosSection(IContainer container, IEnumerable<WorkshopVehicleFileDto> photos)
        {
            var images = photos.Where(p => !IsChasisDiagram(p) && IsRenderableImage(p)).ToList();
            if (images.Count == 0) return;

            container.PreventPageBreak().Column(col =>
            {
                col.Item().Element(c => SectionTitle(c, "Fotos del vehículo"));
                col.Item().PaddingTop(8);

                var pairs = images
                    .Select((photo, index) => (photo, index))
                    .GroupBy(x => x.index / 2)
                    .Select(g => g.Select(x => x.photo).ToList());

                foreach (var pair in pairs)
                {
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        foreach (var photo in pair)
                        {
                            row.RelativeItem().Border(1).BorderColor(Border).Padding(4)
                                .Height(150).Image(photo.FileData, ImageScaling.FitArea);
                        }

                        if (pair.Count == 1)
                            row.RelativeItem();
                    });
                }
            });
        }

        public static void Disclaimer(IContainer container)
        {
            container
                .Background(Surface)
                .Padding(10)
                .Text(
                    "Este reporte refleja el estado del vehículo únicamente al momento de la " +
                    "inspección y tiene fines informativos; no constituye una garantía ni un " +
                    "diagnóstico definitivo de fallas futuras.")
                .FontSize(8).FontColor(Muted).AlignCenter();
        }
    }
}
