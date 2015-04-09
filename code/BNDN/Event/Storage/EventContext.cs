using System.Data.Entity;
using Event.Interfaces;
using Event.Models;
using Event.Models.UriClasses;

namespace Event.Storage
{
    public class EventContext : DbContext, IEventContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventIdentificationModel>()
                .HasMany(ei => ei.Roles)
                .WithRequired(role => role.Event)
                .HasForeignKey(role => role.EventId);
        }

        public DbSet<EventIdentificationModel> EventIdentification { get; set; }
        public DbSet<EventStateModel> EventState { get; set; }
        public DbSet<ConditionUri> Conditions { get; set; }
        public DbSet<ResponseUri> Responses { get; set; }
        public DbSet<InitialEventState> InitialEventState { get; set; }
        public DbSet<InclusionUri> Inclusions { get; set; }
        public DbSet<ExclusionUri> Exclusions { get; set; }
        // LockDto has been extracted out of EventState as it would become a class within a class (and as such would need workaround)
        public DbSet<LockDto> LockDto { get; set; }
    }
}