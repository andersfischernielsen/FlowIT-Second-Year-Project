using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Models;

namespace Models
{
    public class Workflow
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public virtual IList<Activity> Activitys { get; set; }
    }
}