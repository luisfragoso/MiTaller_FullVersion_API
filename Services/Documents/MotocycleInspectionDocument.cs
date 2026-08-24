using MiTaller.DTO.Inspections;
using MiTaller.Helpers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MiTaller.Models.Vehicle;

namespace MiTaller.Services.Documents
{
    public class MotocycleInspectionDocument : IDocument
    {
        private readonly MotocycleInspectionDocumentDto _inspection;

        public MotocycleInspectionDocument(MotocycleInspectionDocumentDto inspection)
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

                    col.Item().Element(c => RenderLightsAndControls(c));
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Element(c => RenderFrameAndSuspension(c));
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

                    // Frenos y llantas - Eje delantero
                    Add("Radios Delanteros", _inspection.FrontRadios);
                    Add("Dibujo de Llanta Delantera", _inspection.FrontTireThreadPattern);
                    Add("Rodamientos Delanteros", _inspection.FrontBearings);
                    Add("Sellos Delanteros", _inspection.FrontStamps);
                    Add("Forro de Freno Delantero", _inspection.FrontBrakeLining);
                    Add("Patrón de Desgaste Delantero", _inspection.FrontWearPattern);

                    // Frenos y llantas - Eje trasero
                    Add("Radios Traseros", _inspection.RearRadios);
                    Add("Dibujo de Llanta Trasera", _inspection.RearTireThreadPattern);
                    Add("Rodamientos Traseros", _inspection.RearBearings);
                    Add("Sellos Traseros", _inspection.RearStamps);
                    Add("Forro de Freno Trasero", _inspection.RearBrakeLining);
                    Add("Patrón de Desgaste Trasero", _inspection.RearWearPattern);
                });

                if (!string.IsNullOrWhiteSpace(_inspection.TiresComments))
                {
                    col.Item().PaddingTop(5).Text($"Comentarios: {_inspection.TiresComments}").Italic();
                }
            });
        }

        private void RenderLightsAndControls(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Luces y Controles").FontSize(14).Bold();

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

                    Add("Faro", _inspection.Headlight);
                    Add("Luz Trasera", _inspection.Taillight);
                    Add("Luces de Giro", _inspection.TurnSignals);
                    Add("Luces de Emergencia", _inspection.HazardLights);
                    Add("Luz de Freno", _inspection.Stoplight);
                    Add("Luz de Placa", _inspection.LicensePlateLight);
                    Add("Espejo Izquierdo", _inspection.LeftMirror);
                    Add("Espejo Derecho", _inspection.RightMirror);
                    Add("Interruptores", _inspection.Switches);
                    Add("Cableado", _inspection.Cabling);
                    Add("Manubrios", _inspection.HandleBars);
                    Add("Palancas y Pedal", _inspection.LeversAndPedal);
                    Add("Mangueras", _inspection.Hoses);
                    Add("Palanca de Acelerador", _inspection.ThrottleLever);
                    Add("Palanca de Clutch", _inspection.ClutchLever);
                    Add("Tapa de Tanque de Combustible", _inspection.FuelTankCap);
                    Add("Instrumentos de Tablero", _inspection.DashboardInstruments);
                    Add("Claxon", _inspection.Horn);
                });

                if (!string.IsNullOrWhiteSpace(_inspection.LightsAndControlsComments))
                {
                    col.Item().PaddingTop(5).Text($"Comentarios: {_inspection.LightsAndControlsComments}").Italic();
                }
            });
        }

        private void RenderFrameAndSuspension(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Estructura y Suspensión").FontSize(14).Bold();

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

                    Add("Condición del Marco", _inspection.FrameCondition);
                    Add("Rodamientos de la Dirección", _inspection.SteeringBearings);
                    Add("Bujes del Basculante", _inspection.SwingarmBushings);
                    Add("Horquillas Delanteras", _inspection.FrontForks);
                    Add("Amortiguadores Traseros", _inspection.RearShockAbsorbers);
                    Add("Cadena o Correa", _inspection.ChainOrStrap);
                    Add("Sujetadores", _inspection.Fasteners);
                    Add("Soporte Central", _inspection.CentralSupport);
                    Add("Soporte Lateral", _inspection.LateralSupport);
                });

                if (!string.IsNullOrWhiteSpace(_inspection.FrameAndSuspensionComments))
                {
                    col.Item().PaddingTop(5).Text($"Comentarios: {_inspection.FrameAndSuspensionComments}").Italic();
                }
            });
        }

        private void RenderOilAndLevels(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Aceite y Otros Niveles").FontSize(14).Bold();

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

                    Add("Aceite del Motor", _inspection.EngineOil);
                    Add("Aceite de Engranajes", _inspection.GearOil);
                    Add("Aceite de Transmisión por Eje", _inspection.AxleTransmissionOil);
                    Add("Fluido Hidráulico", _inspection.HydraulicFluid);
                    Add("Refrigerante", _inspection.Refrigerant);
                    Add("Combustible", _inspection.Fuel);
                    Add("Fugas", _inspection.Leaks);
                });

                if (!string.IsNullOrWhiteSpace(_inspection.OilAndLevelsComments))
                {
                    col.Item().PaddingTop(5).Text($"Comentarios: {_inspection.OilAndLevelsComments}").Italic();
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

                    Add("Terminales de Batería", _inspection.BatteryTerminals);
                    Add("Cables", _inspection.Cables);
                    Add("Montaje", _inspection.Mounting);
                    Add("Condiciones Generales de la Batería", _inspection.GeneralBatteryConditions);
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
