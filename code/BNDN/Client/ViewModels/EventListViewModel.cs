using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public class EventListViewModel : ViewModelBase
    {
        public EventListViewModel()
        {
            EventList = new ObservableCollection<EventViewModel>();
        }

        #region Databindings

        public ObservableCollection<EventViewModel> EventList;

        private EventViewModel _selecteEventViewModel;

        public EventViewModel SelectedEventViewModel
        {
            get { return _selecteEventViewModel; }
            set
            {
                _selecteEventViewModel = value;
                NotifyPropertyChanged("SelectedEventViewModel");
            }
        }

        #endregion

        #region Actions

        #endregion
    }
}
