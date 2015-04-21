using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Event.Models;
using Event.Models.UriClasses;

namespace Event.Interfaces
{
    /// <summary>
    /// This interface represents the properties we want to be able to manipulate in the database.
    /// The primary reason for using this interface is for unit-testing purposes and mocking. 
    /// </summary>
    public interface IEventContext : IDisposable
    {
        DbSet<EventModel> Events { get; set; }
        DbSet<ConditionUri> Conditions { get; set; }
        DbSet<ResponseUri> Responses { get; set; }
        DbSet<InclusionUri> Inclusions { get; set; }
        DbSet<ExclusionUri> Exclusions { get; set; }
        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
