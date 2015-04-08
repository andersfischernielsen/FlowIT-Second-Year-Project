using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Common
{
    public class UserDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public ICollection<WorkflowRole> Roles { get; set; }
    }
}
