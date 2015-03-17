using Common;

namespace Client
{
    public interface IEventConnection
    {
        EventStateDto GetState(EventAddressDto eventDto);
        void Execute(EventAddressDto eventDto);
    }
}
