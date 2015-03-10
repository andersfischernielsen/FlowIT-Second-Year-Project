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
        public virtual List<Activity> Activitys { get; set; }
    }
}