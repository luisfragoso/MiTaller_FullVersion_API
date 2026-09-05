using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddGenericInspectionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InspectionId",
                table: "Quotations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InspectionTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleFormat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    GeneralObservations = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inspections_InspectionTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "InspectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inspections_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inspections_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionTemplateSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTemplateSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTemplateSections_InspectionTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "InspectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionFiles_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResultsSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionHistories_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionSectionComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionSectionComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionSectionComments_InspectionTemplateSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "InspectionTemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionSectionComments_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionTemplateItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    AllowNotApplicable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTemplateItems_InspectionTemplateSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "InspectionTemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionResultItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    TemplateItemId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionResultItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionResultItems_InspectionTemplateItems_TemplateItemId",
                        column: x => x.TemplateItemId,
                        principalTable: "InspectionTemplateItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionResultItems_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplates",
                columns: new[] { "Id", "IsActive", "Name", "VehicleFormat" },
                values: new object[] { -1, true, "Inspección de Yate", "Yate" });

            migrationBuilder.InsertData(
                table: "InspectionTemplateSections",
                columns: new[] { "Id", "Name", "Order", "TemplateId" },
                values: new object[,]
                {
                    { -7, "Cubierta e Interior", 7, -1 },
                    { -6, "Equipo de Seguridad", 6, -1 },
                    { -5, "Navegación y Electrónica", 5, -1 },
                    { -4, "Sentina y Achique", 4, -1 },
                    { -3, "Sistema Eléctrico", 3, -1 },
                    { -2, "Sala de Máquinas / Motor", 2, -1 },
                    { -1, "Casco y Exterior", 1, -1 }
                });

            migrationBuilder.InsertData(
                table: "InspectionTemplateItems",
                columns: new[] { "Id", "AllowNotApplicable", "Name", "Order", "SectionId" },
                values: new object[,]
                {
                    { -47, true, "Winches", 10, -7 },
                    { -46, true, "Velas", 9, -7 },
                    { -45, true, "Jarcia/aparejos", 8, -7 },
                    { -44, true, "Mástil", 7, -7 },
                    { -43, false, "Aire acondicionado/calefacción", 6, -7 },
                    { -42, false, "Sistema de agua dulce", 5, -7 },
                    { -41, false, "Plomería y sanitarios", 4, -7 },
                    { -40, false, "Tapicería/interior", 3, -7 },
                    { -39, false, "Malacate de ancla", 2, -7 },
                    { -38, false, "Herrajes de cubierta", 1, -7 },
                    { -37, false, "Botiquín", 6, -6 },
                    { -36, false, "EPIRB", 5, -6 },
                    { -35, false, "Balsa salvavidas", 4, -6 },
                    { -34, false, "Señales de auxilio", 3, -6 },
                    { -33, false, "Extintores", 2, -6 },
                    { -32, false, "Chalecos salvavidas", 1, -6 },
                    { -31, false, "Compás", 6, -5 },
                    { -30, false, "Sonda", 5, -5 },
                    { -29, false, "Piloto automático", 4, -5 },
                    { -28, false, "Radio VHF", 3, -5 },
                    { -27, false, "Radar", 2, -5 },
                    { -26, false, "GPS/chartplotter", 1, -5 },
                    { -25, false, "Tapones de drenaje", 4, -4 },
                    { -24, false, "Válvulas de pasacascos", 3, -4 },
                    { -23, false, "Condición de la sentina", 2, -4 },
                    { -22, false, "Bombas de achique", 1, -4 },
                    { -21, false, "Luces de navegación", 6, -3 },
                    { -20, false, "Planta/inversor", 5, -3 },
                    { -19, false, "Bomba de sentina", 4, -3 },
                    { -18, false, "Cableado", 3, -3 },
                    { -17, false, "Alternador", 2, -3 },
                    { -16, false, "Baterías", 1, -3 },
                    { -15, false, "Transmisión/cola", 7, -2 },
                    { -14, false, "Sistema de enfriamiento", 6, -2 },
                    { -13, false, "Sistema de escape", 5, -2 },
                    { -12, false, "Sistema de combustible", 4, -2 },
                    { -11, false, "Bandas y mangueras", 3, -2 },
                    { -10, false, "Niveles y fugas de aceite", 2, -2 },
                    { -9, false, "Motor(es)", 1, -2 },
                    { -8, false, "Defensas", 8, -1 },
                    { -7, false, "Hélice de proa", 7, -1 },
                    { -6, false, "Quilla", 6, -1 },
                    { -5, false, "Ánodos de sacrificio", 5, -1 },
                    { -4, false, "Timón", 4, -1 },
                    { -3, false, "Hélices y ejes", 3, -1 },
                    { -2, false, "Pasacascos", 2, -1 },
                    { -1, false, "Casco/gelcoat", 1, -1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_InspectionId",
                table: "Quotations",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFiles_InspectionId",
                table: "InspectionFiles",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionHistories_InspectionId",
                table: "InspectionHistories",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResultItems_InspectionId",
                table: "InspectionResultItems",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResultItems_TemplateItemId",
                table: "InspectionResultItems",
                column: "TemplateItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_CustomerId",
                table: "Inspections",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_TemplateId",
                table: "Inspections",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_VehicleId",
                table: "Inspections",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_WorkshopId",
                table: "Inspections",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSectionComments_InspectionId",
                table: "InspectionSectionComments",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSectionComments_SectionId",
                table: "InspectionSectionComments",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTemplateItems_SectionId",
                table: "InspectionTemplateItems",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTemplateSections_TemplateId",
                table: "InspectionTemplateSections",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Inspections_InspectionId",
                table: "Quotations",
                column: "InspectionId",
                principalTable: "Inspections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Inspections_InspectionId",
                table: "Quotations");

            migrationBuilder.DropTable(
                name: "InspectionFiles");

            migrationBuilder.DropTable(
                name: "InspectionHistories");

            migrationBuilder.DropTable(
                name: "InspectionResultItems");

            migrationBuilder.DropTable(
                name: "InspectionSectionComments");

            migrationBuilder.DropTable(
                name: "InspectionTemplateItems");

            migrationBuilder.DropTable(
                name: "Inspections");

            migrationBuilder.DropTable(
                name: "InspectionTemplateSections");

            migrationBuilder.DropTable(
                name: "InspectionTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_InspectionId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "InspectionId",
                table: "Quotations");
        }
    }
}
