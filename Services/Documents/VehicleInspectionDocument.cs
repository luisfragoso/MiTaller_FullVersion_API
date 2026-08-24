using MiTaller.DTO.Inspections;
using MiTaller.Helpers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MiTaller.Models.Vehicle;

namespace MiTaller.Services.Documents
{
    public class VehicleInspectionDocument : IDocument
    {
        private readonly VehicleInspectionDocumentDto _inspection;

        public VehicleInspectionDocument(VehicleInspectionDocumentDto inspection)
        {
            _inspection = inspection;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Content().Column(col =>
                {
                    col.Item().Text("Reporte de Inspección").FontSize(20).Bold().AlignCenter();
                    col.Item().PaddingVertical(10).LineHorizontal(1);
                    col.Item().Text($"Folio: {_inspection.Folio} | Fecha: {_inspection.InspectionDate:dd/MM/yyyy}").Italic();
                    col.Item().PaddingVertical(5).LineHorizontal(1);

                    col.Item().Element(c => RenderAditionalInfo(c));
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Element(c => RenderClientInfo(c));
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Element(c => RenderWorkshopInfo(c));
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Element(c => RenderBrakesAndTires(c));
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Element(c => RenderInteriorExterior(c));
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Element(c => RenderEngine(c));
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Element(c => RenderBattery(c));
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Element(c => RenderChasis(c));
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Element(c => RenderPhotos(c));
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Element(c => RenderObservations(c));
                });
            });
        }

        private void RenderBrakesAndTires(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Frenos y Neumáticos").FontSize(14).Bold();

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); // Nombre
                        c.RelativeColumn(1); // Estado
                    });

                    void Add(string nombre, VehicleCondition cond)
                    {
                        table.Cell().Text(nombre);
                        table.Cell().Background(cond.ToColor()).Padding(4).Text(cond.ToText());
                    }

                    Add("Freno Delantero Derecho", _inspection.FrontRightBrake);
                    Add("Dibujo Llanta Delantera Derecha", _inspection.FrontRightTireTread);
                    Add("Alineación Delantera Derecha", _inspection.FrontRightTireAlignment);
                    Add("Freno Delantero Izquierdo", _inspection.FrontLeftBrake);
                    Add("Dibujo Llanta Delantera Izquierda", _inspection.FrontLeftTireTread);
                    Add("Alineación Delantera Izquierda", _inspection.FrontLeftTireAlignment);
                    Add("Freno Trasero Derecho", _inspection.RearRightBrake);
                    Add("Dibujo Llanta Trasera Derecha", _inspection.RearRightTireTread);
                    Add("Alineación Trasera Derecha", _inspection.RearRightTireAlignment);
                    Add("Freno Trasero Izquierdo", _inspection.RearLeftBrake);
                    Add("Dibujo Llanta Trasera Izquierda", _inspection.RearLeftTireTread);
                    Add("Alineación Trasera Izquierda", _inspection.RearLeftTireAlignment);
                });

                if (!string.IsNullOrWhiteSpace(_inspection.TiresComments))
                {
                    col.Item().PaddingTop(5).Text($"Comentarios: {_inspection.TiresComments}").Italic();
                }
            });
        }

        private void RenderInteriorExterior(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Interior y Exterior").FontSize(14).Bold();

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); // Nombre
                        c.RelativeColumn(1); // Estado
                    });

                    void Add(string nombre, VehicleCondition cond)
                    {
                        table.Cell().Text(nombre);
                        table.Cell().Background(cond.ToColor()).Padding(4).Text(cond.ToText());
                    }

                    Add("Frenos", _inspection.Brakes);
                    Add("Dibujo de llanta", _inspection.TireTread);
                    Add("Alineación", _inspection.TireAlignment);
                    Add("Luces delanteras", _inspection.Headlights);
                    Add("Luces traseras", _inspection.Taillights);
                    Add("Señales de giro", _inspection.TurnSignals);
                    Add("Luces de freno", _inspection.BrakeLights);
                    Add("Luces de advertencia de peligro", _inspection.HazardLights);
                    Add("Líquido del limpia parabrisa", _inspection.WindshieldWasherFluid);
                    Add("Funcionamiento del limpia parabrisa", _inspection.WindshieldWiperOperation);
                    Add("Escobillas del limpia parabrisa", _inspection.WindshieldWiperBlades);
                    Add("Condición del limpia parabrisa", _inspection.WindshieldCondition);
                    Add("Espejos", _inspection.Mirrors);
                    Add("Freno de emergencia", _inspection.EmergencyBrake);
                    Add("Claxon", _inspection.Horn);
                    Add("Tapa de tanque de combustible", _inspection.FuelTankCap);
                    Add("Filtro de aire acondicionado", _inspection.AirConditioningFilter);
                    Add("Luces de marcha atrás", _inspection.ReversingLights);
                });

                if (!string.IsNullOrWhiteSpace(_inspection.InteriorAndExteriorComments))
                {
                    col.Item().PaddingTop(5).Text($"Comentarios: {_inspection.InteriorAndExteriorComments}").Italic();
                }
            });
        }

        private void RenderEngine(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Motor").FontSize(14).Bold();

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); // Nombre
                        c.RelativeColumn(1); // Estado
                    });

                    void Add(string nombre, VehicleCondition cond)
                    {
                        table.Cell().Text(nombre);
                        table.Cell().Background(cond.ToColor()).Padding(4).Text(cond.ToText());
                    }

                    Add("Niveles de líquido aceite", _inspection.EngineOilLevel);
                    Add("Líquido refrigerante", _inspection.CoolantLevel);
                    Add("Líquido de frenos", _inspection.BrakeFluidLevel);
                    Add("Filtro de aire", _inspection.AirFilter);
                    Add("Mangueras del sistema de refrigeración", _inspection.RadiatorHoses);
                    Add("angueras de calefacción", _inspection.HeatingHoses);
                    Add("Condensador de aire acondicionado", _inspection.AirConditioningCondenser);
                });

                if (!string.IsNullOrWhiteSpace(_inspection.EngineComments))
                {
                    col.Item().PaddingTop(5).Text($"Comentarios: {_inspection.EngineComments}").Italic();
                }
            });
        }

        private void RenderBattery(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Batería").FontSize(14).Bold();

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); // Nombre
                        c.RelativeColumn(1); // Estado
                    });

                    void Add(string nombre, VehicleCondition cond)
                    {
                        table.Cell().Text(nombre);
                        table.Cell().Background(cond.ToColor()).Padding(4).Text(cond.ToText());
                    }

                    Add("Terminales de batería", _inspection.BatteryTerminals);
                    Add("Cables", _inspection.BatteryCables);
                    Add("Montaje", _inspection.BatteryMounting);
                    Add("Condiciones generales de la batería", _inspection.GeneralBatteryCondition);
                });

                if (!string.IsNullOrWhiteSpace(_inspection.BatteryComments))
                {
                    col.Item().PaddingTop(5).Text($"Comentarios: {_inspection.BatteryComments}").Italic();
                }
            });
        }

        private void RenderChasis(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Chasis").FontSize(14).Bold();

                if (!string.IsNullOrWhiteSpace(_inspection.ChasisComments))
                {
                    col.Item().PaddingTop(5).Text($"Comentarios: {_inspection.ChasisComments}").Italic();
                }
            });
        }

        private void RenderObservations(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Observaciones").FontSize(14).Bold();

                if (!string.IsNullOrWhiteSpace(_inspection.Observations))
                {
                    col.Item().PaddingTop(5).Text($"Comentarios: {_inspection.Observations}").Italic();
                }
            });
        }

        private void RenderPhotos(IContainer container)
        {
            if (_inspection.Photos == null || !_inspection.Photos.Any())
                return;

            container.Column(col =>
            {
                col.Item().Text("Fotos del vehículo").FontSize(14).Bold();
                col.Item().PaddingBottom(5);

                // Agrupar de 2 en 2
                var photoPairs = _inspection.Photos
                    .Where(p => p.FileType.StartsWith("image"))
                    .Select((photo, index) => new { photo, index })
                    .GroupBy(x => x.index / 2)
                    .Select(g => g.Select(x => x.photo).ToList())
                    .ToList();

                foreach (var pair in photoPairs)
                {
                    col.Item().Row(row =>
                    {
                        foreach (var photo in pair)
                        {
                            row.RelativeItem().Column(photoCol =>
                            {
                                photoCol.Item().Height(150).Image(photo.FileData, ImageScaling.FitArea);
                                //photoCol.Item().AlignCenter().Text(photo.FileName).FontSize(10).Italic();
                            });
                        }

                        // Si hay una sola imagen, agregamos una columna vacía para balancear
                        if (pair.Count == 1)
                            row.RelativeItem();
                    });

                    col.Item().PaddingBottom(10);
                }
            });
        }


        private void RenderAditionalInfo(IContainer container)
        {
            container.Column(col =>
            {
                if (_inspection.Vehicle.VehicleFormat == "Automóvil")
                {
                    col.Item().Text("Información del vehículo").FontSize(14).Bold();
                }

                if (_inspection.Vehicle.VehicleFormat == "Motocicleta")
                {
                    col.Item().Text("Información de la motocicleta").FontSize(14).Bold();
                }

                col.Item().Text(text =>
                {
                    text.Span($"Placas: ").FontSize(14).Medium();
                    text.Span($"{_inspection.Vehicle.Plates}");
                });

                col.Item().Text(text =>
                {
                    text.Span($"Año: ").FontSize(14).Medium();
                    text.Span($"{_inspection.Vehicle.Year}");
                });

                col.Item().Text(text =>
                {
                    text.Span($"Marca: ").FontSize(14).Medium();
                    text.Span($"{_inspection.Vehicle.Brand}");
                });


                col.Item().Text(text =>
                {
                    text.Span($"Modelo: ").FontSize(14).Medium();
                    text.Span($"{_inspection.Vehicle.Model}");
                });

                col.Item().Text(text =>
                {
                    text.Span($"Versión: ").FontSize(14).Medium();
                    text.Span($"{_inspection.Vehicle.Version}");
                });

                col.Item().Text(text =>
                {
                    text.Span($"Tipo: ").FontSize(14).Medium();
                    text.Span($"{_inspection.Vehicle.Type}");
                });

            });
        }

        private void RenderClientInfo(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Información del Cliente").FontSize(14).Bold();


                col.Item().Text(text =>
                {
                    text.Span($"Nombre: ").FontSize(14).Medium();
                    text.Span($"{_inspection.CustomerName}");
                });

                col.Item().Text(text =>
                {
                    text.Span($"Número de Teléfono: ").FontSize(14).Medium();
                    text.Span($"{_inspection.CustomerPhoneNumber}");
                });

                col.Item().Text(text =>
                {
                    text.Span($"Número de Cliente: ").FontSize(14).Medium();
                    text.Span($"{_inspection.CustomerGuid}");
                });

            });
        }

        private void RenderWorkshopInfo(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Información del Taller").FontSize(14).Bold();

                col.Item().Text(text =>
                {
                    text.Span($"Nombre: ").FontSize(14).Medium();
                    text.Span($"{_inspection.WorkshopName}");
                });

                col.Item().Text(text =>
                {
                    text.Span($"Número de Teléfono: ").FontSize(14).Medium();
                    text.Span($"{_inspection.WorkshopPhoneNumber}");
                });

                col.Item().Text(text =>
                {
                    text.Span($"Número de Taller: ").FontSize(14).Medium();
                    text.Span($"{_inspection.WorkshopGuid}");
                });

            });
        }
    }
}
