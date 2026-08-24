using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopServiceIdInWorkshopIncome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkshopIncomes_Workshops_WorkshopId",
                table: "WorkshopIncomes");

            migrationBuilder.DropIndex(
                name: "IX_WorkshopIncomes_WorkshopId",
                table: "WorkshopIncomes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "WorkshopIncomes");

            migrationBuilder.AddColumn<int>(
                name: "WorkshopServiceId",
                table: "WorkshopIncomes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopIncomes_WorkshopServiceId",
                table: "WorkshopIncomes",
                column: "WorkshopServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkshopIncomes_WorkshopServices_WorkshopServiceId",
                table: "WorkshopIncomes",
                column: "WorkshopServiceId",
                principalTable: "WorkshopServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkshopIncomes_WorkshopServices_WorkshopServiceId",
                table: "WorkshopIncomes");

            migrationBuilder.DropIndex(
                name: "IX_WorkshopIncomes_WorkshopServiceId",
                table: "WorkshopIncomes");

            migrationBuilder.DropColumn(
                name: "WorkshopServiceId",
                table: "WorkshopIncomes");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "WorkshopIncomes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopIncomes_WorkshopId",
                table: "WorkshopIncomes",
                column: "WorkshopId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkshopIncomes_Workshops_WorkshopId",
                table: "WorkshopIncomes",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
