using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Event.Models
{
    public class UriRepresentationBase
    {
        // This Id should be used for internal Entity Framework representation only
        // TODO: Consider making property private
        public int Id { get; set; }
        public string UriString { get; set; }
    }
}