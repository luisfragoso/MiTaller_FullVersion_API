using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionTemplateFormatsAndBoatCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionTemplateFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    VehicleFormat = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTemplateFormats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTemplateFormats_InspectionTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "InspectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "Name", "Type" },
                values: new object[,]
                {
                    { -25, "FibraMex", "Bote" },
                    { -24, "Súper Panga", "Bote" },
                    { -23, "Sea Eagle", "Bote" },
                    { -22, "Achilles", "Bote" },
                    { -21, "AB Inflatables", "Bote" },
                    { -20, "Zodiac", "Bote" },
                    { -19, "Tracker", "Bote" },
                    { -18, "Lund", "Bote" },
                    { -17, "Yellowfin", "Lancha" },
                    { -16, "Chaparral", "Lancha" },
                    { -15, "Robalo", "Lancha" },
                    { -14, "Grady-White", "Lancha" },
                    { -13, "Sea Fox", "Lancha" },
                    { -12, "Yamaha", "Lancha" },
                    { -11, "Bayliner", "Lancha" },
                    { -10, "Boston Whaler", "Lancha" },
                    { -9, "Viking", "Yate" },
                    { -8, "Pershing", "Yate" },
                    { -7, "Riva", "Yate" },
                    { -6, "Sea Ray", "Yate" },
                    { -5, "Princess", "Yate" },
                    { -4, "Ferretti", "Yate" },
                    { -3, "Sunseeker", "Yate" },
                    { -2, "Azimut", "Yate" }
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplateFormats",
                columns: new[] { "Id", "TemplateId", "VehicleFormat" },
                values: new object[,]
                {
                    { -3, -1, "Bote" },
                    { -2, -1, "Lancha" },
                    { -1, -1, "Yate" }
                });

            migrationBuilder.InsertData(
                table: "VehicleModels",
                columns: new[] { "Id", "BrandId", "Model" },
                values: new object[,]
                {
                    { -71, -25, "Delfín 19" },
                    { -70, -25, "Marlín 28" },
                    { -69, -25, "Tiburón 23" },
                    { -68, -24, "Panga 31" },
                    { -67, -24, "Panga 26" },
                    { -66, -24, "Panga 22" },
                    { -65, -23, "FastCat 465" },
                    { -64, -23, "Sport Runabout 344" },
                    { -63, -23, "330" },
                    { -62, -22, "Ranger R-400" },
                    { -61, -22, "HB-260" },
                    { -60, -22, "LT-2" },
                    { -59, -21, "Oceanus 24" },
                    { -58, -21, "Nautilus 15" },
                    { -57, -21, "Lammina 9" },
                    { -56, -20, "Medline 7.5" },
                    { -55, -20, "Open 5.5" },
                    { -54, -20, "Cadet 310" },
                    { -53, -19, "Targa V-19" },
                    { -52, -19, "Pro Team 175" },
                    { -51, -19, "Guide V-16" },
                    { -50, -18, "2075 Tyee" },
                    { -49, -18, "1875 Impact" },
                    { -48, -18, "1650 Rebel" },
                    { -47, -17, "42" },
                    { -46, -17, "32" },
                    { -45, -17, "24 Bay" },
                    { -44, -16, "23 Fish" },
                    { -43, -16, "277 SSX" },
                    { -42, -16, "19 SSi" },
                    { -41, -15, "R302" },
                    { -40, -15, "R242" },
                    { -39, -15, "R222" },
                    { -38, -14, "Canyon 456" },
                    { -37, -14, "Freedom 275" },
                    { -36, -14, "Fisherman 236" },
                    { -35, -13, "328 Commander" },
                    { -34, -13, "200 Viper" },
                    { -33, -12, "275SD" },
                    { -32, -12, "212X" },
                    { -31, -12, "190 FSH" },
                    { -30, -11, "DX2200" },
                    { -29, -11, "VR6" },
                    { -28, -11, "Element" },
                    { -27, -10, "Dauntless" },
                    { -26, -10, "Outrage" },
                    { -25, -10, "Montauk" },
                    { -24, -9, "46 Billfish" },
                    { -23, -9, "90 Enclosed Bridge" },
                    { -22, -9, "54 Convertible" },
                    { -21, -8, "GTX116" },
                    { -20, -8, "8X" },
                    { -19, -8, "6X" },
                    { -18, -7, "Aquariva" },
                    { -17, -7, "88 Domino" },
                    { -16, -7, "Rivamare" },
                    { -15, -6, "L Series" },
                    { -14, -6, "Sundancer" },
                    { -13, -5, "X95" },
                    { -12, -5, "F55" },
                    { -11, -5, "V50" },
                    { -10, -4, "Yachts 850" },
                    { -9, -4, "720" },
                    { -8, -4, "500" },
                    { -7, -3, "Ocean" },
                    { -6, -3, "Manhattan" },
                    { -5, -3, "Predator" },
                    { -4, -2, "Magellano" },
                    { -3, -2, "Atlantis" },
                    { -2, -2, "Grande" }
                });

            migrationBuilder.InsertData(
                table: "VehicleVersions",
                columns: new[] { "Id", "VehicleModelId", "Version" },
                values: new object[,]
                {
                    { -76, -71, "19" },
                    { -75, -70, "28" },
                    { -74, -69, "23" },
                    { -73, -68, "31" },
                    { -72, -67, "26" },
                    { -71, -66, "22" },
                    { -70, -65, "15" },
                    { -69, -64, "11" },
                    { -68, -63, "11" },
                    { -67, -62, "13" },
                    { -66, -61, "9" },
                    { -65, -60, "8" },
                    { -64, -59, "24" },
                    { -63, -58, "15" },
                    { -62, -57, "9" },
                    { -61, -56, "24" },
                    { -60, -55, "18" },
                    { -59, -54, "10" },
                    { -58, -53, "19" },
                    { -57, -52, "17" },
                    { -56, -51, "16" },
                    { -55, -50, "20" },
                    { -54, -49, "18" },
                    { -53, -48, "16" },
                    { -52, -47, "42" },
                    { -51, -46, "32" },
                    { -50, -45, "24" },
                    { -49, -44, "23" },
                    { -48, -43, "27" },
                    { -47, -42, "19" },
                    { -46, -41, "30" },
                    { -45, -40, "24" },
                    { -44, -39, "22" },
                    { -43, -38, "45" },
                    { -42, -37, "27" },
                    { -41, -36, "23" },
                    { -40, -35, "32" },
                    { -39, -34, "20" },
                    { -38, -33, "27" },
                    { -37, -32, "21" },
                    { -36, -31, "19" },
                    { -35, -30, "22" },
                    { -34, -29, "19" },
                    { -33, -28, "16" },
                    { -32, -27, "21" },
                    { -31, -26, "28" },
                    { -30, -26, "23" },
                    { -29, -25, "19" },
                    { -28, -24, "46" },
                    { -27, -23, "90" },
                    { -26, -22, "54" },
                    { -25, -21, "116" },
                    { -24, -20, "88" },
                    { -23, -19, "66" },
                    { -22, -18, "33" },
                    { -21, -17, "88" },
                    { -20, -16, "38" },
                    { -19, -15, "46" },
                    { -18, -14, "65" },
                    { -17, -14, "40" },
                    { -16, -13, "97" },
                    { -15, -12, "57" },
                    { -14, -11, "53" },
                    { -13, -10, "87" },
                    { -12, -9, "74" },
                    { -11, -8, "52" },
                    { -10, -7, "90" },
                    { -9, -6, "52" },
                    { -8, -5, "68" },
                    { -7, -5, "55" },
                    { -6, -4, "55" },
                    { -5, -3, "44" },
                    { -4, -3, "34" },
                    { -3, -2, "88" },
                    { -2, -2, "62" }
                });

            migrationBuilder.InsertData(
                table: "VehicleTypes",
                columns: new[] { "Id", "Type", "VehicleVersionId" },
                values: new object[,]
                {
                    { -76, "Panga de fibra", -76 },
                    { -75, "Panga de fibra", -75 },
                    { -74, "Panga de fibra", -74 },
                    { -73, "Panga de fibra", -73 },
                    { -72, "Panga de fibra", -72 },
                    { -71, "Panga de fibra", -71 },
                    { -70, "Inflable", -70 },
                    { -69, "Inflable", -69 },
                    { -68, "Inflable", -68 },
                    { -67, "Inflable", -67 },
                    { -66, "Inflable", -66 },
                    { -65, "Inflable", -65 },
                    { -64, "Inflable", -64 },
                    { -63, "Inflable", -63 },
                    { -62, "Inflable", -62 },
                    { -61, "Inflable", -61 },
                    { -60, "Inflable", -60 },
                    { -59, "Inflable", -59 },
                    { -58, "Panga de aluminio", -58 },
                    { -57, "Panga de aluminio", -57 },
                    { -56, "Panga de aluminio", -56 },
                    { -55, "Panga de aluminio", -55 },
                    { -54, "Panga de aluminio", -54 },
                    { -53, "Panga de aluminio", -53 },
                    { -52, "Center Console", -52 },
                    { -51, "Center Console", -51 },
                    { -50, "Center Console", -50 },
                    { -49, "Center Console", -49 },
                    { -48, "Bowrider", -48 },
                    { -47, "Bowrider", -47 },
                    { -46, "Center Console", -46 },
                    { -45, "Center Console", -45 },
                    { -44, "Center Console", -44 },
                    { -43, "Center Console", -43 },
                    { -42, "Center Console", -42 },
                    { -41, "Center Console", -41 },
                    { -40, "Center Console", -40 },
                    { -39, "Center Console", -39 },
                    { -38, "Jet Boat", -38 },
                    { -37, "Jet Boat", -37 },
                    { -36, "Jet Boat", -36 },
                    { -35, "Deck Boat", -35 },
                    { -34, "Bowrider", -34 },
                    { -33, "Bowrider", -33 },
                    { -32, "Center Console", -32 },
                    { -31, "Center Console", -31 },
                    { -30, "Center Console", -30 },
                    { -29, "Center Console", -29 },
                    { -28, "Convertible", -28 },
                    { -27, "Flybridge", -27 },
                    { -26, "Convertible", -26 },
                    { -25, "Flybridge", -25 },
                    { -24, "Sport Yacht", -24 },
                    { -23, "Sport Yacht", -23 },
                    { -22, "Sport Yacht", -22 },
                    { -21, "Flybridge", -21 },
                    { -20, "Sport Yacht", -20 },
                    { -19, "Flybridge", -19 },
                    { -18, "Cruiser", -18 },
                    { -17, "Cruiser", -17 },
                    { -16, "Flybridge", -16 },
                    { -15, "Flybridge", -15 },
                    { -14, "Sport Yacht", -14 },
                    { -13, "Flybridge", -13 },
                    { -12, "Flybridge", -12 },
                    { -11, "Flybridge", -11 },
                    { -10, "Flybridge", -10 },
                    { -9, "Flybridge", -9 },
                    { -8, "Sport Yacht", -8 },
                    { -7, "Sport Yacht", -7 },
                    { -6, "Cruiser", -6 },
                    { -5, "Sport Yacht", -5 },
                    { -4, "Sport Yacht", -4 },
                    { -3, "Flybridge", -3 },
                    { -2, "Flybridge", -2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTemplateFormats_TemplateId",
                table: "InspectionTemplateFormats",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionTemplateFormats");

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -76);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -75);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -74);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -73);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -72);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -71);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -70);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -69);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -68);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -67);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -66);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -65);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -64);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -63);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -62);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -61);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -60);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -59);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -58);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -57);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -56);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -55);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -54);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -53);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -52);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -51);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -50);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -49);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -48);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -47);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -46);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -45);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -44);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -43);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -42);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -41);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -40);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -39);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -38);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -37);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -36);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -35);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -34);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -33);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -32);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -31);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -30);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -29);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -28);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -27);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -26);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -25);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -24);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -23);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -22);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -21);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -20);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -19);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -18);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -17);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -16);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -15);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -14);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -13);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -12);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -11);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -10);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -9);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -8);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -7);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -6);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -5);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -4);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -76);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -75);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -74);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -73);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -72);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -71);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -70);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -69);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -68);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -67);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -66);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -65);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -64);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -63);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -62);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -61);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -60);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -59);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -58);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -57);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -56);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -55);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -54);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -53);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -52);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -51);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -50);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -49);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -48);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -47);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -46);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -45);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -44);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -43);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -42);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -41);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -40);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -39);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -38);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -37);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -36);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -35);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -34);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -33);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -32);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -31);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -30);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -29);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -28);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -27);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -26);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -25);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -24);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -23);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -22);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -21);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -20);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -19);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -18);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -17);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -16);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -15);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -14);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -13);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -12);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -11);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -10);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -9);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -8);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -7);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -6);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -5);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -4);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "VehicleVersions",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -71);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -70);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -69);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -68);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -67);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -66);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -65);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -64);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -63);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -62);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -61);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -60);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -59);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -58);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -57);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -56);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -55);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -54);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -53);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -52);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -51);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -50);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -49);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -48);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -47);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -46);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -45);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -44);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -43);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -42);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -41);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -40);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -39);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -38);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -37);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -36);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -35);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -34);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -33);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -32);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -31);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -30);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -29);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -28);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -27);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -26);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -25);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -24);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -23);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -22);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -21);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -20);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -19);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -18);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -17);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -16);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -15);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -14);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -13);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -12);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -11);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -10);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -9);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -8);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -7);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -6);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -5);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -4);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -25);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -24);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -23);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -22);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -21);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -20);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -19);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -18);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -17);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -16);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -15);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -14);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -13);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -12);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -11);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -10);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -9);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -8);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -7);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -6);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -5);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -4);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: -2);
        }
    }
}
