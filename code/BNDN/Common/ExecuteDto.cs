using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ExecuteDto
    {
        [Required]
        public IList<string> Roles { get; set; }
    }
}
