using System.Data.Entity;
using Models;

namespace EntityFramework
{
    class StorageContext : DbContext
    {
        public virtual DbSet<Activity> Activites { get; set; }
        public virtual DbSet<Workflow> Workflows { get; set; }
    }
}
