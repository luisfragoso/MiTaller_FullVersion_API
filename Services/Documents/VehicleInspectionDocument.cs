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
                    col.Item().Element(c => RenderInteriorExterior(c));
                    col.Item().Element(c => RenderEngine(c));
                    col.Item().Element(c => RenderBattery(c));
                    col.Item().Element(c => RenderSuspension(c));
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
                    ("Freno Delantero Derecho", _inspection.FrontRightBrake),
                    ("Dibujo Llanta Delantera Derecha", _inspection.FrontRightTireTread),
                    ("Alineación Delantera Derecha", _inspection.FrontRightTireAlignment),
                    ("Freno Delantero Izquierdo", _inspection.FrontLeftBrake),
                    ("Dibujo Llanta Delantera Izquierda", _inspection.FrontLeftTireTread),
                    ("Alineación Delantera Izquierda", _inspection.FrontLeftTireAlignment),
                    ("Freno Trasero Derecho", _inspection.RearRightBrake),
                    ("Dibujo Llanta Trasera Derecha", _inspection.RearRightTireTread),
                    ("Alineación Trasera Derecha", _inspection.RearRightTireAlignment),
                    ("Freno Trasero Izquierdo", _inspection.RearLeftBrake),
                    ("Dibujo Llanta Trasera Izquierda", _inspection.RearLeftTireTread),
                    ("Alineación Trasera Izquierda", _inspection.RearLeftTireAlignment),
                },
                _inspection.TiresComments);

        private void RenderInteriorExterior(IContainer container) =>
            InspectionDocumentComponents.ConditionSection(
                container,
                "Interior y Exterior",
                new (string, VehicleCondition)[]
                {
                    ("Frenos", _inspection.Brakes),
                    ("Dibujo de llanta", _inspection.TireTread),
                    ("Alineación", _inspection.TireAlignment),
                    ("Luces delanteras", _inspection.Headlights),
                    ("Luces traseras", _inspection.Taillights),
                    ("Señales de giro", _inspection.TurnSignals),
                    ("Luces de freno", _inspection.BrakeLights),
                    ("Luces de advertencia de peligro", _inspection.HazardLights),
                    ("Líquido del limpia parabrisa", _inspection.WindshieldWasherFluid),
                    ("Funcionamiento del limpia parabrisa", _inspection.WindshieldWiperOperation),
                    ("Escobillas del limpia parabrisa", _inspection.WindshieldWiperBlades),
                    ("Condición del limpia parabrisa", _inspection.WindshieldCondition),
                    ("Espejos", _inspection.Mirrors),
                    ("Freno de emergencia", _inspection.EmergencyBrake),
                    ("Claxon", _inspection.Horn),
                    ("Tapa de tanque de combustible", _inspection.FuelTankCap),
                    ("Filtro de aire acondicionado", _inspection.AirConditioningFilter),
                    ("Luces de marcha atrás", _inspection.ReversingLights),
                    ("Luz de placa", _inspection.LicensePlateLight),
                    ("Cinturones de seguridad", _inspection.SeatBelts),
                },
                _inspection.InteriorAndExteriorComments);

        private void RenderEngine(IContainer container) =>
            InspectionDocumentComponents.ConditionSection(
                container,
                "Motor",
                new (string, VehicleCondition)[]
                {
                    ("Niveles de líquido aceite", _inspection.EngineOilLevel),
                    ("Líquido refrigerante", _inspection.CoolantLevel),
                    ("Líquido de frenos", _inspection.BrakeFluidLevel),
                    ("Filtro de aire", _inspection.AirFilter),
                    ("Mangueras del sistema de refrigeración", _inspection.RadiatorHoses),
                    ("Mangueras de calefacción", _inspection.HeatingHoses),
                    ("Condensador de aire acondicionado", _inspection.AirConditioningCondenser),
                    ("Líquido de transmisión", _inspection.TransmissionFluidLevel),
                    ("Líquido de dirección hidráulica", _inspection.PowerSteeringFluidLevel),
                    ("Banda de accesorios", _inspection.AccessoryBelt),
                    ("Sistema de escape", _inspection.ExhaustSystem),
                },
                _inspection.EngineComments);

        private void RenderBattery(IContainer container) =>
            InspectionDocumentComponents.ConditionSection(
                container,
                "Batería",
                new (string, VehicleCondition)[]
                {
                    ("Terminales de batería", _inspection.BatteryTerminals),
                    ("Cables", _inspection.BatteryCables),
                    ("Montaje", _inspection.BatteryMounting),
                    ("Condiciones generales de la batería", _inspection.GeneralBatteryCondition),
                },
                _inspection.BatteryComments);

        private void RenderSuspension(IContainer container) =>
            InspectionDocumentComponents.ConditionSection(
                container,
                "Suspensión y Dirección",
                new (string, VehicleCondition)[]
                {
                    ("Amortiguadores delanteros", _inspection.FrontShockAbsorbers),
                    ("Amortiguadores traseros", _inspection.RearShockAbsorbers),
                    ("Rótulas", _inspection.BallJoints),
                    ("Cremallera y terminales de dirección", _inspection.SteeringRackAndTierods),
                    ("Bujes de suspensión", _inspection.SuspensionBushings),
                },
                _inspection.SuspensionComments);

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
