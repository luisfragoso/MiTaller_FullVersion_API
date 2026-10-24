using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    // Zero items on purpose, same as Automóvil/Motocicleta's Chasis section -
    // it's a tap-diagram step, only its photos/comment persist (generic
    // Files/SectionComments). Bicicleta/Avión each get their own section;
    // Yate/Lancha/Bote share TemplateId -1 so one section covers all three.
    public partial class AddGenericChassisSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InspectionTemplateSections",
                columns: new[] { "Id", "Name", "Order", "TemplateId" },
                values: new object[,]
                {
                    { -39, "Chasis", 7, -9 },
                    { -40, "Chasis", 8, -2 },
                    { -41, "Chasis", 8, -1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "InspectionTemplateSections", keyColumn: "Id", keyValue: -39);
            migrationBuilder.DeleteData(table: "InspectionTemplateSections", keyColumn: "Id", keyValue: -40);
            migrationBuilder.DeleteData(table: "InspectionTemplateSections", keyColumn: "Id", keyValue: -41);
        }
    }
}
