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
        private EventDto _eventDto;
        public EventViewModel()
        {
            
        }
        public EventViewModel(EventDto eventDto)
        {
            _eventDto = eventDto;
        }

        #region Databindings
        #endregion

        #region Actions
        #endregion
    }
}
