using Microsoft.EntityFrameworkCore;
using Vpassbackend.Models;

namespace Vpassbackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentService> AppointmentServices { get; set; }
        public DbSet<BorderPoint> BorderPoints { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<PaymentLog> PaymentLogs { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceCenter> ServiceCenters { get; set; }
        public DbSet<ServiceCenterCheckInPoint> ServiceCenterCheckInPoints { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleServiceHistory> VehicleServiceHistory { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure cascade delete behavior to prevent multiple cascade paths
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Vehicle)
                .WithMany(v => v.Appointments)
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.ServiceCenter)
                .WithMany(sc => sc.Appointments)
                .HasForeignKey(a => a.ServiceCenterId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AppointmentService>()
                .HasOne(as_ => as_.Appointment)
                .WithMany(a => a.AppointmentServices)
                .HasForeignKey(as_ => as_.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppointmentService>()
                .HasOne(as_ => as_.Service)
                .WithMany()
                .HasForeignKey(as_ => as_.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentLog>()
                .HasOne(pl => pl.Appointment)
                .WithMany(a => a.PaymentLogs)
                .HasForeignKey(pl => pl.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure decimal properties to prevent truncation
            modelBuilder.Entity<VehicleServiceHistory>()
                .Property(v => v.Cost)
                .HasColumnType("decimal(10, 2)");

            modelBuilder.Entity<Appointment>()
                .Property(a => a.EstimatedTotalCost)
                .HasColumnType("decimal(10, 2)");

            modelBuilder.Entity<Appointment>()
                .Property(a => a.AdvancePaymentAmount)
                .HasColumnType("decimal(10, 2)");

            modelBuilder.Entity<Appointment>()
                .Property(a => a.ActualTotalCost)
                .HasColumnType("decimal(10, 2)");
        }
    }
}
