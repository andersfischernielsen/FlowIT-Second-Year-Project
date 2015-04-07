using System.Data.Entity;
using Server.Models;

namespace Server.Storage
{
    public class StorageContext : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServerUserModel>()
                .HasMany(user => user.ServerRolesModels)
                .WithMany(role => role.ServerUserModels)
                .Map(m => m
                    .MapLeftKey("UserRefId")
                    .MapRightKey("RoleRefId")
                    .ToTable("UserRoles"));

            modelBuilder.Entity<ServerEventModel>()
                .HasMany(@event => @event.ServerRolesModels)
                .WithMany(role => role.ServerEventModels)
                .Map(m => m
                    .MapLeftKey("EventRefId")
                    .MapRightKey("RoleRefId")
                    .ToTable("EventRoles"));

            modelBuilder.Entity<ServerRoleModel>()
                .HasRequired(role => role.ServerWorkflowModel)
                .WithMany(workflow => workflow.ServerRolesModels)
                .HasForeignKey(role => role.ServerWorkflowModelID);
        }

        static StorageContext()
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<StorageContext>());
        }
        public DbSet<ServerEventModel> Events { get; set; }
        public DbSet<ServerWorkflowModel> Workflows { get; set; }
        public DbSet<ServerUserModel> Users { get; set; }
        public DbSet<ServerRoleModel> Roles { get; set; }
    }
}