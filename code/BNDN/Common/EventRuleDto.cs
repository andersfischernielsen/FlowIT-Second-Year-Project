using System;
using System.ComponentModel.DataAnnotations;

namespace Common
{
    // TODO: Might not be used
    /// <summary>
    /// An EventRuleDto is used by a sender that wants to set rules on a receiver, for instance sender wants to be included in
    /// receiver's Conditions-list; hence he sets Condition to true.
    /// It contains information about the sender (through property "Id").
    /// It's four bool fields states whether receiver should include sender in receivers four corresponding lists. 
    /// </summary>
    public class EventRuleDto
    {
        // TODO: Either a) delete this "Id" property (as it is not currently referenced) or 
        // TODO: b) change route of PUT on [Route("event/rules/{id}")] (in EventRulesController), so the "{id}" instead is simply just included within the provided EventRuleDto
        // TODO: In case, we go with b); set Id to [Required]
        /// <summary>
        /// Should identify sender
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The URI of the sender
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// If true, sender wishes to be a Condition for receiver (and receiver should include him in its Conditions-list)
        /// </summary>
        public bool Condition { get; set; }
        
        /// <summary>
        /// If true, sender wishes to be an Exclusion for receiver (and receiver should include him in its Exclusions-list)
        /// </summary>
        public bool Exclusion { get; set; }
        
        /// <summary>
        /// If true, sender  wishes to be a Response for receiver (and receiver should include him in its Responses-list)
        /// </summary>
        public bool Response { get; set; }

        /// <summary>
        /// If true, sender  wishes to be an Inclusion for receiver (and receiver should include him in its Inclusions-list) 
        /// </summary>
        public bool Inclusion { get; set; }
    }
}
