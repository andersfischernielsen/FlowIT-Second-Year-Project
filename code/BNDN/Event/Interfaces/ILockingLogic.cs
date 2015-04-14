using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Event.Interfaces
{
    interface ILockingLogic : IDisposable
    {
        void LockSelf(string eventId,string callerId);
        void UnlockSelf(string eventId, string callerId);
        void LockAll(string eventId);
        void UnlockAll(string eventId);
        bool IsAllowedToOperate(string eventId, string callerId);
    }
}
