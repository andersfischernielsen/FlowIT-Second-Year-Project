using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Event.Models;
using Event.Models.UriClasses;

namespace Event
{
    public class EventContext : DbContext
    {   
        public DbSet<EventIdentificationModel> EventIdentification { get; set; }
        public DbSet<EventStateModel> EventState { get; set; }
        public DbSet<ConditionUri> Conditions { get; set; }
        public DbSet<ResponseUri> Responses { get; set; }
        public DbSet<InclusionUri> Inclusions { get; set; }
        public DbSet<ExclusionUri> Exclusions { get; set; }
        // LockDto has been extracted out of EventState as it would become a class within a class (and as such would need workaround)
        public DbSet<LockDto> LockDto { get; set; }
    }
}