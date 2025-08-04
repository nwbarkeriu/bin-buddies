using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BinBuddies.Models;

namespace BinBuddies.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Legacy tables (for backward compatibility)
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<AccountRepresentative> AccountRepresentatives { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }

        // New subscription-based tables
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<CustomerProfile> CustomerProfiles { get; set; }
        public DbSet<ServiceArea> ServiceAreas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Contact)
                .WithOne(co => co.Customer)
                .HasForeignKey<Customer>(c => c.ContactId);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.AccountRep)
                .WithMany(ar => ar.Customers)
                .HasForeignKey(c => c.AccountRepresentative)
                .HasPrincipalKey(ar => ar.Name);

            modelBuilder.Entity<EventLog>()
                .HasOne(e => e.Customer)
                .WithMany(c => c.EventLogs)
                .HasForeignKey(e => e.CustomerId)
                .HasPrincipalKey(c => c.Id);

            modelBuilder.Entity<EventLog>()
                .HasOne(e => e.AccountRep)
                .WithMany(ar => ar.EventLogs)
                .HasForeignKey(e => e.RepId);

            // Configure indexes
            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique(false);

            modelBuilder.Entity<AccountRepresentative>()
                .HasIndex(ar => ar.Name)
                .IsUnique();

            modelBuilder.Entity<EventLog>()
                .HasIndex(e => e.EventDate);

            modelBuilder.Entity<EventLog>()
                .HasIndex(e => e.Completed);

            // Configure new subscription-based relationships
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<CustomerProfile>()
                .HasOne(cp => cp.User)
                .WithOne()
                .HasForeignKey<CustomerProfile>(cp => cp.UserId);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.User)
                .WithMany(u => u.ServiceRequests)
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.AssignedEmployee)
                .WithMany()
                .HasForeignKey(sr => sr.AssignedEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes for performance
            modelBuilder.Entity<Subscription>()
                .HasIndex(s => new { s.UserId, s.Status });

            modelBuilder.Entity<ServiceRequest>()
                .HasIndex(sr => new { sr.ScheduledDate, sr.Status });

            modelBuilder.Entity<ServiceRequest>()
                .HasIndex(sr => sr.AssignedEmployeeId);

            modelBuilder.Entity<ServiceArea>()
                .HasIndex(sa => sa.ZipCode);
        }
    }
}
