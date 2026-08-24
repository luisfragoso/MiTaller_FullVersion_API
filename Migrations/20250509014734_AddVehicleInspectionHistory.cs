using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleInspectionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "File",
                table: "VehicleInspectionDetailHistory");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "VehicleInspectionDetailHistory",
                newName: "Title");

            migrationBuilder.AlterColumn<string>(
                name: "TiresComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Observations",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "InteriorAndExteriorComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EngineComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ChasisComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BatteryComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "VehicleInspectionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleInspectionId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspectionHistory_VehicleInspectionId",
                table: "VehicleInspectionHistory",
                column: "VehicleInspectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleInspectionHistory");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "VehicleInspectionDetailHistory",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "TiresComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observations",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InteriorAndExteriorComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EngineComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChasisComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BatteryComments",
                table: "WorkshopVehicleInspections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "File",
                table: "VehicleInspectionDetailHistory",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
