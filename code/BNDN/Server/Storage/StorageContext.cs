using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Server.Models;

namespace Server.Storage
{
    public class StorageContext : DbContext
    {
        static StorageContext()
        {
            Database.SetInitializer(new NullDatabaseInitializer<StorageContext>());
        }
        public DbSet<ServerEventModel> Events { get; set; }
        public DbSet<ServerWorkflowModel> Workflows { get; set; }
    }
}