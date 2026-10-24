using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    // Not read by the app yet - Automóvil/Motocicleta stay excluded from the
    // generic engine's format list until Fase 3.
    // Per-wheel-position grouping (eg Delantero Derecho) has no equivalent in
    // this schema, so position is folded into each item's Name instead.
    // Chasis sections get zero items on purpose: it's a tap-diagram step,
    // only its photos/comment are persisted, both already generic.
    public partial class AddCarAndMotorcycleInspectionTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InspectionTemplates",
                columns: new[] { "Id", "IsActive", "Name", "VehicleFormat" },
                values: new object[,]
                {
                    { -10, true, "Inspección de Automóvil", "Automóvil" },
                    { -11, true, "Inspección de Motocicleta", "Motocicleta" }
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplateFormats",
                columns: new[] { "Id", "TemplateId", "VehicleFormat" },
                values: new object[,]
                {
                    { -6, -10, "Automóvil" },
                    { -7, -11, "Motocicleta" }
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplateSections",
                columns: new[] { "Id", "Name", "Order", "TemplateId" },
                values: new object[,]
                {
                    { -27, "Frenos y neumáticos", 1, -10 },
                    { -28, "Interior y exterior", 2, -10 },
                    { -29, "Motor", 3, -10 },
                    { -30, "Batería", 4, -10 },
                    { -31, "Suspensión y dirección", 5, -10 },
                    { -32, "Chasis", 6, -10 },
                    { -33, "Frenos y neumáticos", 1, -11 },
                    { -34, "Luces y controles", 2, -11 },
                    { -35, "Marco y suspensión", 3, -11 },
                    { -36, "Aceite y otros niveles", 4, -11 },
                    { -37, "Batería", 5, -11 },
                    { -38, "Chasis", 6, -11 }
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplateItems",
                columns: new[] { "Id", "AllowNotApplicable", "Name", "Order", "SectionId" },
                values: new object[,]
                {
                    // Automóvil - Frenos y neumáticos (-27): 3 items x 4 posiciones
                    { -174, false, "Freno - Delantero Derecho", 1, -27 },
                    { -175, false, "Dibujo de llanta - Delantero Derecho", 2, -27 },
                    { -176, false, "Alineación - Delantero Derecho", 3, -27 },
                    { -177, false, "Freno - Delantero Izquierdo", 4, -27 },
                    { -178, false, "Dibujo de llanta - Delantero Izquierdo", 5, -27 },
                    { -179, false, "Alineación - Delantero Izquierdo", 6, -27 },
                    { -180, false, "Freno - Posterior Derecho", 7, -27 },
                    { -181, false, "Dibujo de llanta - Posterior Derecho", 8, -27 },
                    { -182, false, "Alineación - Posterior Derecho", 9, -27 },
                    { -183, false, "Freno - Posterior Izquierdo", 10, -27 },
                    { -184, false, "Dibujo de llanta - Posterior Izquierdo", 11, -27 },
                    { -185, false, "Alineación - Posterior Izquierdo", 12, -27 },

                    // Automóvil - Interior y exterior (-28)
                    { -186, false, "Frenos", 1, -28 },
                    { -187, false, "Dibujo de llanta", 2, -28 },
                    { -188, false, "Alineación", 3, -28 },
                    { -189, false, "Luces delanteras", 4, -28 },
                    { -190, false, "Luces traseras", 5, -28 },
                    { -191, false, "Luces de giro", 6, -28 },
                    { -192, false, "Luces de freno", 7, -28 },
                    { -193, false, "Luces de advertencia de peligro", 8, -28 },
                    { -194, false, "Líquido del limpia parabrisas", 9, -28 },
                    { -195, false, "Funcionamiento de limpia parabrisas", 10, -28 },
                    { -196, false, "Escobillas del limpia parabrisas", 11, -28 },
                    { -197, false, "Condición del limpia parabrisas", 12, -28 },
                    { -198, false, "Espejos", 13, -28 },
                    { -199, false, "Freno de emergencia", 14, -28 },
                    { -200, false, "Claxon", 15, -28 },
                    { -201, false, "Tapa de tanque de combustible", 16, -28 },
                    { -202, false, "Filtro de aire acondicionado", 17, -28 },
                    { -203, false, "Luces de reversa", 18, -28 },
                    { -204, false, "Luz de placa", 19, -28 },
                    { -205, false, "Cinturones de seguridad", 20, -28 },

                    // Automóvil - Motor (-29)
                    { -206, false, "Niveles de líquido aceite", 1, -29 },
                    { -207, false, "Niveles de líquido refrigerante", 2, -29 },
                    { -208, false, "Niveles de líquido frenos", 3, -29 },
                    { -209, false, "Filtro de aire", 4, -29 },
                    { -210, false, "Mangueras del sistema de refrigeración", 5, -29 },
                    { -211, false, "Mangueras de calefacción", 6, -29 },
                    { -212, false, "Condensador del aire acondicionado", 7, -29 },
                    { -213, false, "Líquido de transmisión", 8, -29 },
                    { -214, false, "Líquido de dirección hidráulica", 9, -29 },
                    { -215, false, "Banda de accesorios", 10, -29 },
                    { -216, false, "Sistema de escape", 11, -29 },

                    // Automóvil - Batería (-30)
                    { -217, false, "Terminales de batería", 1, -30 },
                    { -218, false, "Cables", 2, -30 },
                    { -219, false, "Montaje", 3, -30 },
                    { -220, false, "Condiciones generales de la batería", 4, -30 },

                    // Automóvil - Suspensión y dirección (-31)
                    { -221, false, "Amortiguadores delanteros", 1, -31 },
                    { -222, false, "Amortiguadores traseros", 2, -31 },
                    { -223, false, "Rótulas", 3, -31 },
                    { -224, false, "Cremallera y terminales de dirección", 4, -31 },
                    { -225, false, "Bujes de suspensión", 5, -31 },

                    // Motocicleta - Frenos y neumáticos (-33): 6 items x 2 posiciones
                    { -226, false, "Radios - Delantero", 1, -33 },
                    { -227, false, "Dibujo de llanta - Delantero", 2, -33 },
                    { -228, false, "Rodamientos - Delantero", 3, -33 },
                    { -229, false, "Sellos - Delantero", 4, -33 },
                    { -230, false, "Forro de frenos - Delantero", 5, -33 },
                    { -231, false, "Patrón de desgaste - Delantero", 6, -33 },
                    { -232, false, "Radios - Posterior", 7, -33 },
                    { -233, false, "Dibujo de llanta - Posterior", 8, -33 },
                    { -234, false, "Rodamientos - Posterior", 9, -33 },
                    { -235, false, "Sellos - Posterior", 10, -33 },
                    { -236, false, "Forro de frenos - Posterior", 11, -33 },
                    { -237, false, "Patrón de desgaste - Posterior", 12, -33 },

                    // Motocicleta - Luces y controles (-34)
                    { -238, false, "Faro", 1, -34 },
                    { -239, false, "Luz trasera", 2, -34 },
                    { -240, false, "Luces de giro", 3, -34 },
                    { -241, false, "Luces de emergencia", 4, -34 },
                    { -242, false, "Luz de freno", 5, -34 },
                    { -243, false, "Luz de placa", 6, -34 },
                    { -244, false, "Espejo izquierdo", 7, -34 },
                    { -245, false, "Espejo derecho", 8, -34 },
                    { -246, false, "Interruptores", 9, -34 },
                    { -247, false, "Cableado", 10, -34 },
                    { -248, false, "Manubrios", 11, -34 },
                    { -249, false, "Palancas y pedal", 12, -34 },
                    { -250, false, "Mangueras", 13, -34 },
                    { -251, false, "Palanca del acelerador", 14, -34 },
                    { -252, false, "Palanca de clutch", 15, -34 },
                    { -253, false, "Tapa de tanque de combustible", 16, -34 },
                    { -254, false, "Instrumentos del tablero", 17, -34 },
                    { -255, false, "Claxon", 18, -34 },

                    // Motocicleta - Marco y suspensión (-35)
                    { -256, false, "Condición del marco", 1, -35 },
                    { -257, false, "Rodamientos de la dirección", 2, -35 },
                    { -258, false, "Bujes del basculante", 3, -35 },
                    { -259, false, "Horquillas delanteras", 4, -35 },
                    { -260, false, "Amortiguadores traseros", 5, -35 },
                    { -261, false, "Cadena o correa", 6, -35 },
                    { -262, false, "Sujetadores", 7, -35 },
                    { -263, false, "Soporte central", 8, -35 },
                    { -264, false, "Soporte lateral", 9, -35 },

                    // Motocicleta - Aceite y otros niveles (-36)
                    { -265, false, "Aceite del motor", 1, -36 },
                    { -266, false, "Aceite de engranajes", 2, -36 },
                    { -267, false, "Aceite de transmisión por eje", 3, -36 },
                    { -268, false, "Fluido hidráulico", 4, -36 },
                    { -269, false, "Refrigerante", 5, -36 },
                    { -270, false, "Combustible", 6, -36 },
                    { -271, false, "Fugas", 7, -36 },

                    // Motocicleta - Batería (-37)
                    { -272, false, "Terminales de batería", 1, -37 },
                    { -273, false, "Cables", 2, -37 },
                    { -274, false, "Montaje", 3, -37 },
                    { -275, false, "Condiciones generales de la batería", 4, -37 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (var id = -174; id >= -275; id--)
            {
                migrationBuilder.DeleteData(table: "InspectionTemplateItems", keyColumn: "Id", keyValue: id);
            }

            for (var id = -27; id >= -38; id--)
            {
                migrationBuilder.DeleteData(table: "InspectionTemplateSections", keyColumn: "Id", keyValue: id);
            }

            migrationBuilder.DeleteData(table: "InspectionTemplateFormats", keyColumn: "Id", keyValue: -6);
            migrationBuilder.DeleteData(table: "InspectionTemplateFormats", keyColumn: "Id", keyValue: -7);

            migrationBuilder.DeleteData(table: "InspectionTemplates", keyColumn: "Id", keyValue: -10);
            migrationBuilder.DeleteData(table: "InspectionTemplates", keyColumn: "Id", keyValue: -11);
        }
    }
}
