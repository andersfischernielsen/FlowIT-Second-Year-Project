using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Event.Interfaces;
using Event.Storage;

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
            return (await _storage.GetRoles(eventId)).Intersect(roles).Any();
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}