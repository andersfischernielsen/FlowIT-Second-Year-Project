using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Event.Interfaces
{
    public interface ILockingLogic
    {
        bool LockSelf(string eventId,string callerId);
        bool UnlockSelf(string eventId, string callerId);
        bool LockAll(string eventId);
        bool UnlockAll(string eventId);
        bool IsAllowedToOperate(string eventId, string callerId);
    }
}
