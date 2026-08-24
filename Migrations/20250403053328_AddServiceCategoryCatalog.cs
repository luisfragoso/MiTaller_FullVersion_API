using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCategoryCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "WorkshopServices");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "WorkshopServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopServices_ServiceId",
                table: "WorkshopServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceCategoryId",
                table: "Services",
                column: "ServiceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkshopServices_Services_ServiceId",
                table: "WorkshopServices",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkshopServices_Services_ServiceId",
                table: "WorkshopServices");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.DropIndex(
                name: "IX_WorkshopServices_ServiceId",
                table: "WorkshopServices");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "WorkshopServices");

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "WorkshopServices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
