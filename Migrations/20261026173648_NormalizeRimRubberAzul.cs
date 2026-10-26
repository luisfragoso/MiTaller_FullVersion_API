using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeRimRubberAzul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The apps' engomado list said "Azúl"; it now says "Azul", so stored values must match
            migrationBuilder.Sql("UPDATE Vehicles SET RimRubber = N'Azul' WHERE RimRubber = N'Azúl';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reverted: rows that were already "Azul" can't be told apart from the normalized ones
        }
    }
}
