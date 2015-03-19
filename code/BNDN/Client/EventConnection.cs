using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Client
{
    public class EventConnection : IEventConnection
    {
        private AwiaHttpClientToolbox _awiaHttp;
        private EventAddressDto _eventDto;

        public EventConnection(EventAddressDto eventDto)
        {
            this._eventDto = eventDto;
            _awiaHttp = new AwiaHttpClientToolbox(_eventDto.Uri);
        }

        public Task<EventStateDto> GetState()
        {
            return _awiaHttp.Read<EventStateDto>("event/state");
        }
        

        public Task Execute(bool b)
        {
            return _awiaHttp.Update("/event/", b);
        }
    }
}
