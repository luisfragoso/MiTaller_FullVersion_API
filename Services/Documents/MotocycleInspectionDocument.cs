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
                page.Margin(0);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style.FontColor(InspectionDocumentComponents.Ink));

                page.Header().Element(c => InspectionDocumentComponents.Header(
                    c,
                    "Reporte de Inspección",
                    _inspection.WorkshopName,
                    _inspection.Folio,
                    _inspection.InspectionDate));

                page.Content().PaddingHorizontal(32).PaddingVertical(20).Column(col =>
                {
                    col.Spacing(18);

                    col.Item().Element(c => RenderAditionalInfo(c));
                    col.Item().Element(c => RenderClientInfo(c));
                    col.Item().Element(c => RenderWorkshopInfo(c));
                    col.Item().Element(c => RenderBrakesAndTires(c));
                    col.Item().Element(c => RenderLightsAndControls(c));
                    col.Item().Element(c => RenderFrameAndSuspension(c));
                    col.Item().Element(c => RenderOilAndLevels(c));
                    col.Item().Element(c => RenderBattery(c));
                    col.Item().Element(c => RenderChasis(c));
                    col.Item().Element(c => InspectionDocumentComponents.PhotosSection(c, _inspection.Photos));
                    col.Item().Element(c => RenderObservations(c));
                    col.Item().Element(InspectionDocumentComponents.Disclaimer);
                });

                page.Footer().PaddingHorizontal(32).PaddingBottom(16)
                    .Element(InspectionDocumentComponents.Footer);
            });
        }

        private void RenderBrakesAndTires(IContainer container) =>
            InspectionDocumentComponents.ConditionSection(
                container,
                "Frenos y Neumáticos",
                new (string, VehicleCondition)[]
                {
                    ("Radios Delanteros", _inspection.FrontRadios),
                    ("Dibujo de Llanta Delantera", _inspection.FrontTireThreadPattern),
                    ("Rodamientos Delanteros", _inspection.FrontBearings),
                    ("Sellos Delanteros", _inspection.FrontStamps),
                    ("Forro de Freno Delantero", _inspection.FrontBrakeLining),
                    ("Patrón de Desgaste Delantero", _inspection.FrontWearPattern),
                    ("Radios Traseros", _inspection.RearRadios),
                    ("Dibujo de Llanta Trasera", _inspection.RearTireThreadPattern),
                    ("Rodamientos Traseros", _inspection.RearBearings),
                    ("Sellos Traseros", _inspection.RearStamps),
                    ("Forro de Freno Trasero", _inspection.RearBrakeLining),
                    ("Patrón de Desgaste Trasero", _inspection.RearWearPattern),
                },
                _inspection.TiresComments);

        private void RenderLightsAndControls(IContainer container) =>
            InspectionDocumentComponents.ConditionSection(
                container,
                "Luces y Controles",
                new (string, VehicleCondition)[]
                {
                    ("Faro", _inspection.Headlight),
                    ("Luz Trasera", _inspection.Taillight),
                    ("Luces de Giro", _inspection.TurnSignals),
                    ("Luces de Emergencia", _inspection.HazardLights),
                    ("Luz de Freno", _inspection.Stoplight),
                    ("Luz de Placa", _inspection.LicensePlateLight),
                    ("Espejo Izquierdo", _inspection.LeftMirror),
                    ("Espejo Derecho", _inspection.RightMirror),
                    ("Interruptores", _inspection.Switches),
                    ("Cableado", _inspection.Cabling),
                    ("Manubrios", _inspection.HandleBars),
                    ("Palancas y Pedal", _inspection.LeversAndPedal),
                    ("Mangueras", _inspection.Hoses),
                    ("Palanca de Acelerador", _inspection.ThrottleLever),
                    ("Palanca de Clutch", _inspection.ClutchLever),
                    ("Tapa de Tanque de Combustible", _inspection.FuelTankCap),
                    ("Instrumentos de Tablero", _inspection.DashboardInstruments),
                    ("Claxon", _inspection.Horn),
                },
                _inspection.LightsAndControlsComments);

        private void RenderFrameAndSuspension(IContainer container) =>
            InspectionDocumentComponents.ConditionSection(
                container,
                "Estructura y Suspensión",
                new (string, VehicleCondition)[]
                {
                    ("Condición del Marco", _inspection.FrameCondition),
                    ("Rodamientos de la Dirección", _inspection.SteeringBearings),
                    ("Bujes del Basculante", _inspection.SwingarmBushings),
                    ("Horquillas Delanteras", _inspection.FrontForks),
                    ("Amortiguadores Traseros", _inspection.RearShockAbsorbers),
                    ("Cadena o Correa", _inspection.ChainOrStrap),
                    ("Sujetadores", _inspection.Fasteners),
                    ("Soporte Central", _inspection.CentralSupport),
                    ("Soporte Lateral", _inspection.LateralSupport),
                },
                _inspection.FrameAndSuspensionComments);

        private void RenderOilAndLevels(IContainer container) =>
            InspectionDocumentComponents.ConditionSection(
                container,
                "Aceite y Otros Niveles",
                new (string, VehicleCondition)[]
                {
                    ("Aceite del Motor", _inspection.EngineOil),
                    ("Aceite de Engranajes", _inspection.GearOil),
                    ("Aceite de Transmisión por Eje", _inspection.AxleTransmissionOil),
                    ("Fluido Hidráulico", _inspection.HydraulicFluid),
                    ("Refrigerante", _inspection.Refrigerant),
                    ("Combustible", _inspection.Fuel),
                    ("Fugas", _inspection.Leaks),
                },
                _inspection.OilAndLevelsComments);

        private void RenderBattery(IContainer container) =>
            InspectionDocumentComponents.ConditionSection(
                container,
                "Batería",
                new (string, VehicleCondition)[]
                {
                    ("Terminales de Batería", _inspection.BatteryTerminals),
                    ("Cables", _inspection.Cables),
                    ("Montaje", _inspection.Mounting),
                    ("Condiciones Generales de la Batería", _inspection.GeneralBatteryConditions),
                },
                _inspection.BatteryComments);

        private void RenderChasis(IContainer container)
        {
            var diagram = _inspection.Photos.FirstOrDefault(InspectionDocumentComponents.IsChasisDiagram);
            var hasComments = !string.IsNullOrWhiteSpace(_inspection.ChasisComments);

            if (diagram == null && !hasComments) return;

            container.PreventPageBreak().Column(col =>
            {
                col.Item().Element(c => InspectionDocumentComponents.SectionTitle(c, "Chasis"));

                if (diagram != null)
                {
                    col.Item().PaddingTop(8)
                        .Element(c => InspectionDocumentComponents.ChasisDiagram(c, diagram));
                }

                if (hasComments)
                {
                    col.Item().PaddingTop(8)
                        .Element(c => InspectionDocumentComponents.NoteBox(c, _inspection.ChasisComments!));
                }
            });
        }

        private void RenderObservations(IContainer container)
        {
            if (string.IsNullOrWhiteSpace(_inspection.Observations)) return;

            container.Column(col =>
            {
                col.Item().Element(c => InspectionDocumentComponents.SectionTitle(c, "Observaciones"));
                col.Item().PaddingTop(8)
                    .Element(c => InspectionDocumentComponents.NoteBox(c, _inspection.Observations!));
            });
        }

        private void RenderAditionalInfo(IContainer container)
        {
            var title = _inspection.Vehicle.VehicleFormat == "Motocicleta"
                ? "Información de la motocicleta"
                : "Información del vehículo";

            InspectionDocumentComponents.InfoCard(container, title, new (string, string)[]
            {
                ("Placas", _inspection.Vehicle.Plates),
                ("Año", _inspection.Vehicle.Year),
                ("Marca", _inspection.Vehicle.Brand),
                ("Modelo", _inspection.Vehicle.Model),
                ("Versión", _inspection.Vehicle.Version),
                ("Tipo", _inspection.Vehicle.Type),
            });
        }

        private void RenderClientInfo(IContainer container) =>
            InspectionDocumentComponents.InfoCard(container, "Información del Cliente", new (string, string)[]
            {
                ("Nombre", _inspection.CustomerName),
                ("Número de Teléfono", _inspection.CustomerPhoneNumber),
                ("Número de Cliente", _inspection.CustomerGuid),
            });

        private void RenderWorkshopInfo(IContainer container) =>
            InspectionDocumentComponents.InfoCard(container, "Información del Taller", new (string, string)[]
            {
                ("Nombre", _inspection.WorkshopName),
                ("Número de Teléfono", _inspection.WorkshopPhoneNumber),
                ("Número de Taller", _inspection.WorkshopGuid),
            });
    }
}
