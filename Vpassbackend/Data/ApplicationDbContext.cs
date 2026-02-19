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
        public DbSet<ServiceCenterService> ServiceCenterServices { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<ServiceReminder> ServiceReminders { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleServiceHistory> VehicleServiceHistories { get; set; }
        public DbSet<VehicleTransfer> VehicleTransfers { get; set; }
        public DbSet<EmergencyCallCenter> EmergencyCallCenters { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<FuelEfficiency> FuelEfficiencies { get; set; }
        public DbSet<ClosureSchedule> ClosureSchedules { get; set; }
        public DbSet<ServiceAvailability> ServiceAvailabilities { get; set; }
        public DbSet<ServiceCenterDailyLimit> ServiceCenterDailyLimits { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Chatbot RAG Models
        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<KnowledgeDocument> KnowledgeDocuments { get; set; }


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
                .HasOne(a => a.ServiceCenter)
                .WithMany()
                .HasForeignKey(a => a.Station_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointment - Service relationship
            modelBuilder.Entity<AppointmentService>()
                .HasKey(asv => new { asv.AppointmentId, asv.ServiceId });

            modelBuilder.Entity<AppointmentService>()
                .HasOne(asv => asv.Appointment)
                .WithMany(a => a.AppointmentServices)
                .HasForeignKey(asv => asv.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppointmentService>()
                .HasOne(asv => asv.Service)
                .WithMany()
                .HasForeignKey(asv => asv.ServiceId)
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

            // Configure ServiceCenterService-Package relationship
            modelBuilder.Entity<ServiceCenterService>()
                .HasOne(scs => scs.Package)
                .WithMany(p => p.ServiceCenterServices)
                .HasForeignKey(scs => scs.PackageId)
                .OnDelete(DeleteBehavior.SetNull);

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

            // Configure Feedback relationships
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Customer)
                .WithMany()
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.ServiceCenter)
                .WithMany()
                .HasForeignKey(f => f.ServiceCenterId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Vehicle)
                .WithMany()
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FuelEfficiency>()
                .HasOne(fe => fe.Vehicle)
                .WithMany(v => v.FuelEfficiencies)
                .HasForeignKey(fe => fe.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Notification relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Customer)
                .WithMany()
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.ServiceReminder)
                .WithMany()
                .HasForeignKey(n => n.ServiceReminderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Vehicle)
                .WithMany()
                .HasForeignKey(n => n.VehicleId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Appointment)
                .WithMany()
                .HasForeignKey(n => n.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure VehicleTransfer relationships
            modelBuilder.Entity<VehicleTransfer>()
                .HasOne(vt => vt.Vehicle)
                .WithMany(v => v.TransferHistory)
                .HasForeignKey(vt => vt.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VehicleTransfer>()
                .HasOne(vt => vt.FromOwner)
                .WithMany()
                .HasForeignKey(vt => vt.FromOwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<VehicleTransfer>()
                .HasOne(vt => vt.ToOwner)
                .WithMany()
                .HasForeignKey(vt => vt.ToOwnerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
