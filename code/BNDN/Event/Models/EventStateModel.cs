using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event.Models
{
    public class EventStateModel
    {
        [Key, ForeignKey("EventIdentificationModel")]
        public string Id { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public bool Pending { get; set; }

        [Required]
        public virtual EventIdentificationModel EventIdentificationModel { get; set; }
    }
}