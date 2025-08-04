using Microsoft.EntityFrameworkCore;
using BinBuddies.Models;

namespace BinBuddies.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<AccountRepresentative> AccountRepresentatives { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }

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
        }
    }
}
