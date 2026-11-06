using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class VehicleEmergencyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmergencyCode",
                table: "Vehicles",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            // Existing vehicles get a code before the unique index; random duplicates are cleared and retried
            migrationBuilder.Sql(@"
DECLARE @a char(32) = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
WHILE EXISTS (SELECT 1 FROM Vehicles WHERE EmergencyCode IS NULL)
BEGIN
    UPDATE Vehicles SET EmergencyCode = 'MT'
        + SUBSTRING(@a, ABS(CHECKSUM(NEWID())) % 32 + 1, 1) + SUBSTRING(@a, ABS(CHECKSUM(NEWID())) % 32 + 1, 1)
        + SUBSTRING(@a, ABS(CHECKSUM(NEWID())) % 32 + 1, 1) + SUBSTRING(@a, ABS(CHECKSUM(NEWID())) % 32 + 1, 1)
        + SUBSTRING(@a, ABS(CHECKSUM(NEWID())) % 32 + 1, 1) + SUBSTRING(@a, ABS(CHECKSUM(NEWID())) % 32 + 1, 1)
    WHERE EmergencyCode IS NULL;
    UPDATE v SET EmergencyCode = NULL FROM Vehicles v
    WHERE EXISTS (SELECT 1 FROM Vehicles o WHERE o.EmergencyCode = v.EmergencyCode AND o.Id < v.Id);
END");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_EmergencyCode",
                table: "Vehicles",
                column: "EmergencyCode",
                unique: true,
                filter: "[EmergencyCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_EmergencyCode",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "EmergencyCode",
                table: "Vehicles");
        }
    }
}
