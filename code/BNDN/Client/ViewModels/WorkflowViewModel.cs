using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Client.Connections;
using Common;

namespace Client.ViewModels
{
    public class WorkflowViewModel : ViewModelBase
    {
        private readonly WorkflowDto _workflowDto;
        private bool _resetEventRuns = false;
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

            IServerConnection connection = new ServerConnection(new Uri(@"http://flowit.azurewebsites.net/"));

            var test = (await connection.GetEventsFromWorkflow(_workflowDto))
                .AsParallel()
                .Select(eventAddressDto => new EventViewModel(eventAddressDto, this))
                .ToList();

            // Use this for ordering the events.
            //EventList = new ObservableCollection<EventViewModel>(test
            //    .OrderByDescending(model => model.Executable)
            //    .ThenByDescending(model => model.Pending)
            //    .ThenBy(model => model.Name));
            
            EventList = new ObservableCollection<EventViewModel>(test);

            SelectedEventViewModel = EventList.Count >= 1 ? EventList[0] : null;
            
            NotifyPropertyChanged("");
        }
        /// <summary>
        /// This method resets all the events on the workflow by deleting them and adding them again.
        /// This Method ONLY EXISTS FOR TESTING!
        /// This method is called when the button "Reset is called".
        /// </summary>
        public async void ResetWorkflow()
        {
            if (_resetEventRuns) return;
            _resetEventRuns = true;
            
            IServerConnection serverConnection = new ServerConnection(new Uri(@"http://flowit.azurewebsites.net/"));

            var adminEventList = (await serverConnection.GetEventsFromWorkflow(_workflowDto))
                .AsParallel()
                .ToList();

            // Reset all the events.
            foreach (var eventViewModel in adminEventList)
            {
                IEventConnection connection = new EventConnection(eventViewModel);
                await connection.ResetEvent();
            }
            NotifyPropertyChanged("");
            GetEvents();
            _resetEventRuns = false;
        }
        #endregion

        /// <summary>
        /// This method is used by the list in the UI to represent each object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //.FormatString(this string myString) is an extension.
            return Name;
        }
    }
}
