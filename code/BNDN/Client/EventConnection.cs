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
        private readonly HttpClientToolbox _http;
        private readonly EventAddressDto _eventDto;

        public EventConnection(EventAddressDto eventDto)
        {
            this._eventDto = eventDto;
            _http = new HttpClientToolbox(_eventDto.Uri);
        }

        public async Task<EventStateDto> GetState()
        {
            return await _http.Read<EventStateDto>("event/state");
        }
        

        public async Task Execute(bool b)
        {
            await _http.Update("event/executed", b);
        }
    }
}
