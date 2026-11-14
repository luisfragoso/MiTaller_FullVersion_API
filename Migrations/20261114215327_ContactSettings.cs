using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class ContactSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InfoEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    SupportEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ContactSettings",
                columns: new[] { "Id", "InfoEmail", "SupportEmail", "UpdatedAt" },
                values: new object[] { 1, "info@mitaller.io", "info@mitaller.io", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactSettings");
        }
    }
}
