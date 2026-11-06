using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AccidentGuidedReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasInjured",
                table: "Accidents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "Accidents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "Accidents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "Accidents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Accidents",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotifiedContacts",
                table: "Accidents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "Accidents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleName",
                table: "Accidents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasInjured",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "NotifiedContacts",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "VehicleName",
                table: "Accidents");
        }
    }
}
