using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class BackfillInspectionFolios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Inspections without a folio (native ones created before folios existed,
            // and copied legacy rows that never had a history row): same folio shape
            // as FolioGenerator, with "HST" in place of the random letters.
            migrationBuilder.Sql(@"
UPDATE i
SET i.Title = ISNULL(i.Title, N'Solicitud de reparación'),
    i.Folio = FORMAT(i.InspectionDate, 'yyyyMMddHHmmss') + 'HST' + UPPER(LEFT(ISNULL(v.VehicleFormat, 'X'), 1))
FROM Inspections i
JOIN Vehicles v ON v.Id = i.VehicleId
WHERE i.Folio IS NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE Inspections
SET Folio = NULL, Title = NULL
WHERE Folio LIKE '%HST_' AND Title = N'Solicitud de reparación';
");
        }
    }
}
