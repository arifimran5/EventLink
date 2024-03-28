using EventLink.Models;
using Microsoft.EntityFrameworkCore;

namespace EventLink.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.HostedEvents)
                .WithOne(e => e.Host)
                .HasForeignKey(e => e.HostId);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Attendees)
                .WithOne(ea => ea.Event)
                .HasForeignKey(ea => ea.EventId);

            modelBuilder.Entity<EventAttendee>()
                .HasKey(ea => new { ea.UserId, ea.EventId });
        }
    }
}
