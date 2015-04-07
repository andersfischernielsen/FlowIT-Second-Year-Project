using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class EventAddressDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public Uri Uri { get; set; }
        public IEnumerable<string> Roles { get; set; } 
    }
}
