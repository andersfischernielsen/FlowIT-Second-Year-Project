using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Common
{
    public class RoleDto
    {
        [Required]
        public IList<string> Roles { get; set; }
    }
}
