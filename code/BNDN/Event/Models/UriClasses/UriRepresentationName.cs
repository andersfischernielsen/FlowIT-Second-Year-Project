using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event.Models.UriClasses
{
    public class UriRepresentationBase
    {
        // This Id should be used for internal Entity Framework representation only
        // TODO: Consider making property private
        [Key]
        public int Id { get; set; }

        [Required]
        public string UriString { get; set; }
        [Required]
        public string EventId { get; set; }


        public string EventIdentificationModelId { get; set; }
        public virtual EventIdentificationModel EventIdentificationModel { get; set; }
    }
}