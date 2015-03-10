using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Models;

namespace Models
{
    public class Activity
    {
        public int Id { get; set; }
        public bool Executed { get; set; }
        public bool Requested { get; set; }
        public bool Excluded { get; set; }
        public string Title { get; set; }
        public Workflow ParentWorkflow { get; set; }
        public int ParentId { get; set; }
    }
}