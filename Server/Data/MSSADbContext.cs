using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Finals.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Data
{
    public class MSSADbContext : DbContext
    {
        public MSSADbContext(DbContextOptions<MSSADbContext> options) : base(options)
        {
        }

        // Add DbSets for all tables
        public DbSet<MSSA_Handler> MSSA_Handlers { get; set; }
        public DbSet<MSSA_HandlerMembership> MSSA_HandlerMemberships { get; set; }
        public DbSet<MSSA_State> MSSA_States { get; set; }
        public DbSet<MSSA_Dog> MSSA_Dogs { get; set; }
        public DbSet<MSSA_Event> MSSA_Events { get; set; }
        public DbSet<MSSA_Trial> MSSA_Trials { get; set; }
        public DbSet<MSSA_Class> MSSA_Classes { get; set; }
        public DbSet<MSSA_Entry> MSSA_Entries { get; set; }
        public DbSet<MSSA_DogFuturityParticipation> MSSA_DogFuturityParticipation { get; set; }
        public DbSet<MSSA_User> MSSA_Users { get; set; }
        public DbSet<MSSA_FinalsData> MSSA_FinalsData { get; set; }
        public DbSet<MSSA_FinalsResult> vw_AllFinalsResults { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to match your SQL schema
            modelBuilder.Entity<MSSA_Handler>().ToTable("MSSA_Handlers");
            modelBuilder.Entity<MSSA_HandlerMembership>().ToTable("MSSA_HandlerMemberships");
            modelBuilder.Entity<MSSA_State>().ToTable("MSSA_States");
            modelBuilder.Entity<MSSA_Dog>().ToTable("MSSA_Dogs");
            modelBuilder.Entity<MSSA_Event>().ToTable("MSSA_Events");
            modelBuilder.Entity<MSSA_Trial>().ToTable("MSSA_Trials");
            modelBuilder.Entity<MSSA_Class>().ToTable("MSSA_Classes");
            modelBuilder.Entity<MSSA_Entry>().ToTable("MSSA_Entries");
            modelBuilder.Entity<MSSA_DogFuturityParticipation>().ToTable("MSSA_DogFuturityParticipation");
            modelBuilder.Entity<MSSA_User>().ToTable("MSSA_Users");

            // Configure any specific relationships or constraints if needed

            modelBuilder.Entity<MSSA_FinalsData>()
                .HasKey(f => f.FinalsResultId);

            modelBuilder.Entity<MSSA_FinalsResult>()
                .HasNoKey()
                .ToView("vw_AllFinalsResults");

            modelBuilder.Entity<MSSA_Handler>()
                .HasOne<MSSA_State>()
                .WithMany()
                .HasForeignKey(h => h.StateCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MSSA_HandlerMembership>()
                .HasOne<MSSA_Handler>()
                .WithMany()
                .HasForeignKey(m => m.HandlerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MSSA_DogFuturityParticipation>()
                .HasOne<MSSA_Dog>()
                .WithMany()
                .HasForeignKey(f => f.DogId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MSSA_Event>()
                .HasOne<MSSA_State>()
                .WithMany()
                .HasForeignKey(e => e.StateCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MSSA_Trial>()
                .HasOne<MSSA_Event>()
                .WithMany()
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MSSA_Entry>()
                .HasOne<MSSA_Trial>()
                .WithMany()
                .HasForeignKey(e => e.TrialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MSSA_Entry>()
                .HasOne<MSSA_Handler>()
                .WithMany()
                .HasForeignKey(e => e.HandlerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MSSA_Entry>()
                .HasOne<MSSA_Dog>()
                .WithMany()
                .HasForeignKey(e => e.DogId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MSSA_Entry>()
                .HasOne<MSSA_Class>()
                .WithMany()
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
