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
            _workflowDto = new WorkflowDto();
        }

        public WorkflowViewModel(WorkflowDto workflowDto)
        {
            EventList = new ObservableCollection<EventViewModel>();
            _workflowDto = workflowDto;
        }

        #region Databindings

        public string Name
        {
            get { return _workflowDto.Name; }
            set
            {
                _workflowDto.Name = value;
                NotifyPropertyChanged("Name");
            }
        }

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
                #if DEBUG
                var connection = ServerConnection.GetStorage(new Uri("http://localhost:13768/")); // todo get the real server address here
                #else
                var connection = ServerConnection.GetStorage(new Uri("Server"));
                #endif

                EventList = new ObservableCollection<EventViewModel>((await connection.GetEventsFromWorkflow(_workflowDto)).Select(EventAddressDto => new EventViewModel(EventAddressDto)));
                if (EventList.Count >= 1)
                {
                    SelectedEventViewModel = EventList[0];
                }
                else
                {
                    SelectedEventViewModel = null;
                }
                NotifyPropertyChanged("");
            });
        }
        #endregion

        public override string ToString()
        {
            //.FormatString(this string myString) is an extension.
            return Name;
        }
    }
}
