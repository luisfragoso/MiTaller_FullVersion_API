using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuotationServiceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationServices_WorkshopServices_ServiceId",
                table: "QuotationServices");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "QuotationServices",
                newName: "WorkshopServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_QuotationServices_ServiceId",
                table: "QuotationServices",
                newName: "IX_QuotationServices_WorkshopServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationServices_WorkshopServices_WorkshopServiceId",
                table: "QuotationServices",
                column: "WorkshopServiceId",
                principalTable: "WorkshopServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationServices_WorkshopServices_WorkshopServiceId",
                table: "QuotationServices");

            migrationBuilder.RenameColumn(
                name: "WorkshopServiceId",
                table: "QuotationServices",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_QuotationServices_WorkshopServiceId",
                table: "QuotationServices",
                newName: "IX_QuotationServices_ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationServices_WorkshopServices_ServiceId",
                table: "QuotationServices",
                column: "ServiceId",
                principalTable: "WorkshopServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
