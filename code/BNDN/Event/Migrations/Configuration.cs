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
                    Roles = null,
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
        }
    }
}
