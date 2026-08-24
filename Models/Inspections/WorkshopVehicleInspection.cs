using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MiTaller.Models.Inspections;

namespace MiTaller.Models.Vehicle
{
    public class WorkshopVehicleInspection
    {
        [Key]
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop.Workshop Workshop { get; set; }

        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer.Customer Customer { get; set; }

        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        public DateTime InspectionDate { get; set; } = DateTime.Now;
        public bool IsNewCustomer { get; set; } = false;
        public bool IsNewVehicle { get; set; } = false;
        public string Status { get; set; } = "Pendiente";
        public bool IsActive { get; set; } = true;


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

        public string? InteriorAndExteriorComments { get; set; } = string.Empty;


        // Engine //
        public VehicleCondition EngineOilLevel { get; set; } = VehicleCondition.Optimal; // Niveles de líquido aceite
        public VehicleCondition CoolantLevel { get; set; } = VehicleCondition.Optimal; // Líquido refrigerante
        public VehicleCondition BrakeFluidLevel { get; set; } = VehicleCondition.Optimal; // Líquido de frenos
        public VehicleCondition AirFilter { get; set; } = VehicleCondition.Optimal; // Filtro de aire
        public VehicleCondition RadiatorHoses { get; set; } = VehicleCondition.Optimal; // Mangueras del sistema de refrigeración
        public VehicleCondition HeatingHoses { get; set; } = VehicleCondition.Optimal; // Mangueras de calefacción
        public VehicleCondition AirConditioningCondenser { get; set; } = VehicleCondition.Optimal; // Condensador de aire acondicionado

        public string? EngineComments { get; set; } = string.Empty;


        // Battery //
        public VehicleCondition BatteryTerminals { get; set; } = VehicleCondition.Optimal; // Terminales de batería
        public VehicleCondition BatteryCables { get; set; } = VehicleCondition.Optimal; // Cables
        public VehicleCondition BatteryMounting { get; set; } = VehicleCondition.Optimal; // Montaje
        public VehicleCondition GeneralBatteryCondition { get; set; } = VehicleCondition.Optimal; // Condiciones generales de la batería

        public string? BatteryComments { get; set; } = string.Empty;


        // Chasis // 
        public List<WorkshopVehicleFile> Files { get; set; } = new();
        public string? ChasisComments { get; set; } = string.Empty;


        // Observations //
        public string? Observations { get; set; } = string.Empty;

    }

    public enum VehicleCondition
    {
        Optimal = 0,         // Estado óptimo (Verde)
        NeedsAttention = 1,  // Requiere atención (Amarillo)
        UrgentAttention = 2  // Atención inmediata (Rojo)
    }
}
