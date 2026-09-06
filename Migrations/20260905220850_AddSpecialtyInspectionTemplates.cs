using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialtyInspectionTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionTemplateSpecialties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTemplateSpecialties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTemplateSpecialties_InspectionTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "InspectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplates",
                columns: new[] { "Id", "IsActive", "Name", "VehicleFormat" },
                values: new object[,]
                {
                    { -8, true, "Inspección de Aire Acondicionado", "Aire Acondicionado" },
                    { -7, true, "Inspección de Suspensión y Alineación", "Suspensión y Alineación" },
                    { -6, true, "Inspección de Frenos", "Frenos" },
                    { -5, true, "Inspección de Mofles y Escapes", "Mofles y Escapes" },
                    { -4, true, "Inspección Eléctrica Automotriz", "Eléctrico Automotriz" },
                    { -3, true, "Inspección de Radiadores", "Radiadores" }
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplateSections",
                columns: new[] { "Id", "Name", "Order", "TemplateId" },
                values: new object[,]
                {
                    { -20, "Sistema de Aire Acondicionado", 1, -8 },
                    { -19, "Suspensión y Dirección", 1, -7 },
                    { -18, "Sistema de Frenos", 1, -6 },
                    { -17, "Sistema de Escape", 1, -5 },
                    { -16, "Sistema Eléctrico", 1, -4 },
                    { -15, "Sistema de Enfriamiento", 1, -3 }
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplateSpecialties",
                columns: new[] { "Id", "Specialty", "TemplateId" },
                values: new object[,]
                {
                    { -6, "Aire Acondicionado", -8 },
                    { -5, "Suspensión y Alineación", -7 },
                    { -4, "Frenos", -6 },
                    { -3, "Mofles y Escapes", -5 },
                    { -2, "Eléctrico Automotriz", -4 },
                    { -1, "Radiadores", -3 }
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplateItems",
                columns: new[] { "Id", "AllowNotApplicable", "Name", "Order", "SectionId" },
                values: new object[,]
                {
                    { -138, false, "Funcionamiento general del sistema", 6, -20 },
                    { -137, false, "Filtro de cabina", 5, -20 },
                    { -136, false, "Fugas en el sistema", 4, -20 },
                    { -135, false, "Condensador", 3, -20 },
                    { -134, false, "Compresor", 2, -20 },
                    { -133, false, "Nivel de gas refrigerante", 1, -20 },
                    { -132, false, "Balanceo", 7, -19 },
                    { -131, false, "Alineación", 6, -19 },
                    { -130, false, "Bujes de suspensión", 5, -19 },
                    { -129, false, "Terminales de dirección", 4, -19 },
                    { -128, false, "Rótulas", 3, -19 },
                    { -127, false, "Amortiguadores traseros", 2, -19 },
                    { -126, false, "Amortiguadores delanteros", 1, -19 },
                    { -125, true, "Sistema ABS", 7, -18 },
                    { -124, false, "Freno de mano", 6, -18 },
                    { -123, false, "Mangueras de freno", 5, -18 },
                    { -122, false, "Nivel de líquido de frenos", 4, -18 },
                    { -121, false, "Discos/tambores", 3, -18 },
                    { -120, false, "Balatas/pastillas traseras", 2, -18 },
                    { -119, false, "Balatas/pastillas delanteras", 1, -18 },
                    { -118, true, "Sensor de oxígeno", 6, -17 },
                    { -117, false, "Fugas de escape", 5, -17 },
                    { -116, false, "Abrazaderas y soportes", 4, -17 },
                    { -115, false, "Catalizador", 3, -17 },
                    { -114, false, "Tubo de escape", 2, -17 },
                    { -113, false, "Mofle/silenciador", 1, -17 },
                    { -112, false, "Luces interiores", 7, -16 },
                    { -111, false, "Luces exteriores", 6, -16 },
                    { -110, false, "Cableado y conexiones", 5, -16 },
                    { -109, false, "Fusibles y relevadores", 4, -16 },
                    { -108, false, "Motor de arranque", 3, -16 },
                    { -107, false, "Alternador", 2, -16 },
                    { -106, false, "Batería (carga y bornes)", 1, -16 },
                    { -105, false, "Bomba de agua", 7, -15 },
                    { -104, false, "Termostato", 6, -15 },
                    { -103, false, "Ventilador de enfriamiento", 5, -15 },
                    { -102, false, "Tapón de radiador", 4, -15 },
                    { -101, false, "Estado de mangueras", 3, -15 },
                    { -100, false, "Fugas en radiador", 2, -15 },
                    { -99, false, "Nivel de refrigerante", 1, -15 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTemplateSpecialties_TemplateId",
                table: "InspectionTemplateSpecialties",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionTemplateSpecialties");

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -138);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -137);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -136);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -135);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -134);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -133);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -132);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -131);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -130);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -129);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -128);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -127);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -126);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -125);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -124);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -123);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -122);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -121);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -120);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -119);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -118);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -117);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -116);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -115);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -114);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -113);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -112);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -111);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -110);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -109);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -108);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -107);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -106);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -105);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -104);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -103);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -102);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -101);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -100);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateItems",
                keyColumn: "Id",
                keyValue: -99);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateSections",
                keyColumn: "Id",
                keyValue: -20);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateSections",
                keyColumn: "Id",
                keyValue: -19);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateSections",
                keyColumn: "Id",
                keyValue: -18);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateSections",
                keyColumn: "Id",
                keyValue: -17);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateSections",
                keyColumn: "Id",
                keyValue: -16);

            migrationBuilder.DeleteData(
                table: "InspectionTemplateSections",
                keyColumn: "Id",
                keyValue: -15);

            migrationBuilder.DeleteData(
                table: "InspectionTemplates",
                keyColumn: "Id",
                keyValue: -8);

            migrationBuilder.DeleteData(
                table: "InspectionTemplates",
                keyColumn: "Id",
                keyValue: -7);

            migrationBuilder.DeleteData(
                table: "InspectionTemplates",
                keyColumn: "Id",
                keyValue: -6);

            migrationBuilder.DeleteData(
                table: "InspectionTemplates",
                keyColumn: "Id",
                keyValue: -5);

            migrationBuilder.DeleteData(
                table: "InspectionTemplates",
                keyColumn: "Id",
                keyValue: -4);

            migrationBuilder.DeleteData(
                table: "InspectionTemplates",
                keyColumn: "Id",
                keyValue: -3);
        }
    }
}
