using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class WorkshopRoadsideSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkshopRoadsideSettings",
                columns: table => new
                {
                    WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OffersRoadsideAssistance = table.Column<bool>(type: "bit", nullable: false),
                    CoverageRadiusKm = table.Column<int>(type: "int", nullable: false),
                    Available24h = table.Column<bool>(type: "bit", nullable: false),
                    EmergencyPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopRoadsideSettings", x => x.WorkshopId);
                    table.ForeignKey(
                        name: "FK_WorkshopRoadsideSettings_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Workshops that already listed the roadside service keep showing up for customers
            migrationBuilder.Sql(@"
INSERT INTO WorkshopRoadsideSettings (WorkshopId, OffersRoadsideAssistance, CoverageRadiusKm, Available24h, EmergencyPhone, UpdatedAt)
SELECT DISTINCT ws.WorkshopId, 1, 20, 0, N'', GETDATE()
FROM WorkshopServices ws
JOIN Services s ON s.Id = ws.ServiceId
WHERE s.Name = N'Asistencia en carretera y grúa' AND ws.IsDeleted = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkshopRoadsideSettings");
        }
    }
}
