using System.Data.Entity;
using Models;

namespace Storage
{
    public class StorageContext : DbContext
    {
        public virtual DbSet<Activity> Activities { get; set; }
        public virtual DbSet<Workflow> Workflows { get; set; }
    }
}
