using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Client.ViewModels
{
    public class WorkflowViewModel : ViewModelBase
    {
        private WorkflowDto _workflowDto;
        public WorkflowViewModel()
        {
            EventList = new ObservableCollection<EventViewModel>();
        }

        public WorkflowViewModel(WorkflowDto workflowDto)
        {
            EventList = new ObservableCollection<EventViewModel>();
            _workflowDto = workflowDto;
        }

        #region Databindings

        public ObservableCollection<EventViewModel> EventList { get; set; }

        private EventViewModel _selectedEventViewModel;

        public EventViewModel SelectedEventViewModel
        {
            get { return _selectedEventViewModel; }
            set
            {
                _selectedEventViewModel = value;
                NotifyPropertyChanged("SelectedEventViewModel");
            }
        }
        #endregion

        #region Actions

        public void GetEvents()
        {
            Task.Run(async () =>
            {
                EventList.Clear();
                var connection = ServerConnection.GetStorage(new Uri("servers")); // todo get the real server address here
                EventList = new ObservableCollection<EventViewModel>((await connection.GetEventsFromWorkflow(_workflowDto)).Select(EventAddressDto => new EventViewModel(EventAddressDto)));
                if (EventList.Count >= 1)
                {
                    SelectedEventViewModel = EventList[0];
                }
                NotifyPropertyChanged("");
            });
        }
        #endregion
    }
}
