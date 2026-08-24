using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class WorkshopVehicleInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Workshops",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Workshops",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkshopBills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopBills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopBills_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkshopNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopNotes_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkshopVehicleInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsNewCustomer = table.Column<bool>(type: "bit", nullable: false),
                    FrontRightBrake = table.Column<int>(type: "int", nullable: false),
                    FrontRightTireTread = table.Column<int>(type: "int", nullable: false),
                    FrontRightTireAlignment = table.Column<int>(type: "int", nullable: false),
                    FrontLeftBrake = table.Column<int>(type: "int", nullable: false),
                    FrontLeftTireTread = table.Column<int>(type: "int", nullable: false),
                    FrontLeftTireAlignment = table.Column<int>(type: "int", nullable: false),
                    RearRightBrake = table.Column<int>(type: "int", nullable: false),
                    RearRightTireTread = table.Column<int>(type: "int", nullable: false),
                    RearRightTireAlignment = table.Column<int>(type: "int", nullable: false),
                    RearLeftBrake = table.Column<int>(type: "int", nullable: false),
                    RearLeftTireTread = table.Column<int>(type: "int", nullable: false),
                    RearLeftTireAlignment = table.Column<int>(type: "int", nullable: false),
                    TiresComments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brakes = table.Column<int>(type: "int", nullable: false),
                    TireTread = table.Column<int>(type: "int", nullable: false),
                    TireAlignment = table.Column<int>(type: "int", nullable: false),
                    Headlights = table.Column<int>(type: "int", nullable: false),
                    Taillights = table.Column<int>(type: "int", nullable: false),
                    TurnSignals = table.Column<int>(type: "int", nullable: false),
                    BrakeLights = table.Column<int>(type: "int", nullable: false),
                    HazardLights = table.Column<int>(type: "int", nullable: false),
                    WindshieldWasherFluid = table.Column<int>(type: "int", nullable: false),
                    WindshieldWiperOperation = table.Column<int>(type: "int", nullable: false),
                    WindshieldWiperBlades = table.Column<int>(type: "int", nullable: false),
                    WindshieldCondition = table.Column<int>(type: "int", nullable: false),
                    Mirrors = table.Column<int>(type: "int", nullable: false),
                    EmergencyBrake = table.Column<int>(type: "int", nullable: false),
                    Horn = table.Column<int>(type: "int", nullable: false),
                    FuelTankCap = table.Column<int>(type: "int", nullable: false),
                    AirConditioningFilter = table.Column<int>(type: "int", nullable: false),
                    ReversingLights = table.Column<int>(type: "int", nullable: false),
                    InteriorAndExteriorComments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EngineOilLevel = table.Column<int>(type: "int", nullable: false),
                    CoolantLevel = table.Column<int>(type: "int", nullable: false),
                    BrakeFluidLevel = table.Column<int>(type: "int", nullable: false),
                    AirFilter = table.Column<int>(type: "int", nullable: false),
                    RadiatorHoses = table.Column<int>(type: "int", nullable: false),
                    HeatingHoses = table.Column<int>(type: "int", nullable: false),
                    AirConditioningCondenser = table.Column<int>(type: "int", nullable: false),
                    EngineComments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteryTerminals = table.Column<int>(type: "int", nullable: false),
                    BatteryCables = table.Column<int>(type: "int", nullable: false),
                    BatteryMounting = table.Column<int>(type: "int", nullable: false),
                    GeneralBatteryCondition = table.Column<int>(type: "int", nullable: false),
                    BatteryComments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChasisComments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "WorkshopVehicleFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopVehicleInspectionId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
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
                name: "IX_WorkshopBills_WorkshopId",
                table: "WorkshopBills",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopNotes_WorkshopId",
                table: "WorkshopNotes",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkshopBills");

            migrationBuilder.DropTable(
                name: "WorkshopNotes");

            migrationBuilder.DropTable(
                name: "WorkshopVehicleFiles");

            migrationBuilder.DropTable(
                name: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Workshops");
        }
    }
}
