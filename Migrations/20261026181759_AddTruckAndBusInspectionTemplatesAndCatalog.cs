using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    // Camión ligero + Camión de carga share template -12; Tractocamión (-13, adds Quinta rueda)
    // and Autobús (-14, adds Área de Pasajeros) get their own. Ids are derived in order from the
    // data below, so the same rows come out in every database.
    public partial class AddTruckAndBusInspectionTemplatesAndCatalog : Migration
    {
        private record Item(string Name, bool AllowNotApplicable = false);
        private record Section(string Name, Item[] Items);
        private record Template(int Id, string Name, string VehicleFormat, string[] Formats, Section[] Sections);
        private record CatalogModel(string Model, string Version, string Type);
        private record CatalogBrand(string Format, string Name, CatalogModel[] Models);

        private const int FirstFormatId = -8;
        private const int FirstSectionId = -42;
        private const int FirstItemId = -276;
        private const int FirstBrandId = -42;
        private const int FirstModelId = -120;
        private const int FirstVersionId = -125;
        private const int FirstTypeId = -125;

        private static readonly Section Cabina = new("Cabina y Carrocería", new Item[]
        {
            new("Parabrisas y cristales"), new("Espejos retrovisores"), new("Limpiaparabrisas"),
            new("Puertas, chapas y bisagras"), new("Asiento y cinturón del conductor"),
            new("Tablero e indicadores"), new("Carrocería/caja (golpes y corrosión)"), new("Defensas y loderas"),
        });

        private static readonly Section Motor = new("Motor", new Item[]
        {
            new("Nivel y fugas de aceite"), new("Refrigerante y radiador"), new("Bandas y poleas"),
            new("Mangueras"), new("Filtro de aire"), new("Sistema de combustible (diésel)"),
            new("Turbocargador", true), new("Escape y DPF/catalizador", true), new("Soportes del motor"),
        });

        private static readonly Section Transmision = new("Transmisión y Tren motriz", new Item[]
        {
            new("Clutch/embrague"), new("Caja de velocidades"), new("Flecha cardán y crucetas"),
            new("Diferencial(es)"), new("Nivel y fugas de aceite de transmisión"), new("Toma de fuerza (PTO)", true),
        });

        // Light trucks are usually hydraulic, so the air-brake items allow "No aplica"
        private static readonly Section Frenos = new("Frenos", new Item[]
        {
            new("Balatas/zapatas"), new("Tambores/discos"), new("Líneas y mangueras de freno"),
            new("Freno de estacionamiento"), new("Compresor de aire", true), new("Tanques de aire y purga", true),
            new("Cámaras y matracas", true), new("Freno de motor", true),
        });

        private static readonly Section FrenosAire = new("Frenos de aire", new Item[]
        {
            new("Compresor de aire"), new("Tanques de aire y purga"), new("Cámaras de freno"),
            new("Matracas (ajustadores)"), new("Balatas/zapatas"), new("Tambores/discos"),
            new("Válvulas y líneas de aire"), new("Freno de motor", true),
        });

        private static readonly Section Suspension = new("Suspensión y Ejes", new Item[]
        {
            new("Muelles/hojas"), new("Bolsas de aire", true), new("Amortiguadores"),
            new("Bujes y abrazaderas"), new("Ejes y mazas"), new("Terminales y barra de dirección"),
            new("Caja de dirección"),
        });

        private static readonly Section Llantas = new("Llantas y Rines", new Item[]
        {
            new("Llantas delanteras"), new("Llantas traseras/duales"), new("Presión de llantas"),
            new("Rines"), new("Birlos y tuercas"), new("Llanta de refacción", true),
        });

        private static readonly Section Electrico = new("Sistema Eléctrico y Luces", new Item[]
        {
            new("Baterías"), new("Alternador"), new("Marcha (motor de arranque)"), new("Faros"),
            new("Luces de freno y direccionales"), new("Luces de gálibo/demarcación"),
            new("Claxon y alarma de reversa"), new("Cableado"),
        });

        private static readonly Section SeguridadCamion = new("Equipo de Seguridad", new Item[]
        {
            new("Extintor"), new("Triángulos reflejantes"), new("Botiquín"),
            new("Calzas para llantas", true), new("Cintas reflejantes"), new("Herramienta básica"),
        });

        private static readonly Section QuintaRueda = new("Quinta rueda y Remolque", new Item[]
        {
            new("Quinta rueda (placa y mordaza)"), new("Perno rey"),
            new("Líneas de aire al remolque (manitas)"), new("Conector eléctrico del remolque"),
            new("Patines/soportes del remolque", true), new("Guardafangos y loderas"),
        });

        private static readonly Section CarroceriaBus = new("Carrocería y Puertas", new Item[]
        {
            new("Parabrisas y cristales"), new("Espejos retrovisores"), new("Limpiaparabrisas"),
            new("Puertas de servicio y mecanismo"), new("Compartimentos de equipaje", true),
            new("Carrocería (golpes y corrosión)"), new("Defensas"),
        });

        private static readonly Section Pasajeros = new("Área de Pasajeros", new Item[]
        {
            new("Asientos"), new("Cinturones de seguridad", true), new("Pasamanos y barras"),
            new("Salidas de emergencia"), new("Aire acondicionado/ventilación", true),
            new("Iluminación interior"), new("Sanitario", true), new("Piso y escalones"),
        });

        private static readonly Section SeguridadBus = new("Equipo de Seguridad", new Item[]
        {
            new("Extintor"), new("Triángulos reflejantes"), new("Botiquín"),
            new("Martillos rompevidrios", true), new("Señalización de salidas"), new("Herramienta básica"),
        });

        // Zero items on purpose: the tap-diagram step, same as every other format's Chasis
        private static readonly Section Chasis = new("Chasis", System.Array.Empty<Item>());

        private static readonly Template[] Templates =
        {
            new(-12, "Inspección de Camión", "Camión de carga", new[] { "Camión ligero", "Camión de carga" },
                new[] { Cabina, Motor, Transmision, Frenos, Suspension, Llantas, Electrico, SeguridadCamion, Chasis }),
            new(-13, "Inspección de Tractocamión", "Tractocamión", new[] { "Tractocamión" },
                new[] { Cabina, Motor, Transmision, FrenosAire, Suspension, Llantas, Electrico, QuintaRueda, SeguridadCamion, Chasis }),
            new(-14, "Inspección de Autobús", "Autobús", new[] { "Autobús" },
                new[] { CarroceriaBus, Motor, Transmision, FrenosAire, Suspension, Llantas, Electrico, Pasajeros, SeguridadBus, Chasis }),
        };

        private static readonly CatalogBrand[] Catalog =
        {
            new("Camión ligero", "Isuzu", new CatalogModel[] { new("ELF 300", "Chasis cabina", "Reparto"), new("ELF 400", "Chasis cabina", "Caja seca") }),
            new("Camión ligero", "Hino", new CatalogModel[] { new("Serie 300", "616", "Reparto") }),
            new("Camión ligero", "Foton", new CatalogModel[] { new("Aumark", "Chasis cabina", "Reparto") }),
            new("Camión ligero", "Mercedes-Benz", new CatalogModel[] { new("Accelo", "1016", "Reparto") }),

            new("Camión de carga", "International", new CatalogModel[] { new("4300", "Estándar", "Torton"), new("MV", "Estándar", "Rabón") }),
            new("Camión de carga", "Freightliner", new CatalogModel[] { new("M2 106", "Estándar", "Torton") }),
            new("Camión de carga", "Hino", new CatalogModel[] { new("Serie 500", "1726", "Torton") }),
            new("Camión de carga", "Isuzu", new CatalogModel[] { new("Forward", "1100", "Rabón") }),
            new("Camión de carga", "Kenworth", new CatalogModel[] { new("T370", "Estándar", "Torton") }),
            new("Camión de carga", "Volvo", new CatalogModel[] { new("VM", "330", "Torton") }),

            new("Tractocamión", "Kenworth", new CatalogModel[] { new("T680", "Estándar", "Sleeper 6x4"), new("W900", "Estándar", "Sleeper 6x4") }),
            new("Tractocamión", "Freightliner", new CatalogModel[] { new("Cascadia", "Estándar", "Sleeper 6x4") }),
            new("Tractocamión", "International", new CatalogModel[] { new("LT", "Estándar", "Sleeper 6x4") }),
            new("Tractocamión", "Volvo", new CatalogModel[] { new("VNL", "760", "Sleeper 6x4") }),
            new("Tractocamión", "Peterbilt", new CatalogModel[] { new("579", "Estándar", "Sleeper 6x4") }),
            new("Tractocamión", "Scania", new CatalogModel[] { new("Serie R", "450", "Cabina corta 6x4") }),
            new("Tractocamión", "Mercedes-Benz", new CatalogModel[] { new("Actros", "2645", "Cabina corta 6x4") }),

            new("Autobús", "Mercedes-Benz", new CatalogModel[] { new("Sprinter", "Pasajeros", "Microbús"), new("Torino", "Estándar", "Urbano") }),
            new("Autobús", "Volvo", new CatalogModel[] { new("9800", "Estándar", "Foráneo") }),
            new("Autobús", "Scania", new CatalogModel[] { new("Serie K", "410", "Foráneo") }),
            new("Autobús", "Foton", new CatalogModel[] { new("AUV", "Estándar", "Urbano") }),
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var formatId = FirstFormatId;
            var sectionId = FirstSectionId;
            var itemId = FirstItemId;

            foreach (var template in Templates)
            {
                migrationBuilder.InsertData(
                    table: "InspectionTemplates",
                    columns: new[] { "Id", "IsActive", "Name", "VehicleFormat" },
                    values: new object[] { template.Id, true, template.Name, template.VehicleFormat });

                foreach (var format in template.Formats)
                {
                    migrationBuilder.InsertData(
                        table: "InspectionTemplateFormats",
                        columns: new[] { "Id", "TemplateId", "VehicleFormat" },
                        values: new object[] { formatId--, template.Id, format });
                }

                for (var s = 0; s < template.Sections.Length; s++)
                {
                    var section = template.Sections[s];
                    var currentSectionId = sectionId--;
                    migrationBuilder.InsertData(
                        table: "InspectionTemplateSections",
                        columns: new[] { "Id", "Name", "Order", "TemplateId" },
                        values: new object[] { currentSectionId, section.Name, s + 1, template.Id });

                    for (var i = 0; i < section.Items.Length; i++)
                    {
                        var item = section.Items[i];
                        migrationBuilder.InsertData(
                            table: "InspectionTemplateItems",
                            columns: new[] { "Id", "AllowNotApplicable", "Name", "Order", "SectionId" },
                            values: new object[] { itemId--, item.AllowNotApplicable, item.Name, i + 1, currentSectionId });
                    }
                }
            }

            var brandId = FirstBrandId;
            var modelId = FirstModelId;
            var versionId = FirstVersionId;
            var typeId = FirstTypeId;

            foreach (var brand in Catalog)
            {
                var currentBrandId = brandId--;
                migrationBuilder.InsertData(
                    table: "Brands",
                    columns: new[] { "Id", "Name", "Type" },
                    values: new object[] { currentBrandId, brand.Name, brand.Format });

                foreach (var model in brand.Models)
                {
                    var currentModelId = modelId--;
                    var currentVersionId = versionId--;
                    migrationBuilder.InsertData(
                        table: "VehicleModels",
                        columns: new[] { "Id", "BrandId", "Model" },
                        values: new object[] { currentModelId, currentBrandId, model.Model });
                    migrationBuilder.InsertData(
                        table: "VehicleVersions",
                        columns: new[] { "Id", "VehicleModelId", "Version" },
                        values: new object[] { currentVersionId, currentModelId, model.Version });
                    migrationBuilder.InsertData(
                        table: "VehicleTypes",
                        columns: new[] { "Id", "Type", "VehicleVersionId" },
                        values: new object[] { typeId--, model.Type, currentVersionId });
                }
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var modelCount = Catalog.Sum(b => b.Models.Length);
            var sectionCount = Templates.Sum(t => t.Sections.Length);
            var itemCount = Templates.Sum(t => t.Sections.Sum(s => s.Items.Length));
            var formatCount = Templates.Sum(t => t.Formats.Length);

            migrationBuilder.Sql($"DELETE FROM VehicleTypes WHERE Id BETWEEN {FirstTypeId - modelCount + 1} AND {FirstTypeId};");
            migrationBuilder.Sql($"DELETE FROM VehicleVersions WHERE Id BETWEEN {FirstVersionId - modelCount + 1} AND {FirstVersionId};");
            migrationBuilder.Sql($"DELETE FROM VehicleModels WHERE Id BETWEEN {FirstModelId - modelCount + 1} AND {FirstModelId};");
            migrationBuilder.Sql($"DELETE FROM Brands WHERE Id BETWEEN {FirstBrandId - Catalog.Length + 1} AND {FirstBrandId};");
            migrationBuilder.Sql($"DELETE FROM InspectionTemplateItems WHERE Id BETWEEN {FirstItemId - itemCount + 1} AND {FirstItemId};");
            migrationBuilder.Sql($"DELETE FROM InspectionTemplateSections WHERE Id BETWEEN {FirstSectionId - sectionCount + 1} AND {FirstSectionId};");
            migrationBuilder.Sql($"DELETE FROM InspectionTemplateFormats WHERE Id BETWEEN {FirstFormatId - formatCount + 1} AND {FirstFormatId};");
            migrationBuilder.Sql("DELETE FROM InspectionTemplates WHERE Id IN (-12, -13, -14);");
        }
    }
}
