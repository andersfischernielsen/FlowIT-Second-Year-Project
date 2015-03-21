using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Client.ViewModels
{
    public class EventViewModel : ViewModelBase
    {
        private EventAddressDto __eventAddressDto;
        public EventViewModel()
        {
            
        }
        public EventViewModel(EventAddressDto _eventAddressDto)
        {
            __eventAddressDto = _eventAddressDto;
        }

        #region Databindings
        #endregion

        #region Actions
        #endregion
    }
}
