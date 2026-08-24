using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleFormatInVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VehicleFormat",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleFormat",
                table: "Vehicles");
        }
    }
}
