using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Exceptions;
using Event.Exceptions;
using Event.Interfaces;

namespace Event.Logic
{
    /// <summary>
    /// AuthLogic is a logic-layer, that determines successfull authorization among roles and events.  
    /// </summary>
    public class AuthLogic : IAuthLogic
    {
        private readonly IEventStorage _storage;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="storage">An instance of an IEventStorage-implementation</param>
        public AuthLogic(IEventStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// IsAuthorized will, if provided a set of roles when called, determine whether at least one of the 
        /// provided roles match the roles needed to execute the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="roles">The set of roles, that you may have, but you wish to check is authorized to execute the specified Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        public async Task<bool> IsAuthorized(string workflowId, string eventId, IEnumerable<string> roles)
        {
            if (eventId == null || workflowId == null || roles == null)
            {
                throw new ArgumentNullException("eventId", "eventId was null");
            }

            var eventRoles = await _storage.GetRoles(workflowId, eventId);

            return eventRoles.Intersect(roles).Any();
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}