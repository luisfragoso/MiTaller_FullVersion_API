using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunitySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunitySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VotesPerMonth = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunitySettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CommunitySettings",
                columns: new[] { "Id", "UpdatedAt", "VotesPerMonth" },
                values: new object[] { 1, null, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunitySettings");
        }
    }
}
