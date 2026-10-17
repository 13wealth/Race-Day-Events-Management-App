
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;
using RouteModel = RaceDay.API.Models.Route;

namespace RaceDay.API.Data
{
    public class RaceDayContext : DbContext
    {
        /*
         * The RaceDayContext class is a DbContext that represents the database context for the RaceDay application.
         * It provides access to the database tables and allows querying and saving data.
         */
        public RaceDayContext(DbContextOptions<RaceDayContext> options)
            : base(options)
        {
        }

        public DbSet<RaceDayUser> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<RaceEvent> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RouteModel> Routes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<RaceResult> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Participant)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Event)
                .WithMany(ev => ev.Enrollments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
