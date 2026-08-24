using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleInspectionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkshopMotocycleInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsNewCustomer = table.Column<bool>(type: "bit", nullable: false),
                    IsNewVehicle = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrontRadios = table.Column<int>(type: "int", nullable: false),
                    FrontTireThreadPattern = table.Column<int>(type: "int", nullable: false),
                    FrontBearings = table.Column<int>(type: "int", nullable: false),
                    FrontStamps = table.Column<int>(type: "int", nullable: false),
                    FrontBrakeLining = table.Column<int>(type: "int", nullable: false),
                    FrontWearPattern = table.Column<int>(type: "int", nullable: false),
                    RearRadios = table.Column<int>(type: "int", nullable: false),
                    RearTireThreadPattern = table.Column<int>(type: "int", nullable: false),
                    RearBearings = table.Column<int>(type: "int", nullable: false),
                    RearStamps = table.Column<int>(type: "int", nullable: false),
                    RearBrakeLining = table.Column<int>(type: "int", nullable: false),
                    RearWearPattern = table.Column<int>(type: "int", nullable: false),
                    TiresComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Headlight = table.Column<int>(type: "int", nullable: false),
                    Taillight = table.Column<int>(type: "int", nullable: false),
                    TurnSignals = table.Column<int>(type: "int", nullable: false),
                    HazardLights = table.Column<int>(type: "int", nullable: false),
                    Stoplight = table.Column<int>(type: "int", nullable: false),
                    LicensePlateLight = table.Column<int>(type: "int", nullable: false),
                    LeftMirror = table.Column<int>(type: "int", nullable: false),
                    RightMirror = table.Column<int>(type: "int", nullable: false),
                    Switches = table.Column<int>(type: "int", nullable: false),
                    Cabling = table.Column<int>(type: "int", nullable: false),
                    HandleBars = table.Column<int>(type: "int", nullable: false),
                    LeversAndPedal = table.Column<int>(type: "int", nullable: false),
                    Hoses = table.Column<int>(type: "int", nullable: false),
                    ThrottleLever = table.Column<int>(type: "int", nullable: false),
                    ClutchLever = table.Column<int>(type: "int", nullable: false),
                    FuelTankCap = table.Column<int>(type: "int", nullable: false),
                    DashboardInstruments = table.Column<int>(type: "int", nullable: false),
                    Horn = table.Column<int>(type: "int", nullable: false),
                    LightsAndControlsComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FrameCondition = table.Column<int>(type: "int", nullable: false),
                    SteeringBearings = table.Column<int>(type: "int", nullable: false),
                    SwingarmBushings = table.Column<int>(type: "int", nullable: false),
                    FrontForks = table.Column<int>(type: "int", nullable: false),
                    RearShockAbsorbers = table.Column<int>(type: "int", nullable: false),
                    ChainOrStrap = table.Column<int>(type: "int", nullable: false),
                    Fasteners = table.Column<int>(type: "int", nullable: false),
                    CentralSupport = table.Column<int>(type: "int", nullable: false),
                    LateralSupport = table.Column<int>(type: "int", nullable: false),
                    FrameAndSuspensionComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineOil = table.Column<int>(type: "int", nullable: false),
                    GearOil = table.Column<int>(type: "int", nullable: false),
                    AxleTransmissionOil = table.Column<int>(type: "int", nullable: false),
                    HydraulicFluid = table.Column<int>(type: "int", nullable: false),
                    Refrigerant = table.Column<int>(type: "int", nullable: false),
                    Fuel = table.Column<int>(type: "int", nullable: false),
                    Leaks = table.Column<int>(type: "int", nullable: false),
                    OilAndLevelsComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatteryTerminals = table.Column<int>(type: "int", nullable: false),
                    Cables = table.Column<int>(type: "int", nullable: false),
                    Mounting = table.Column<int>(type: "int", nullable: false),
                    GeneralBatteryConditions = table.Column<int>(type: "int", nullable: false),
                    BatteryComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChasisComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "MotocycleInspectionDetailHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MotocycleInspectionId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
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
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MotocycleInspectionDetailHistory");

            migrationBuilder.DropTable(
                name: "MotocycleInspectionFiles");

            migrationBuilder.DropTable(
                name: "MotocycleInspectionHistory");

            migrationBuilder.DropTable(
                name: "WorkshopMotocycleInspections");
        }
    }
}
