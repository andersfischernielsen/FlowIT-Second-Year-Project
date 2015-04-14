using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Event.Interfaces
{
    public interface ILockingLogic : IDisposable
    {
        Task LockSelf(string eventId, LockDto lockDto);
        Task UnlockSelf(string eventId, string callerId);
        Task<bool> LockAll(string eventId);

        Task<bool> UnlockAll(string eventId);
        Task<bool> IsAllowedToOperate(string eventId, string callerId);
    }
}
