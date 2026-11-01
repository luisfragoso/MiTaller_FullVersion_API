using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class SeedServiceCatalogFamilies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Catalog for the vehicle families added after Automóviles/Motocicletas
            migrationBuilder.Sql(@"
DECLARE @c int;
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Bicicletas', N'Mantenimiento General', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Ajuste general (tune-up)'), (@c, N'Limpieza y lubricación de transmisión'), (@c, N'Revisión de seguridad'), (@c, N'Armado de bicicleta nueva');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Bicicletas', N'Frenos', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Ajuste de frenos'), (@c, N'Cambio de pastillas o zapatas'), (@c, N'Purgado de frenos hidráulicos'), (@c, N'Cambio de cables y fundas');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Bicicletas', N'Transmisión', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Ajuste de cambios'), (@c, N'Cambio de cadena'), (@c, N'Cambio de cassette o piñones'), (@c, N'Cambio de platos y bielas');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Bicicletas', N'Ruedas y Llantas', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Reparación de ponchadura'), (@c, N'Cambio de llanta o cámara'), (@c, N'Centrado de rin'), (@c, N'Conversión a tubeless');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Bicicletas', N'Suspensión', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Servicio de horquilla'), (@c, N'Servicio de amortiguador trasero');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Bicicletas', N'Bicicletas Eléctricas', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Diagnóstico eléctrico'), (@c, N'Revisión de batería'), (@c, N'Actualización de controlador');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Náutico', N'Motor', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Servicio de motor fuera de borda'), (@c, N'Servicio de motor dentro de borda o centrocola'), (@c, N'Cambio de impulsor de bomba de agua'), (@c, N'Cambio de aceite de pata'), (@c, N'Afinación de motor');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Náutico', N'Casco y Cubierta', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Limpieza y pulido de casco'), (@c, N'Pintura antiincrustante'), (@c, N'Reparación de fibra de vidrio'), (@c, N'Restauración de gelcoat');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Náutico', N'Hélice y Transmisión', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Reparación y balanceo de hélice'), (@c, N'Cambio de ánodos de sacrificio'), (@c, N'Revisión de flecha y sellos');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Náutico', N'Sistema Eléctrico y Electrónica', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Revisión de sistema eléctrico'), (@c, N'Instalación de electrónica (GPS, sonar, radio)'), (@c, N'Mantenimiento de baterías');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Náutico', N'Seguridad y Temporada', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Revisión de equipo de seguridad'), (@c, N'Revisión de bombas de achique'), (@c, N'Preparación para almacenaje'), (@c, N'Sacado y botado');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Aviación', N'Inspecciones', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Inspección de 50 horas'), (@c, N'Inspección de 100 horas'), (@c, N'Inspección anual');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Aviación', N'Motor', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Cambio de aceite y filtro'), (@c, N'Inspección boroscópica'), (@c, N'Servicio de magnetos'), (@c, N'Coordinación de overhaul de motor');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Aviación', N'Hélice', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Inspección de hélice'), (@c, N'Balanceo dinámico de hélice');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Aviación', N'Aviónica', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Revisión de aviónica'), (@c, N'Instalación de equipo de aviónica'), (@c, N'Prueba de pitot-estático y transpondedor');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Aviación', N'Estructura y Tren de Aterrizaje', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Revisión de tren de aterrizaje'), (@c, N'Cambio de llantas y frenos'), (@c, N'Tratamiento de corrosión'), (@c, N'Reparación estructural menor');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Pesados', N'Mantenimiento Preventivo', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Servicio de motor diésel (aceite y filtros)'), (@c, N'Cambio de filtros de combustible y aire'), (@c, N'Engrasado general'), (@c, N'Revisión de niveles y fugas');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Pesados', N'Frenos de Aire', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Ajuste de frenos de aire'), (@c, N'Cambio de balatas'), (@c, N'Revisión de compresor y válvulas'), (@c, N'Cambio de cámaras de freno');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Pesados', N'Motor Diésel', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Diagnóstico de motor diésel'), (@c, N'Calibración de inyectores'), (@c, N'Reparación de turbo'), (@c, N'Overhaul de motor');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Pesados', N'Transmisión y Diferencial', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Servicio de transmisión'), (@c, N'Cambio de clutch'), (@c, N'Servicio de diferencial');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Pesados', N'Suspensión y Dirección', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Cambio de muelles'), (@c, N'Suspensión neumática (bolsas de aire)'), (@c, N'Alineación de ejes');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Pesados', N'Eléctrico y Diagnóstico', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Diagnóstico electrónico con escáner de pesados'), (@c, N'Reparación de sistema eléctrico');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Pesados', N'Llantas', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Cambio y rotación de llantas'), (@c, N'Balanceo de llantas'), (@c, N'Llantas renovadas');
INSERT INTO ServiceCategories (Type, Name, CreatedAt) VALUES (N'Pesados', N'Verificación', GETDATE()); SET @c = SCOPE_IDENTITY();
INSERT INTO Services (ServiceCategoryId, Name) VALUES (@c, N'Preparación para verificación físico-mecánica');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE s FROM Services s JOIN ServiceCategories c ON c.Id = s.ServiceCategoryId
WHERE s.WorkshopId IS NULL AND c.Type IN (N'Bicicletas', N'Náutico', N'Aviación', N'Pesados');
DELETE c FROM ServiceCategories c
WHERE c.Type IN (N'Bicicletas', N'Náutico', N'Aviación', N'Pesados') AND NOT EXISTS (SELECT 1 FROM Services s WHERE s.ServiceCategoryId = c.Id);
");
        }
    }
}
