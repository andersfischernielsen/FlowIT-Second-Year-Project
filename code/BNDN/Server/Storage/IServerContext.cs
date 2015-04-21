using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.History;
using Server.Models;

namespace Server.Storage
{
    public interface IServerContext : IDisposable
    {
        DbSet<ServerEventModel> Events { get; set; }
        DbSet<ServerWorkflowModel> Workflows { get; set; }
        DbSet<ServerUserModel> Users { get; set; }
        DbSet<ServerRoleModel> Roles { get; set; }
        DbSet<HistoryModel> History { get; set; }
        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
