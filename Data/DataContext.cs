using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiTaller.Models.Customer;
using MiTaller.Models.Workshop;
using MiTaller.Models.Domain;
using MiTaller.Models.Address;
using MiTaller.Models.Vehicle;
using System.Reflection.Emit;
using MiTaller.Models.Tags;
using MiTaller.Models.Reviews;
using MiTaller.Models;
using MiTaller.Models.Auth;
using MiTaller.Models.Accident;
using MiTaller.Models.Notification;
using MiTaller.Models.Advertisement;
using MiTaller.Models.Services;
using MiTaller.Models.Inspections;
using MiTaller.Models.Audit;
using System.Linq;
using System.Collections.Generic;

namespace MiTaller.Data
{
    public class DataContext : IdentityDbContext<BaseIdentityUser, IdentityRole<Guid>, Guid>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }


        // Admin (cuenta única de administrador de plataforma)
        public DbSet<Admin> Admins { get; set; }

        // Customer
        public DbSet<Customer> Customers { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<CustomerFile> CustomerFiles { get; set; }
        //public DbSet<CustomerWorkshops> CustomerWorkshops { get; set; } // Workshops associated to a customer


        // Workshop
        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkshopFile> WorkshopFiles { get; set; }
        public DbSet<WorkshopCustomers> WorkshopCustomers { get; set; } // Customers associated to a workshop
        public DbSet<WorkshopEmployees> WorkshopEmployees { get; set; }
        public DbSet<WorkshopServices> WorkshopServices { get; set; }
        public DbSet<WorkshopVehicleFormat> WorkshopVehicleFormats { get; set; }
        public DbSet<WorkshopNote> WorkshopNotes { get; set; }
        public DbSet<WorkshopBill> WorkshopBills { get; set; }
        public DbSet<WorkshopVehicleInspection> WorkshopVehicleInspections { get; set; }
        public DbSet<WorkshopVehicleFile> WorkshopVehicleFiles { get; set; }
        public DbSet<WorkshopIncomes> WorkshopIncomes { get; set; }
        public DbSet<WorkshopInbox> WorkshopInbox { get; set; }


        // Tags
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CustomerAssociatedTag> CustomerAssociatedTags { get; set; }

        // Address
        public DbSet<State> States { get; set; }
        public DbSet<Town> Towns { get; set; }
        public DbSet<Suburb> Suburbs { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<WorkshopAddress> WorkshopAddresses { get; set; }


        // Vehicle
        // Hierarchy: Brand -> VehicleModel -> VehicleVersion -> VehicleType
        public DbSet<Brand> Brands { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<VehicleVersion> VehicleVersions { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        // Interaction Customer - Workshop
        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationService> QuotationServices { get; set; }

        //Reviews
        public DbSet<Review> Reviews { get; set; }

        // Accidents
        public DbSet<Accident> Accidents { get; set; }

        // Notifications
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
        public DbSet<Notifications> Notifications { get; set; }

        // Advertisement
        public DbSet<Advertisement> Advertisements { get; set; }

        // Services
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Service> Services { get; set; }

        // RoadsideAssistance
        public DbSet<RoadsideAssistance> RoadsideAssistances { get; set; }

        // Inspections
        public DbSet<VehicleInspectionDetailHistory> VehicleInspectionDetailHistory { get; set; }
        public DbSet<VehicleInspectionHistory> VehicleInspectionHistory { get; set; }
        public DbSet<WorkshopMotocycleInspection> WorkshopMotocycleInspections { get; set; }
        public DbSet<MotocycleInspectionFile> MotocycleInspectionFiles { get; set; }
        public DbSet<MotocycleInspectionHistory> MotocycleInspectionHistory { get; set; }
        public DbSet<MotocycleInspectionDetailHistory> MotocycleInspectionDetailHistory { get; set; }

        // Generic (template-driven) inspections — used by any vehicle format
        // other than Automóvil/Motocicleta (e.g. Yate), which keep using the
        // hardcoded tables above unchanged.
        public DbSet<InspectionTemplate> InspectionTemplates { get; set; }
        public DbSet<InspectionTemplateFormat> InspectionTemplateFormats { get; set; }
        public DbSet<InspectionTemplateSpecialty> InspectionTemplateSpecialties { get; set; }
        public DbSet<InspectionTemplateSection> InspectionTemplateSections { get; set; }
        public DbSet<InspectionTemplateItem> InspectionTemplateItems { get; set; }
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<InspectionResultItem> InspectionResultItems { get; set; }
        public DbSet<InspectionSectionComment> InspectionSectionComments { get; set; }
        public DbSet<InspectionFile> InspectionFiles { get; set; }
        public DbSet<InspectionHistory> InspectionHistories { get; set; }
        public DbSet<InspectionDetailHistory> InspectionDetailHistories { get; set; }

        // Workshop-Customer Notes
        public DbSet<WorkshopCustomerNotes> WorkshopCustomerNotes { get; set; }

        // Audit log (historial global de cambios, solo lectura para el portal admin)
        public DbSet<AuditLog> AuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // AuditLog - índices para las consultas del portal admin (por registro,
            // por fecha, y por quién hizo el cambio).
            builder.Entity<AuditLog>()
                   .HasIndex(a => new { a.EntityName, a.EntityId });
            builder.Entity<AuditLog>()
                   .HasIndex(a => a.ChangedAt);
            builder.Entity<AuditLog>()
                   .HasIndex(a => new { a.ChangedByUserId, a.ChangedAt });

            builder.Entity<Customer>().ToTable("Customers");
            builder.Entity<Workshop>().ToTable("Workshops");
            builder.Entity<Employee>().ToTable("Employees");
            builder.Entity<Admin>().ToTable("Admins");

            // Seeding del valor "Other" en cada tabla usando Id = -1
            builder.Entity<Brand>().HasData(new Brand { Id = -1, Name = "Other", Type = "Generic" });
            builder.Entity<VehicleModel>().HasData(new VehicleModel { Id = -1, BrandId = null, Model = "Other" });
            builder.Entity<VehicleVersion>().HasData(new VehicleVersion { Id = -1, VehicleModelId = null, Version = "Other" });
            builder.Entity<VehicleType>().HasData(new VehicleType { Id = -1, VehicleVersionId = null, Type = "Other" });




            // Identity Users
            builder.Entity<BaseIdentityUser>()
                   .HasIndex(u => u.Email)
                   .IsUnique(false);

            builder.Entity<BaseIdentityUser>()
                   .Property(u => u.UserType)
                   .HasConversion<int>();

            builder.Entity<BaseIdentityUser>()
                   .HasIndex(u => u.UserName)
                   .IsUnique(false);

            builder.Entity<BaseIdentityUser>()
                   .HasIndex(u => u.PhoneNumber)
                   .IsUnique();

            // Addresses
            builder.Entity<Town>()
                   .HasOne(t => t.State)
                   .WithMany()
                   .HasForeignKey(t => t.StateId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Suburb>()
                   .HasOne(s => s.Town)
                   .WithMany()
                   .HasForeignKey(s => s.TownId)
                   .OnDelete(DeleteBehavior.Restrict);

            // CustomerAddresses
            builder.Entity<CustomerAddress>()
                .HasOne(ca => ca.Customer)
                .WithMany()
                .HasForeignKey(ca => ca.CustomerId);

            builder.Entity<CustomerAddress>()
                .HasOne(ca => ca.Suburb)
                .WithMany()
                .HasForeignKey(ca => ca.SuburbId);

            // WorkshopAddresses
            builder.Entity<WorkshopAddress>()
                .HasOne(wa => wa.Workshop)
                .WithMany()
                .HasForeignKey(wa => wa.WorkshopId);

            builder.Entity<WorkshopAddress>()
                .HasOne(wa => wa.Suburb)
                .WithMany()
                .HasForeignKey(wa => wa.SuburbId);

            // Vehicles
            builder.Entity<VehicleModel>()
            .HasOne(vm => vm.Brand)
            .WithMany()
            .HasForeignKey(vm => vm.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VehicleVersion>()
                .HasOne(vv => vv.VehicleModel)
                .WithMany()
                .HasForeignKey(vv => vv.VehicleModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VehicleType>()
                .HasOne(vt => vt.VehicleVersion)
                .WithMany()
                .HasForeignKey(vt => vt.VehicleVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Customer)
                .WithMany()
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Brand)
                .WithMany()
                .HasForeignKey(v => v.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.VehicleModel)
                .WithMany()
                .HasForeignKey(v => v.VehicleModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.VehicleVersion)
                .WithMany()
                .HasForeignKey(v => v.VehicleVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.VehicleType)
                .WithMany()
                .HasForeignKey(v => v.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // CustomerFiles
            builder.Entity<CustomerFile>()
                .OwnsOne(cf => cf.File);

            // WorkshopFiles
            builder.Entity<WorkshopFile>()
                .OwnsOne(cf => cf.File);

            // Tags
            builder.Entity<Tag>()
                .HasOne(t => t.Workshop)
                .WithMany()
                .HasForeignKey(t => t.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CustomerAssociatedTag>()
                .HasOne(cat => cat.Workshop)
                .WithMany()
                .HasForeignKey(cat => cat.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CustomerAssociatedTag>()
                .HasOne(cat => cat.Customer)
                .WithMany()
                .HasForeignKey(cat => cat.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CustomerAssociatedTag>()
                .HasOne(cat => cat.Tag)
                .WithMany(t => t.CustomerAssociatedTags)
                .HasForeignKey(cat => cat.TagId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reviews
            builder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Workshop)
                .WithMany()
                .HasForeignKey(r => r.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkshopClients
            builder.Entity<WorkshopCustomers>()
                .HasOne(wc => wc.Workshop)
                .WithMany()
                .HasForeignKey(wc => wc.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkshopCustomers>()
                .HasOne(wc => wc.Customer)
                .WithMany()
                .HasForeignKey(wc => wc.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // CustomerWorkshops
            //builder.Entity<CustomerWorkshops>()
            //    .HasOne(cw => cw.Customer)
            //    .WithMany()
            //    .HasForeignKey(cw => cw.CustomerId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.Entity<CustomerWorkshops>()
            //    .HasOne(cw => cw.Workshop)
            //    .WithMany()
            //    .HasForeignKey(cw => cw.WorkshopId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // WorkshopEmployees
            builder.Entity<WorkshopEmployees>()
                .HasOne(we => we.Workshop)
                .WithMany()
                .HasForeignKey(we => we.WorkshopId);

            builder.Entity<WorkshopEmployees>()
                .HasOne(we => we.Employee)
                .WithMany()
                .HasForeignKey(we => we.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkshopServices
            builder.Entity<WorkshopServices>()
                .HasOne(ws => ws.Workshop)
                .WithMany()
                .HasForeignKey(ws => ws.WorkshopId);

            builder.Entity<WorkshopServices>()
                .HasOne(ws => ws.Service)
                .WithMany()
                .HasForeignKey(ws => ws.ServiceId);

            // WorkshopVehicleFormats
            builder.Entity<WorkshopVehicleFormat>()
                .HasOne(wf => wf.Workshop)
                .WithMany()
                .HasForeignKey(wf => wf.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkshopIncomes
            builder.Entity<WorkshopIncomes>()
                .HasOne(ws => ws.WorkshopServices)
                .WithMany()
                .HasForeignKey(ws => ws.WorkshopServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointments
            builder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Workshop)
                .WithMany()
                .HasForeignKey(a => a.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Vehicle)
                .WithMany()
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quotation
            builder.Entity<Quotation>()
                .HasOne(q => q.Customer)
                .WithMany()
                .HasForeignKey(q => q.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Quotation>()
                .HasOne(q => q.Workshop)
                .WithMany()
                .HasForeignKey(q => q.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Quotation>()
                .HasOne(q => q.Vehicle)
                .WithMany()
                .HasForeignKey(q => q.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Quotation>()
                .HasOne(q => q.Inspection)
                .WithMany()
                .HasForeignKey(q => q.InspectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuotationService
            builder.Entity<QuotationService>()
                .HasOne(qs => qs.Quotation)
                .WithMany(q => q.Services)
                .HasForeignKey(qs => qs.QuotationId)
                .OnDelete(DeleteBehavior.Cascade); // Si se borra una cotización, se eliminan los servicios asociados

            builder.Entity<QuotationService>()
                .HasOne(qs => qs.Service)
                .WithMany()
                .HasForeignKey(qs => qs.WorkshopServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkshopNotes
            builder.Entity<WorkshopNote>()
                .HasOne(wn => wn.Workshop)
                .WithMany()
                .HasForeignKey(wn => wn.WorkshopId);

            // WorkshopBills
            builder.Entity<WorkshopBill>()
                .HasOne(wb => wb.Workshop)
                .WithMany()
                .HasForeignKey(wb => wb.WorkshopId);

            // WorkshopVehicleInspections
            builder.Entity<WorkshopVehicleInspection>()
                .HasOne(wvi => wvi.Workshop)
                .WithMany()
                .HasForeignKey(wvi => wvi.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkshopVehicleInspection>()
                .HasOne(wvi => wvi.Customer)
                .WithMany()
                .HasForeignKey(wvi => wvi.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkshopVehicleInspection>()
                .HasOne(wvi => wvi.Vehicle)
                .WithMany()
                .HasForeignKey(wvi => wvi.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkshopVehicleFile
            builder.Entity<WorkshopVehicleFile>()
                .HasOne(wvf => wvf.WorkshopVehicleInspection)
                .WithMany(wvi => wvi.Files)
                .HasForeignKey(wvf => wvf.WorkshopVehicleInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Accidents
            builder.Entity<Accident>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId);

            // WorkshopInbox
            builder.Entity<WorkshopInbox>()
                .HasOne(wi => wi.Customer)
                .WithMany()
                .HasForeignKey(wi => wi.CustomerId);

            builder.Entity<WorkshopInbox>()
                .HasOne(wi => wi.Vehicle)
                .WithMany()
                .HasForeignKey(wi => wi.VehicleId);

            // RoadsideAssistances
            builder.Entity<RoadsideAssistance>()
                .HasOne(ra => ra.Vehicle)
                .WithMany()
                .HasForeignKey(ra => ra.VehicleId);

            builder.Entity<RoadsideAssistance>()
                .HasOne(ra => ra.Customer)
                .WithMany()
                .HasForeignKey(ra => ra.CustomerId);

            // Inspections History
            builder.Entity<VehicleInspectionDetailHistory>()
                .HasOne(v => v.VehicleInspection)
                .WithMany()
                .HasForeignKey(v => v.VehicleInspectionId);

            builder.Entity<VehicleInspectionHistory>()
                .HasOne(v => v.VehicleInspection)
                .WithMany()
                .HasForeignKey(v => v.VehicleInspectionId);

            builder.Entity<MotocycleInspectionDetailHistory>()
                .HasOne(v => v.MotocycleInspection)
                .WithMany()
                .HasForeignKey(v => v.MotocycleInspectionId);

            builder.Entity<MotocycleInspectionHistory>()
                .HasOne(v => v.MotocycleInspection)
                .WithMany()
                .HasForeignKey(v => v.MotocycleInspectionId);

            // WorkshopMotocycleInspections
            builder.Entity<WorkshopMotocycleInspection>()
                .HasOne(wvi => wvi.Workshop)
                .WithMany()
                .HasForeignKey(wvi => wvi.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkshopMotocycleInspection>()
                .HasOne(wvi => wvi.Customer)
                .WithMany()
                .HasForeignKey(wvi => wvi.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkshopMotocycleInspection>()
                .HasOne(wvi => wvi.Vehicle)
                .WithMany()
                .HasForeignKey(wvi => wvi.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // MotocycleInspectionFile
            builder.Entity<MotocycleInspectionFile>()
                .HasOne(wvf => wvf.WorkshopMotocycleInspection)
                .WithMany(wvi => wvi.Files)
                .HasForeignKey(wvf => wvf.WorkshopMotocycleInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Generic Inspection System (templates + inspections) //

            builder.Entity<InspectionTemplateFormat>()
                .HasOne(f => f.Template)
                .WithMany()
                .HasForeignKey(f => f.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InspectionTemplateSpecialty>()
                .HasOne(s => s.Template)
                .WithMany()
                .HasForeignKey(s => s.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InspectionTemplateSection>()
                .HasOne(s => s.Template)
                .WithMany(t => t.Sections)
                .HasForeignKey(s => s.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InspectionTemplateItem>()
                .HasOne(i => i.Section)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Inspection>()
                .HasOne(i => i.Template)
                .WithMany()
                .HasForeignKey(i => i.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Inspection>()
                .HasOne(i => i.Workshop)
                .WithMany()
                .HasForeignKey(i => i.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Inspection>()
                .HasOne(i => i.Customer)
                .WithMany()
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Inspection>()
                .HasOne(i => i.Vehicle)
                .WithMany()
                .HasForeignKey(i => i.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InspectionResultItem>()
                .HasOne(r => r.Inspection)
                .WithMany(i => i.Results)
                .HasForeignKey(r => r.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InspectionResultItem>()
                .HasOne(r => r.TemplateItem)
                .WithMany()
                .HasForeignKey(r => r.TemplateItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InspectionSectionComment>()
                .HasOne(c => c.Inspection)
                .WithMany(i => i.SectionComments)
                .HasForeignKey(c => c.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InspectionSectionComment>()
                .HasOne(c => c.Section)
                .WithMany()
                .HasForeignKey(c => c.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InspectionFile>()
                .HasOne(f => f.Inspection)
                .WithMany(i => i.Files)
                .HasForeignKey(f => f.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InspectionHistory>()
                .HasOne(h => h.Inspection)
                .WithMany()
                .HasForeignKey(h => h.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InspectionDetailHistory>()
                .HasOne(h => h.Inspection)
                .WithMany()
                .HasForeignKey(h => h.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            SeedYachtInspectionTemplate(builder);
            SeedBoatVehicleCatalog(builder);
            SeedSpecialtyInspectionTemplates(builder);
        }

        /// <summary>
        /// Seeds the "Yate" inspection template (7 sections / items) via HasData
        /// so it's created deterministically by the AddGenericInspectionSystem
        /// migration. Uses fixed negative Ids, same convention as the
        /// Brand/VehicleModel/VehicleVersion/VehicleType "Other" seeds above.
        /// </summary>
        private static void SeedYachtInspectionTemplate(ModelBuilder builder)
        {
            const int templateId = -1;

            builder.Entity<InspectionTemplate>().HasData(new InspectionTemplate
            {
                Id = templateId,
                VehicleFormat = "Yate",
                Name = "Inspección de Yate",
                IsActive = true,
            });

            // "Lancha" y "Bote" comparten el mismo checklist que "Yate" (mismas
            // secciones/ítems) en vez de duplicarlo: las 3 apuntan al mismo TemplateId.
            builder.Entity<InspectionTemplateFormat>().HasData(
                new InspectionTemplateFormat { Id = -1, TemplateId = templateId, VehicleFormat = "Yate" },
                new InspectionTemplateFormat { Id = -2, TemplateId = templateId, VehicleFormat = "Lancha" },
                new InspectionTemplateFormat { Id = -3, TemplateId = templateId, VehicleFormat = "Bote" }
            );

            var sectionsWithItems = new (int SectionId, string SectionName, int Order, (string Name, bool AllowNotApplicable)[] Items)[]
            {
                (-1, "Casco y Exterior", 1, new (string, bool)[]
                {
                    ("Casco/gelcoat", false),
                    ("Pasacascos", false),
                    ("Hélices y ejes", false),
                    ("Timón", false),
                    ("Ánodos de sacrificio", false),
                    ("Quilla", false),
                    ("Hélice de proa", false),
                    ("Defensas", false),
                }),
                (-2, "Sala de Máquinas / Motor", 2, new (string, bool)[]
                {
                    ("Motor(es)", false),
                    ("Niveles y fugas de aceite", false),
                    ("Bandas y mangueras", false),
                    ("Sistema de combustible", false),
                    ("Sistema de escape", false),
                    ("Sistema de enfriamiento", false),
                    ("Transmisión/cola", false),
                }),
                (-3, "Sistema Eléctrico", 3, new (string, bool)[]
                {
                    ("Baterías", false),
                    ("Alternador", false),
                    ("Cableado", false),
                    ("Bomba de sentina", false),
                    ("Planta/inversor", false),
                    ("Luces de navegación", false),
                }),
                (-4, "Sentina y Achique", 4, new (string, bool)[]
                {
                    ("Bombas de achique", false),
                    ("Condición de la sentina", false),
                    ("Válvulas de pasacascos", false),
                    ("Tapones de drenaje", false),
                }),
                (-5, "Navegación y Electrónica", 5, new (string, bool)[]
                {
                    ("GPS/chartplotter", false),
                    ("Radar", false),
                    ("Radio VHF", false),
                    ("Piloto automático", false),
                    ("Sonda", false),
                    ("Compás", false),
                }),
                (-6, "Equipo de Seguridad", 6, new (string, bool)[]
                {
                    ("Chalecos salvavidas", false),
                    ("Extintores", false),
                    ("Señales de auxilio", false),
                    ("Balsa salvavidas", false),
                    ("EPIRB", false),
                    ("Botiquín", false),
                }),
                (-7, "Cubierta e Interior", 7, new (string, bool)[]
                {
                    ("Herrajes de cubierta", false),
                    ("Malacate de ancla", false),
                    ("Tapicería/interior", false),
                    ("Plomería y sanitarios", false),
                    ("Sistema de agua dulce", false),
                    ("Aire acondicionado/calefacción", false),
                    ("Mástil", true),
                    ("Jarcia/aparejos", true),
                    ("Velas", true),
                    ("Winches", true),
                }),
            };

            var sections = sectionsWithItems.Select(s => new InspectionTemplateSection
            {
                Id = s.SectionId,
                TemplateId = templateId,
                Name = s.SectionName,
                Order = s.Order,
            }).ToArray();

            builder.Entity<InspectionTemplateSection>().HasData(sections);

            var items = new List<InspectionTemplateItem>();
            var nextItemId = -1;

            foreach (var section in sectionsWithItems)
            {
                var order = 1;
                foreach (var (name, allowNotApplicable) in section.Items)
                {
                    items.Add(new InspectionTemplateItem
                    {
                        Id = nextItemId,
                        SectionId = section.SectionId,
                        Name = name,
                        Order = order,
                        AllowNotApplicable = allowNotApplicable,
                    });

                    nextItemId--;
                    order++;
                }
            }

            builder.Entity<InspectionTemplateItem>().HasData(items.ToArray());
        }

        /// <summary>
        /// Seeds a real Marca -> Modelo -> Versión (eslora en pies) -> Tipo (categoría de
        /// casco) catalog for the new "Yate"/"Lancha"/"Bote" vehicle formats, via HasData
        /// (same InsertData-via-migration mechanism used for the yacht inspection template
        /// above). Uses negative Ids, same convention as the "-1 = Other" seeds, so these
        /// rows never collide with the real (positive-Id) Brand/VehicleModel/VehicleVersion/
        /// VehicleType rows already loaded in the database.
        /// </summary>
        private static void SeedBoatVehicleCatalog(ModelBuilder builder)
        {
            var catalog = new (string BrandType, string BrandName, (string Model, (string Version, string Type)[] Versions)[] Models)[]
            {
                ("Yate", "Azimut", new (string, (string, string)[])[]
                {
                    ("Grande", new[] { ("62", "Flybridge"), ("88", "Flybridge") }),
                    ("Atlantis", new[] { ("34", "Sport Yacht"), ("44", "Sport Yacht") }),
                    ("Magellano", new[] { ("55", "Cruiser") }),
                }),
                ("Yate", "Sunseeker", new (string, (string, string)[])[]
                {
                    ("Predator", new[] { ("55", "Sport Yacht"), ("68", "Sport Yacht") }),
                    ("Manhattan", new[] { ("52", "Flybridge") }),
                    ("Ocean", new[] { ("90", "Flybridge") }),
                }),
                ("Yate", "Ferretti", new (string, (string, string)[])[]
                {
                    ("500", new[] { ("52", "Flybridge") }),
                    ("720", new[] { ("74", "Flybridge") }),
                    ("Yachts 850", new[] { ("87", "Flybridge") }),
                }),
                ("Yate", "Princess", new (string, (string, string)[])[]
                {
                    ("V50", new[] { ("53", "Sport Yacht") }),
                    ("F55", new[] { ("57", "Flybridge") }),
                    ("X95", new[] { ("97", "Flybridge") }),
                }),
                ("Yate", "Sea Ray", new (string, (string, string)[])[]
                {
                    ("Sundancer", new[] { ("40", "Cruiser"), ("65", "Cruiser") }),
                    ("L Series", new[] { ("46", "Flybridge") }),
                }),
                ("Yate", "Riva", new (string, (string, string)[])[]
                {
                    ("Rivamare", new[] { ("38", "Sport Yacht") }),
                    ("88 Domino", new[] { ("88", "Flybridge") }),
                    ("Aquariva", new[] { ("33", "Sport Yacht") }),
                }),
                ("Yate", "Pershing", new (string, (string, string)[])[]
                {
                    ("6X", new[] { ("66", "Sport Yacht") }),
                    ("8X", new[] { ("88", "Sport Yacht") }),
                    ("GTX116", new[] { ("116", "Flybridge") }),
                }),
                ("Yate", "Viking", new (string, (string, string)[])[]
                {
                    ("54 Convertible", new[] { ("54", "Convertible") }),
                    ("90 Enclosed Bridge", new[] { ("90", "Flybridge") }),
                    ("46 Billfish", new[] { ("46", "Convertible") }),
                }),

                ("Lancha", "Boston Whaler", new (string, (string, string)[])[]
                {
                    ("Montauk", new[] { ("19", "Center Console") }),
                    ("Outrage", new[] { ("23", "Center Console"), ("28", "Center Console") }),
                    ("Dauntless", new[] { ("21", "Center Console") }),
                }),
                ("Lancha", "Bayliner", new (string, (string, string)[])[]
                {
                    ("Element", new[] { ("16", "Bowrider") }),
                    ("VR6", new[] { ("19", "Bowrider") }),
                    ("DX2200", new[] { ("22", "Deck Boat") }),
                }),
                ("Lancha", "Yamaha", new (string, (string, string)[])[]
                {
                    ("190 FSH", new[] { ("19", "Jet Boat") }),
                    ("212X", new[] { ("21", "Jet Boat") }),
                    ("275SD", new[] { ("27", "Jet Boat") }),
                }),
                ("Lancha", "Sea Fox", new (string, (string, string)[])[]
                {
                    ("200 Viper", new[] { ("20", "Center Console") }),
                    ("328 Commander", new[] { ("32", "Center Console") }),
                }),
                ("Lancha", "Grady-White", new (string, (string, string)[])[]
                {
                    ("Fisherman 236", new[] { ("23", "Center Console") }),
                    ("Freedom 275", new[] { ("27", "Center Console") }),
                    ("Canyon 456", new[] { ("45", "Center Console") }),
                }),
                ("Lancha", "Robalo", new (string, (string, string)[])[]
                {
                    ("R222", new[] { ("22", "Center Console") }),
                    ("R242", new[] { ("24", "Center Console") }),
                    ("R302", new[] { ("30", "Center Console") }),
                }),
                ("Lancha", "Chaparral", new (string, (string, string)[])[]
                {
                    ("19 SSi", new[] { ("19", "Bowrider") }),
                    ("277 SSX", new[] { ("27", "Bowrider") }),
                    ("23 Fish", new[] { ("23", "Center Console") }),
                }),
                ("Lancha", "Yellowfin", new (string, (string, string)[])[]
                {
                    ("24 Bay", new[] { ("24", "Center Console") }),
                    ("32", new[] { ("32", "Center Console") }),
                    ("42", new[] { ("42", "Center Console") }),
                }),

                ("Bote", "Lund", new (string, (string, string)[])[]
                {
                    ("1650 Rebel", new[] { ("16", "Panga de aluminio") }),
                    ("1875 Impact", new[] { ("18", "Panga de aluminio") }),
                    ("2075 Tyee", new[] { ("20", "Panga de aluminio") }),
                }),
                ("Bote", "Tracker", new (string, (string, string)[])[]
                {
                    ("Guide V-16", new[] { ("16", "Panga de aluminio") }),
                    ("Pro Team 175", new[] { ("17", "Panga de aluminio") }),
                    ("Targa V-19", new[] { ("19", "Panga de aluminio") }),
                }),
                ("Bote", "Zodiac", new (string, (string, string)[])[]
                {
                    ("Cadet 310", new[] { ("10", "Inflable") }),
                    ("Open 5.5", new[] { ("18", "Inflable") }),
                    ("Medline 7.5", new[] { ("24", "Inflable") }),
                }),
                ("Bote", "AB Inflatables", new (string, (string, string)[])[]
                {
                    ("Lammina 9", new[] { ("9", "Inflable") }),
                    ("Nautilus 15", new[] { ("15", "Inflable") }),
                    ("Oceanus 24", new[] { ("24", "Inflable") }),
                }),
                ("Bote", "Achilles", new (string, (string, string)[])[]
                {
                    ("LT-2", new[] { ("8", "Inflable") }),
                    ("HB-260", new[] { ("9", "Inflable") }),
                    ("Ranger R-400", new[] { ("13", "Inflable") }),
                }),
                ("Bote", "Sea Eagle", new (string, (string, string)[])[]
                {
                    ("330", new[] { ("11", "Inflable") }),
                    ("Sport Runabout 344", new[] { ("11", "Inflable") }),
                    ("FastCat 465", new[] { ("15", "Inflable") }),
                }),
                ("Bote", "Súper Panga", new (string, (string, string)[])[]
                {
                    ("Panga 22", new[] { ("22", "Panga de fibra") }),
                    ("Panga 26", new[] { ("26", "Panga de fibra") }),
                    ("Panga 31", new[] { ("31", "Panga de fibra") }),
                }),
                ("Bote", "FibraMex", new (string, (string, string)[])[]
                {
                    ("Tiburón 23", new[] { ("23", "Panga de fibra") }),
                    ("Marlín 28", new[] { ("28", "Panga de fibra") }),
                    ("Delfín 19", new[] { ("19", "Panga de fibra") }),
                }),
            };

            var brands = new List<Brand>();
            var models = new List<VehicleModel>();
            var versions = new List<VehicleVersion>();
            var types = new List<VehicleType>();

            // -1 ya está usado por el seed "Other" de cada tabla; se arranca en -2 y
            // se decrementa, igual que el patrón de nextItemId de arriba.
            var nextBrandId = -2;
            var nextModelId = -2;
            var nextVersionId = -2;
            var nextTypeId = -2;

            foreach (var brandEntry in catalog)
            {
                var brandId = nextBrandId--;
                brands.Add(new Brand { Id = brandId, Type = brandEntry.BrandType, Name = brandEntry.BrandName });

                foreach (var modelEntry in brandEntry.Models)
                {
                    var modelId = nextModelId--;
                    models.Add(new VehicleModel { Id = modelId, BrandId = brandId, Model = modelEntry.Model });

                    foreach (var (version, type) in modelEntry.Versions)
                    {
                        var versionId = nextVersionId--;
                        versions.Add(new VehicleVersion { Id = versionId, VehicleModelId = modelId, Version = version });

                        var typeId = nextTypeId--;
                        types.Add(new VehicleType { Id = typeId, VehicleVersionId = versionId, Type = type });
                    }
                }
            }

            builder.Entity<Brand>().HasData(brands.ToArray());
            builder.Entity<VehicleModel>().HasData(models.ToArray());
            builder.Entity<VehicleVersion>().HasData(versions.ToArray());
            builder.Entity<VehicleType>().HasData(types.ToArray());
        }

        /// <summary>
        /// Seeds one short InspectionTemplate per "taller especializado" specialty
        /// (Radiadores, Eléctrico Automotriz, ...). Unlike Yate/Avión these don't
        /// map to a VehicleFormat - the vehicle being checked stays a normal
        /// Automóvil/Motocicleta, only the checklist run against it changes. Each
        /// specialty gets a single short section instead of the multi-section
        /// templates used for full vehicle formats, per Luis's "más ligero" ask.
        /// </summary>
        private static void SeedSpecialtyInspectionTemplates(ModelBuilder builder)
        {
            var specialties = new (string Specialty, string TemplateName, string SectionName, (string Name, bool AllowNotApplicable)[] Items)[]
            {
                ("Radiadores", "Inspección de Radiadores", "Sistema de Enfriamiento", new (string, bool)[]
                {
                    ("Nivel de refrigerante", false),
                    ("Fugas en radiador", false),
                    ("Estado de mangueras", false),
                    ("Tapón de radiador", false),
                    ("Ventilador de enfriamiento", false),
                    ("Termostato", false),
                    ("Bomba de agua", false),
                }),
                ("Eléctrico Automotriz", "Inspección Eléctrica Automotriz", "Sistema Eléctrico", new (string, bool)[]
                {
                    ("Batería (carga y bornes)", false),
                    ("Alternador", false),
                    ("Motor de arranque", false),
                    ("Fusibles y relevadores", false),
                    ("Cableado y conexiones", false),
                    ("Luces exteriores", false),
                    ("Luces interiores", false),
                }),
                ("Mofles y Escapes", "Inspección de Mofles y Escapes", "Sistema de Escape", new (string, bool)[]
                {
                    ("Mofle/silenciador", false),
                    ("Tubo de escape", false),
                    ("Catalizador", false),
                    ("Abrazaderas y soportes", false),
                    ("Fugas de escape", false),
                    ("Sensor de oxígeno", true),
                }),
                ("Frenos", "Inspección de Frenos", "Sistema de Frenos", new (string, bool)[]
                {
                    ("Balatas/pastillas delanteras", false),
                    ("Balatas/pastillas traseras", false),
                    ("Discos/tambores", false),
                    ("Nivel de líquido de frenos", false),
                    ("Mangueras de freno", false),
                    ("Freno de mano", false),
                    ("Sistema ABS", true),
                }),
                ("Suspensión y Alineación", "Inspección de Suspensión y Alineación", "Suspensión y Dirección", new (string, bool)[]
                {
                    ("Amortiguadores delanteros", false),
                    ("Amortiguadores traseros", false),
                    ("Rótulas", false),
                    ("Terminales de dirección", false),
                    ("Bujes de suspensión", false),
                    ("Alineación", false),
                    ("Balanceo", false),
                }),
                ("Aire Acondicionado", "Inspección de Aire Acondicionado", "Sistema de Aire Acondicionado", new (string, bool)[]
                {
                    ("Nivel de gas refrigerante", false),
                    ("Compresor", false),
                    ("Condensador", false),
                    ("Fugas en el sistema", false),
                    ("Filtro de cabina", false),
                    ("Funcionamiento general del sistema", false),
                }),
            };

            var templates = new List<InspectionTemplate>();
            var templateFormats = new List<InspectionTemplateSpecialty>();
            var sections = new List<InspectionTemplateSection>();
            var items = new List<InspectionTemplateItem>();

            var nextTemplateId = -3;
            var nextSpecialtyId = -1;
            var nextSectionId = -15;
            var nextItemId = -99;

            foreach (var (specialty, templateName, sectionName, specialtyItems) in specialties)
            {
                var templateId = nextTemplateId--;
                templates.Add(new InspectionTemplate
                {
                    Id = templateId,
                    VehicleFormat = specialty,
                    Name = templateName,
                    IsActive = true,
                });

                templateFormats.Add(new InspectionTemplateSpecialty
                {
                    Id = nextSpecialtyId--,
                    TemplateId = templateId,
                    Specialty = specialty,
                });

                var sectionId = nextSectionId--;
                sections.Add(new InspectionTemplateSection
                {
                    Id = sectionId,
                    TemplateId = templateId,
                    Name = sectionName,
                    Order = 1,
                });

                var order = 1;
                foreach (var (name, allowNotApplicable) in specialtyItems)
                {
                    items.Add(new InspectionTemplateItem
                    {
                        Id = nextItemId--,
                        SectionId = sectionId,
                        Name = name,
                        Order = order++,
                        AllowNotApplicable = allowNotApplicable,
                    });
                }
            }

            builder.Entity<InspectionTemplate>().HasData(templates.ToArray());
            builder.Entity<InspectionTemplateSpecialty>().HasData(templateFormats.ToArray());
            builder.Entity<InspectionTemplateSection>().HasData(sections.ToArray());
            builder.Entity<InspectionTemplateItem>().HasData(items.ToArray());
        }

    }

}
