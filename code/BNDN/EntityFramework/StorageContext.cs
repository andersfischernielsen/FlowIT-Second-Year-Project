using System.Data.Entity;
using Models;

namespace EntityFramework
{
    public class StorageContext : DbContext
    {
        public virtual DbSet<Activity> Activities { get; set; }
        public virtual DbSet<Workflow> Workflows { get; set; }
    }
}
