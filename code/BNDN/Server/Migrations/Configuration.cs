using System.Collections.Generic;
using System.Data.Entity.Migrations;
using Server.Models;

namespace Server.Migrations
{

    internal sealed class Configuration : DbMigrationsConfiguration<Storage.StorageContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(Storage.StorageContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            context.Workflows.AddOrUpdate(workflow => workflow.ID,
                new ServerWorkflowModel
                {
                    ID = "course",
                    Name = "Course Workflow"
                },
                new ServerWorkflowModel
                {
                    ID = "gasstation",
                    Name = "Gas Station Workflow"
                });

            context.Users.AddOrUpdate(user => user.ID,
                new ServerUserModel
                {
                    ID = 1,
                    Name = "Fischer",
                }, new ServerUserModel
                {
                    ID = 2,
                    Name = "Wind"
                }, new ServerUserModel
                {
                    ID = 3,
                    Name = "Cecilie"
                }, new ServerUserModel
                {
                    ID = 4,
                    Name = "Adam"
                }, new ServerUserModel
                {
                    ID = 5,
                    Name = "Morten"
                }, new ServerUserModel
                {
                    ID = 6,
                    Name = "Mikael"
                });

            // Save current changes to make sure that the users can be found in the next calls:
            context.SaveChanges();

            context.Roles.AddOrUpdate(role => role.ID,
                new ServerRolesModel
                {
                    ID = "teacher",
                    ServerWorklowModelID = "course",
                    ServerUserModels = new List<ServerUserModel>
                    {
                        context.Users.Find(1)
                    }
                }, new ServerRolesModel
                {
                    ID = "student",
                    ServerWorklowModelID = "course",
                    ServerUserModels = new List<ServerUserModel>
                    {
                        context.Users.Find(4),
                        context.Users.Find(6)
                    }
                }, new ServerRolesModel
                {
                    ID = "customer",
                    ServerWorklowModelID = "gasstation",
                    ServerUserModels = new List<ServerUserModel>
                    {
                        context.Users.Find(2),
                        context.Users.Find(3)
                    }
                }, new ServerRolesModel
                {
                    ID = "owner",
                    ServerWorklowModelID = "gasstation",
                    ServerUserModels = new List<ServerUserModel>
                    {
                        context.Users.Find(5)
                    }
                });
            context.SaveChanges();
        }
    }
}
