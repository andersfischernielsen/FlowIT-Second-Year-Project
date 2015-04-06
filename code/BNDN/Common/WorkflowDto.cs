using System.ComponentModel.DataAnnotations;

namespace Common
{
    public class WorkflowDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Id { get; set; }
        //Todo: Discuss whether a Workflow should have a more user-friendly Title or/and a Description.
    }
}
