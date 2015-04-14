using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Interfaces
{
    interface IAuthLogic
    {
        bool IsAuthorized(string eventId, IEnumerable<string> roles);
    }
}
