using MiTaller.Models.Inspections;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;

namespace MiTaller.Services.Documents
{
    /// <summary>
    /// QuestPDF document for the generic (template-driven) inspection system.
    /// Unlike VehicleInspectionDocument/MotocycleInspectionDocument, this one
    /// doesn't hardcode sections/items: it iterates whatever the associated
    /// InspectionTemplate defines, so it works for Yate today and any future
    /// format (avión, especializados, ...) without changes.
    /// </summary>
    public class GenericInspectionDocument : IDocument
    {
        private readonly Inspection _inspection;
        private readonly InspectionTemplate _template;

        public GenericInspectionDocument(Inspection inspection, InspectionTemplate template)
        {
            _inspection = inspection;
            _template = template;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style.FontColor(InspectionDocumentComponents.Ink));

                page.Header().Element(c => InspectionDocumentComponents.Header(
                    c,
                    _template.Name,
                    _inspection.Workshop?.WorkshopName ?? string.Empty,
                    _inspection.Id.ToString(),
                    _inspection.InspectionDate));

                page.Content().PaddingHorizontal(32).PaddingVertical(20).Column(col =>
                {
                    col.Spacing(18);

                    col.Item().Element(RenderVehicleInfo);
                    col.Item().Element(RenderClientInfo);
                    col.Item().Element(RenderWorkshopInfo);

                    foreach (var section in _template.Sections.OrderBy(s => s.Order))
                    {
                        col.Item().Element(c => RenderSection(c, section));
                    }

                    col.Item().Element(RenderGeneralObservations);
                    col.Item().Element(InspectionDocumentComponents.Disclaimer);
                });

                page.Footer().PaddingHorizontal(32).PaddingBottom(16)
                    .Element(InspectionDocumentComponents.Footer);
            });
        }

        private void RenderVehicleInfo(IContainer container)
        {
            var vehicle = _inspection.Vehicle;
            InspectionDocumentComponents.InfoCard(container, "Información de la embarcación/vehículo", new (string, string)[]
            {
                ("Placas/Matrícula", vehicle?.Plates ?? string.Empty),
                ("Año", vehicle?.Year ?? string.Empty),
                ("Marca", vehicle?.BrandId == -1 ? vehicle?.OtherBrand ?? string.Empty : vehicle?.Brand?.Name ?? string.Empty),
                ("Modelo", vehicle?.VehicleModelId == -1 ? vehicle?.OtherVehicleModel ?? string.Empty : vehicle?.VehicleModel?.Model ?? string.Empty),
                ("Formato", vehicle?.VehicleFormat ?? string.Empty),
            });
        }

        private void RenderClientInfo(IContainer container) =>
            InspectionDocumentComponents.InfoCard(container, "Información del Cliente", new (string, string)[]
            {
                ("Nombre", _inspection.Customer?.FullName ?? string.Empty),
                ("Número de Teléfono", _inspection.Customer?.NormalizedPhoneNumber ?? string.Empty),
            });

        private void RenderWorkshopInfo(IContainer container) =>
            InspectionDocumentComponents.InfoCard(container, "Información del Taller", new (string, string)[]
            {
                ("Nombre", _inspection.Workshop?.WorkshopName ?? string.Empty),
                ("Número de Teléfono", _inspection.Workshop?.NormalizedPhoneNumber ?? string.Empty),
            });

        private void RenderSection(IContainer container, InspectionTemplateSection section)
        {
            var items = section.Items.OrderBy(i => i.Order).ToList();
            var comment = _inspection.SectionComments
                .FirstOrDefault(c => c.SectionId == section.Id)?.Comment;

            container.PreventPageBreak().Column(col =>
            {
                col.Item().Element(c => InspectionDocumentComponents.SectionTitle(c, section.Name));

                col.Item().PaddingTop(8).Border(1).BorderColor(InspectionDocumentComponents.Border).Column(listCol =>
                {
                    for (var i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        var value = _inspection.Results
                            .FirstOrDefault(r => r.TemplateItemId == item.Id)?.Value
                            ?? InspectionItemValue.Optimal;

                        listCol.Item()
                            .Background(i % 2 == 0 ? Colors.White : InspectionDocumentComponents.Surface)
                            .Padding(8)
                            .Row(row =>
                            {
                                row.RelativeItem(2).AlignMiddle().Text(item.Name)
                                    .FontSize(9.5f).FontColor(InspectionDocumentComponents.Ink);
                                row.RelativeItem(1).Row(badgeRow =>
                                {
                                    badgeRow.RelativeItem();
                                    badgeRow.AutoItem().Element(c => ConditionBadge(c, value));
                                });
                            });
                    }
                });

                if (!string.IsNullOrWhiteSpace(comment))
                {
                    col.Item().PaddingTop(8).Element(c => InspectionDocumentComponents.NoteBox(c, comment!));
                }
            });
        }

        private void RenderGeneralObservations(IContainer container)
        {
            if (string.IsNullOrWhiteSpace(_inspection.GeneralObservations)) return;

            container.Column(col =>
            {
                col.Item().Element(c => InspectionDocumentComponents.SectionTitle(c, "Observaciones generales"));
                col.Item().PaddingTop(8)
                    .Element(c => InspectionDocumentComponents.NoteBox(c, _inspection.GeneralObservations!));
            });
        }

        private static void ConditionBadge(IContainer container, InspectionItemValue value)
        {
            var (background, foreground, border, text) = value switch
            {
                InspectionItemValue.Optimal => ("#E8F5E9", "#2E7D32", "#A5D6A7", "Óptimo"),
                InspectionItemValue.NeedsAttention => ("#FFF8E1", "#B78103", "#FFE082", "Requiere atención"),
                InspectionItemValue.UrgentAttention => ("#FFEBEE", "#C62828", "#EF9A9A", "Atención urgente"),
                InspectionItemValue.NotApplicable => ("#F5F5F5", "#616161", "#E0E0E0", "No aplica"),
                _ => ("#F5F5F5", "#616161", "#E0E0E0", "-"),
            };

            container
                .Background(background)
                .Border(1).BorderColor(border)
                .PaddingHorizontal(8).PaddingVertical(3)
                .Text(text).FontSize(8).Bold().FontColor(foreground);
        }
    }
}
