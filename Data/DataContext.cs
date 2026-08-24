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

namespace MiTaller.Data
{
    public class DataContext : IdentityDbContext<BaseIdentityUser, IdentityRole<Guid>, Guid>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }


        // Customer
        public DbSet<Customer> Customers { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<CustomerFile> CustomerFiles { get; set; }
        //public DbSet<CustomerWorkshops> CustomerWorkshops { get; set; } // Workshops associated to a customer


        // Workshop
        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkshopFile> WorkshopFiles { get; set; }
        //public DbSet<WorkshopCustomers> WorkshopCustomers { get; set; } // Customers associated to a workshop
        public DbSet<WorkshopEmployees> WorkshopEmployees { get; set; }
        public DbSet<WorkshopServices> WorkshopServices { get; set; }
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

        // Workshop-Customer Notes
        public DbSet<WorkshopCustomerNotes> WorkshopCustomerNotes { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Customer>().ToTable("Customers");
            builder.Entity<Workshop>().ToTable("Workshops");
            builder.Entity<Employee>().ToTable("Employees");

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


        }


    }

}
