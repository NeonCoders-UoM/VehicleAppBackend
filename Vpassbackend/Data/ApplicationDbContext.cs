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
        public DbSet<BorderPoint> BorderPoints { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<PaymentLog> PaymentLogs { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceCenter> ServiceCenters { get; set; }
        public DbSet<ServiceCenterCheckInPoint> ServiceCenterCheckInPoints { get; set; }
        public DbSet<ServiceCenterService> ServiceCenterServices { get; set; }
        public DbSet<ServiceReminder> ServiceReminders { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleServiceHistory> VehicleServiceHistories { get; set; }

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
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.ServiceCenter)
                .WithMany()
                .HasForeignKey(a => a.Station_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ServiceCenterService relationships
            modelBuilder.Entity<ServiceCenterService>()
                .HasOne(scs => scs.Service)
                .WithMany(s => s.ServiceCenterServices)
                .HasForeignKey(scs => scs.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceCenterService>()
                .HasOne(scs => scs.ServiceCenter)
                .WithMany(sc => sc.ServiceCenterServices)
                .HasForeignKey(scs => scs.Station_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Create a unique constraint to prevent duplicate service-center combinations
            modelBuilder.Entity<ServiceCenterService>()
                .HasIndex(scs => new { scs.ServiceId, scs.Station_id })
                .IsUnique();

            // Configure VehicleServiceHistory relationships
            modelBuilder.Entity<VehicleServiceHistory>()
                .HasOne(vsh => vsh.Vehicle)
                .WithMany(v => v.ServiceHistory)
                .HasForeignKey(vsh => vsh.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VehicleServiceHistory>()
                .HasOne(vsh => vsh.ServiceCenter)
                .WithMany()
                .HasForeignKey(vsh => vsh.ServiceCenterId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<VehicleServiceHistory>()
                .HasOne(vsh => vsh.ServicedByUser)
                .WithMany()
                .HasForeignKey(vsh => vsh.ServicedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure ServiceReminder relationships
            modelBuilder.Entity<ServiceReminder>()
                .HasOne(sr => sr.Vehicle)
                .WithMany(v => v.ServiceReminders)
                .HasForeignKey(sr => sr.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceReminder>()
                .HasOne(sr => sr.Service)
                .WithMany(s => s.ServiceReminders)
                .HasForeignKey(sr => sr.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
