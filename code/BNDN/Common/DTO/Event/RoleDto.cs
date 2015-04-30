using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Common.DTO.Event
{
    public class RoleDto
    {
        [Required]
        public IList<string> Roles { get; set; }
    }
}
