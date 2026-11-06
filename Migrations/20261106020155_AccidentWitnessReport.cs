using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AccidentWitnessReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Plates are compared without dashes/spaces, upper-case
            migrationBuilder.Sql("UPDATE Accidents SET Plates = UPPER(REPLACE(REPLACE(Plates, '-', ''), ' ', ''))");

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "Accidents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "Accidents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotifiedContacts",
                table: "Accidents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "NotifiedContacts",
                table: "Accidents");
        }
    }
}
