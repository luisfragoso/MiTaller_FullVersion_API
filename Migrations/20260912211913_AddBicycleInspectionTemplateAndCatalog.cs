using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddBicycleInspectionTemplateAndCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InspectionTemplates",
                columns: new[] { "Id", "IsActive", "Name", "VehicleFormat" },
                values: new object[] { -9, true, "Inspección de Bicicleta", "Bicicleta" });

            migrationBuilder.InsertData(
                table: "InspectionTemplateFormats",
                columns: new[] { "Id", "TemplateId", "VehicleFormat" },
                values: new object[] { -5, -9, "Bicicleta" });

            migrationBuilder.InsertData(
                table: "InspectionTemplateSections",
                columns: new[] { "Id", "Name", "Order", "TemplateId" },
                values: new object[,]
                {
                    { -21, "Cuadro y Dirección", 1, -9 },
                    { -22, "Transmisión", 2, -9 },
                    { -23, "Frenos", 3, -9 },
                    { -24, "Ruedas y Llantas", 4, -9 },
                    { -25, "Sistema Eléctrico (Bicicletas Eléctricas)", 5, -9 },
                    { -26, "Accesorios y Seguridad", 6, -9 }
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplateItems",
                columns: new[] { "Id", "AllowNotApplicable", "Name", "Order", "SectionId" },
                values: new object[,]
                {
                    { -139, false, "Cuadro (grietas/golpes)", 1, -21 },
                    { -140, false, "Horquilla", 2, -21 },
                    { -141, false, "Juego de dirección (tazas)", 3, -21 },
                    { -142, false, "Manubrio", 4, -21 },
                    { -143, false, "Potencia (stem)", 5, -21 },
                    { -144, false, "Puños/cintas de manubrio", 6, -21 },
                    { -145, false, "Cadena (desgaste/lubricación)", 1, -22 },
                    { -146, false, "Cassette/piñones", 2, -22 },
                    { -147, true, "Desviador delantero", 3, -22 },
                    { -148, false, "Desviador trasero", 4, -22 },
                    { -149, false, "Bielas y plato(s)", 5, -22 },
                    { -150, false, "Pedales", 6, -22 },
                    { -151, false, "Manijas de cambio (shifters)", 7, -22 },
                    { -152, false, "Pastillas/zapatas de freno", 1, -23 },
                    { -153, true, "Discos de freno", 2, -23 },
                    { -154, false, "Cables/mangueras de freno", 3, -23 },
                    { -155, false, "Palancas de freno", 4, -23 },
                    { -156, false, "Ajuste y potencia de frenado", 5, -23 },
                    { -157, false, "Rines", 1, -24 },
                    { -158, false, "Rayos y tensión", 2, -24 },
                    { -159, false, "Bujes (delantero/trasero)", 3, -24 },
                    { -160, false, "Llantas (desgaste)", 4, -24 },
                    { -161, false, "Presión de llantas", 5, -24 },
                    { -162, false, "Cámaras/válvulas", 6, -24 },
                    { -163, true, "Batería", 1, -25 },
                    { -164, true, "Motor eléctrico", 2, -25 },
                    { -165, true, "Controlador/display", 3, -25 },
                    { -166, true, "Cableado eléctrico", 4, -25 },
                    { -167, true, "Cargador", 5, -25 },
                    { -168, false, "Sillín/asiento", 1, -26 },
                    { -169, false, "Tija de asiento", 2, -26 },
                    { -170, true, "Luces delantera/trasera", 3, -26 },
                    { -171, true, "Reflejantes", 4, -26 },
                    { -172, true, "Timbre/claxon", 5, -26 },
                    { -173, true, "Guardafangos", 6, -26 }
                });

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "Name", "Type" },
                values: new object[,]
                {
                    { -34, "Trek", "Bicicleta" },
                    { -35, "Specialized", "Bicicleta" },
                    { -36, "Giant", "Bicicleta" },
                    { -37, "Cannondale", "Bicicleta" },
                    { -38, "Scott", "Bicicleta" },
                    { -39, "Bianchi", "Bicicleta" },
                    { -40, "Cervélo", "Bicicleta" },
                    { -41, "Santa Cruz", "Bicicleta" }
                });

            migrationBuilder.InsertData(
                table: "VehicleModels",
                columns: new[] { "Id", "BrandId", "Model" },
                values: new object[,]
                {
                    { -96, -34, "Domane" },
                    { -97, -34, "Marlin" },
                    { -98, -34, "Verve" },
                    { -99, -35, "Allez" },
                    { -100, -35, "Rockhopper" },
                    { -101, -35, "Turbo Vado" },
                    { -102, -36, "Defy" },
                    { -103, -36, "Talon" },
                    { -104, -36, "Escape" },
                    { -105, -37, "CAAD" },
                    { -106, -37, "Trail" },
                    { -107, -37, "Quick" },
                    { -108, -38, "Addict" },
                    { -109, -38, "Aspect" },
                    { -110, -38, "Sub Cross" },
                    { -111, -39, "Via Nirone" },
                    { -112, -39, "Duel" },
                    { -113, -39, "C-Sport" },
                    { -114, -40, "R5" },
                    { -115, -40, "Caledonia" },
                    { -116, -40, "S5" },
                    { -117, -41, "Tallboy" },
                    { -118, -41, "Hightower" },
                    { -119, -41, "Chameleon" }
                });

            migrationBuilder.InsertData(
                table: "VehicleVersions",
                columns: new[] { "Id", "VehicleModelId", "Version" },
                values: new object[,]
                {
                    { -101, -96, "SL 5" },
                    { -102, -97, "7" },
                    { -103, -98, "2" },
                    { -104, -99, "Sport" },
                    { -105, -100, "Comp" },
                    { -106, -101, "3.0" },
                    { -107, -102, "Advanced 2" },
                    { -108, -103, "3" },
                    { -109, -104, "3" },
                    { -110, -105, "13" },
                    { -111, -106, "5" },
                    { -112, -107, "CX 3" },
                    { -113, -108, "20" },
                    { -114, -109, "950" },
                    { -115, -110, "10" },
                    { -116, -111, "7 Sora" },
                    { -117, -112, "27.3" },
                    { -118, -113, "1" },
                    { -119, -114, "Force AXS" },
                    { -120, -115, "5 GRX" },
                    { -121, -116, "Ultegra Di2" },
                    { -122, -117, "C S" },
                    { -123, -118, "C R" },
                    { -124, -119, "AL D" }
                });

            migrationBuilder.InsertData(
                table: "VehicleTypes",
                columns: new[] { "Id", "Type", "VehicleVersionId" },
                values: new object[,]
                {
                    { -101, "Ruta", -101 },
                    { -102, "Montaña", -102 },
                    { -103, "Urbana", -103 },
                    { -104, "Ruta", -104 },
                    { -105, "Montaña", -105 },
                    { -106, "Eléctrica", -106 },
                    { -107, "Ruta", -107 },
                    { -108, "Montaña", -108 },
                    { -109, "Urbana", -109 },
                    { -110, "Ruta", -110 },
                    { -111, "Montaña", -111 },
                    { -112, "Urbana", -112 },
                    { -113, "Ruta", -113 },
                    { -114, "Montaña", -114 },
                    { -115, "Urbana", -115 },
                    { -116, "Ruta", -116 },
                    { -117, "Montaña", -117 },
                    { -118, "Urbana", -118 },
                    { -119, "Ruta", -119 },
                    { -120, "Gravel", -120 },
                    { -121, "Ruta", -121 },
                    { -122, "Montaña", -122 },
                    { -123, "Montaña", -123 },
                    { -124, "Montaña", -124 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (var id = -101; id >= -124; id--)
            {
                migrationBuilder.DeleteData(table: "VehicleTypes", keyColumn: "Id", keyValue: id);
            }

            for (var id = -101; id >= -124; id--)
            {
                migrationBuilder.DeleteData(table: "VehicleVersions", keyColumn: "Id", keyValue: id);
            }

            for (var id = -96; id >= -119; id--)
            {
                migrationBuilder.DeleteData(table: "VehicleModels", keyColumn: "Id", keyValue: id);
            }

            for (var id = -34; id >= -41; id--)
            {
                migrationBuilder.DeleteData(table: "Brands", keyColumn: "Id", keyValue: id);
            }

            for (var id = -139; id >= -173; id--)
            {
                migrationBuilder.DeleteData(table: "InspectionTemplateItems", keyColumn: "Id", keyValue: id);
            }

            for (var id = -21; id >= -26; id--)
            {
                migrationBuilder.DeleteData(table: "InspectionTemplateSections", keyColumn: "Id", keyValue: id);
            }

            migrationBuilder.DeleteData(table: "InspectionTemplateFormats", keyColumn: "Id", keyValue: -5);

            migrationBuilder.DeleteData(table: "InspectionTemplates", keyColumn: "Id", keyValue: -9);
        }
    }
}
