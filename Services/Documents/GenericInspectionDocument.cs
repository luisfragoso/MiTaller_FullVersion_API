using MiTaller.DTO.Inspections;
using MiTaller.Models.Inspections;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;

using MiTaller.Services.Localization;

namespace MiTaller.Services.Documents
{
    /// <summary>
    /// QuestPDF document for template-driven inspections: renders whatever sections
    /// the template defines, so it serves every vehicle format.
    /// </summary>
    public class GenericInspectionDocument : IDocument
    {
        private readonly Inspection _inspection;
        private readonly InspectionTemplate _template;
        private readonly List<WorkshopVehicleFileDto> _files;
        private readonly string _lang;

        public GenericInspectionDocument(Inspection inspection, InspectionTemplate template, string lang = Lang.Es)
        {
            _inspection = inspection;
            _template = template;
            _lang = lang;
            _files = inspection.Files
                .Select(f => new WorkshopVehicleFileDto { FileName = f.FileName, FileType = f.FileType, FileData = f.FileData })
                .ToList();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        private static readonly HashSet<string> Watercraft = new() { "Yate", "Lancha", "Bote" };

        // Yate, Lancha and Bote share one template: title it after the actual vessel
        private string DocumentTitle()
        {
            var format = _inspection.Vehicle?.VehicleFormat;
            return format != null && format != _template.VehicleFormat
                   && Watercraft.Contains(format) && Watercraft.Contains(_template.VehicleFormat ?? string.Empty)
                ? T.Get(_lang, "pdf.inspectionOf", format.ToLower())
                : _template.Name;
        }

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
                    DocumentTitle(),
                    _inspection.Workshop?.WorkshopName ?? string.Empty,
                    _inspection.Folio ?? _inspection.Id.ToString(),
                    _inspection.InspectionDate,
                    _lang));

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

                    col.Item().Element(c => InspectionDocumentComponents.PhotosSection(c, _files, _lang));
                    col.Item().Element(RenderGeneralObservations);
                    col.Item().Element(c => InspectionDocumentComponents.Disclaimer(c, _lang));
                });

                page.Footer().PaddingHorizontal(32).PaddingBottom(16)
                    .Element(c => InspectionDocumentComponents.Footer(c, _lang));
            });
        }

        private void RenderVehicleInfo(IContainer container)
        {
            var vehicle = _inspection.Vehicle;
            var title = vehicle?.VehicleFormat switch
            {
                "Motocicleta" => T.Get(_lang, "pdf.info.motorcycle"),
                "Yate" or "Lancha" or "Bote" => T.Get(_lang, "pdf.info.watercraft"),
                "Avión" => T.Get(_lang, "pdf.info.aircraft"),
                _ => T.Get(_lang, "pdf.info.vehicle"),
            };

            InspectionDocumentComponents.InfoCard(container, title, new (string, string)[]
            {
                (T.Get(_lang, "pdf.plates"), vehicle?.Plates ?? string.Empty),
                (T.Get(_lang, "pdf.year"), vehicle?.Year ?? string.Empty),
                (T.Get(_lang, "pdf.make"), vehicle?.BrandId == -1 ? vehicle?.OtherBrand ?? string.Empty : vehicle?.Brand?.Name ?? string.Empty),
                (T.Get(_lang, "pdf.model"), vehicle?.VehicleModelId == -1 ? vehicle?.OtherVehicleModel ?? string.Empty : vehicle?.VehicleModel?.Model ?? string.Empty),
                (T.Get(_lang, "pdf.trim"), vehicle?.VehicleVersionId == -1 ? vehicle?.OtherVehicleVersion ?? string.Empty : vehicle?.VehicleVersion?.Version ?? string.Empty),
                (T.Get(_lang, "pdf.type"), vehicle?.VehicleTypeId == -1 ? vehicle?.OtherVehicleType ?? string.Empty : vehicle?.VehicleType?.Type ?? string.Empty),
            });
        }

        // Same short id shown on the legacy Auto/Moto reports
        private static string ShortId(Guid? id) => id?.ToString().Substring(0, 13) ?? string.Empty;

        private void RenderClientInfo(IContainer container) =>
            InspectionDocumentComponents.InfoCard(container, T.Get(_lang, "pdf.customerInfo"), new (string, string)[]
            {
                (T.Get(_lang, "pdf.name"), _inspection.Customer?.FullName ?? string.Empty),
                (T.Get(_lang, "pdf.phone"), _inspection.Customer?.NormalizedPhoneNumber ?? string.Empty),
                (T.Get(_lang, "pdf.customerNumber"), ShortId(_inspection.Customer?.Id)),
            });

        private void RenderWorkshopInfo(IContainer container) =>
            InspectionDocumentComponents.InfoCard(container, T.Get(_lang, "pdf.shopInfo"), new (string, string)[]
            {
                (T.Get(_lang, "pdf.name"), _inspection.Workshop?.WorkshopName ?? string.Empty),
                (T.Get(_lang, "pdf.phone"), _inspection.Workshop?.NormalizedPhoneNumber ?? string.Empty),
                (T.Get(_lang, "pdf.shopNumber"), ShortId(_inspection.Workshop?.Id)),
            });

        private void RenderSection(IContainer container, InspectionTemplateSection section)
        {
            var items = section.Items.OrderBy(i => i.Order).ToList();
            var comment = _inspection.SectionComments
                .FirstOrDefault(c => c.SectionId == section.Id)?.Comment;

            // An item-less section is the Chasis one: it shows the marked-up diagram instead of a checklist
            var diagram = items.Count == 0
                ? _files.FirstOrDefault(InspectionDocumentComponents.IsChasisDiagram)
                : null;
            if (items.Count == 0 && diagram == null && string.IsNullOrWhiteSpace(comment)) return;

            container.PreventPageBreak().Column(col =>
            {
                col.Item().Element(c => InspectionDocumentComponents.SectionTitle(c, section.Name));

                if (diagram != null)
                {
                    col.Item().PaddingTop(8).Element(c => InspectionDocumentComponents.ChasisDiagram(c, diagram));
                }

                if (items.Count > 0)
                {
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
                }

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
                col.Item().Element(c => InspectionDocumentComponents.SectionTitle(c, T.Get(_lang, "pdf.generalObservations")));
                col.Item().PaddingTop(8)
                    .Element(c => InspectionDocumentComponents.NoteBox(c, _inspection.GeneralObservations!));
            });
        }

        private void ConditionBadge(IContainer container, InspectionItemValue value)
        {
            var (background, foreground, border, text) = value switch
            {
                InspectionItemValue.Optimal => ("#E8F5E9", "#2E7D32", "#A5D6A7", T.Get(_lang, "pdf.optimal")),
                InspectionItemValue.NeedsAttention => ("#FFF8E1", "#B78103", "#FFE082", T.Get(_lang, "pdf.needsAttention")),
                InspectionItemValue.UrgentAttention => ("#FFEBEE", "#C62828", "#EF9A9A", T.Get(_lang, "pdf.urgent")),
                InspectionItemValue.NotApplicable => ("#F5F5F5", "#616161", "#E0E0E0", T.Get(_lang, "pdf.notApplicable")),
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
