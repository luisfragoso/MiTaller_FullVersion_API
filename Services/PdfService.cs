using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MiTaller.Services
{
    
    public class SampleDocument : IDocument
    {
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Content()
                    .Column(col =>
                    {
                        col.Item().Text("Ejemplo de PDF generado con QuestPDF").FontSize(20).Bold();
                        col.Item().Text("Este documento fue generado desde un endpoint en .NET.");
                    });
            });
        }
    }
}
