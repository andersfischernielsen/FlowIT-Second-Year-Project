using System.ComponentModel.DataAnnotations;

namespace Event.Models
{
    public class EventStateModel
    {
        [Key]
        public int Id { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public bool Pending { get; set; }
    }
}