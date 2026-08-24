using MiTaller.DTO.Vehicle;
using MiTaller.Models.Vehicle;

namespace MiTaller.DTO.Inspections
{
    public class MotocycleInspectionDocumentDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhoneNumber { get; set; } = string.Empty;
        public string CustomerGuid { get; set; } = string.Empty;
        public string WorkshopName { get; set; } = string.Empty;
        public string WorkshopPhoneNumber { get; set; } = string.Empty;
        public string WorkshopGuid { get; set; } = string.Empty;
        public VehicleResponseDto Vehicle { get; set; }

        public string Folio { get; set; } = string.Empty;
        public DateTime InspectionDate { get; set; }


        // Brakes and tires (Front)
        public VehicleCondition FrontRadios { get; set; } = VehicleCondition.Optimal; // Radios
        public VehicleCondition FrontTireThreadPattern { get; set; } = VehicleCondition.Optimal; // Dibujo de llanta
        public VehicleCondition FrontBearings { get; set; } = VehicleCondition.Optimal; // Rodamientos
        public VehicleCondition FrontStamps { get; set; } = VehicleCondition.Optimal; // Sellos
        public VehicleCondition FrontBrakeLining { get; set; } = VehicleCondition.Optimal; // Forro de frenos
        public VehicleCondition FrontWearPattern { get; set; } = VehicleCondition.Optimal; // Patron de desgaste


        // Brakes and tires (Rear)
        public VehicleCondition RearRadios { get; set; } = VehicleCondition.Optimal; // Radios
        public VehicleCondition RearTireThreadPattern { get; set; } = VehicleCondition.Optimal; // Dibujo de llanta
        public VehicleCondition RearBearings { get; set; } = VehicleCondition.Optimal; // Rodamientos
        public VehicleCondition RearStamps { get; set; } = VehicleCondition.Optimal; // Sellos
        public VehicleCondition RearBrakeLining { get; set; } = VehicleCondition.Optimal; // Forro de frenos
        public VehicleCondition RearWearPattern { get; set; } = VehicleCondition.Optimal; // Patron de desgaste

        public string? TiresComments { get; set; } = string.Empty;


        // Lights and controls
        public VehicleCondition Headlight { get; set; } = VehicleCondition.Optimal; // Faro
        public VehicleCondition Taillight { get; set; } = VehicleCondition.Optimal; // Luz trasera
        public VehicleCondition TurnSignals { get; set; } = VehicleCondition.Optimal; // Luces de giro
        public VehicleCondition HazardLights { get; set; } = VehicleCondition.Optimal; // Luces de emergencia
        public VehicleCondition Stoplight { get; set; } = VehicleCondition.Optimal; // Luz de freno
        public VehicleCondition LicensePlateLight { get; set; } = VehicleCondition.Optimal; // Luz de placa
        public VehicleCondition LeftMirror { get; set; } = VehicleCondition.Optimal; // Espejo izquierdo
        public VehicleCondition RightMirror { get; set; } = VehicleCondition.Optimal; // Espejo derecho
        public VehicleCondition Switches { get; set; } = VehicleCondition.Optimal; // Interruptores
        public VehicleCondition Cabling { get; set; } = VehicleCondition.Optimal; // Cableado
        public VehicleCondition HandleBars { get; set; } = VehicleCondition.Optimal; // Manubrios
        public VehicleCondition LeversAndPedal { get; set; } = VehicleCondition.Optimal; // Palancas y Pedal
        public VehicleCondition Hoses { get; set; } = VehicleCondition.Optimal; // Mangueras
        public VehicleCondition ThrottleLever { get; set; } = VehicleCondition.Optimal; // Palanca de Acelerador
        public VehicleCondition ClutchLever { get; set; } = VehicleCondition.Optimal; // Palanca de Clutch
        public VehicleCondition FuelTankCap { get; set; } = VehicleCondition.Optimal; // Tapa de tanque de combustible
        public VehicleCondition DashboardInstruments { get; set; } = VehicleCondition.Optimal; // Instrumentos de tablero
        public VehicleCondition Horn { get; set; } = VehicleCondition.Optimal; // Claxon

        public string? LightsAndControlsComments { get; set; } = string.Empty;


        // Frame and suspension
        public VehicleCondition FrameCondition { get; set; } = VehicleCondition.Optimal; // Condición del marco
        public VehicleCondition SteeringBearings { get; set; } = VehicleCondition.Optimal; // Rodamientos de la dirección
        public VehicleCondition SwingarmBushings { get; set; } = VehicleCondition.Optimal; // Bujes del basculante
        public VehicleCondition FrontForks { get; set; } = VehicleCondition.Optimal; // Horquillas delanteras
        public VehicleCondition RearShockAbsorbers { get; set; } = VehicleCondition.Optimal; // Amortiguadores traseros
        public VehicleCondition ChainOrStrap { get; set; } = VehicleCondition.Optimal; // Cadena o correa
        public VehicleCondition Fasteners { get; set; } = VehicleCondition.Optimal; // Sujetadores
        public VehicleCondition CentralSupport { get; set; } = VehicleCondition.Optimal; // Soporte central
        public VehicleCondition LateralSupport { get; set; } = VehicleCondition.Optimal; // Soporte lateral

        public string? FrameAndSuspensionComments { get; set; } = string.Empty;

        // Oil and other levels
        public VehicleCondition EngineOil { get; set; } = VehicleCondition.Optimal; // Aceite del motor
        public VehicleCondition GearOil { get; set; } = VehicleCondition.Optimal; // Aceite de engranajes
        public VehicleCondition AxleTransmissionOil { get; set; } = VehicleCondition.Optimal; // Aceite de transmisión por eje
        public VehicleCondition HydraulicFluid { get; set; } = VehicleCondition.Optimal; // Fluido hidráulico
        public VehicleCondition Refrigerant { get; set; } = VehicleCondition.Optimal; // Refrigerante
        public VehicleCondition Fuel { get; set; } = VehicleCondition.Optimal; // Combustible
        public VehicleCondition Leaks { get; set; } = VehicleCondition.Optimal; // Fugas
        public string? OilAndLevelsComments { get; set; } = string.Empty;


        // Baterry
        public VehicleCondition BatteryTerminals { get; set; } = VehicleCondition.Optimal; // Terminales de batería
        public VehicleCondition Cables { get; set; } = VehicleCondition.Optimal; // Cables
        public VehicleCondition Mounting { get; set; } = VehicleCondition.Optimal; // Montaje
        public VehicleCondition GeneralBatteryConditions { get; set; } = VehicleCondition.Optimal; // Condiciones generales de la batería
        public string? BatteryComments { get; set; } = string.Empty;

        // Chasis // 
        public List<WorkshopVehicleFileDto> Photos { get; set; } = new();
        public string? ChasisComments { get; set; } = string.Empty;


        // Observations //
        public string? Observations { get; set; } = string.Empty;
    }
}
