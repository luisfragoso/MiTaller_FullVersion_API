using MiTaller.DTO.Vehicle;
using MiTaller.Models.Inspections;
using MiTaller.Models.Vehicle;

namespace MiTaller.DTO.Inspections
{
    public class VehicleInspectionDocumentDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhoneNumber { get; set; } = string.Empty;
        public string CustomerGuid {  get; set; } = string.Empty;
        public string WorkshopName {  get; set; } = string.Empty;
        public string WorkshopPhoneNumber { get; set; } = string.Empty;
        public string WorkshopGuid { get; set; } = string.Empty;
        public VehicleResponseDto Vehicle { get; set; }

        public string Folio { get; set; } = string.Empty;
        public DateTime InspectionDate { get; set; }

        // Brakes and tires //
        // Front Right
        public VehicleCondition FrontRightBrake { get; set; } = VehicleCondition.Optimal;
        public VehicleCondition FrontRightTireTread { get; set; } = VehicleCondition.Optimal;
        public VehicleCondition FrontRightTireAlignment { get; set; } = VehicleCondition.Optimal;

        // Front Left
        public VehicleCondition FrontLeftBrake { get; set; } = VehicleCondition.Optimal;
        public VehicleCondition FrontLeftTireTread { get; set; } = VehicleCondition.Optimal;
        public VehicleCondition FrontLeftTireAlignment { get; set; } = VehicleCondition.Optimal;

        // Rear Right
        public VehicleCondition RearRightBrake { get; set; } = VehicleCondition.Optimal;
        public VehicleCondition RearRightTireTread { get; set; } = VehicleCondition.Optimal;
        public VehicleCondition RearRightTireAlignment { get; set; } = VehicleCondition.Optimal;

        // Rear Left
        public VehicleCondition RearLeftBrake { get; set; } = VehicleCondition.Optimal;
        public VehicleCondition RearLeftTireTread { get; set; } = VehicleCondition.Optimal;
        public VehicleCondition RearLeftTireAlignment { get; set; } = VehicleCondition.Optimal;

        public string? TiresComments { get; set; } = string.Empty;

        // Interior and Exterior //
        public VehicleCondition Brakes { get; set; } = VehicleCondition.Optimal; // Frenos
        public VehicleCondition TireTread { get; set; } = VehicleCondition.Optimal; // Dibujo de llanta
        public VehicleCondition TireAlignment { get; set; } = VehicleCondition.Optimal; // Alineación
        public VehicleCondition Headlights { get; set; } = VehicleCondition.Optimal; // Luces delanteras
        public VehicleCondition Taillights { get; set; } = VehicleCondition.Optimal; // Luces traseras
        public VehicleCondition TurnSignals { get; set; } = VehicleCondition.Optimal; // Señales de giro
        public VehicleCondition BrakeLights { get; set; } = VehicleCondition.Optimal; // Luces de freno
        public VehicleCondition HazardLights { get; set; } = VehicleCondition.Optimal; // Luces de advertencia de peligro
        public VehicleCondition WindshieldWasherFluid { get; set; } = VehicleCondition.Optimal; // Líquido del limpia parabrisa
        public VehicleCondition WindshieldWiperOperation { get; set; } = VehicleCondition.Optimal; // Funcionamiento del limpia parabrisa
        public VehicleCondition WindshieldWiperBlades { get; set; } = VehicleCondition.Optimal; // Escobillas del limpia parabrisa
        public VehicleCondition WindshieldCondition { get; set; } = VehicleCondition.Optimal; // Escobillas del limpia parabrisa
        public VehicleCondition Mirrors { get; set; } = VehicleCondition.Optimal; // Espejos
        public VehicleCondition EmergencyBrake { get; set; } = VehicleCondition.Optimal; // Freno de emergencia
        public VehicleCondition Horn { get; set; } = VehicleCondition.Optimal; // Claxon
        public VehicleCondition FuelTankCap { get; set; } = VehicleCondition.Optimal; // Tapa de tanque de combustible
        public VehicleCondition AirConditioningFilter { get; set; } = VehicleCondition.Optimal; // Filtro de aire acondicionado
        public VehicleCondition ReversingLights { get; set; } = VehicleCondition.Optimal; // Luces de marcha atrás
        public VehicleCondition LicensePlateLight { get; set; } = VehicleCondition.Optimal; // Luz de placa
        public VehicleCondition SeatBelts { get; set; } = VehicleCondition.Optimal; // Cinturones de seguridad

        public string? InteriorAndExteriorComments { get; set; } = string.Empty;


        // Engine //
        public VehicleCondition EngineOilLevel { get; set; } = VehicleCondition.Optimal; // Niveles de líquido aceite
        public VehicleCondition CoolantLevel { get; set; } = VehicleCondition.Optimal; // Líquido refrigerante
        public VehicleCondition BrakeFluidLevel { get; set; } = VehicleCondition.Optimal; // Líquido de frenos
        public VehicleCondition AirFilter { get; set; } = VehicleCondition.Optimal; // Filtro de aire
        public VehicleCondition RadiatorHoses { get; set; } = VehicleCondition.Optimal; // Mangueras del sistema de refrigeración
        public VehicleCondition HeatingHoses { get; set; } = VehicleCondition.Optimal; // Mangueras de calefacción
        public VehicleCondition AirConditioningCondenser { get; set; } = VehicleCondition.Optimal; // Condensador de aire acondicionado
        public VehicleCondition TransmissionFluidLevel { get; set; } = VehicleCondition.Optimal; // Líquido de transmisión
        public VehicleCondition PowerSteeringFluidLevel { get; set; } = VehicleCondition.Optimal; // Líquido de dirección hidráulica
        public VehicleCondition AccessoryBelt { get; set; } = VehicleCondition.Optimal; // Banda de accesorios
        public VehicleCondition ExhaustSystem { get; set; } = VehicleCondition.Optimal; // Sistema de escape

        public string? EngineComments { get; set; } = string.Empty;


        // Battery //
        public VehicleCondition BatteryTerminals { get; set; } = VehicleCondition.Optimal; // Terminales de batería
        public VehicleCondition BatteryCables { get; set; } = VehicleCondition.Optimal; // Cables
        public VehicleCondition BatteryMounting { get; set; } = VehicleCondition.Optimal; // Montaje
        public VehicleCondition GeneralBatteryCondition { get; set; } = VehicleCondition.Optimal; // Condiciones generales de la batería

        public string? BatteryComments { get; set; } = string.Empty;


        // Suspension and Steering //
        public VehicleCondition FrontShockAbsorbers { get; set; } = VehicleCondition.Optimal; // Amortiguadores delanteros
        public VehicleCondition RearShockAbsorbers { get; set; } = VehicleCondition.Optimal; // Amortiguadores traseros
        public VehicleCondition BallJoints { get; set; } = VehicleCondition.Optimal; // Rótulas
        public VehicleCondition SteeringRackAndTierods { get; set; } = VehicleCondition.Optimal; // Cremallera y terminales de dirección
        public VehicleCondition SuspensionBushings { get; set; } = VehicleCondition.Optimal; // Bujes de suspensión

        public string? SuspensionComments { get; set; } = string.Empty;


        // Chasis //
        public List<WorkshopVehicleFileDto> Photos { get; set; } = new();
        public string? ChasisComments { get; set; } = string.Empty;


        // Observations //
        public string? Observations { get; set; } = string.Empty;



    }
}
