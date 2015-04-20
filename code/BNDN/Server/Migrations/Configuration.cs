using System.Collections.Generic;
using System.Data.Entity.Migrations;
using Server.Models;
using Server.Storage;

namespace Server.Migrations
{

    internal sealed class Configuration : DbMigrationsConfiguration<Storage.StorageContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }
    }
}
