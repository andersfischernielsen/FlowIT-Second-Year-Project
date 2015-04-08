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

        public async void GetEvents()
        {
            SelectedEventViewModel = null;
            EventList.Clear();

            //TODO: Get the actual server address here.
            var connection = new ServerConnection(new Uri(@"http://localhost:13768/"));

            var test = (await connection.GetEventsFromWorkflow(_workflowDto))
                .AsParallel()
                .Select(eventAddressDto => new EventViewModel(eventAddressDto, this))
                .ToList();

            //EventList = new ObservableCollection<EventViewModel>(test
            //    .OrderByDescending(model => model.Executable)
            //    .ThenByDescending(model => model.Pending)
            //    .ThenBy(model => model.Name));
            
            // brug denne for hurtigere loading.
            EventList = new ObservableCollection<EventViewModel>(test);

            SelectedEventViewModel = EventList.Count >= 1 ? EventList[0] : null;
            
            NotifyPropertyChanged("");
        }
        /// <summary>
        /// This method resets all the events on the workflow by deleting them and adding them again.
        /// This Method ONLY EXISTS FOR TESTING!
        /// </summary>
        public async void ResetWorkflow()
        {
            // Delete all the events.
            foreach (var eventViewModel in EventList)
            {
                var connection = new EventConnection(new EventAddressDto { Id = eventViewModel.Id, Uri = eventViewModel.Uri});
                await connection.DeleteEvent();
            }
            // Create them again.
            // TODO put create events again here.
        }
        #endregion

        public override string ToString()
        {
            //.FormatString(this string myString) is an extension.
            return Name;
        }
    }
}
