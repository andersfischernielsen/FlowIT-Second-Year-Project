using System.Data.Entity.Migrations;
using Event.Storage;

namespace Event.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<EventContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }
    }
}
