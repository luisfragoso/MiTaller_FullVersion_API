using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddComplementaryQuotationInspectionLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkshopVehicleInspectionId",
                table: "Quotations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_WorkshopVehicleInspectionId",
                table: "Quotations",
                column: "WorkshopVehicleInspectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_WorkshopVehicleInspections_WorkshopVehicleInspectionId",
                table: "Quotations",
                column: "WorkshopVehicleInspectionId",
                principalTable: "WorkshopVehicleInspections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_WorkshopVehicleInspections_WorkshopVehicleInspectionId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_WorkshopVehicleInspectionId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "WorkshopVehicleInspectionId",
                table: "Quotations");
        }
    }
}
