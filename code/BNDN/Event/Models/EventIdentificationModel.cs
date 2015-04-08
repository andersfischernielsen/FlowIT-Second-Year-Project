using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Event.Models.UriClasses;

namespace Event.Models
{
    public class EventIdentificationModel
    {
        public EventIdentificationModel()
        {
            ResponseUris = new List<ResponseUri>();
            InclusionUris = new List<InclusionUri>();
            ExclusionUris = new List<ExclusionUri>();
            ConditionUris = new List<ConditionUri>();
        }
        [Key]
        public string Id { get; set; }

        public string OwnUri { get; set; }
        public string WorkflowId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<EventRoleModel> Roles { get; set; }

        public virtual EventStateModel EventStateModel { get; set; }
        public virtual LockDto LockDto { get; set; }

        public ICollection<ResponseUri> ResponseUris { get; set; }
        public ICollection<InclusionUri> InclusionUris { get; set; }
        public ICollection<ExclusionUri> ExclusionUris { get; set; }
        public ICollection<ConditionUri> ConditionUris { get; set; }
    }
}