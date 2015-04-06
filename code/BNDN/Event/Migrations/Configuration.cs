using Event.Models;
using Event.Models.UriClasses;
using Event.Storage;

namespace Event.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<EventContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(EventContext context)
        {
            // Remove all the old stuff:
            context.EventIdentification.RemoveRange(context.EventIdentification.ToList());
            context.EventState.RemoveRange(context.EventState.ToList());

            context.EventIdentification.AddOrUpdate(ei => ei.Id,
                new EventIdentificationModel
                {
                    Id = "",
                    Name = null,
                    OwnUri = null,
                    Role = null,
                    WorkflowId = null
                });

            context.EventState.AddOrUpdate(es => es.Id,
                new EventStateModel
                {
                    Executed = false,
                    Id = "",
                    Included = false,
                    Pending = false
                });

            /*
            context.EventUriIdMappings.Add(new EventUriIdMapping()
            {
                Id = null,
                Uri = null
            });

            
            context.Exclusions.Add(new ExclusionUri()
            {
                Id = 0,
                UriString = null
            });

            context.Inclusions.Add(new InclusionUri()
            {
                Id = 0,
                UriString = null
            });

            context.Conditions.Add(new ConditionUri()
            {
                Id = 0,
                UriString = null
            });

            context.Responses.Add(new ResponseUri()
            {
                Id = 0,
                UriString = null
            });*/



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
        }
    }
}
