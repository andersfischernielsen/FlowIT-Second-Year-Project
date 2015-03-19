using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Event.Models
{
    public interface IEventCommunicator
    {
        void SendNotify(IPEndPoint endPoint, params NotifyDto[] dtos);
    }
}
