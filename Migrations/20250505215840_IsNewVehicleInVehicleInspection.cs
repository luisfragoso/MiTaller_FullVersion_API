using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class IsNewVehicleInVehicleInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNewVehicle",
                table: "WorkshopVehicleInspections",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNewVehicle",
                table: "WorkshopVehicleInspections");
        }
    }
}
