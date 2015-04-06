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
        }

        static StorageContext()
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<StorageContext>());
        }
        public DbSet<ServerEventModel> Events { get; set; }
        public DbSet<ServerWorkflowModel> Workflows { get; set; }
        public DbSet<ServerUserModel> Users { get; set; }
        public DbSet<ServerRolesModel> Roles { get; set; }
    }
}