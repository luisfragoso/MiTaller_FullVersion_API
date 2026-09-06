using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddAirplaneInspectionTemplateAndCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InspectionTemplates",
                columns: new[] { "Id", "IsActive", "Name", "VehicleFormat" },
                values: new object[] { -2, true, "Inspección de Avión", "Avión" });

            migrationBuilder.InsertData(
                table: "InspectionTemplateFormats",
                columns: new[] { "Id", "TemplateId", "VehicleFormat" },
                values: new object[] { -4, -2, "Avión" });

            migrationBuilder.InsertData(
                table: "InspectionTemplateSections",
                columns: new[] { "Id", "Name", "Order", "TemplateId" },
                values: new object[,]
                {
                    { -8, "Fuselaje y Exterior", 1, -2 },
                    { -9, "Motor y Hélice", 2, -2 },
                    { -10, "Sistema Eléctrico", 3, -2 },
                    { -11, "Aviónica y Navegación", 4, -2 },
                    { -12, "Tren de Aterrizaje", 5, -2 },
                    { -13, "Cabina e Instrumentos", 6, -2 },
                    { -14, "Equipo de Seguridad", 7, -2 }
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplateItems",
                columns: new[] { "Id", "AllowNotApplicable", "Name", "Order", "SectionId" },
                values: new object[,]
                {
                    { -48, false, "Piel del fuselaje", 1, -8 },
                    { -49, false, "Alas y superficies de control", 2, -8 },
                    { -50, false, "Empenaje y cola", 3, -8 },
                    { -51, false, "Puertas y ventanas", 4, -8 },
                    { -52, false, "Antenas", 5, -8 },
                    { -53, true, "Descargadores de estática", 6, -8 },
                    { -54, false, "Pintura y corrosión", 7, -8 },
                    { -55, false, "Motor(es)", 1, -9 },
                    { -56, false, "Niveles y fugas de aceite", 2, -9 },
                    { -57, false, "Compresión de cilindros", 3, -9 },
                    { -58, false, "Bujías", 4, -9 },
                    { -59, false, "Filtro de aire", 5, -9 },
                    { -60, false, "Sistema de combustible", 6, -9 },
                    { -61, false, "Sistema de escape", 7, -9 },
                    { -62, false, "Bandas y mangueras", 8, -9 },
                    { -63, false, "Hélice (palas)", 9, -9 },
                    { -64, false, "Cono y spinner de hélice", 10, -9 },
                    { -65, false, "Soportes del motor", 11, -9 },
                    { -66, true, "Sistema de enfriamiento", 12, -9 },
                    { -67, false, "Batería", 1, -10 },
                    { -68, false, "Alternador/generador", 2, -10 },
                    { -69, false, "Cableado", 3, -10 },
                    { -70, false, "Luces de navegación", 4, -10 },
                    { -71, false, "Luz estroboscópica/anticolisión", 5, -10 },
                    { -72, false, "Luz de aterrizaje", 6, -10 },
                    { -73, false, "GPS", 1, -11 },
                    { -74, false, "Transpondedor", 2, -11 },
                    { -75, false, "Radio COM", 3, -11 },
                    { -76, false, "Radio NAV", 4, -11 },
                    { -77, false, "Altímetro", 5, -11 },
                    { -78, false, "Indicador de velocidad", 6, -11 },
                    { -79, false, "Brújula", 7, -11 },
                    { -80, false, "Horizonte artificial/actitud", 8, -11 },
                    { -81, true, "Piloto automático", 9, -11 },
                    { -82, false, "Tren principal", 1, -12 },
                    { -83, false, "Tren de nariz/cola", 2, -12 },
                    { -84, false, "Amortiguadores", 3, -12 },
                    { -85, false, "Frenos", 4, -12 },
                    { -86, false, "Neumáticos", 5, -12 },
                    { -87, false, "Presión de neumáticos", 6, -12 },
                    { -88, false, "Cinturones de seguridad", 1, -13 },
                    { -89, false, "Asientos", 2, -13 },
                    { -90, false, "Panel de instrumentos", 3, -13 },
                    { -91, false, "Controles de vuelo (yugo/palanca)", 4, -13 },
                    { -92, false, "Pedales de timón", 5, -13 },
                    { -93, true, "Aire acondicionado/calefacción", 6, -13 },
                    { -94, false, "Extintor", 1, -14 },
                    { -95, false, "Botiquín", 2, -14 },
                    { -96, false, "ELT (transmisor localizador de emergencia)", 3, -14 },
                    { -97, true, "Chalecos salvavidas", 4, -14 },
                    { -98, false, "Manual de vuelo y documentos", 5, -14 }
                });

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "Name", "Type" },
                values: new object[,]
                {
                    { -26, "Cessna", "Avión" },
                    { -27, "Piper", "Avión" },
                    { -28, "Beechcraft", "Avión" },
                    { -29, "Cirrus", "Avión" },
                    { -30, "Diamond Aircraft", "Avión" },
                    { -31, "Mooney", "Avión" },
                    { -32, "Bombardier", "Avión" },
                    { -33, "Embraer", "Avión" }
                });

            migrationBuilder.InsertData(
                table: "VehicleModels",
                columns: new[] { "Id", "BrandId", "Model" },
                values: new object[,]
                {
                    { -72, -26, "172 Skyhawk" },
                    { -73, -26, "182 Skylane" },
                    { -74, -26, "208 Caravan" },
                    { -75, -27, "PA-28 Cherokee" },
                    { -76, -27, "PA-32 Saratoga" },
                    { -77, -27, "PA-46 Malibu" },
                    { -78, -28, "Bonanza G36" },
                    { -79, -28, "Baron G58" },
                    { -80, -28, "King Air 350" },
                    { -81, -29, "SR20" },
                    { -82, -29, "SR22" },
                    { -83, -29, "SR22T" },
                    { -84, -30, "DA40 NG" },
                    { -85, -30, "DA42 Twin Star" },
                    { -86, -30, "DA62" },
                    { -87, -31, "M20 Ovation" },
                    { -88, -31, "M20 Acclaim" },
                    { -89, -31, "M20 Eagle" },
                    { -90, -32, "Learjet 75" },
                    { -91, -32, "Challenger 350" },
                    { -92, -32, "Global 6000" },
                    { -93, -33, "Phenom 100" },
                    { -94, -33, "Phenom 300" },
                    { -95, -33, "Legacy 500" }
                });

            migrationBuilder.InsertData(
                table: "VehicleVersions",
                columns: new[] { "Id", "VehicleModelId", "Version" },
                values: new object[,]
                {
                    { -77, -72, "SP" },
                    { -78, -73, "T" },
                    { -79, -74, "EX" },
                    { -80, -75, "180" },
                    { -81, -76, "II TC" },
                    { -82, -77, "Mirage" },
                    { -83, -78, "G36" },
                    { -84, -79, "G58" },
                    { -85, -80, "350i" },
                    { -86, -81, "G6" },
                    { -87, -82, "G6" },
                    { -88, -83, "G6" },
                    { -89, -84, "NG" },
                    { -90, -85, "VI" },
                    { -91, -86, "62" },
                    { -92, -87, "Ovation" },
                    { -93, -88, "Acclaim Ultra" },
                    { -94, -89, "Eagle" },
                    { -95, -90, "75 Liberty" },
                    { -96, -91, "350" },
                    { -97, -92, "6000" },
                    { -98, -93, "100EV" },
                    { -99, -94, "300E" },
                    { -100, -95, "500" }
                });

            migrationBuilder.InsertData(
                table: "VehicleTypes",
                columns: new[] { "Id", "Type", "VehicleVersionId" },
                values: new object[,]
                {
                    { -77, "Monomotor Pistón", -77 },
                    { -78, "Monomotor Pistón", -78 },
                    { -79, "Turbohélice", -79 },
                    { -80, "Monomotor Pistón", -80 },
                    { -81, "Monomotor Pistón", -81 },
                    { -82, "Monomotor Turbohélice", -82 },
                    { -83, "Monomotor Pistón", -83 },
                    { -84, "Bimotor Pistón", -84 },
                    { -85, "Turbohélice", -85 },
                    { -86, "Monomotor Pistón", -86 },
                    { -87, "Monomotor Pistón", -87 },
                    { -88, "Monomotor Turbo", -88 },
                    { -89, "Monomotor Pistón", -89 },
                    { -90, "Bimotor Pistón", -90 },
                    { -91, "Bimotor Pistón", -91 },
                    { -92, "Monomotor Pistón", -92 },
                    { -93, "Monomotor Turbo", -93 },
                    { -94, "Monomotor Pistón", -94 },
                    { -95, "Jet Ejecutivo", -95 },
                    { -96, "Jet Ejecutivo", -96 },
                    { -97, "Jet Ejecutivo", -97 },
                    { -98, "Jet Ejecutivo", -98 },
                    { -99, "Jet Ejecutivo", -99 },
                    { -100, "Jet Ejecutivo", -100 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (var id = -77; id >= -100; id--)
            {
                migrationBuilder.DeleteData(table: "VehicleTypes", keyColumn: "Id", keyValue: id);
            }

            for (var id = -77; id >= -100; id--)
            {
                migrationBuilder.DeleteData(table: "VehicleVersions", keyColumn: "Id", keyValue: id);
            }

            for (var id = -72; id >= -95; id--)
            {
                migrationBuilder.DeleteData(table: "VehicleModels", keyColumn: "Id", keyValue: id);
            }

            for (var id = -26; id >= -33; id--)
            {
                migrationBuilder.DeleteData(table: "Brands", keyColumn: "Id", keyValue: id);
            }

            for (var id = -48; id >= -98; id--)
            {
                migrationBuilder.DeleteData(table: "InspectionTemplateItems", keyColumn: "Id", keyValue: id);
            }

            for (var id = -8; id >= -14; id--)
            {
                migrationBuilder.DeleteData(table: "InspectionTemplateSections", keyColumn: "Id", keyValue: id);
            }

            migrationBuilder.DeleteData(table: "InspectionTemplateFormats", keyColumn: "Id", keyValue: -4);

            migrationBuilder.DeleteData(table: "InspectionTemplates", keyColumn: "Id", keyValue: -2);
        }
    }
}
