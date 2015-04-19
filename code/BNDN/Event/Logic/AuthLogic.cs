using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event.Exceptions;
using Event.Interfaces;

namespace Event.Logic
{
    public class AuthLogic : IAuthLogic
    {
        private readonly IEventStorage _storage;

        public AuthLogic(IEventStorage storage)
        {
            _storage = storage;
        }

        public async Task<bool> IsAuthorized(string eventId, IEnumerable<string> roles)
        {
            var eventRoles = await _storage.GetRoles(eventId);

            if (eventRoles == null) throw new NotFoundException();

            return eventRoles.Intersect(roles).Any();
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}