using System;
using System.Collections.ObjectModel;
using System.Linq;
using Common;

namespace Client.ViewModels
{
    public class WorkflowViewModel : ViewModelBase
    {
        private readonly WorkflowDto _workflowDto;
        public string WorkflowId { get { return _workflowDto.Id; } }
        private readonly WorkflowListViewModel _parent;
        public WorkflowViewModel()
        {
            EventList = new ObservableCollection<EventViewModel>();
            _workflowDto = new WorkflowDto();
        }

        public WorkflowViewModel(WorkflowDto workflowDto, WorkflowListViewModel listViewModel)
        {
            EventList = new ObservableCollection<EventViewModel>();
            _parent = listViewModel;
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

        public async void GetEvents()
        {
            SelectedEventViewModel = null;
            EventList.Clear();

            //TODO: Get the actual server address here.
            var connection = new ServerConnection(new Uri(@"http://localhost:13768/"));

            var test = await connection.GetEventsFromWorkflow(_workflowDto);
            EventList = new ObservableCollection<EventViewModel>(test.Select(eventAddressDto => new EventViewModel(eventAddressDto, this)));
            SelectedEventViewModel = EventList.Count >= 1 ? EventList[0] : null;
            
            NotifyPropertyChanged("");
        }
        #endregion

        public override string ToString()
        {
            //.FormatString(this string myString) is an extension.
            return Name;
        }
    }
}
