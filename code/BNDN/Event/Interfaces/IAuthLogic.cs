using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Event.Interfaces
{
    public interface IAuthLogic : IDisposable
    {
        Task<bool> IsAuthorized(string eventId, IEnumerable<string> roles);
    }
}
