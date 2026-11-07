using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyInspectionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_WorkshopVehicleInspections_WorkshopVehicleInspectionId",
                table: "Quotations");

            migrationBuilder.DropTable(
                name: "MotocycleInspectionDetailHistory");

            migrationBuilder.DropTable(
                name: "MotocycleInspectionFiles");

            migrationBuilder.DropTable(
                name: "MotocycleInspectionHistory");

            migrationBuilder.DropTable(
                name: "VehicleInspectionDetailHistory");

            migrationBuilder.DropTable(
                name: "VehicleInspectionHistory");

            migrationBuilder.DropTable(
                name: "WorkshopVehicleFiles");

            migrationBuilder.DropTable(
                name: "WorkshopMotocycleInspections");

            migrationBuilder.DropTable(
                name: "WorkshopVehicleInspections");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_WorkshopVehicleInspectionId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "WorkshopVehicleInspectionId",
                table: "Quotations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkshopVehicleInspectionId",
                table: "Quotations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkshopMotocycleInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AxleTransmissionOil = table.Column<int>(type: "int", nullable: false),
                    BatteryComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatteryTerminals = table.Column<int>(type: "int", nullable: false),
                    Cables = table.Column<int>(type: "int", nullable: false),
                    Cabling = table.Column<int>(type: "int", nullable: false),
                    CentralSupport = table.Column<int>(type: "int", nullable: false),
                    ChainOrStrap = table.Column<int>(type: "int", nullable: false),
                    ChasisComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClutchLever = table.Column<int>(type: "int", nullable: false),
                    DashboardInstruments = table.Column<int>(type: "int", nullable: false),
                    EngineOil = table.Column<int>(type: "int", nullable: false),
                    Fasteners = table.Column<int>(type: "int", nullable: false),
                    FrameAndSuspensionComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FrameCondition = table.Column<int>(type: "int", nullable: false),
                    FrontBearings = table.Column<int>(type: "int", nullable: false),
                    FrontBrakeLining = table.Column<int>(type: "int", nullable: false),
                    FrontForks = table.Column<int>(type: "int", nullable: false),
                    FrontRadios = table.Column<int>(type: "int", nullable: false),
                    FrontStamps = table.Column<int>(type: "int", nullable: false),
                    FrontTireThreadPattern = table.Column<int>(type: "int", nullable: false),
                    FrontWearPattern = table.Column<int>(type: "int", nullable: false),
                    Fuel = table.Column<int>(type: "int", nullable: false),
                    FuelTankCap = table.Column<int>(type: "int", nullable: false),
                    GearOil = table.Column<int>(type: "int", nullable: false),
                    GeneralBatteryConditions = table.Column<int>(type: "int", nullable: false),
                    HandleBars = table.Column<int>(type: "int", nullable: false),
                    HazardLights = table.Column<int>(type: "int", nullable: false),
                    Headlight = table.Column<int>(type: "int", nullable: false),
                    Horn = table.Column<int>(type: "int", nullable: false),
                    Hoses = table.Column<int>(type: "int", nullable: false),
                    HydraulicFluid = table.Column<int>(type: "int", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsNewCustomer = table.Column<bool>(type: "bit", nullable: false),
                    IsNewVehicle = table.Column<bool>(type: "bit", nullable: false),
                    LateralSupport = table.Column<int>(type: "int", nullable: false),
                    Leaks = table.Column<int>(type: "int", nullable: false),
                    LeftMirror = table.Column<int>(type: "int", nullable: false),
                    LeversAndPedal = table.Column<int>(type: "int", nullable: false),
                    LicensePlateLight = table.Column<int>(type: "int", nullable: false),
                    LightsAndControlsComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mounting = table.Column<int>(type: "int", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OilAndLevelsComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RearBearings = table.Column<int>(type: "int", nullable: false),
                    RearBrakeLining = table.Column<int>(type: "int", nullable: false),
                    RearRadios = table.Column<int>(type: "int", nullable: false),
                    RearShockAbsorbers = table.Column<int>(type: "int", nullable: false),
                    RearStamps = table.Column<int>(type: "int", nullable: false),
                    RearTireThreadPattern = table.Column<int>(type: "int", nullable: false),
                    RearWearPattern = table.Column<int>(type: "int", nullable: false),
                    Refrigerant = table.Column<int>(type: "int", nullable: false),
                    RightMirror = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SteeringBearings = table.Column<int>(type: "int", nullable: false),
                    Stoplight = table.Column<int>(type: "int", nullable: false),
                    SwingarmBushings = table.Column<int>(type: "int", nullable: false),
                    Switches = table.Column<int>(type: "int", nullable: false),
                    Taillight = table.Column<int>(type: "int", nullable: false),
                    ThrottleLever = table.Column<int>(type: "int", nullable: false),
                    TiresComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TurnSignals = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopMotocycleInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopMotocycleInspections_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkshopMotocycleInspections_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkshopMotocycleInspections_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkshopVehicleInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessoryBelt = table.Column<int>(type: "int", nullable: false),
                    AirConditioningCondenser = table.Column<int>(type: "int", nullable: false),
                    AirConditioningFilter = table.Column<int>(type: "int", nullable: false),
                    AirFilter = table.Column<int>(type: "int", nullable: false),
                    BallJoints = table.Column<int>(type: "int", nullable: false),
                    BatteryCables = table.Column<int>(type: "int", nullable: false),
                    BatteryComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatteryMounting = table.Column<int>(type: "int", nullable: false),
                    BatteryTerminals = table.Column<int>(type: "int", nullable: false),
                    BrakeFluidLevel = table.Column<int>(type: "int", nullable: false),
                    BrakeLights = table.Column<int>(type: "int", nullable: false),
                    Brakes = table.Column<int>(type: "int", nullable: false),
                    ChasisComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoolantLevel = table.Column<int>(type: "int", nullable: false),
                    EmergencyBrake = table.Column<int>(type: "int", nullable: false),
                    EngineComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineOilLevel = table.Column<int>(type: "int", nullable: false),
                    ExhaustSystem = table.Column<int>(type: "int", nullable: false),
                    FrontLeftBrake = table.Column<int>(type: "int", nullable: false),
                    FrontLeftTireAlignment = table.Column<int>(type: "int", nullable: false),
                    FrontLeftTireTread = table.Column<int>(type: "int", nullable: false),
                    FrontRightBrake = table.Column<int>(type: "int", nullable: false),
                    FrontRightTireAlignment = table.Column<int>(type: "int", nullable: false),
                    FrontRightTireTread = table.Column<int>(type: "int", nullable: false),
                    FrontShockAbsorbers = table.Column<int>(type: "int", nullable: false),
                    FuelTankCap = table.Column<int>(type: "int", nullable: false),
                    GeneralBatteryCondition = table.Column<int>(type: "int", nullable: false),
                    HazardLights = table.Column<int>(type: "int", nullable: false),
                    Headlights = table.Column<int>(type: "int", nullable: false),
                    HeatingHoses = table.Column<int>(type: "int", nullable: false),
                    Horn = table.Column<int>(type: "int", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InteriorAndExteriorComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsNewCustomer = table.Column<bool>(type: "bit", nullable: false),
                    IsNewVehicle = table.Column<bool>(type: "bit", nullable: false),
                    LicensePlateLight = table.Column<int>(type: "int", nullable: false),
                    Mirrors = table.Column<int>(type: "int", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PowerSteeringFluidLevel = table.Column<int>(type: "int", nullable: false),
                    RadiatorHoses = table.Column<int>(type: "int", nullable: false),
                    RearLeftBrake = table.Column<int>(type: "int", nullable: false),
                    RearLeftTireAlignment = table.Column<int>(type: "int", nullable: false),
                    RearLeftTireTread = table.Column<int>(type: "int", nullable: false),
                    RearRightBrake = table.Column<int>(type: "int", nullable: false),
                    RearRightTireAlignment = table.Column<int>(type: "int", nullable: false),
                    RearRightTireTread = table.Column<int>(type: "int", nullable: false),
                    RearShockAbsorbers = table.Column<int>(type: "int", nullable: false),
                    ReversingLights = table.Column<int>(type: "int", nullable: false),
                    SeatBelts = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SteeringRackAndTierods = table.Column<int>(type: "int", nullable: false),
                    SuspensionBushings = table.Column<int>(type: "int", nullable: false),
                    SuspensionComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Taillights = table.Column<int>(type: "int", nullable: false),
                    TireAlignment = table.Column<int>(type: "int", nullable: false),
                    TireTread = table.Column<int>(type: "int", nullable: false),
                    TiresComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransmissionFluidLevel = table.Column<int>(type: "int", nullable: false),
                    TurnSignals = table.Column<int>(type: "int", nullable: false),
                    WindshieldCondition = table.Column<int>(type: "int", nullable: false),
                    WindshieldWasherFluid = table.Column<int>(type: "int", nullable: false),
                    WindshieldWiperBlades = table.Column<int>(type: "int", nullable: false),
                    WindshieldWiperOperation = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopVehicleInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopVehicleInspections_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkshopVehicleInspections_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkshopVehicleInspections_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MotocycleInspectionDetailHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MotocycleInspectionId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotocycleInspectionDetailHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotocycleInspectionDetailHistory_WorkshopMotocycleInspections_MotocycleInspectionId",
                        column: x => x.MotocycleInspectionId,
                        principalTable: "WorkshopMotocycleInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MotocycleInspectionFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopMotocycleInspectionId = table.Column<int>(type: "int", nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotocycleInspectionFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotocycleInspectionFiles_WorkshopMotocycleInspections_WorkshopMotocycleInspectionId",
                        column: x => x.WorkshopMotocycleInspectionId,
                        principalTable: "WorkshopMotocycleInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MotocycleInspectionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MotocycleInspectionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Folio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotocycleInspectionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotocycleInspectionHistory_WorkshopMotocycleInspections_MotocycleInspectionId",
                        column: x => x.MotocycleInspectionId,
                        principalTable: "WorkshopMotocycleInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleInspectionDetailHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleInspectionId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleInspectionDetailHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleInspectionDetailHistory_WorkshopVehicleInspections_VehicleInspectionId",
                        column: x => x.VehicleInspectionId,
                        principalTable: "WorkshopVehicleInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleInspectionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleInspectionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Folio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleInspectionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleInspectionHistory_WorkshopVehicleInspections_VehicleInspectionId",
                        column: x => x.VehicleInspectionId,
                        principalTable: "WorkshopVehicleInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkshopVehicleFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopVehicleInspectionId = table.Column<int>(type: "int", nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopVehicleFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopVehicleFiles_WorkshopVehicleInspections_WorkshopVehicleInspectionId",
                        column: x => x.WorkshopVehicleInspectionId,
                        principalTable: "WorkshopVehicleInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_WorkshopVehicleInspectionId",
                table: "Quotations",
                column: "WorkshopVehicleInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MotocycleInspectionDetailHistory_MotocycleInspectionId",
                table: "MotocycleInspectionDetailHistory",
                column: "MotocycleInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MotocycleInspectionFiles_WorkshopMotocycleInspectionId",
                table: "MotocycleInspectionFiles",
                column: "WorkshopMotocycleInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MotocycleInspectionHistory_MotocycleInspectionId",
                table: "MotocycleInspectionHistory",
                column: "MotocycleInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspectionDetailHistory_VehicleInspectionId",
                table: "VehicleInspectionDetailHistory",
                column: "VehicleInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspectionHistory_VehicleInspectionId",
                table: "VehicleInspectionHistory",
                column: "VehicleInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopMotocycleInspections_CustomerId",
                table: "WorkshopMotocycleInspections",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopMotocycleInspections_VehicleId",
                table: "WorkshopMotocycleInspections",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopMotocycleInspections_WorkshopId",
                table: "WorkshopMotocycleInspections",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopVehicleFiles_WorkshopVehicleInspectionId",
                table: "WorkshopVehicleFiles",
                column: "WorkshopVehicleInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopVehicleInspections_CustomerId",
                table: "WorkshopVehicleInspections",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopVehicleInspections_VehicleId",
                table: "WorkshopVehicleInspections",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopVehicleInspections_WorkshopId",
                table: "WorkshopVehicleInspections",
                column: "WorkshopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_WorkshopVehicleInspections_WorkshopVehicleInspectionId",
                table: "Quotations",
                column: "WorkshopVehicleInspectionId",
                principalTable: "WorkshopVehicleInspections",
                principalColumn: "Id");
        }
    }
}
