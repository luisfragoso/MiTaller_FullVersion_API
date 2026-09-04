using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddCarSuspensionAndExtraChecklistFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessoryBelt",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BallJoints",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExhaustSystem",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FrontShockAbsorbers",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LicensePlateLight",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PowerSteeringFluidLevel",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RearShockAbsorbers",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeatBelts",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SteeringRackAndTierods",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SuspensionBushings",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransmissionFluidLevel",
                table: "WorkshopVehicleInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessoryBelt",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "BallJoints",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "ExhaustSystem",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "FrontShockAbsorbers",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "LicensePlateLight",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "PowerSteeringFluidLevel",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "RearShockAbsorbers",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "SeatBelts",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "SteeringRackAndTierods",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "SuspensionBushings",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "SuspensionComments",
                table: "WorkshopVehicleInspections");

            migrationBuilder.DropColumn(
                name: "TransmissionFluidLevel",
                table: "WorkshopVehicleInspections");
        }
    }
}
